import {
  BadRequestException,
  ConflictException,
  ForbiddenException,
  Injectable,
  NotFoundException,
  UnauthorizedException,
} from '@nestjs/common';
import { JwtService } from '@nestjs/jwt';
import { pbkdf2Sync, randomBytes, timingSafeEqual } from 'crypto';
import { PrismaService } from '../prisma/prisma.service';
import {
  PortalLeadDto,
  PortalLoginDto,
  PortalRegisterDto,
  PortalRegisterParentDto,
  PortalUpdateProfileDto,
} from './dto/erp-portal.dto';

/** Sama persis dengan skema hash erp-users (pbkdf2 sha256, 310k iterasi). */
function hashPassword(password: string): string {
  const salt = randomBytes(16).toString('hex');
  const hash = pbkdf2Sync(password, salt, 310000, 32, 'sha256').toString('hex');
  return `${salt}:${hash}`;
}

function verifyPassword(password: string, stored: string): boolean {
  const [salt, hash] = stored.split(':');
  if (!salt || !hash) return false;
  const calc = pbkdf2Sync(password, salt, 310000, 32, 'sha256');
  const expected = Buffer.from(hash, 'hex');
  return calc.length === expected.length && timingSafeEqual(calc, expected);
}

const id = (v: bigint | null | undefined) => (v == null ? null : v.toString());

/**
 * Fase 3 W2/W3 — akun portal sekolah: registrasi (PENDING) → persetujuan admin
 * (membuat/menautkan partner CUST-SCHOOL + profil A1) → login JWT portal.
 * W1 — lead landing page langsung menjadi partner PROSPEK di CRM Sekolah.
 */
@Injectable()
export class ErpPortalAccountsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly jwt: JwtService,
  ) {}

  private accountView(a: any) {
    const meta = (a.metadata ?? {}) as Record<string, unknown>;
    return {
      id: id(a.id),
      email: a.email,
      fullName: a.fullName,
      phone: a.phone,
      role: a.role,
      status: a.status,
      schoolName: a.schoolName,
      npsn: a.npsn,
      jenjang: a.jenjang,
      partnerId: id(a.partnerId),
      ...(meta.kind === 'parent'
        ? {
            studentName: (meta.studentName as string) ?? null,
            studentClass: (meta.studentClass as string) ?? null,
          }
        : {}),
      lastLoginAt: a.lastLoginAt,
      createdAt: a.createdAt,
    };
  }

  private async schoolTypeId(): Promise<bigint> {
    const t = await this.prisma.erpPartnerType.findFirst({
      where: { code: 'CUST-SCHOOL', deletedAt: null },
    });
    if (!t) throw new BadRequestException('Tipe partner CUST-SCHOOL tidak ditemukan');
    return t.id;
  }

  private async createSchoolPartner(name: string, codePrefix: string) {
    const typeId = await this.schoolTypeId();
    const code = `${codePrefix}-${Date.now().toString(36).toUpperCase()}`;
    return this.prisma.erpPartner.create({
      data: { code, name, partnerTypeId: typeId },
    });
  }

  // ── Public: registration & login ──────────────────────────────────────────

  async register(dto: PortalRegisterDto) {
    const email = dto.email.trim().toLowerCase();
    const existing = await this.prisma.erpPortalAccount.findUnique({ where: { email } });
    if (existing && !existing.deletedAt) {
      throw new ConflictException('Email sudah terdaftar di portal');
    }
    const account = await this.prisma.erpPortalAccount.create({
      data: {
        email,
        passwordHash: hashPassword(dto.password),
        fullName: dto.fullName,
        phone: dto.phone,
        role: (dto.role as any) ?? 'OPERATOR',
        schoolName: dto.schoolName,
        npsn: dto.npsn,
        jenjang: dto.jenjang as any,
        address: dto.address,
        status: 'PENDING',
        metadata: { origin: 'portal-register' },
      },
    });
    return {
      ...this.accountView(account),
      message: 'Pendaftaran terkirim. Tim Bahtera Madani akan memverifikasi sekolah Anda sebelum akun aktif.',
    };
  }

  // ── Public: daftar sekolah untuk pemilih pendaftaran orang tua (W4) ─────

  async listPublicSchools() {
    const typeId = await this.schoolTypeId();
    const partners = await this.prisma.erpPartner.findMany({
      where: { partnerTypeId: typeId, deletedAt: null },
      orderBy: { name: 'asc' },
      take: 500,
      select: { id: true, name: true },
    });
    const profiles = partners.length
      ? await this.prisma.erpSchoolProfile.findMany({
          where: { partnerId: { in: partners.map((p) => p.id) } },
          select: { partnerId: true, jenjang: true },
        })
      : [];
    const jenjang = new Map(profiles.map((p) => [p.partnerId.toString(), p.jenjang]));
    return {
      data: partners.map((p) => ({
        id: id(p.id),
        name: p.name,
        jenjang: jenjang.get(p.id.toString()) ?? null,
      })),
      total: partners.length,
    };
  }

  // ── Public: pendaftaran orang tua (W4) ────────────────────────────────────
  // Orang tua memilih sekolah yang SUDAH ada; persetujuan admin menautkan
  // akun ke partner sekolah itu (tidak membuat partner baru) dan data siswa
  // (nama + kelas) disimpan di metadata akun.

  async registerParent(dto: PortalRegisterParentDto) {
    const email = dto.email.trim().toLowerCase();
    const existing = await this.prisma.erpPortalAccount.findUnique({ where: { email } });
    if (existing && !existing.deletedAt) {
      throw new ConflictException('Email sudah terdaftar di portal');
    }
    const typeId = await this.schoolTypeId();
    const school = await this.prisma.erpPartner.findFirst({
      where: { id: BigInt(dto.schoolPartnerId), partnerTypeId: typeId, deletedAt: null },
    });
    if (!school) throw new BadRequestException('Sekolah tidak ditemukan. Pilih sekolah dari daftar.');
    const account = await this.prisma.erpPortalAccount.create({
      data: {
        email,
        passwordHash: hashPassword(dto.password),
        fullName: dto.fullName,
        phone: dto.phone,
        role: 'ORANG_TUA' as any,
        schoolName: school.name,
        status: 'PENDING',
        metadata: {
          kind: 'parent',
          schoolPartnerId: dto.schoolPartnerId,
          studentName: dto.studentName,
          studentClass: dto.studentClass,
          origin: 'portal-register-parent',
        },
      },
    });
    return {
      ...this.accountView(account),
      message: 'Pendaftaran orang tua terkirim. Akun aktif setelah diverifikasi admin Bahtera Madani.',
    };
  }

  async login(dto: PortalLoginDto) {
    const email = dto.email.trim().toLowerCase();
    const account = await this.prisma.erpPortalAccount.findUnique({ where: { email } });
    if (!account || account.deletedAt || !verifyPassword(dto.password, account.passwordHash)) {
      throw new UnauthorizedException('Email atau kata sandi salah');
    }
    if (account.status === 'PENDING') {
      throw new ForbiddenException('Pendaftaran Anda masih menunggu persetujuan admin');
    }
    if (account.status === 'REJECTED') {
      throw new ForbiddenException(
        `Pendaftaran ditolak${account.rejectedReason ? `: ${account.rejectedReason}` : ''}`,
      );
    }
    if (account.status === 'SUSPENDED') {
      throw new ForbiddenException('Akun ditangguhkan. Hubungi admin Bahtera Madani');
    }
    await this.prisma.erpPortalAccount.update({
      where: { id: account.id },
      data: { lastLoginAt: new Date() },
    });
    const accessToken = await this.jwt.signAsync({
      sub: account.id.toString(),
      aud: 'portal',
      partnerId: id(account.partnerId),
    });
    return { accessToken, account: this.accountView(account) };
  }

  // ── Portal: own profile ───────────────────────────────────────────────────

  async me(account: any) {
    const partner = account.partnerId
      ? await this.prisma.erpPartner.findUnique({ where: { id: account.partnerId } })
      : null;
    const profile = account.partnerId
      ? await this.prisma.erpSchoolProfile.findUnique({ where: { partnerId: account.partnerId } })
      : null;
    return {
      account: this.accountView(account),
      school: partner
        ? {
            partnerId: id(partner.id),
            code: partner.code,
            name: partner.name,
            npsn: profile?.npsn ?? account.npsn,
            jenjang: profile?.jenjang ?? account.jenjang,
            negeriSwasta: profile?.negeriSwasta ?? null,
            accreditation: profile?.accreditation ?? null,
            studentCount: profile?.studentCount ?? null,
            classCount: profile?.classCount ?? null,
            pipelineStage: profile?.pipelineStage ?? null,
            bosPagu: profile?.bosPagu != null ? profile.bosPagu.toString() : null,
            bosPeriodYear: profile?.bosPeriodYear ?? null,
          }
        : null,
    };
  }

  async updateProfile(account: any, dto: PortalUpdateProfileDto) {
    if (dto.fullName !== undefined || dto.phone !== undefined) {
      await this.prisma.erpPortalAccount.update({
        where: { id: account.id },
        data: { fullName: dto.fullName, phone: dto.phone },
      });
    }
    if (
      account.partnerId &&
      (dto.studentCount !== undefined || dto.classCount !== undefined)
    ) {
      await this.prisma.erpSchoolProfile.updateMany({
        where: { partnerId: account.partnerId },
        data: { studentCount: dto.studentCount, classCount: dto.classCount },
      });
    }
    const fresh = await this.prisma.erpPortalAccount.findUnique({
      where: { id: account.id },
    });
    return this.me(fresh);
  }

  // ── Public: landing-page lead (W1) ────────────────────────────────────────

  async createLead(dto: PortalLeadDto) {
    const partner = await this.createSchoolPartner(dto.schoolName, 'LEAD');
    await this.prisma.erpSchoolProfile.create({
      data: {
        partnerId: partner.id,
        jenjang: dto.jenjang as any,
        pipelineStage: 'PROSPEK',
        visitNotes: dto.message
          ? `Lead landing page: ${dto.message}`
          : 'Lead dari landing page',
      },
    });
    const lead = await this.prisma.erpPortalLead.create({
      data: {
        schoolName: dto.schoolName,
        contactName: dto.contactName,
        phone: dto.phone,
        email: dto.email?.trim().toLowerCase(),
        jenjang: dto.jenjang as any,
        message: dto.message,
        partnerId: partner.id,
        status: 'CONVERTED',
        metadata: { origin: 'landing-page' },
      },
    });
    return {
      id: id(lead.id),
      partnerId: id(partner.id),
      message: 'Terima kasih. Permintaan penawaran Anda sudah kami terima dan akan segera ditindaklanjuti.',
    };
  }

  // ── Admin (internal JWT): approval queue (W3) ─────────────────────────────

  async listAccounts(status?: string, search?: string) {
    const rows = await this.prisma.erpPortalAccount.findMany({
      where: {
        deletedAt: null,
        ...(status ? { status: status as any } : {}),
        ...(search
          ? {
              OR: [
                { email: { contains: search, mode: 'insensitive' } },
                { schoolName: { contains: search, mode: 'insensitive' } },
                { fullName: { contains: search, mode: 'insensitive' } },
              ],
            }
          : {}),
      },
      orderBy: { createdAt: 'desc' },
      take: 200,
    });
    return { data: rows.map((a) => this.accountView(a)), total: rows.length };
  }

  async listLeads() {
    const rows = await this.prisma.erpPortalLead.findMany({
      where: { deletedAt: null },
      orderBy: { createdAt: 'desc' },
      take: 200,
    });
    return {
      data: rows.map((l) => ({
        id: id(l.id),
        schoolName: l.schoolName,
        contactName: l.contactName,
        phone: l.phone,
        email: l.email,
        jenjang: l.jenjang,
        message: l.message,
        status: l.status,
        partnerId: id(l.partnerId),
        createdAt: l.createdAt,
      })),
      total: rows.length,
    };
  }

  private async mustAccount(accountId: string) {
    const a = await this.prisma.erpPortalAccount.findUnique({
      where: { id: BigInt(accountId) },
    });
    if (!a || a.deletedAt) throw new NotFoundException('Akun portal tidak ditemukan');
    return a;
  }

  async approve(accountId: string, actorId?: string) {
    const account = await this.mustAccount(accountId);
    let partnerId = account.partnerId;
    if (!partnerId && account.role === 'ORANG_TUA') {
      // W4: orang tua tertaut ke partner sekolah yang dipilih saat daftar —
      // TIDAK membuat partner/profil baru.
      const meta = (account.metadata ?? {}) as Record<string, unknown>;
      const schoolPartnerId = meta.schoolPartnerId as string | undefined;
      if (!schoolPartnerId) {
        throw new BadRequestException('Akun orang tua tidak memiliki tautan sekolah.');
      }
      const typeId = await this.schoolTypeId();
      const school = await this.prisma.erpPartner.findFirst({
        where: { id: BigInt(schoolPartnerId), partnerTypeId: typeId, deletedAt: null },
      });
      if (!school) throw new BadRequestException('Sekolah tautan akun orang tua tidak ditemukan.');
      partnerId = school.id;
    }
    if (!partnerId) {
      const partner = await this.createSchoolPartner(account.schoolName, 'SCH');
      partnerId = partner.id;
      await this.prisma.erpSchoolProfile.create({
        data: {
          partnerId,
          npsn: account.npsn,
          jenjang: account.jenjang as any,
          pipelineStage: 'PROSPEK',
        },
      });
    }
    const updated = await this.prisma.erpPortalAccount.update({
      where: { id: account.id },
      data: {
        partnerId,
        status: 'ACTIVE',
        approvedAt: new Date(),
        approvedById: actorId ? BigInt(actorId) : null,
        rejectedReason: null,
      },
    });
    return this.accountView(updated);
  }

  async reject(accountId: string, reason?: string) {
    await this.mustAccount(accountId);
    const updated = await this.prisma.erpPortalAccount.update({
      where: { id: BigInt(accountId) },
      data: { status: 'REJECTED', rejectedReason: reason ?? null },
    });
    return this.accountView(updated);
  }

  async setStatus(accountId: string, status: 'SUSPENDED' | 'ACTIVE') {
    const account = await this.mustAccount(accountId);
    if (status === 'ACTIVE' && !account.partnerId) {
      throw new BadRequestException('Akun belum tertaut partner; setujui dulu pendaftarannya');
    }
    const updated = await this.prisma.erpPortalAccount.update({
      where: { id: account.id },
      data: { status },
    });
    return this.accountView(updated);
  }
}

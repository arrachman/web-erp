import {
  CanActivate,
  ExecutionContext,
  Injectable,
  UnauthorizedException,
} from '@nestjs/common';
import { JwtService } from '@nestjs/jwt';
import { PrismaService } from '../prisma/prisma.service';

/**
 * Fase 3 W2 — guard khusus portal sekolah. Token portal ditandatangani dengan
 * secret yang sama dengan auth internal, tetapi WAJIB membawa claim
 * `aud: 'portal'` dan akunnya harus ACTIVE + sudah tertaut partner, sehingga
 * token internal dan token portal tidak bisa saling dipakai.
 */
@Injectable()
export class ErpPortalAuthGuard implements CanActivate {
  constructor(
    private readonly jwt: JwtService,
    private readonly prisma: PrismaService,
  ) {}

  async canActivate(context: ExecutionContext): Promise<boolean> {
    const req = context.switchToHttp().getRequest();
    const header = req.headers['authorization'] as string | undefined;
    if (!header?.startsWith('Bearer ')) {
      throw new UnauthorizedException('Token portal tidak ditemukan');
    }
    let payload: { sub?: string; aud?: string } | null = null;
    try {
      payload = await this.jwt.verifyAsync(header.slice(7));
    } catch {
      throw new UnauthorizedException('Token portal tidak valid atau kedaluwarsa');
    }
    if (!payload?.sub || payload.aud !== 'portal') {
      throw new UnauthorizedException('Token bukan token portal');
    }
    const account = await this.prisma.erpPortalAccount.findUnique({
      where: { id: BigInt(payload.sub) },
    });
    if (!account || account.deletedAt || !account.isActive) {
      throw new UnauthorizedException('Akun portal tidak ditemukan');
    }
    if (account.status !== 'ACTIVE' || !account.partnerId) {
      throw new UnauthorizedException('Akun portal belum aktif');
    }
    req.portalAccount = account;
    return true;
  }
}

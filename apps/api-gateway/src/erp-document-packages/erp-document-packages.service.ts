import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { randomUUID } from 'crypto';
import { promises as fs } from 'fs';
import * as path from 'path';
import { PrismaService } from '../prisma/prisma.service';
import {
  GenerateDocumentsDto,
  QueryDocumentPackagesDto,
  RecordAcceptanceDto,
  SignDocumentDto,
} from './dto/document-packages.dto';
import {
  availableDocKinds,
  buildDocData,
  DocKind,
  DocVariant,
  loadOrderChain,
} from './docpkg.data';
import { renderPackage, renderSingle } from './docpkg.render';

/**
 * A3 — Paket dokumen pengadaan. Documents are rendered from the order's
 * transaction chain (no re-entry), stored as PDF files, and archived per
 * school + budget year with versioning and a sign audit trail.
 */
@Injectable()
export class ErpDocumentPackagesService {
  private readonly uploadDir =
    process.env.ERP_TXN_UPLOAD_DIR ?? path.join(process.cwd(), 'uploads', 'erp-transactions');

  constructor(private readonly prisma: PrismaService) {}

  private async persistPdf(buffer: Buffer): Promise<{ storedName: string }> {
    await fs.mkdir(this.uploadDir, { recursive: true });
    const storedName = `docpkg-${randomUUID()}.pdf`;
    await fs.writeFile(path.join(this.uploadDir, storedName), buffer);
    return { storedName };
  }

  async availability(orderId: bigint) {
    const chain = await loadOrderChain(this.prisma, orderId);
    if (!chain) throw new NotFoundException('Sales order tidak ditemukan.');
    const existing = await this.prisma.erpSlsGeneratedDocument.findMany({
      where: { orderId, deletedAt: null },
      orderBy: { id: 'desc' },
      select: { docType: true, version: true, status: true },
    });
    const latest = new Map<string, { version: number; status: string }>();
    for (const e of existing) {
      if (!latest.has(e.docType)) latest.set(e.docType, { version: e.version, status: e.status });
    }
    return {
      success: true,
      data: {
        orderId: orderId.toString(),
        orderNumber: chain.order.docNumber,
        customer: chain.customer ? { id: chain.customer.id.toString(), name: chain.customer.name } : null,
        defaultVariant: (chain.order.fundingSource === 'NON_BOS' ? 'NON_BOS' : 'BOS') as DocVariant,
        available: availableDocKinds(chain).map((kind) => ({
          kind,
          latest: latest.get(kind) ?? null,
        })),
      },
    };
  }

  async generate(dto: GenerateDocumentsDto, actorId?: string) {
    const orderId = BigInt(dto.orderId);
    const chain = await loadOrderChain(this.prisma, orderId);
    if (!chain) throw new NotFoundException('Sales order tidak ditemukan.');
    const available = availableDocKinds(chain);
    const kinds = [...new Set(dto.docTypes)] as DocKind[];
    const blocked = kinds.filter((k) => !available.includes(k));
    if (blocked.length) {
      throw new BadRequestException(
        `Dokumen belum bisa dibuat untuk order ini: ${blocked.join(', ')} (dokumen sumbernya belum ada).`,
      );
    }
    const variant: DocVariant =
      dto.variant ?? (chain.order.fundingSource === 'NON_BOS' ? 'NON_BOS' : 'BOS');
    const actor = actorId ? BigInt(actorId) : null;
    const schoolId = chain.order.customerId ?? null;
    const budgetYear = chain.order.budgetYear ?? null;
    const created: any[] = [];

    const createRow = async (
      docType: string,
      sourceDocType: string,
      sourceId: bigint,
      buffer: Buffer,
      fileName: string,
    ) => {
      const last = await this.prisma.erpSlsGeneratedDocument.findFirst({
        where: { orderId, docType, deletedAt: null },
        orderBy: { version: 'desc' },
        select: { version: true },
      });
      const version = (last?.version ?? 0) + 1;
      await this.prisma.erpSlsGeneratedDocument.updateMany({
        where: { orderId, docType, deletedAt: null, status: { in: ['GENERATED', 'SIGNED'] } },
        data: { status: 'SUPERSEDED' },
      });
      const { storedName } = await this.persistPdf(buffer);
      const row = await this.prisma.erpSlsGeneratedDocument.create({
        data: {
          orderId,
          docType,
          variant,
          sourceDocType,
          sourceId,
          version,
          fileName,
          storedName,
          mimeType: 'application/pdf',
          sizeBytes: buffer.length,
          status: 'GENERATED',
          schoolId,
          budgetYear,
          createdById: actor,
        },
      });
      created.push(row);
    };

    if (dto.package && kinds.length > 1) {
      const datas = [];
      for (const kind of kinds) datas.push(await buildDocData(this.prisma, chain, kind, variant));
      const buffer = await renderPackage(datas);
      await createRow(
        'PAKET',
        'sls_orders',
        orderId,
        buffer,
        `Paket-${chain.order.docNumber}-v${Date.now()}.pdf`,
      );
    } else {
      for (const kind of kinds) {
        const data = await buildDocData(this.prisma, chain, kind, variant);
        const buffer = await renderSingle(data);
        await createRow(
          kind,
          data.sourceDocType,
          BigInt(data.sourceId),
          buffer,
          `${kind}-${data.sourceDocNumber}.pdf`,
        );
      }
    }
    return { success: true, data: created };
  }

  async findAll(query: QueryDocumentPackagesDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 25;
    const where: Prisma.ErpSlsGeneratedDocumentWhereInput = { deletedAt: null };
    if (query.schoolId) where.schoolId = BigInt(query.schoolId);
    if (query.budgetYear != null) where.budgetYear = query.budgetYear;
    if (query.docType) where.docType = query.docType;
    if (query.status) where.status = query.status;
    if (query.orderId) where.orderId = BigInt(query.orderId);
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { fileName: { contains: q, mode: 'insensitive' } },
        { order: { docNumber: { contains: q, mode: 'insensitive' } } },
      ];
    }
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpSlsGeneratedDocument.findMany({
        where,
        orderBy: { id: 'desc' },
        skip: (page - 1) * limit,
        take: limit,
        include: { order: { select: { id: true, docNumber: true, customerId: true } } },
      }),
      this.prisma.erpSlsGeneratedDocument.count({ where }),
    ]);
    const schoolIds = [...new Set(items.map((i) => i.schoolId).filter((v): v is bigint => v != null))];
    const schools = schoolIds.length
      ? await this.prisma.erpPartner.findMany({
          where: { id: { in: schoolIds } },
          select: { id: true, code: true, name: true },
        })
      : [];
    const byId = new Map(schools.map((s) => [s.id.toString(), s]));
    return {
      success: true,
      data: items.map((i) => ({
        ...i,
        school: i.schoolId ? byId.get(i.schoolId.toString()) ?? null : null,
      })),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  async sign(id: bigint, dto: SignDocumentDto, actorId?: string) {
    const row = await this.prisma.erpSlsGeneratedDocument.findFirst({
      where: { id, deletedAt: null },
    });
    if (!row) throw new NotFoundException('Dokumen tidak ditemukan.');
    if (row.status !== 'GENERATED') {
      throw new BadRequestException(`Dokumen berstatus ${row.status} tidak bisa ditandatangani.`);
    }
    const updated = await this.prisma.erpSlsGeneratedDocument.update({
      where: { id },
      data: {
        status: 'SIGNED',
        signedByName: dto.signerName,
        signedById: actorId ? BigInt(actorId) : null,
        signedAt: new Date(),
        signatureNote: dto.note ?? null,
      },
    });
    return { success: true, data: updated };
  }

  async getFile(id: bigint): Promise<{ buffer: Buffer; fileName: string; mimeType: string }> {
    const row = await this.prisma.erpSlsGeneratedDocument.findFirst({
      where: { id, deletedAt: null },
    });
    if (!row) throw new NotFoundException('Dokumen tidak ditemukan.');
    try {
      const buffer = await fs.readFile(path.join(this.uploadDir, row.storedName));
      return { buffer, fileName: row.fileName, mimeType: row.mimeType };
    } catch {
      throw new NotFoundException('Berkas dokumen tidak ditemukan di penyimpanan.');
    }
  }

  // ── BAST acceptance on delivery reports ────────────────────────────────────

  async listDeliveryReports(acceptance?: string) {
    const drs = await this.prisma.erpSlsDeliveryReport.findMany({
      where: { deletedAt: null },
      orderBy: { id: 'desc' },
      take: 100,
      select: {
        id: true,
        docNumber: true,
        docDate: true,
        status: true,
        acceptedAt: true,
        acceptedByName: true,
        acceptedByTitle: true,
        deliveryOrderId: true,
        customerId: true,
      },
    });
    const filtered = drs.filter((d) =>
      acceptance === 'pending' ? !d.acceptedAt : acceptance === 'done' ? !!d.acceptedAt : true,
    );
    const doIds = [...new Set(filtered.map((d) => d.deliveryOrderId).filter((v): v is bigint => v != null))];
    const dos = doIds.length
      ? await this.prisma.erpSlsDeliveryOrder.findMany({
          where: { id: { in: doIds } },
          select: { id: true, docNumber: true, orderId: true },
        })
      : [];
    const doById = new Map(dos.map((d) => [d.id.toString(), d]));
    const customerIds = [...new Set(filtered.map((d) => d.customerId).filter((v): v is bigint => v != null))];
    const customers = customerIds.length
      ? await this.prisma.erpPartner.findMany({
          where: { id: { in: customerIds } },
          select: { id: true, code: true, name: true },
        })
      : [];
    const custById = new Map(customers.map((c) => [c.id.toString(), c]));
    return {
      success: true,
      data: filtered.map((d) => {
        const doRow = d.deliveryOrderId ? doById.get(d.deliveryOrderId.toString()) : undefined;
        return {
          ...d,
          deliveryOrder: doRow
            ? { id: doRow.id.toString(), docNumber: doRow.docNumber, orderId: doRow.orderId?.toString() ?? null }
            : null,
          customer: d.customerId ? custById.get(d.customerId.toString()) ?? null : null,
        };
      }),
    };
  }

  async recordAcceptance(drId: bigint, dto: RecordAcceptanceDto) {
    const dr = await this.prisma.erpSlsDeliveryReport.findFirst({
      where: { id: drId, deletedAt: null },
    });
    if (!dr) throw new NotFoundException('Laporan penerimaan (DR) tidak ditemukan.');
    const updated = await this.prisma.erpSlsDeliveryReport.update({
      where: { id: drId },
      data: {
        acceptedAt: dto.acceptedAt ? new Date(dto.acceptedAt) : new Date(),
        acceptedByName: dto.acceptedByName,
        acceptedByTitle: dto.acceptedByTitle ?? null,
        acceptanceNotes: dto.notes ?? null,
      },
    });
    return { success: true, data: updated };
  }
}

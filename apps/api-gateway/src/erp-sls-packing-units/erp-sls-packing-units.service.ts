import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { GeneratePackingUnitsDto, PackingStudentDto } from './dto/packing-unit.dto';

/**
 * Fase 2 P6 — Packing per siswa: di atas Packing List yang sudah ada, unit
 * kemas per siswa/kelas dibuat dari roster (array atau teks CSV nama,kelas),
 * isinya snapshot baris packing list (atau ditentukan eksplisit), lalu
 * ditandai PACKED satu per satu dengan progres per packing list.
 */
@Injectable()
export class ErpSlsPackingUnitsService {
  constructor(private readonly prisma: PrismaService) {}

  private present(row: any) {
    return {
      id: row.id?.toString(),
      packingListId: row.packingListId?.toString(),
      sequenceNo: row.sequenceNo,
      studentName: row.studentName,
      className: row.className,
      contents: row.contents ?? [],
      status: row.status,
      packedAt: row.packedAt,
      packedById: row.packedById?.toString() ?? null,
      notes: row.notes,
    };
  }

  private async packingListOrThrow(id: bigint) {
    const pl = await this.prisma.erpSlsPackingList.findFirst({
      where: { id, deletedAt: null },
      select: { id: true, docNumber: true, customerId: true },
    });
    if (!pl) throw new NotFoundException('Packing list tidak ditemukan.');
    return pl;
  }

  async overview() {
    const lists = await this.prisma.erpSlsPackingList.findMany({
      where: { deletedAt: null },
      orderBy: { docDate: 'desc' },
      take: 100,
      select: { id: true, docNumber: true, docDate: true, customerId: true, status: true },
    });
    const grouped = await this.prisma.erpSlsPackingUnit.groupBy({
      by: ['packingListId', 'status'],
      where: { deletedAt: null },
      _count: { _all: true },
    });
    const counts = new Map<string, { total: number; packed: number }>();
    for (const g of grouped) {
      const key = g.packingListId.toString();
      const cur = counts.get(key) ?? { total: 0, packed: 0 };
      cur.total += g._count._all;
      if (g.status === 'PACKED') cur.packed += g._count._all;
      counts.set(key, cur);
    }
    const customerIds = [...new Set(lists.map((l) => l.customerId).filter(Boolean))] as bigint[];
    const customers = customerIds.length
      ? await this.prisma.erpPartner.findMany({
          where: { id: { in: customerIds } },
          select: { id: true, name: true },
        })
      : [];
    const nameById = new Map(customers.map((c) => [c.id.toString(), c.name]));
    return {
      data: lists.map((l) => ({
        id: l.id.toString(),
        docNumber: l.docNumber,
        docDate: l.docDate ? new Date(l.docDate).toISOString().slice(0, 10) : null,
        customerName: l.customerId ? (nameById.get(l.customerId.toString()) ?? null) : null,
        status: l.status,
        units: counts.get(l.id.toString()) ?? { total: 0, packed: 0 },
      })),
    };
  }

  async listUnits(packingListId: string) {
    const pl = await this.packingListOrThrow(BigInt(packingListId));
    const rows = await this.prisma.erpSlsPackingUnit.findMany({
      where: { packingListId: pl.id, deletedAt: null },
      orderBy: { sequenceNo: 'asc' },
    });
    const packed = rows.filter((r) => r.status === 'PACKED').length;
    return {
      data: rows.map((r) => this.present(r)),
      progress: { total: rows.length, packed },
    };
  }

  private parseRoster(dto: GeneratePackingUnitsDto): PackingStudentDto[] {
    if (dto.students?.length) return dto.students;
    if (dto.rosterCsv?.trim()) {
      const out: PackingStudentDto[] = [];
      for (const raw of dto.rosterCsv.split(/\r?\n/)) {
        const line = raw.trim();
        if (!line) continue;
        const sep = line.includes(';') ? ';' : ',';
        const [name, kelas] = line.split(sep).map((s) => s?.trim());
        if (name) out.push({ studentName: name, className: kelas || undefined });
      }
      return out;
    }
    return [];
  }

  async generate(packingListId: string, dto: GeneratePackingUnitsDto, actorId?: string) {
    const pl = await this.packingListOrThrow(BigInt(packingListId));
    const students = this.parseRoster(dto);
    if (!students.length) {
      throw new BadRequestException('Roster kosong — kirim students[] atau rosterCsv.');
    }
    let contents: { itemId: string; name: string | null; quantity: string }[];
    if (dto.items?.length) {
      contents = [];
      for (const it of dto.items) {
        const item = await this.prisma.erpItem.findUnique({
          where: { id: BigInt(it.itemId) },
          select: { name: true },
        });
        contents.push({ itemId: it.itemId, name: item?.name ?? null, quantity: it.quantity });
      }
    } else {
      const lines = await this.prisma.erpSlsPackingListLine.findMany({
        where: { packingListId: pl.id },
        orderBy: { id: 'asc' },
        select: { itemId: true, quantity: true },
      });
      contents = [];
      for (const l of lines) {
        const item = await this.prisma.erpItem.findUnique({
          where: { id: l.itemId },
          select: { name: true },
        });
        contents.push({
          itemId: l.itemId.toString(),
          name: item?.name ?? null,
          quantity: l.quantity.toString(),
        });
      }
    }
    const last = await this.prisma.erpSlsPackingUnit.findFirst({
      where: { packingListId: pl.id, deletedAt: null },
      orderBy: { sequenceNo: 'desc' },
      select: { sequenceNo: true },
    });
    let seq = last?.sequenceNo ?? 0;
    const actor = actorId ? BigInt(actorId) : null;
    await this.prisma.erpSlsPackingUnit.createMany({
      data: students.map((s) => ({
        packingListId: pl.id,
        sequenceNo: ++seq,
        studentName: s.studentName,
        className: s.className ?? null,
        contents: contents as unknown as Prisma.InputJsonValue,
        createdById: actor,
        updatedById: actor,
      })),
    });
    return this.listUnits(packingListId);
  }

  async setPacked(unitId: string, packed: boolean, actorId?: string) {
    const unit = await this.prisma.erpSlsPackingUnit.findFirst({
      where: { id: BigInt(unitId), deletedAt: null },
    });
    if (!unit) throw new NotFoundException('Unit packing tidak ditemukan.');
    const updated = await this.prisma.erpSlsPackingUnit.update({
      where: { id: unit.id },
      data: {
        status: packed ? 'PACKED' : 'PENDING',
        packedAt: packed ? new Date() : null,
        packedById: packed ? (actorId ? BigInt(actorId) : null) : null,
      },
    });
    return this.present(updated);
  }

  async removeUnit(unitId: string) {
    const unit = await this.prisma.erpSlsPackingUnit.findFirst({
      where: { id: BigInt(unitId), deletedAt: null },
    });
    if (!unit) throw new NotFoundException('Unit packing tidak ditemukan.');
    await this.prisma.erpSlsPackingUnit.update({
      where: { id: unit.id },
      data: { deletedAt: new Date() },
    });
    return { success: true };
  }
}

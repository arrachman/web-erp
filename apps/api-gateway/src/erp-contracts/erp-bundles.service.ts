import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { UpsertBundleDto } from './dto/contracts.dto';

const id = (v: bigint | null | undefined) => (v == null ? null : v.toString());

export interface BundleComponent {
  itemId: string;
  name: string | null;
  code: string | null;
  quantity: string;
  unit: string | null;
}

export interface BundleView {
  itemId: string;
  components: BundleComponent[];
}

/**
 * Fase 3 W7 — paket/bundle: item paket (mis. "Paket Kelas 1" berisi seragam,
 * buku, ATK) didefinisikan sekali sebagai master; portal menampilkan isi
 * paketnya dan item paket terjual sebagai satu baris order berharga paket.
 * Ekspansi komponen untuk picking/packing mengikuti isi paket ini.
 */
@Injectable()
export class ErpBundlesService {
  constructor(private readonly prisma: PrismaService) {}

  private async itemInfo(itemIds: bigint[]) {
    if (!itemIds.length) return new Map<string, { name: string; code: string; unit: string | null }>();
    const items = await this.prisma.erpItem.findMany({
      where: { id: { in: itemIds } },
      select: { id: true, name: true, code: true, baseUnitId: true },
    });
    const unitIds = [...new Set(items.map((i) => i.baseUnitId).filter(Boolean))] as bigint[];
    const units = unitIds.length
      ? await this.prisma.erpUnit.findMany({ where: { id: { in: unitIds } }, select: { id: true, name: true } })
      : [];
    const unitName = new Map(units.map((u) => [u.id.toString(), u.name]));
    return new Map(
      items.map((i) => [
        i.id.toString(),
        { name: i.name, code: i.code, unit: i.baseUnitId ? unitName.get(i.baseUnitId.toString()) ?? null : null },
      ]),
    );
  }

  private async present(bundle: any) {
    const lines = await this.prisma.erpItemBundleLine.findMany({
      where: { bundleId: bundle.id, deletedAt: null },
      orderBy: { lineNo: 'asc' },
    });
    const infos = await this.itemInfo([bundle.itemId, ...lines.map((l) => l.componentItemId)]);
    const self = infos.get(bundle.itemId.toString());
    return {
      id: id(bundle.id),
      itemId: id(bundle.itemId),
      itemName: self?.name ?? null,
      itemCode: self?.code ?? null,
      name: bundle.name ?? null,
      notes: bundle.notes ?? null,
      isActive: bundle.isActive,
      lines: lines.map((l) => {
        const info = infos.get(l.componentItemId.toString());
        return {
          id: id(l.id),
          componentItemId: id(l.componentItemId),
          componentName: info?.name ?? null,
          componentCode: info?.code ?? null,
          quantity: l.quantity.toString(),
          notes: l.notes ?? null,
        };
      }),
      createdAt: bundle.createdAt,
    };
  }

  async list() {
    const bundles = await this.prisma.erpItemBundle.findMany({
      where: { deletedAt: null },
      orderBy: { createdAt: 'desc' },
      take: 200,
    });
    const data = [];
    for (const b of bundles) data.push(await this.present(b));
    return { data, total: data.length };
  }

  async get(bundleId: string) {
    const bundle = await this.prisma.erpItemBundle.findFirst({
      where: { id: BigInt(bundleId), deletedAt: null },
    });
    if (!bundle) throw new NotFoundException('Paket tidak ditemukan.');
    return this.present(bundle);
  }

  /** Peta itemId paket → isi paket, untuk katalog portal. */
  async bundlesForItems(itemIds: bigint[]): Promise<Map<string, BundleView>> {
    const map = new Map<string, BundleView>();
    if (!itemIds.length) return map;
    const bundles = await this.prisma.erpItemBundle.findMany({
      where: { itemId: { in: itemIds }, isActive: true, deletedAt: null },
    });
    if (!bundles.length) return map;
    const lines = await this.prisma.erpItemBundleLine.findMany({
      where: { bundleId: { in: bundles.map((b) => b.id) }, deletedAt: null },
      orderBy: { lineNo: 'asc' },
    });
    const infos = await this.itemInfo(lines.map((l) => l.componentItemId));
    for (const b of bundles) {
      map.set(b.itemId.toString(), {
        itemId: b.itemId.toString(),
        components: lines
          .filter((l) => l.bundleId === b.id)
          .map((l) => {
            const info = infos.get(l.componentItemId.toString());
            return {
              itemId: l.componentItemId.toString(),
              name: info?.name ?? null,
              code: info?.code ?? null,
              quantity: l.quantity.toString(),
              unit: info?.unit ?? null,
            };
          }),
      });
    }
    return map;
  }

  async upsert(dto: UpsertBundleDto, actorId?: string) {
    const item = await this.prisma.erpItem.findFirst({
      where: { id: BigInt(dto.itemId), deletedAt: null },
    });
    if (!item) throw new BadRequestException('Item paket tidak ditemukan.');
    if (!dto.lines?.length) throw new BadRequestException('Paket harus memiliki minimal satu komponen.');
    const seen = new Set<string>();
    for (const line of dto.lines) {
      if (line.componentItemId === dto.itemId) {
        throw new BadRequestException('Komponen paket tidak boleh item paket itu sendiri.');
      }
      if (seen.has(line.componentItemId)) {
        throw new BadRequestException('Komponen paket duplikat.');
      }
      seen.add(line.componentItemId);
      if (new Prisma.Decimal(line.quantity).lte(0)) {
        throw new BadRequestException('Qty komponen harus lebih dari 0.');
      }
    }
    const components = await this.prisma.erpItem.findMany({
      where: { id: { in: dto.lines.map((l) => BigInt(l.componentItemId)) }, deletedAt: null },
      select: { id: true },
    });
    if (components.length !== dto.lines.length) {
      throw new BadRequestException('Sebagian komponen paket tidak ditemukan.');
    }
    const actor = actorId ? BigInt(actorId) : null;
    const bundle = await this.prisma.$transaction(async (tx) => {
      const existing = await tx.erpItemBundle.findFirst({
        where: { itemId: item.id, deletedAt: null },
      });
      const saved = existing
        ? await tx.erpItemBundle.update({
            where: { id: existing.id },
            data: {
              name: dto.name,
              notes: dto.notes,
              isActive: dto.isActive ?? existing.isActive,
              updatedById: actor,
            },
          })
        : await tx.erpItemBundle.create({
            data: {
              itemId: item.id,
              name: dto.name,
              notes: dto.notes,
              isActive: dto.isActive ?? true,
              createdById: actor,
              updatedById: actor,
            },
          });
      await tx.erpItemBundleLine.updateMany({
        where: { bundleId: saved.id, deletedAt: null },
        data: { deletedAt: new Date() },
      });
      await tx.erpItemBundleLine.createMany({
        data: dto.lines.map((l, idx) => ({
          bundleId: saved.id,
          componentItemId: BigInt(l.componentItemId),
          quantity: new Prisma.Decimal(l.quantity),
          lineNo: idx + 1,
          notes: l.notes,
          createdById: actor,
          updatedById: actor,
        })),
      });
      return saved;
    });
    return this.present(bundle);
  }

  async remove(bundleId: string) {
    const bundle = await this.prisma.erpItemBundle.findFirst({
      where: { id: BigInt(bundleId), deletedAt: null },
    });
    if (!bundle) throw new NotFoundException('Paket tidak ditemukan.');
    await this.prisma.$transaction([
      this.prisma.erpItemBundle.update({
        where: { id: bundle.id },
        data: { deletedAt: new Date() },
      }),
      this.prisma.erpItemBundleLine.updateMany({
        where: { bundleId: bundle.id, deletedAt: null },
        data: { deletedAt: new Date() },
      }),
    ]);
    return { success: true };
  }
}

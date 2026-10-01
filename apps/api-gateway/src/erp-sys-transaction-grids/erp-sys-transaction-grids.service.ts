import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { SaveGridsDto } from './dto/save-grid-columns.dto';

@Injectable()
export class ErpSysTransactionGridsService {
  constructor(private readonly prisma: PrismaService) {}

  /** Flat list of active transaction types (FE builds the module→transaction tree). */
  listTypes() {
    return this.prisma.erpTransactionType.findMany({
      where: { isActive: true, deletedAt: null },
      orderBy: { sortOrder: 'asc' },
    });
  }

  private async typeByCode(code: string) {
    const type = await this.prisma.erpTransactionType.findFirst({
      where: { code, deletedAt: null },
    });
    if (!type) throw new NotFoundException(`Jenis transaksi "${code}" tidak ditemukan`);
    return type;
  }

  private gridsOfType(typeId: bigint) {
    return this.prisma.erpTransactionGrid.findMany({
      where: { transactionTypeId: typeId, deletedAt: null },
      orderBy: { sortOrder: 'asc' },
      include: { columns: { where: { deletedAt: null }, orderBy: { sortOrder: 'asc' } } },
    });
  }

  /** All grids (tabs) of a transaction type + their columns. Lazily creates a
   *  primary "main" grid when a type has none yet (so the editor always opens). */
  async getGrids(code: string) {
    const type = await this.typeByCode(code);
    let grids = await this.gridsOfType(type.id);
    if (grids.length === 0) {
      await this.prisma.erpTransactionGrid.create({
        data: {
          transactionTypeId: type.id, key: 'main', label: 'Utama', sortOrder: 0,
          lineTable: type.lineTable, isPrimary: true,
        },
      });
      grids = await this.gridsOfType(type.id);
    }
    return { type, grids };
  }

  /** Compat: columns of the primary (or first) grid — read by the live entry grid. */
  async getColumns(code: string) {
    const { type, grids } = await this.getGrids(code);
    const primary = grids.find((g) => g.isPrimary) ?? grids[0];
    return { type, columns: primary?.columns ?? [] };
  }

  /** Replace the full grid/tab + column set for a transaction type (bulk save). */
  async saveGrids(code: string, dto: SaveGridsDto, actorId?: string) {
    const type = await this.typeByCode(code);
    const createdById = actorId ? BigInt(actorId) : null;
    const grids = dto.grids;
    const hasPrimary = grids.some((g) => g.isPrimary);

    await this.prisma.$transaction(async (tx) => {
      // Cascade delete removes columns when grids are dropped.
      await tx.erpTransactionGrid.deleteMany({ where: { transactionTypeId: type.id } });
      for (const [gi, g] of grids.entries()) {
        await tx.erpTransactionGrid.create({
          data: {
            transactionTypeId: type.id,
            key: g.key,
            label: g.label,
            sortOrder: gi,
            lineTable: g.lineTable ?? type.lineTable,
            isPrimary: g.isPrimary ?? (!hasPrimary && gi === 0),
            isActive: true,
            createdById,
            updatedById: createdById,
            columns: {
              create: g.columns.map((c, ci) => ({
                sortOrder: ci,
                headerText: c.headerText,
                dataField: c.dataField,
                width: c.width,
                isVisible: c.isVisible,
                isRequired: c.isRequired,
                isEditable: c.isEditable,
                isSkippable: c.isSkippable ?? false,
                kind: c.kind,
                dataType: c.dataType,
                lookupSource: c.lookupSource ?? null,
                lookupDefaultFilter: (c.lookupDefaultFilter as Prisma.InputJsonValue) ?? Prisma.DbNull,
                lookupDefaultSort: c.lookupDefaultSort ?? null,
                placeholder: c.placeholder ?? null,
                defaultValue: c.defaultValue ?? null,
                defaultValueLabel: c.defaultValueLabel ?? null,
                columnType: c.columnType ?? null,
                labelFormatter: c.labelFormatter ?? null,
                headerRenderer: c.headerRenderer ?? null,
                cellRenderer: c.cellRenderer ?? null,
                cellEditor: c.cellEditor ?? null,
                createdById,
                updatedById: createdById,
              })),
            },
          },
        });
      }
    });
    return this.getGrids(code);
  }
}

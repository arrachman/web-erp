import { Injectable } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { ErpSlsOrdersService } from '../erp-sls-orders/erp-sls-orders.service';
import { CreateSlsOrderDto } from '../erp-sls-orders/dto/create-sls-order.dto';
import { ImportSiplahDto, ImportSiplahRowDto } from './dto/import-order-hub.dto';

export interface ImportRowResult {
  externalOrderId: string;
  status: 'created' | 'skipped' | 'error';
  docNumber?: string;
  message?: string;
}

/**
 * A2 — SIPLah order import. Rows arrive pre-parsed (CSV/XLSX is read in the
 * browser) and are grouped by externalOrderId into sales orders created via
 * the regular orders service, so doc numbering (SO), period resolution and
 * totals stay identical to manual entry. Idempotent on
 * (channel = SIPLAH, externalOrderId): re-importing the same file skips
 * existing orders instead of duplicating them.
 */
@Injectable()
export class ErpOrderHubImportService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly orders: ErpSlsOrdersService,
  ) {}

  private async resolveSchool(row: ImportSiplahRowDto) {
    if (row.schoolCode) {
      const byCode = await this.prisma.erpPartner.findFirst({
        where: { code: row.schoolCode, deletedAt: null },
        select: { id: true },
      });
      if (byCode) return byCode;
    }
    if (row.schoolNpsn) {
      const profile = await this.prisma.erpSchoolProfile.findFirst({
        where: { npsn: row.schoolNpsn, deletedAt: null },
        select: { partnerId: true },
      });
      if (profile) return { id: profile.partnerId };
    }
    if (row.schoolName) {
      const byName = await this.prisma.erpPartner.findFirst({
        where: { name: { equals: row.schoolName, mode: 'insensitive' }, deletedAt: null },
        select: { id: true },
      });
      if (byName) return byName;
    }
    return null;
  }

  async importSiplah(dto: ImportSiplahDto, actorId?: string) {
    // Group flat rows into orders by external order id (first row = header).
    const groups = new Map<string, ImportSiplahRowDto[]>();
    for (const row of dto.rows) {
      groups.set(row.externalOrderId, [...(groups.get(row.externalOrderId) ?? []), row]);
    }

    const branch = await this.prisma.erpBranch.findFirst({
      where: { deletedAt: null },
      orderBy: { id: 'asc' },
      select: { id: true },
    });
    const currency = await this.prisma.erpCurrency.findFirst({
      where: { code: 'IDR', deletedAt: null },
      select: { id: true },
    });
    if (!branch) throw new Error('Tidak ada cabang aktif untuk impor.');
    if (!currency) throw new Error('Mata uang IDR tidak ditemukan.');

    const results: ImportRowResult[] = [];
    for (const [externalOrderId, rows] of groups) {
      const head = rows[0];
      try {
        const existing = await this.prisma.erpSlsOrder.findFirst({
          where: { channel: 'SIPLAH', externalOrderId, deletedAt: null },
          select: { id: true, docNumber: true },
        });
        if (existing) {
          results.push({ externalOrderId, status: 'skipped', docNumber: existing.docNumber, message: 'Sudah pernah diimpor.' });
          continue;
        }
        const school = await this.resolveSchool(head);
        if (!school) {
          results.push({ externalOrderId, status: 'error', message: 'Sekolah tidak ditemukan (kode/NPSN/nama).' });
          continue;
        }
        const lines = [];
        let lineError: string | null = null;
        for (let i = 0; i < rows.length; i++) {
          const r = rows[i];
          const item = await this.prisma.erpItem.findFirst({
            where: { code: r.itemCode, deletedAt: null },
            select: { id: true, baseUnitId: true, salePrice: true },
          });
          if (!item) {
            lineError = `Item ${r.itemCode} tidak ditemukan.`;
            break;
          }
          const unitId = item.baseUnitId;
          if (!unitId) {
            lineError = `Item ${r.itemCode} belum punya satuan.`;
            break;
          }
          lines.push({
            itemId: item.id.toString(),
            quantity: r.quantity,
            unitId: unitId.toString(),
            unitPrice: r.unitPrice ?? item.salePrice?.toString() ?? '0',
            lineNo: i + 1,
          });
        }
        if (lineError) {
          results.push({ externalOrderId, status: 'error', message: lineError });
          continue;
        }

        const payload = {
          docDate: head.orderDate,
          branchId: branch.id.toString(),
          currencyId: currency.id.toString(),
          exchangeRate: '1',
          customerId: school.id.toString(),
          channel: 'SIPLAH',
          externalOrderId,
          fundingSource: head.fundingSource ?? 'BOS',
          budgetYear: head.budgetYear ?? Number(head.orderDate.slice(0, 4)),
          budgetStage: head.budgetStage,
          lines,
        } as unknown as CreateSlsOrderDto;

        const created: any = await this.orders.create(payload, actorId);
        const orderId = BigInt(created.data.id);
        if (head.marketplaceFee || head.disbursementRef) {
          await this.prisma.erpSlsOrder.update({
            where: { id: orderId },
            data: {
              marketplaceFee: head.marketplaceFee ?? null,
              disbursementRef: head.disbursementRef ?? null,
            },
          });
        }
        await this.prisma.erpSlsOrderStatusLog.create({
          data: { orderId, hubStatus: 'BARU', source: 'IMPORT', note: `Impor SIPLah ${externalOrderId}` },
        });
        results.push({ externalOrderId, status: 'created', docNumber: created.data.docNumber });
      } catch (err) {
        results.push({
          externalOrderId,
          status: 'error',
          message: err instanceof Error ? err.message : 'Gagal membuat order.',
        });
      }
    }

    return {
      success: true,
      data: {
        totalOrders: groups.size,
        created: results.filter((r) => r.status === 'created').length,
        skipped: results.filter((r) => r.status === 'skipped').length,
        errors: results.filter((r) => r.status === 'error').length,
        results,
      },
    };
  }
}

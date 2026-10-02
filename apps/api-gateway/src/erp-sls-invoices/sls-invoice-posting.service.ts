import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  buildLedgerRows,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';
import { InvStockMovementPostingService } from '../erp-inv-stock-movements/inv-stock-movement-posting.service';
import { applyInvoiceAdvance, releaseInvoiceAdvance } from './sls-invoice-advance.helpers';
import { assertLedgerRowsPeriodOpen } from '../erp-common/utils/ledger-period-guard';
import { reverseWithOffset } from '../erp-inv-gl/ledger-offset.helpers';

const SLS_GL_SOURCE = 'SALES';
const SLS_GL_DOCTYPE = 'sls_invoices';
const MOVEMENT_SOURCE = 'SLS_INVOICE';
const MOVEMENT_DOC_CODE = 'SII';

type InvoiceWithLines = Prisma.ErpSlsInvoiceGetPayload<{
  include: { lines: true };
}>;

/**
 * GL + stock posting for Sales Invoices → fin_ledger_entries /
 * inv_stock_movements.
 *
 * SI is the primary AR event in the sales chain (§2 posting matrix,
 * DECISIONS.md "Aturan posting jurnal dan stok"):
 *   Dr  Piutang Usaha (header receivableAccountId, fallback customer)   grandTotal
 *   Cr  Penjualan per baris (item.salesAccountId, fallback kategori)    lineNet (per line)
 *   Cr  PPN Keluaran per baris (tax.saleAccountId, override header)     line.tax1Amount/tax2Amount
 *   Dr  Diskon Penjualan (header discountAccountId)                     discountAmount (bila ada)
 *   FR-SLS-04: bila advanceId+advanceAmount diisi, Piutang berkurang sebesar potongan dan
 *   Dr Uang Muka Penjualan ditambahkan; AS.appliedAmount naik (di-release saat reverse).
 *
 * Stock (FR-SLS-02, "Aturan anti posting ganda"): "SI yang dibuat dari DO
 * tidak boleh memotong stok lagi. SI tanpa DO memotong stok sendiri." When
 * `invoice.deliveryOrderId` is set, stock was already moved by that DO —
 * no new movement here. When it's null (stand-alone SI), this creates its
 * own ISSUE movement, delegated fully to InvStockMovementPostingService
 * (ISSUE's Dr COGS/Cr Inventory direction is correct for a sale — same
 * reasoning as SlsDeliveryOrderPostingService).
 *
 * Mirrors CashBankPostingService / InvStockMovementPostingService: append-on-post,
 * reverseLedger hard-deletes this document's own rows before re-posting.
 */
@Injectable()
export class SlsInvoicePostingService {
  constructor(private readonly invPosting: InvStockMovementPostingService) {}

  async postToLedger(
    tx: Prisma.TransactionClient,
    invoice: InvoiceWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    if (!invoice.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }

    const receivableAccountId = await this.resolveReceivableAccount(tx, invoice);
    const itemIds = [...new Set(invoice.lines.map((l) => l.itemId))];
    const items = await tx.erpItem.findMany({
      where: { id: { in: itemIds } },
      select: {
        id: true,
        salesAccountId: true,
        category: { select: { salesAccountId: true } },
      },
    });
    const itemById = new Map(items.map((i) => [i.id.toString(), i]));

    const taxIds = [
      ...new Set(
        invoice.lines.flatMap((l) => [l.tax1Id?.toString(), l.tax2Id?.toString()]).filter((v): v is string => !!v),
      ),
    ];
    const taxes = taxIds.length
      ? await tx.erpTax.findMany({
          where: { id: { in: taxIds.map(BigInt) } },
          select: { id: true, saleAccountId: true },
        })
      : [];
    const taxAccountById = new Map(taxes.map((t) => [t.id.toString(), t.saleAccountId]));

    const advanceLeg = await applyInvoiceAdvance(tx, invoice);
    const receivableDebit = new Prisma.Decimal(invoice.grandTotal).sub(advanceLeg?.debit ?? 0);

    const legs: LedgerLeg[] = [
      {
        accountId: receivableAccountId,
        debit: receivableDebit,
        credit: new Prisma.Decimal(0),
        description: invoice.description,
        partnerId: invoice.customerId,
      },
    ];

    if (advanceLeg) legs.push(advanceLeg);

    for (const line of invoice.lines) {
      const item = itemById.get(line.itemId.toString());
      const salesAccountId = item?.salesAccountId ?? item?.category?.salesAccountId;
      if (!salesAccountId) {
        throw new BadRequestException(
          `Item pada baris ${line.lineNo} tidak punya akun penjualan (sales account) — set di master item atau kategori.`,
        );
      }
      const gross = new Prisma.Decimal(line.quantity).mul(new Prisma.Decimal(line.unitValue)).mul(new Prisma.Decimal(line.unitPrice));
      const discount = line.discountAmount
        ? new Prisma.Decimal(line.discountAmount)
        : line.discountPercent
          ? gross.mul(new Prisma.Decimal(line.discountPercent)).div(100)
          : new Prisma.Decimal(0);
      const net = gross.sub(discount);

      legs.push({
        accountId: salesAccountId,
        debit: new Prisma.Decimal(0),
        credit: net,
        description: line.notes,
        costCenterId: line.costCenterId,
        divisionId: line.divisionId,
        subdivisionId: line.subdivisionId,
        projectId: line.projectId,
      });

      if (line.tax1Id && line.tax1Amount) {
        legs.push(this.taxLeg(invoice, line.tax1Id, line.tax1Amount, taxAccountById, invoice.tax1AccountId));
      }
      if (line.tax2Id && line.tax2Amount) {
        legs.push(this.taxLeg(invoice, line.tax2Id, line.tax2Amount, taxAccountById, invoice.tax2AccountId));
      }
    }

    if (invoice.discountAmount && new Prisma.Decimal(invoice.discountAmount).gt(0)) {
      if (!invoice.discountAccountId) {
        throw new BadRequestException('Ada diskon header tapi akun diskon (discountAccountId) belum diset.');
      }
      legs.push({
        accountId: invoice.discountAccountId,
        debit: new Prisma.Decimal(invoice.discountAmount),
        credit: new Prisma.Decimal(0),
        description: 'Diskon penjualan',
      });
    }

    const base: LedgerBase = {
      branchId: invoice.branchId,
      locationId: invoice.locationId,
      sourceDocType: SLS_GL_DOCTYPE,
      sourceId: invoice.id,
      docNumber: invoice.docNumber,
      entryDate: invoice.docDate,
      fiscalPeriodId: invoice.fiscalPeriodId,
      currencyId: invoice.currencyId,
      exchangeRate: invoice.exchangeRate,
      actorId,
    };

    const rows = buildLedgerRows(base, legs).map((r) => ({ ...r, source: SLS_GL_SOURCE }));
    await assertLedgerRowsPeriodOpen(tx, rows);
    const created = await tx.erpFinLedgerEntry.createMany({ data: rows });
    void created;

    const arRow = await tx.erpFinLedgerEntry.findFirst({
      where: { sourceDocType: SLS_GL_DOCTYPE, sourceId: invoice.id, accountId: receivableAccountId },
      orderBy: { id: 'asc' },
      select: { id: true },
    });
    await tx.erpSlsInvoice.update({
      where: { id: invoice.id },
      data: {
        arLedgerEntryId: arRow?.id ?? null,
        ...(advanceLeg && receivableDebit.lte(0) ? { settlementStatus: 'PAID' as const, settledDate: invoice.docDate } : {}),
      },
    });

    if (!invoice.deliveryOrderId) {
      await this.postStockMovement(tx, invoice, actorId);
    }
  }

  private async postStockMovement(
    tx: Prisma.TransactionClient,
    invoice: InvoiceWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    const missingWarehouse = invoice.lines.find((l) => !l.warehouseId && !invoice.warehouseId);
    if (missingWarehouse) {
      throw new BadRequestException(
        `Baris ${missingWarehouse.lineNo} tidak punya gudang (warehouse) — set di header atau baris.`,
      );
    }

    const docNumber = await this.genMovementDocNumber(tx);
    const movement = await tx.erpInvStockMovement.create({
      data: {
        docNumber,
        autoNumber: docNumber,
        movementType: 'ISSUE',
        branchId: invoice.branchId,
        locationId: invoice.locationId,
        sourceWarehouseId: invoice.warehouseId,
        source: MOVEMENT_SOURCE,
        movementDate: invoice.docDate,
        fiscalPeriodId: invoice.fiscalPeriodId,
        requestedPartnerId: invoice.customerId,
        referenceNo: invoice.docNumber,
        referenceDate: invoice.docDate,
        description: `Barang keluar SI ${invoice.docNumber} (tanpa DO)`,
        status: 'POSTED',
        postingStatus: 'UNPOSTED',
        postedAt: new Date(),
        metadata: { sourceDocType: SLS_GL_DOCTYPE, sourceId: invoice.id.toString() },
        createdById: actorId,
        updatedById: actorId,
        lines: {
          create: invoice.lines.map((l, i) => ({
            itemId: l.itemId,
            quantity: l.quantity,
            unitId: l.unitId,
            unitValue: l.unitValue,
            baseQuantity: l.baseQuantity,
            baseUnitId: l.baseUnitId,
            unitCost: l.unitCost,
            sourceWarehouseId: l.warehouseId ?? invoice.warehouseId,
            costCenterId: l.costCenterId,
            divisionId: l.divisionId,
            subdivisionId: l.subdivisionId,
            projectId: l.projectId,
            notes: l.notes,
            lineNo: i + 1,
          })),
        },
      },
      include: { lines: true },
    });

    await this.invPosting.postMovement(tx, movement, actorId);
    await tx.erpInvStockMovement.update({
      where: { id: movement.id },
      data: { postingStatus: 'POSTED' },
    });
  }

  async reverseLedger(tx: Prisma.TransactionClient, invoiceId: bigint): Promise<void> {
    await releaseInvoiceAdvance(tx, invoiceId);
    await reverseInvLedger(tx, SLS_GL_DOCTYPE, invoiceId);
    await tx.erpSlsInvoice.update({
      where: { id: invoiceId },
      data: { arLedgerEntryId: null },
    });

    const movements = await tx.erpInvStockMovement.findMany({
      where: {
        source: MOVEMENT_SOURCE,
        deletedAt: null,
        metadata: { path: ['sourceId'], equals: invoiceId.toString() },
      },
    });
    for (const movement of movements) {
      await this.invPosting.reverseMovement(tx, movement.id);
      await tx.erpInvStockMovementLine.deleteMany({ where: { stockMovementId: movement.id } });
      await tx.erpInvStockMovement.delete({ where: { id: movement.id } });
    }
  }

  /**
   * VOID dokumen POSTED: jurnal AR/pendapatan & HPP dibalik dengan baris offset bertanggal
   * (bukan hard-delete), movement stok turunan ditandai VOID (keluar dari on-hand), potongan
   * uang muka dikembalikan. Baris ledger asli tetap sebagai jejak audit.
   */
  async voidLedger(
    tx: Prisma.TransactionClient,
    invoiceId: bigint,
    opts: { entryDate: Date; fiscalPeriodId: bigint; actorId: bigint | null },
  ): Promise<void> {
    await releaseInvoiceAdvance(tx, invoiceId);
    await reverseWithOffset(tx, SLS_GL_DOCTYPE, invoiceId, opts);
    const movements = await tx.erpInvStockMovement.findMany({
      where: {
        source: MOVEMENT_SOURCE,
        deletedAt: null,
        status: 'POSTED',
        metadata: { path: ['sourceId'], equals: invoiceId.toString() },
      },
      select: { id: true },
    });
    for (const movement of movements) {
      await reverseWithOffset(tx, 'inv_stock_movements', movement.id, { ...opts, allowEmpty: true });
      await tx.erpInvStockMovement.update({
        where: { id: movement.id },
        data: { status: 'VOID', updatedById: opts.actorId },
      });
    }
  }

  private async genMovementDocNumber(tx: Prisma.TransactionClient): Promise<string> {
    const numbering = await tx.erpDocumentNumbering.findFirst({
      where: { documentCode: MOVEMENT_DOC_CODE, deletedAt: null },
    });
    if (numbering) {
      // Atomic increment: row lock serializes concurrent saves (no duplicate / skipped numbers).
      const bumped = await tx.erpDocumentNumbering.update({
        where: { id: numbering.id },
        data: { nextNumber: { increment: 1 } },
        select: { nextNumber: true },
      });
      const seq = bumped.nextNumber - 1;
      return `${numbering.prefix}${String(seq).padStart(numbering.digitCount, '0')}`;
    }
    const count = await tx.erpInvStockMovement.count();
    return `${MOVEMENT_DOC_CODE}${String(count + 1).padStart(6, '0')}`;
  }

  private async resolveReceivableAccount(
    tx: Prisma.TransactionClient,
    invoice: InvoiceWithLines,
  ): Promise<bigint> {
    if (invoice.receivableAccountId) return invoice.receivableAccountId;
    if (invoice.customerId) {
      const customer = await tx.erpPartner.findUnique({
        where: { id: invoice.customerId },
        select: { receivableAccountId: true },
      });
      if (customer?.receivableAccountId) return customer.receivableAccountId;
    }
    throw new BadRequestException(
      'Tidak bisa posting: akun piutang (receivable account) tidak ditemukan di dokumen maupun master pelanggan.',
    );
  }

  private taxLeg(
    invoice: InvoiceWithLines,
    taxId: bigint,
    amount: Prisma.Decimal,
    taxAccountById: Map<string, bigint | null>,
    headerOverrideAccountId: bigint | null,
  ): LedgerLeg {
    const accountId = headerOverrideAccountId ?? taxAccountById.get(taxId.toString());
    if (!accountId) {
      throw new BadRequestException('Pajak pada baris tidak punya akun PPN Keluaran — set di master pajak.');
    }
    return {
      accountId,
      debit: new Prisma.Decimal(0),
      credit: new Prisma.Decimal(amount),
      description: 'PPN Keluaran',
    };
  }
}

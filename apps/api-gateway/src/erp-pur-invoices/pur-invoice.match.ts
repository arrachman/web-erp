import { NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';

const QTY_TOL = 0.0001;
const PRICE_TOL_RATIO = 0.01; // 1% price tolerance vs PO price

const n = (v: Prisma.Decimal | number | null | undefined): number =>
  v == null ? 0 : Number(v);

export interface PurInvoiceMatchLine {
  lineId: number;
  itemId: number;
  itemLabel: string;
  orderLineId: number | null;
  orderedQty: number | null;
  receivedQty: number | null;
  invoicedQty: number;
  orderPrice: number | null;
  invoicePrice: number;
  qtyMatched: boolean | null;
  priceMatched: boolean | null;
  issues: string[];
}

export interface PurInvoiceMatchResult {
  invoiceId: number;
  docNumber: string;
  orderId: number | null;
  persistedStatus: string;
  computedStatus: 'PENDING' | 'MATCHED' | 'MISMATCH';
  lines: PurInvoiceMatchLine[];
}

/**
 * D2 — three-way match for one purchase invoice: PO line (ordered qty &
 * price) vs goods receipts (accepted qty) vs this invoice (invoiced qty &
 * price). Pure computation; persisting is the caller's choice.
 */
export async function computePurInvoiceMatch(
  prisma: PrismaService,
  invoiceId: bigint,
): Promise<PurInvoiceMatchResult> {
  const invoice = await prisma.erpPurInvoice.findFirst({
    where: { id: invoiceId, deletedAt: null },
    select: {
      id: true, docNumber: true, orderId: true, matchStatus: true,
      lines: {
        orderBy: { lineNo: 'asc' },
        select: {
          id: true, itemId: true, orderLineId: true, goodsReceiptLineId: true,
          baseQuantity: true, unitPrice: true,
        },
      },
    },
  });
  if (!invoice) throw new NotFoundException('Purchase invoice tidak ditemukan');
  const itemIds = [...new Set(invoice.lines.map((l) => l.itemId))];
  const items = itemIds.length
    ? await prisma.erpItem.findMany({
        where: { id: { in: itemIds } }, select: { id: true, code: true, name: true },
      })
    : [];
  const itemLabel = new Map(items.map((i) => [i.id.toString(), `${i.code} — ${i.name}`]));

  // Resolve PO lines (directly, or via the linked GRN line).
  const orderLineIds = new Set<bigint>();
  const grnLineIds = invoice.lines
    .map((l) => l.goodsReceiptLineId)
    .filter((v): v is bigint => v != null);
  const grnLines = grnLineIds.length
    ? await prisma.erpPurGoodsReceiptLine.findMany({
        where: { id: { in: grnLineIds } },
        select: { id: true, orderLineId: true },
      })
    : [];
  const orderLineOfGrn = new Map(grnLines.map((g) => [g.id.toString(), g.orderLineId]));
  for (const l of invoice.lines) {
    if (l.orderLineId) orderLineIds.add(l.orderLineId);
    else if (l.goodsReceiptLineId) {
      const ol = orderLineOfGrn.get(l.goodsReceiptLineId.toString());
      if (ol) orderLineIds.add(ol);
    }
  }
  const poLines = orderLineIds.size
    ? await prisma.erpPurOrderLine.findMany({
        where: { id: { in: [...orderLineIds] } },
        select: { id: true, baseQuantity: true, unitPrice: true },
      })
    : [];
  const poById = new Map(poLines.map((p) => [p.id.toString(), p]));

  // Received (accepted) base qty per PO line across all goods receipts.
  const receivedByPoLine = new Map<string, number>();
  if (orderLineIds.size) {
    const receipts = await prisma.erpPurGoodsReceiptLine.findMany({
      where: {
        orderLineId: { in: [...orderLineIds] },
        goodsReceipt: { deletedAt: null },
      },
      select: { orderLineId: true, quantity: true, acceptedQty: true, baseQuantity: true },
    });
    for (const r of receipts) {
      if (!r.orderLineId) continue;
      const qty = n(r.quantity);
      const acceptedBase = qty > 0 ? n(r.baseQuantity) * (n(r.acceptedQty) / qty) : n(r.baseQuantity);
      const key = r.orderLineId.toString();
      receivedByPoLine.set(key, (receivedByPoLine.get(key) ?? 0) + acceptedBase);
    }
  }

  let anyLinked = false;
  let anyMismatch = false;
  const lines: PurInvoiceMatchLine[] = invoice.lines.map((l) => {
    const orderLineId =
      l.orderLineId ??
      (l.goodsReceiptLineId ? orderLineOfGrn.get(l.goodsReceiptLineId.toString()) ?? null : null);
    const invoicedQty = n(l.baseQuantity);
    const invoicePrice = n(l.unitPrice);
    const issues: string[] = [];
    let orderedQty: number | null = null;
    let receivedQty: number | null = null;
    let orderPrice: number | null = null;
    let qtyMatched: boolean | null = null;
    let priceMatched: boolean | null = null;

    if (orderLineId) {
      anyLinked = true;
      const po = poById.get(orderLineId.toString());
      if (po) {
        orderedQty = n(po.baseQuantity);
        orderPrice = n(po.unitPrice);
        receivedQty = receivedByPoLine.get(orderLineId.toString()) ?? 0;
        qtyMatched = invoicedQty <= receivedQty + QTY_TOL && invoicedQty <= orderedQty + QTY_TOL;
        if (invoicedQty > receivedQty + QTY_TOL) {
          issues.push(`Qty ditagih ${invoicedQty} melebihi qty diterima ${receivedQty}`);
        }
        if (invoicedQty > orderedQty + QTY_TOL) {
          issues.push(`Qty ditagih ${invoicedQty} melebihi qty PO ${orderedQty}`);
        }
        const tol = Math.max(1, Math.abs(orderPrice) * PRICE_TOL_RATIO);
        priceMatched = Math.abs(invoicePrice - orderPrice) <= tol;
        if (!priceMatched) {
          issues.push(`Harga ditagih ${invoicePrice} berbeda dari harga PO ${orderPrice}`);
        }
        if (!qtyMatched || !priceMatched) anyMismatch = true;
      }
    } else {
      issues.push('Baris tidak tertaut ke PO (orderLineId kosong)');
    }

    return {
      lineId: Number(l.id), itemId: Number(l.itemId),
      itemLabel: itemLabel.get(l.itemId.toString()) ?? String(l.itemId),
      orderLineId: orderLineId ? Number(orderLineId) : null,
      orderedQty, receivedQty, invoicedQty, orderPrice, invoicePrice,
      qtyMatched, priceMatched, issues,
    };
  });

  return {
    invoiceId: Number(invoice.id),
    docNumber: invoice.docNumber,
    orderId: invoice.orderId ? Number(invoice.orderId) : null,
    persistedStatus: invoice.matchStatus,
    computedStatus: !anyLinked ? 'PENDING' : anyMismatch ? 'MISMATCH' : 'MATCHED',
    lines,
  };
}

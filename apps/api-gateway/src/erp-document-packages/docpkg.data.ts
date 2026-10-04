import { PrismaService } from '../prisma/prisma.service';
import { terbilangRupiah } from './docpkg.terbilang';

export type DocKind =
  | 'PENAWARAN'
  | 'SURAT_PESANAN'
  | 'INVOICE'
  | 'KUITANSI'
  | 'SURAT_JALAN'
  | 'BAST';

export type DocVariant = 'BOS' | 'NON_BOS';

export const DOC_KIND_TITLES: Record<DocKind, string> = {
  PENAWARAN: 'SURAT PENAWARAN',
  SURAT_PESANAN: 'SURAT PESANAN',
  INVOICE: 'INVOICE',
  KUITANSI: 'KUITANSI',
  SURAT_JALAN: 'SURAT JALAN',
  BAST: 'BERITA ACARA SERAH TERIMA',
};

export interface DocLine {
  itemCode: string;
  itemName: string;
  qty: string;
  unitName: string;
  unitPrice: string;
  amount: string;
}

export interface DocData {
  kind: DocKind;
  title: string;
  variant: DocVariant;
  sourceDocType: string;
  sourceId: string;
  sourceDocNumber: string;
  docDate: Date;
  company: { name: string; address: string; phone: string; email: string; npwp: string };
  party: { name: string; address: string; npwp: string; npsn: string };
  orderNumber: string;
  fundingNote: string | null;
  lines: DocLine[];
  subtotal: string;
  discount: string;
  tax: string;
  total: string;
  terbilang: string;
  acceptance: {
    acceptedAt: Date | null;
    byName: string | null;
    byTitle: string | null;
    notes: string | null;
    photoCount: number;
  } | null;
  receiptNote: string | null;
}

interface RawLine {
  itemId: bigint;
  unitId: bigint;
  quantity: { toString(): string };
  unitPrice: { toString(): string };
  discountAmount?: { toString(): string } | null;
}

async function toDocLines(
  prisma: PrismaService,
  lines: RawLine[],
): Promise<DocLine[]> {
  const itemIds = [...new Set(lines.map((l) => l.itemId))];
  const unitIds = [...new Set(lines.map((l) => l.unitId))];
  const items = itemIds.length
    ? await prisma.erpItem.findMany({ where: { id: { in: itemIds } }, select: { id: true, code: true, name: true } })
    : [];
  const units = unitIds.length
    ? await prisma.erpUnit.findMany({ where: { id: { in: unitIds } }, select: { id: true, code: true, name: true } })
    : [];
  const itemById = new Map(items.map((i) => [i.id.toString(), i]));
  const unitById = new Map(units.map((u) => [u.id.toString(), u]));
  return lines.map((l) => {
    const qty = Number(l.quantity.toString());
    const price = Number(l.unitPrice.toString());
    const disc = Number(l.discountAmount?.toString() ?? 0);
    const item = itemById.get(l.itemId.toString());
    const unit = unitById.get(l.unitId.toString());
    return {
      itemCode: item?.code ?? '',
      itemName: item?.name ?? '',
      qty: l.quantity.toString(),
      unitName: unit?.name ?? unit?.code ?? '',
      unitPrice: l.unitPrice.toString(),
      amount: String(qty * price - disc),
    };
  });
}

export async function loadCompany(prisma: PrismaService) {
  const rows = await prisma.erpSetting.findMany({ where: { group: 'company' } });
  const map = new Map(rows.map((r) => [r.key, r.value ?? '']));
  const address = [map.get('address_line1'), map.get('address_line2'), map.get('city'), map.get('postal_code')]
    .filter(Boolean)
    .join(', ');
  return {
    name: map.get('name') || 'CV Bahtera Madani',
    address,
    phone: map.get('phone') || '',
    email: map.get('email') || '',
    npwp: map.get('npwp') || '',
  };
}

export interface OrderChain {
  order: any;
  customer: any;
  npsn: string | null;
  partyAddress: string;
  quotation: any | null;
  deliveryOrder: any | null;
  deliveryReport: any | null;
  invoice: any | null;
  receipt: any | null;
}

export async function loadOrderChain(prisma: PrismaService, orderId: bigint): Promise<OrderChain | null> {
  const order = await prisma.erpSlsOrder.findFirst({
    where: { id: orderId, deletedAt: null },
    include: { lines: { orderBy: { lineNo: 'asc' } } },
  });
  if (!order) return null;

  const customer = order.customerId
    ? await prisma.erpPartner.findUnique({ where: { id: order.customerId } })
    : null;
  const profile = order.customerId
    ? await prisma.erpSchoolProfile.findFirst({ where: { partnerId: order.customerId, deletedAt: null } })
    : null;
  const address = order.customerId
    ? await prisma.erpPartnerAddress.findFirst({
        where: { partnerId: order.customerId, deletedAt: null },
        orderBy: [{ isDefault: 'desc' }, { id: 'asc' }],
      })
    : null;
  const city = address?.cityId
    ? await prisma.erpCity.findUnique({ where: { id: address.cityId }, select: { name: true } })
    : null;
  const partyAddress = address
    ? [address.addressLine1, address.addressLine2, city?.name].filter(Boolean).join(', ')
    : '';

  const quotation = order.quotationId
    ? await prisma.erpSlsQuotation.findFirst({
        where: { id: order.quotationId, deletedAt: null },
        include: { lines: { orderBy: { lineNo: 'asc' } } },
      })
    : null;

  const dos = await prisma.erpSlsDeliveryOrder.findMany({
    where: { orderId, deletedAt: null },
    orderBy: { docDate: 'asc' },
    include: { lines: { orderBy: { lineNo: 'asc' } } },
  });
  const deliveryOrder = dos.find((d) => d.postingStatus === 'POSTED') ?? dos[0] ?? null;

  const drs = dos.length
    ? await prisma.erpSlsDeliveryReport.findMany({
        where: { deliveryOrderId: { in: dos.map((d) => d.id) }, deletedAt: null },
        orderBy: { docDate: 'asc' },
        include: { lines: { orderBy: { lineNo: 'asc' } } },
      })
    : [];
  const deliveryReport = drs.find((d) => d.acceptedAt) ?? drs[0] ?? null;

  const invoices = await prisma.erpSlsInvoice.findMany({
    where: { orderId, deletedAt: null },
    orderBy: { docDate: 'asc' },
    include: { lines: { orderBy: { lineNo: 'asc' } } },
  });
  const invoice = invoices.find((i) => i.postingStatus === 'POSTED') ?? invoices[0] ?? null;

  let receipt: any = null;
  if (invoice) {
    const alloc = await prisma.erpFinSettlementAllocation.findFirst({
      where: { invoiceRef: invoice.id.toString(), arReceiptId: { not: null } },
    });
    if (alloc?.arReceiptId) {
      receipt = await prisma.erpFinArReceipt.findFirst({ where: { id: alloc.arReceiptId } });
    }
  }

  return {
    order,
    customer,
    npsn: profile?.npsn ?? null,
    partyAddress,
    quotation,
    deliveryOrder,
    deliveryReport,
    invoice,
    receipt,
  };
}

export function availableDocKinds(chain: OrderChain): DocKind[] {
  const kinds: DocKind[] = [];
  if (chain.quotation) kinds.push('PENAWARAN');
  kinds.push('SURAT_PESANAN');
  if (chain.deliveryOrder) kinds.push('SURAT_JALAN');
  if (chain.deliveryReport) kinds.push('BAST');
  if (chain.invoice) kinds.push('INVOICE');
  if (chain.receipt || chain.invoice?.settlementStatus === 'PAID') kinds.push('KUITANSI');
  return kinds;
}

function fundingNote(chain: OrderChain, variant: DocVariant): string | null {
  if (variant !== 'BOS') return null;
  const o = chain.order;
  const parts = ['Sumber dana: Dana BOS'];
  if (o.budgetYear) parts.push(`Tahun Anggaran ${o.budgetYear}`);
  if (o.budgetStage) parts.push(`Tahap ${o.budgetStage}`);
  return parts.join(' · ');
}

export async function buildDocData(
  prisma: PrismaService,
  chain: OrderChain,
  kind: DocKind,
  variant: DocVariant,
): Promise<DocData> {
  const company = await loadCompany(prisma);
  const o = chain.order;
  const party = {
    name: chain.customer?.name ?? '-',
    address: chain.partyAddress,
    npwp: chain.customer?.taxNumber ?? '',
    npsn: chain.npsn ?? '',
  };
  const base = {
    kind,
    title: DOC_KIND_TITLES[kind],
    variant,
    company,
    party,
    orderNumber: o.docNumber,
    fundingNote: fundingNote(chain, variant),
    acceptance: null as DocData['acceptance'],
    receiptNote: null as string | null,
  };

  const totals = (doc: any) => ({
    subtotal: doc.subtotal.toString(),
    discount: doc.discountAmount?.toString() ?? '0',
    tax: String(Number(doc.tax1Amount?.toString() ?? 0) + Number(doc.tax2Amount?.toString() ?? 0)),
    total: doc.grandTotal.toString(),
    terbilang: terbilangRupiah(doc.grandTotal.toString()),
  });

  switch (kind) {
    case 'PENAWARAN': {
      const q = chain.quotation;
      return {
        ...base,
        sourceDocType: 'sls_quotations',
        sourceId: q.id.toString(),
        sourceDocNumber: q.docNumber,
        docDate: q.docDate,
        lines: await toDocLines(prisma, q.lines),
        ...totals(q),
      };
    }
    case 'SURAT_PESANAN': {
      return {
        ...base,
        sourceDocType: 'sls_orders',
        sourceId: o.id.toString(),
        sourceDocNumber: o.docNumber,
        docDate: o.docDate,
        lines: await toDocLines(prisma, o.lines),
        ...totals(o),
      };
    }
    case 'SURAT_JALAN': {
      const d = chain.deliveryOrder;
      return {
        ...base,
        sourceDocType: 'sls_delivery_orders',
        sourceId: d.id.toString(),
        sourceDocNumber: d.docNumber,
        docDate: d.docDate,
        lines: await toDocLines(prisma, d.lines),
        ...totals(d),
      };
    }
    case 'BAST': {
      const r = chain.deliveryReport;
      const photoCount = await prisma.erpSlsTransactionAttachment.count({
        where: { docType: 'DR', docId: r.id },
      });
      const linesSource = r.lines?.length ? r.lines : (chain.deliveryOrder?.lines ?? o.lines);
      return {
        ...base,
        sourceDocType: 'sls_delivery_reports',
        sourceId: r.id.toString(),
        sourceDocNumber: r.docNumber,
        docDate: r.acceptedAt ?? r.docDate,
        lines: await toDocLines(prisma, linesSource),
        ...totals(r),
        acceptance: {
          acceptedAt: r.acceptedAt ?? null,
          byName: r.acceptedByName ?? null,
          byTitle: r.acceptedByTitle ?? null,
          notes: r.acceptanceNotes ?? null,
          photoCount,
        },
      };
    }
    case 'INVOICE': {
      const inv = chain.invoice;
      return {
        ...base,
        sourceDocType: 'sls_invoices',
        sourceId: inv.id.toString(),
        sourceDocNumber: inv.docNumber,
        docDate: inv.docDate,
        lines: await toDocLines(prisma, inv.lines),
        ...totals(inv),
      };
    }
    case 'KUITANSI': {
      const r = chain.receipt;
      const inv = chain.invoice;
      if (r) {
        return {
          ...base,
          sourceDocType: 'fin_ar_receipts',
          sourceId: r.id.toString(),
          sourceDocNumber: r.docNumber,
          docDate: r.transactionDate,
          lines: [],
          subtotal: r.amount.toString(),
          discount: '0',
          tax: '0',
          total: r.amount.toString(),
          terbilang: terbilangRupiah(r.amount.toString()),
          receiptNote: `Pembayaran atas Invoice ${inv?.docNumber ?? ''} — ${r.description ?? ''}`,
        };
      }
      return {
        ...base,
        sourceDocType: 'sls_invoices',
        sourceId: inv.id.toString(),
        sourceDocNumber: inv.docNumber,
        docDate: inv.docDate,
        lines: [],
        subtotal: inv.grandTotal.toString(),
        discount: '0',
        tax: '0',
        total: inv.grandTotal.toString(),
        terbilang: terbilangRupiah(inv.grandTotal.toString()),
        receiptNote: `Pembayaran lunas atas Invoice ${inv.docNumber}`,
      };
    }
  }
}

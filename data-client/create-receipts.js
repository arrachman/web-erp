// Container api-gateway: buat AR receipt DRAFT untuk pembayaran yang datanya konsisten. --apply = commit.
const { PrismaClient, Prisma } = require('/app/node_modules/@prisma/client');
const APPLY = process.argv.includes('--apply');
const p = new PrismaClient();
class Rollback extends Error {}
const PAYMENTS = [{ inv: '09/0004', school: 'TK AR ROHMAN 9', date: '2026-09-04', amount: '100000.0000', doc: 'PAY-09/0004',
  note: 'Sumber: LAPORAN PEMBAYARAN/IVAN (bayar 100000, saldo 0) + TAGIHAN MARKETING 4-Sep. Metode & akun kas diasumsikan.' }];
(async () => {
  const [admin] = await p.$queryRawUnsafe(`select id from adm_users where email='admin@senti-erp.local' limit 1`);
  try {
    await p.$transaction(async (tx) => {
      for (const x of PAYMENTS) {
        if (await tx.erpFinArReceipt.findFirst({ where: { docNumber: x.doc, deletedAt: null }, select: { id: true } })) { console.log(x.doc, 'SKIP'); continue; }
        const inv = await tx.erpSlsInvoice.findFirst({ where: { docNumber: x.inv, legacyCode: 'data-client' }, select: { id: true, customerId: true, grandTotal: true } });
        if (!inv || !inv.customerId) throw new Error('invoice/customer tidak ditemukan ' + x.inv);
        if (!inv.grandTotal.equals(new Prisma.Decimal(x.amount))) console.log('catatan: pembayaran', x.amount, '!= total invoice', String(inv.grandTotal));
        const d = new Date(x.date);
        const per = await tx.erpFiscalPeriod.findFirst({ where: { deletedAt: null, startDate: { lte: d }, endDate: { gte: d } }, select: { id: true } });
        const row = await tx.erpFinArReceipt.create({ data: {
          docNumber: x.doc, autoNumber: null, branchId: 1n, source: 'AR-RECEIPT', transactionDate: d, fiscalPeriodId: per.id, partnerId: inv.customerId,
          description: `Pembayaran invoice ${x.inv} ${x.school}`, notes: x.note, currencyId: 1n, exchangeRate: new Prisma.Decimal(1),
          amount: new Prisma.Decimal(x.amount), allocatedAmount: new Prisma.Decimal(x.amount), paymentStatus: 'UNPAID', status: 'DRAFT', postingStatus: 'UNPOSTED',
          legacyCode: 'data-client', metadata: { draftAllocations: [{ invoiceId: String(inv.id), amount: x.amount, lineNo: 1 }] },
          createdById: admin ? admin.id : null, updatedById: admin ? admin.id : null,
          instruments: { create: [{ method: 'CASH', bankAccountId: 183n, currencyId: 1n, exchangeRate: new Prisma.Decimal(1), amount: new Prisma.Decimal(x.amount), lineNo: 1 }] },
        }, select: { id: true, docNumber: true } });
        console.log(row.docNumber, 'id', String(row.id), 'invoice', x.inv, x.amount);
      }
      if (!APPLY) throw new Rollback('dry');
    });
  } catch (e) { if (!(e instanceof Rollback)) { console.log('ERROR', String(e.message).split('\n').slice(-6).join('\n')); process.exitCode = 1; } }
  console.log(APPLY ? 'COMMITTED' : 'DRY-RUN (rolled back)');
  await p.$disconnect();
})();

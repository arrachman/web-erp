// Dijalankan di container api-gateway: membuat invoice penjualan DRAFT (reuse helper dist). --apply = commit.
const { PrismaClient, Prisma } = require('/app/node_modules/@prisma/client');
const h = require('/app/dist/erp-sls-invoices/sls-invoice.helpers.js');
const m = require('/app/dist/erp-sls-invoices/sls-invoice-persistence.mapper.js');
const APPLY = process.argv.includes('--apply');
const p = new PrismaClient();
class Rollback extends Error {}
(async () => {
  const items = require('/tmp/invs.json');
  const [admin] = await p.$queryRawUnsafe(`select id from adm_users where email='admin@senti-erp.local' limit 1`);
  const actor = admin ? admin.id : null;
  const created = [];
  try {
    await p.$transaction(async (tx) => {
      for (const { dto, meta, school, hasCustomer } of items) {
        if (await tx.erpSlsInvoice.findFirst({ where: { docNumber: dto.docNumber, deletedAt: null }, select: { id: true } })) {
          console.log(dto.docNumber, 'SKIP (sudah ada)'); continue;
        }
        const d = new Date(dto.docDate);
        const per = await tx.erpFiscalPeriod.findFirst({ where: { deletedAt: null, startDate: { lte: d }, endDate: { gte: d } }, select: { id: true } });
        if (!per) throw new Error('periode fiskal tidak ada untuk ' + dto.docDate);
        const t = h.computeInvoiceTotals(dto.lines, dto, new Map(), dto.priceMode);
        const data = m.buildSlsInvoiceCreateData(dto, { docNumber: dto.docNumber, wantAuto: false, fiscalPeriodId: per.id, dueDate: null, actor,
          priceMode: dto.priceMode, header: { currencyId: dto.currencyId, exchangeRate: dto.exchangeRate }, subtotal: t.subtotal, grandTotal: t.grandTotal,
          discountAmount: t.discountAmount, otherCostAmount: t.otherCostAmount, computedLines: t.lines });
        data.settlementStatus = 'UNPAID';
        data.legacyCode = 'data-client';
        data.metadata = { ...meta, sourceSchoolResolved: school, customerMissing: !hasCustomer };
        const row = await tx.erpSlsInvoice.create({ data, select: { id: true, docNumber: true, grandTotal: true } });
        created.push(row);
        console.log(row.docNumber, school, hasCustomer ? '' : '[TANPA CUSTOMER]', 'total', String(row.grandTotal), 'id', String(row.id));
      }
      if (!APPLY) throw new Rollback('dry-run');
    }, { timeout: 60000 });
  } catch (e) { if (!(e instanceof Rollback)) { console.log('ERROR', String(e.message).split('\n').slice(-6).join('\n')); process.exitCode = 1; } }
  console.log(APPLY ? 'COMMITTED' : 'DRY-RUN (rolled back)', created.length, 'invoice');
  await p.$disconnect();
})();

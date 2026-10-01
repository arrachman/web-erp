/* eslint-disable no-console */
/**
 * Senti MDP — bulk dummy data seed (≥50 rows / feature).
 *
 * Idempotent: every row is tagged `metadata.seed = 'DUMMY_BULK'`. On each run we
 * delete prior DUMMY_BULK rows (reverse-dependency order) then re-insert, so real
 * data (existing production orders, foundation masters, the 31 real menus) is
 * untouched. mdp_menus / mdp_role_menus are intentionally NOT seeded — they are
 * navigation SSOT config and (until role mappings are seeded) the sidebar renders
 * the full tree, so dummy menus would pollute every user's nav.
 *
 * Run inside the api-gateway container (prisma client + DATABASE_URL live there):
 *   docker cp prisma/seed-mdp-bulk-dummy.ts sentient-infra-api-gateway:/app/prisma/
 *   docker exec sentient-infra-api-gateway sh -lc 'cd /app && npx ts-node prisma/seed-mdp-bulk-dummy.ts'
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

const N = 55; // safely above the 50-row minimum
const MARKER = 'DUMMY_BULK';
const meta = { seed: MARKER };
const seedFilter = { metadata: { path: ['seed'], equals: MARKER } } as const;

// Models without a `metadata` column carry the marker in `notes` instead.
const NOTES_NMARK = `[${MARKER}]`;
const NOTES_MODELS = new Set([
  'mdpMntSparePart', 'mdpDmsRevision', 'mdpDmsAcknowledgement', 'mdpLmsEnrollment',
  'mdpPrtEscalation', 'mdpQmsInspectionCharacteristic', 'mdpQmsInspectionResult', 'mdpWmsPick',
]);
const note = (i: number, label: string): string => `${NOTES_NMARK} ${label} ${pad(i)}`;
const filterFor = (model: string): Record<string, unknown> =>
  NOTES_MODELS.has(model) ? { notes: { startsWith: NOTES_NMARK } } : seedFilter;

// ---- reference pools (real cross-app ids; fall back to a small range) --------
let ITEM_IDS: bigint[] = [];
let USER_IDS: bigint[] = [];
let WAREHOUSE_IDS: bigint[] = [];

// ---- helpers -----------------------------------------------------------------
const pick = <T>(a: T[], i: number): T => a[i % a.length];
const rnd = (i: number, mod: number, base = 0): number => base + ((i * 2654435761) % mod);
const dec = (i: number, lo: number, hi: number): string =>
  (lo + ((i * 37) % (hi - lo + 1)) + (i % 4) * 0.25).toFixed(4);
const daysAgo = (d: number): Date => new Date(Date.now() - d * 86_400_000);
const hoursAgo = (h: number): Date => new Date(Date.now() - h * 3_600_000);
const pad = (i: number): string => String(i + 1).padStart(3, '0');
const item = (i: number): bigint => (ITEM_IDS.length ? pick(ITEM_IDS, i) : BigInt(101 + (i % 50)));
const user = (i: number): bigint => (USER_IDS.length ? pick(USER_IDS, i) : BigInt(1 + (i % 3)));
const whs = (i: number): bigint => (WAREHOUSE_IDS.length ? pick(WAREHOUSE_IDS, i) : BigInt(1 + i));

async function ids(model: string): Promise<bigint[]> {
  const rows = await (prisma as any)[model].findMany({ where: filterFor(model), select: { id: true } });
  return rows.map((r: { id: bigint }) => r.id);
}
async function bulk(model: string, rows: Record<string, unknown>[]): Promise<void> {
  await (prisma as any)[model].createMany({ data: rows, skipDuplicates: true });
  console.log(`  + ${model}: ${rows.length}`);
}

// ---- cleanup (reverse dependency order) --------------------------------------
async function wipe(): Promise<void> {
  const order = [
    // MES leaves first
    'mdpLaborLog', 'mdpDowntimeEvent', 'mdpMaterialConsumption', 'mdpProductionLog',
    'mdpOperation', 'mdpProductionOrder',
    // QMS
    'mdpQmsCapaAction', 'mdpQmsNonconformance', 'mdpQmsInspectionResult',
    'mdpQmsInspection', 'mdpQmsInspectionCharacteristic', 'mdpQmsInspectionPlan',
    // CMMS
    'mdpMntSparePart', 'mdpMntWorkOrder', 'mdpMntPmSchedule', 'mdpMntFailureCode',
    // WMS
    'mdpWmsMovement', 'mdpWmsPick', 'mdpWmsTask', 'mdpWmsHandlingUnit',
    // PRTS
    'mdpPrtEscalation', 'mdpPrtIssue',
    // DMS
    'mdpDmsAcknowledgement', 'mdpDmsRevision', 'mdpDmsDocument',
    // IMS
    'mdpEhsPermit', 'mdpEhsAudit', 'mdpEhsIncident',
    // LMS
    'mdpLmsCompetency', 'mdpLmsEnrollment', 'mdpLmsCourse',
    // masters last
    'mdpWorkCenter', 'mdpAsset', 'mdpReasonCode', 'mdpShift', 'mdpWorkCalendar',
  ];
  console.log('Wiping prior DUMMY_BULK rows...');
  for (const m of order) {
    const r = await (prisma as any)[m].deleteMany({ where: filterFor(m) });
    if (r.count) console.log(`  - ${m}: ${r.count}`);
  }
}

// ---- seeders -----------------------------------------------------------------
async function masters(): Promise<void> {
  console.log('Masters (eam/mdp)...');
  await bulk('mdpWorkCalendar', Array.from({ length: N }, (_, i) => ({
    code: `DCAL-${pad(i)}`, name: `Kalender Kerja ${pad(i)}`, isActive: true, metadata: meta,
  })));

  await bulk('mdpShift', Array.from({ length: N }, (_, i) => {
    const sh = pick(['06:00', '14:00', '22:00', '08:00', '20:00'], i);
    const eh = pick(['14:00', '22:00', '06:00', '17:00', '08:00'], i);
    return { code: `DSHF-${pad(i)}`, name: `Shift ${pad(i)}`, startTime: sh, endTime: eh, isActive: true, metadata: meta };
  }));

  await bulk('mdpReasonCode', Array.from({ length: N }, (_, i) => ({
    code: `DRSN-${pad(i)}`, name: `Alasan ${pad(i)}`,
    category: pick(['DOWNTIME', 'SCRAP', 'DELAY', 'QUALITY', 'OTHER'], i),
    isActive: true, metadata: meta,
  })));

  await bulk('mdpAsset', Array.from({ length: N }, (_, i) => ({
    code: `DAST-${pad(i)}`, name: `Aset/Mesin ${pad(i)}`, isActive: true, metadata: meta,
  })));

  const assetIds = await ids('mdpAsset');
  await bulk('mdpWorkCenter', Array.from({ length: N }, (_, i) => ({
    code: `DWC-${pad(i)}`, name: `Work Center ${pad(i)}`,
    assetId: pick(assetIds, i), isActive: true, metadata: meta,
  })));
}

async function mes(): Promise<void> {
  console.log('MES...');
  const wcIds = await ids('mdpWorkCenter');
  const shiftIds = await ids('mdpShift');
  const reasonIds = await ids('mdpReasonCode');
  const assetIds = await ids('mdpAsset');
  const scrapReasons = (await prisma.mdpReasonCode.findMany({
    where: { ...seedFilter, category: 'SCRAP' }, select: { id: true },
  })).map((r) => r.id);

  await bulk('mdpProductionOrder', Array.from({ length: N }, (_, i) => ({
    code: `DPO-${pad(i)}`, itemId: item(i), plannedQty: dec(i, 100, 5000),
    producedGoodQty: dec(i, 0, 100), producedScrapQty: dec(i, 0, 20),
    workCenterId: pick(wcIds, i),
    status: pick(['RELEASED', 'IN_PROGRESS', 'PAUSED', 'COMPLETED', 'CLOSED'], i),
    plannedStartAt: daysAgo(rnd(i, 30, 1)), plannedEndAt: daysAgo(rnd(i, 5)),
    metadata: meta,
  })));
  const poIds = await ids('mdpProductionOrder');

  await bulk('mdpOperation', Array.from({ length: N }, (_, i) => ({
    productionOrderId: pick(poIds, i), sequence: (i % 5) + 1,
    name: `Operasi ${pick(['Cutting', 'Welding', 'Assembly', 'Painting', 'Packing'], i)} ${pad(i)}`,
    workCenterId: pick(wcIds, i),
    status: pick(['PENDING', 'IN_PROGRESS', 'COMPLETED', 'SKIPPED'], i),
    goodQty: dec(i, 0, 500), scrapQty: dec(i, 0, 30), metadata: meta,
  })));
  const opIds = await ids('mdpOperation');

  await bulk('mdpProductionLog', Array.from({ length: N }, (_, i) => ({
    productionOrderId: pick(poIds, i), operationId: pick(opIds, i),
    shiftId: pick(shiftIds, i), startedAt: hoursAgo(rnd(i, 240, 1)),
    endedAt: hoursAgo(rnd(i, 240)), goodQty: dec(i, 0, 400), scrapQty: dec(i, 0, 25),
    scrapReasonId: scrapReasons.length ? pick(scrapReasons, i) : null, metadata: meta,
  })));

  await bulk('mdpMaterialConsumption', Array.from({ length: N }, (_, i) => ({
    productionOrderId: pick(poIds, i), operationId: pick(opIds, i),
    itemId: item(i + 7), qty: dec(i, 1, 200), consumedAt: hoursAgo(rnd(i, 200, 1)),
    sourceBinId: null, postingStatus: pick(['PENDING', 'POSTED'], i), metadata: meta,
  })));

  await bulk('mdpDowntimeEvent', Array.from({ length: N }, (_, i) => {
    const start = hoursAgo(rnd(i, 300, 2));
    return {
      workCenterId: pick(wcIds, i), assetId: pick(assetIds, i),
      productionOrderId: pick(poIds, i), operationId: pick(opIds, i),
      reasonId: pick(reasonIds, i), type: pick(['PLANNED', 'UNPLANNED'], i),
      startedAt: start, endedAt: new Date(start.getTime() + rnd(i, 120, 5) * 60_000),
      metadata: meta,
    };
  }));

  await bulk('mdpLaborLog', Array.from({ length: N }, (_, i) => {
    const start = hoursAgo(rnd(i, 280, 1));
    return {
      operationId: pick(opIds, i), operatorId: user(i), shiftId: pick(shiftIds, i),
      startedAt: start, endedAt: new Date(start.getTime() + rnd(i, 480, 30) * 60_000),
      metadata: meta,
    };
  }));
}

async function qms(): Promise<void> {
  console.log('QMS...');
  await bulk('mdpQmsInspectionPlan', Array.from({ length: N }, (_, i) => ({
    code: `DQP-${pad(i)}`, name: `Rencana Inspeksi ${pad(i)}`,
    type: pick(['INCOMING', 'IN_PROCESS', 'FINAL'], i), isActive: true, metadata: meta,
  })));
  const planIds = await ids('mdpQmsInspectionPlan');

  await bulk('mdpQmsInspectionCharacteristic', Array.from({ length: N }, (_, i) => ({
    planId: pick(planIds, i), sequence: (i % 8) + 1,
    name: `Karakteristik ${pick(['Dimensi', 'Berat', 'Warna', 'Kekerasan', 'Visual'], i)} ${pad(i)}`,
    characteristicType: pick(['VARIABLE', 'ATTRIBUTE'], i),
    nominal: dec(i, 1, 100), lowerLimit: dec(i, 0, 1), upperLimit: dec(i, 100, 200),
    uomCode: pick(['mm', 'kg', 'pcs', '%', 'HRC'], i),
  })));
  const charIds = await ids('mdpQmsInspectionCharacteristic');

  await bulk('mdpQmsInspection', Array.from({ length: N }, (_, i) => ({
    code: `DQI-${pad(i)}`, planId: pick(planIds, i),
    type: pick(['INCOMING', 'IN_PROCESS', 'FINAL'], i),
    result: pick(['PENDING', 'PASS', 'FAIL'], i),
    inspectedAt: daysAgo(rnd(i, 60)), inspectedById: user(i),
    lotSize: dec(i, 10, 1000), sampleSize: dec(i, 1, 50), metadata: meta,
  })));
  const inspIds = await ids('mdpQmsInspection');

  await bulk('mdpQmsInspectionResult', Array.from({ length: N }, (_, i) => ({
    inspectionId: pick(inspIds, i), characteristicId: pick(charIds, i),
    measuredValue: dec(i, 1, 150), status: pick(['PASS', 'FAIL', 'NA'], i),
    notes: note(i, 'Hasil ukur'),
  })));

  await bulk('mdpQmsNonconformance', Array.from({ length: N }, (_, i) => ({
    code: `DNCR-${pad(i)}`, name: `Ketidaksesuaian ${pad(i)}`,
    inspectionId: pick(inspIds, i),
    severity: pick(['MINOR', 'MAJOR', 'CRITICAL'], i),
    status: pick(['OPEN', 'UNDER_REVIEW', 'CONTAINED', 'CLOSED'], i),
    disposition: pick(['PENDING', 'USE_AS_IS', 'REWORK', 'REPAIR', 'SCRAP'], i),
    detectedAt: daysAgo(rnd(i, 50)), qtyAffected: dec(i, 1, 100), metadata: meta,
  })));
  const ncrIds = await ids('mdpQmsNonconformance');

  await bulk('mdpQmsCapaAction', Array.from({ length: N }, (_, i) => ({
    code: `DCAPA-${pad(i)}`, name: `Tindakan CAPA ${pad(i)}`,
    nonconformanceId: pick(ncrIds, i),
    type: pick(['CORRECTIVE', 'PREVENTIVE'], i),
    status: pick(['OPEN', 'IN_PROGRESS', 'IMPLEMENTED', 'VERIFIED', 'CLOSED'], i),
    dueDate: daysAgo(-rnd(i, 30, 1)), assignedToId: user(i), metadata: meta,
  })));
}

async function cmms(): Promise<void> {
  console.log('CMMS...');
  await bulk('mdpMntFailureCode', Array.from({ length: N }, (_, i) => ({
    code: `DFC-${pad(i)}`, name: `Kode Kegagalan ${pad(i)}`,
    type: pick(['FAILURE', 'CAUSE', 'REMEDY'], i), isActive: true, metadata: meta,
  })));
  const fcIds = await ids('mdpMntFailureCode');

  await bulk('mdpMntPmSchedule', Array.from({ length: N }, (_, i) => ({
    code: `DPM-${pad(i)}`, name: `Jadwal PM ${pad(i)}`,
    assetId: null, triggerType: pick(['TIME_BASED', 'METER_BASED'], i),
    intervalDays: rnd(i, 90, 7), isActive: true, metadata: meta,
  })));
  const pmIds = await ids('mdpMntPmSchedule');
  const assetIds = await ids('mdpAsset');
  const wcIds = await ids('mdpWorkCenter');

  await bulk('mdpMntWorkOrder', Array.from({ length: N }, (_, i) => ({
    code: `DWO-${pad(i)}`, name: `Work Order Maintenance ${pad(i)}`,
    type: pick(['CORRECTIVE', 'PREVENTIVE', 'PREDICTIVE', 'INSPECTION'], i),
    status: pick(['OPEN', 'SCHEDULED', 'IN_PROGRESS', 'ON_HOLD', 'COMPLETED'], i),
    priority: pick(['LOW', 'MEDIUM', 'HIGH', 'URGENT'], i),
    assetId: pick(assetIds, i), workCenterId: pick(wcIds, i),
    pmScheduleId: i % 2 === 0 ? pick(pmIds, i) : null,
    failureCodeId: pick(fcIds, i), reportedById: user(i), assignedToId: user(i + 1),
    scheduledStartAt: daysAgo(rnd(i, 40)), metadata: meta,
  })));
  const woIds = await ids('mdpMntWorkOrder');

  await bulk('mdpMntSparePart', Array.from({ length: N }, (_, i) => ({
    workOrderId: pick(woIds, i), itemId: item(i), qty: dec(i, 1, 50),
    postingStatus: pick(['PENDING', 'POSTED'], i), notes: note(i, 'Spare part'),
  })));
}

async function wms(): Promise<void> {
  console.log('WMS...');
  await bulk('mdpWmsHandlingUnit', Array.from({ length: N }, (_, i) => ({
    code: `DHU-${pad(i)}`,
    status: pick(['OPEN', 'CLOSED', 'SHIPPED'], i), metadata: meta,
  })));
  const huIds = await ids('mdpWmsHandlingUnit');

  await bulk('mdpWmsTask', Array.from({ length: N }, (_, i) => ({
    code: `DTSK-${pad(i)}`,
    type: pick(['PUTAWAY', 'PICK', 'MOVE', 'COUNT', 'REPLENISH'], i),
    status: pick(['OPEN', 'IN_PROGRESS', 'COMPLETED'], i),
    itemId: item(i), qty: dec(i, 1, 200), priority: (i % 5) + 1,
    assignedToId: user(i), metadata: meta,
  })));
  const taskIds = await ids('mdpWmsTask');

  await bulk('mdpWmsPick', Array.from({ length: N }, (_, i) => ({
    taskId: pick(taskIds, i), itemId: item(i), handlingUnitId: pick(huIds, i),
    qtyRequested: dec(i, 1, 200), qtyPicked: dec(i, 0, 200), notes: note(i, 'Pick line'),
  })));

  await bulk('mdpWmsMovement', Array.from({ length: N }, (_, i) => ({
    code: `DMOV-${pad(i)}`, taskId: pick(taskIds, i), handlingUnitId: pick(huIds, i),
    itemId: item(i), qty: dec(i, 1, 300), movedAt: hoursAgo(rnd(i, 300, 1)),
    postingStatus: pick(['PENDING', 'POSTED'], i), metadata: meta,
  })));
}

async function prts(): Promise<void> {
  console.log('PRTS...');
  await bulk('mdpPrtIssue', Array.from({ length: N }, (_, i) => ({
    code: `DISS-${pad(i)}`, name: `Andon Issue ${pad(i)}`,
    type: pick(['QUALITY', 'MACHINE', 'SAFETY', 'MATERIAL', 'PROCESS'], i),
    severity: pick(['LOW', 'MEDIUM', 'HIGH', 'CRITICAL'], i),
    status: pick(['OPEN', 'ACKNOWLEDGED', 'IN_PROGRESS', 'RESOLVED', 'CLOSED'], i),
    raisedAt: hoursAgo(rnd(i, 400, 1)), reportedById: user(i), metadata: meta,
  })));
  const issueIds = await ids('mdpPrtIssue');

  await bulk('mdpPrtEscalation', Array.from({ length: N }, (_, i) => ({
    issueId: pick(issueIds, i), level: (i % 3) + 1,
    status: pick(['PENDING', 'ACKNOWLEDGED', 'RESOLVED'], i),
    escalatedAt: hoursAgo(rnd(i, 350, 1)), escalatedToId: user(i), notes: note(i, 'Eskalasi'),
  })));
}

async function dms(): Promise<void> {
  console.log('DMS...');
  await bulk('mdpDmsDocument', Array.from({ length: N }, (_, i) => ({
    code: `DDOC-${pad(i)}`, name: `Dokumen ${pad(i)}`,
    category: pick(['SOP', 'WORK_INSTRUCTION', 'DRAWING', 'POLICY', 'FORM', 'RECORD'], i),
    status: pick(['DRAFT', 'IN_REVIEW', 'APPROVED', 'RELEASED', 'OBSOLETE'], i),
    metadata: meta,
  })));
  const docIds = await ids('mdpDmsDocument');

  await bulk('mdpDmsRevision', Array.from({ length: N }, (_, i) => ({
    documentId: pick(docIds, i), revisionCode: `Rev-${(i % 9) + 1}.${i % 5}`,
    status: pick(['DRAFT', 'IN_REVIEW', 'APPROVED', 'SUPERSEDED'], i),
    notes: note(i, 'Revisi'),
  })));
  const revIds = await ids('mdpDmsRevision');

  await bulk('mdpDmsAcknowledgement', Array.from({ length: N }, (_, i) => ({
    documentId: pick(docIds, i), revisionId: pick(revIds, i),
    userId: user(i), acknowledgedAt: daysAgo(rnd(i, 90)), notes: note(i, 'Acknowledge'),
  })));
}

async function ims(): Promise<void> {
  console.log('IMS/QHSE...');
  await bulk('mdpEhsIncident', Array.from({ length: N }, (_, i) => ({
    code: `DINC-${pad(i)}`, name: `Insiden ${pad(i)}`,
    type: pick(['INJURY', 'NEAR_MISS', 'PROPERTY_DAMAGE', 'ENVIRONMENTAL', 'SECURITY'], i),
    severity: pick(['MINOR', 'MODERATE', 'MAJOR', 'FATAL'], i),
    status: pick(['REPORTED', 'UNDER_INVESTIGATION', 'ACTION_PENDING', 'CLOSED'], i),
    occurredAt: daysAgo(rnd(i, 120)), reportedById: user(i), metadata: meta,
  })));

  await bulk('mdpEhsAudit', Array.from({ length: N }, (_, i) => ({
    code: `DAUD-${pad(i)}`, name: `Audit ${pad(i)}`,
    type: pick(['SAFETY', 'ENVIRONMENTAL', 'QUALITY', 'FIVE_S', 'INTERNAL', 'EXTERNAL'], i),
    status: pick(['PLANNED', 'IN_PROGRESS', 'COMPLETED', 'CANCELLED'], i),
    scheduledAt: daysAgo(rnd(i, 80) - 20), metadata: meta,
  })));

  await bulk('mdpEhsPermit', Array.from({ length: N }, (_, i) => ({
    code: `DPRM-${pad(i)}`, name: `Izin Kerja ${pad(i)}`,
    type: pick(['HOT_WORK', 'CONFINED_SPACE', 'WORKING_AT_HEIGHT', 'ELECTRICAL', 'CHEMICAL'], i),
    status: pick(['REQUESTED', 'APPROVED', 'ACTIVE', 'CLOSED', 'EXPIRED'], i),
    validFrom: daysAgo(rnd(i, 30)), validTo: daysAgo(-rnd(i, 15, 1)), metadata: meta,
  })));
}

async function lms(): Promise<void> {
  console.log('LMS...');
  await bulk('mdpLmsCourse', Array.from({ length: N }, (_, i) => ({
    code: `DCRS-${pad(i)}`, name: `Pelatihan ${pad(i)}`,
    category: pick(['SAFETY', 'QUALITY', 'TECHNICAL', 'ONBOARDING', 'COMPLIANCE'], i),
    status: pick(['DRAFT', 'ACTIVE', 'ARCHIVED'], i),
    durationHours: dec(i, 1, 40), isMandatory: i % 3 === 0, metadata: meta,
  })));
  const courseIds = await ids('mdpLmsCourse');

  await bulk('mdpLmsEnrollment', Array.from({ length: N }, (_, i) => ({
    courseId: pick(courseIds, i), userId: user(i),
    status: pick(['ENROLLED', 'IN_PROGRESS', 'COMPLETED', 'FAILED', 'EXPIRED'], i),
    enrolledAt: daysAgo(rnd(i, 100)), score: dec(i, 50, 100), notes: note(i, 'Enrollment'),
  })));

  await bulk('mdpLmsCompetency', Array.from({ length: N }, (_, i) => ({
    code: `DCMP-${pad(i)}`, name: `Kompetensi ${pad(i)}`,
    requiredCourseId: pick(courseIds, i), metadata: meta,
  })));
}

async function main(): Promise<void> {
  console.log(`MDP bulk dummy seed — target ${N} rows/feature\n`);
  ITEM_IDS = (await prisma.$queryRawUnsafe<{ id: bigint }[]>(
    'select id from md_items where deleted_at is null order by id limit 60',
  )).map((r) => r.id);
  USER_IDS = (await prisma.$queryRawUnsafe<{ id: bigint }[]>(
    'select id from adm_users where deleted_at is null order by id limit 30',
  )).map((r) => r.id);
  WAREHOUSE_IDS = (await prisma.$queryRawUnsafe<{ id: bigint }[]>(
    'select id from md_warehouses order by id limit 20',
  )).map((r) => r.id);
  console.log(`refs: items=${ITEM_IDS.length} users=${USER_IDS.length} warehouses=${WAREHOUSE_IDS.length}\n`);

  await wipe();
  await masters();
  await mes();
  await qms();
  await cmms();
  await wms();
  await prts();
  await dms();
  await ims();
  await lms();
  console.log('\nDone.');
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());

/**
 * Seed MDP foundation master data + a few sample MES production orders so the
 * web-mdp UI (port 3220) is testable end-to-end.
 *
 * Idempotent: upserts by unique `code`. Re-running updates in place; nothing is
 * deleted. Production orders reference real ERP `md_items` rows (scalar FK) —
 * if no items exist, that step is skipped with a warning (masters still seed).
 *
 * Run: npx ts-node -r tsconfig-paths/register prisma/seed-mdp-foundation.ts
 *  or: npm run db:seed:mdp
 */

import { MdpMesOrderStatus, MdpReasonCodeCategory, PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

// ─── Definitions ────────────────────────────────────────────────────────────

interface AssetDef {
  code: string;
  name: string;
}

const ASSET_DEFS: AssetDef[] = [
  { code: 'AST-CUT-01', name: 'Cutting Machine 01' },
  { code: 'AST-PRESS-01', name: 'Hydraulic Press 01' },
  { code: 'AST-WELD-01', name: 'Welding Station 01' },
  { code: 'AST-PACK-01', name: 'Packing Line 01' },
];

interface WorkCenterDef {
  code: string;
  name: string;
  assetCode?: string;
  idealCycleSeconds?: number;
}

const WORK_CENTER_DEFS: WorkCenterDef[] = [
  {
    code: 'WC-CUTTING-01',
    name: 'Cutting Line 1',
    assetCode: 'AST-CUT-01',
    idealCycleSeconds: 12.5,
  },
  { code: 'WC-PRESS-01', name: 'Press Line 1', assetCode: 'AST-PRESS-01', idealCycleSeconds: 8 },
  {
    code: 'WC-WELDING-01',
    name: 'Welding Cell 1',
    assetCode: 'AST-WELD-01',
    idealCycleSeconds: 30,
  },
  { code: 'WC-ASSY-01', name: 'Assembly Line 1', idealCycleSeconds: 45 },
  { code: 'WC-PACKING-01', name: 'Packing Line 1', assetCode: 'AST-PACK-01', idealCycleSeconds: 6 },
];

interface ShiftDef {
  code: string;
  name: string;
  startTime: string;
  endTime: string;
}

const SHIFT_DEFS: ShiftDef[] = [
  { code: 'SHIFT-1', name: 'Shift Pagi', startTime: '07:00', endTime: '15:00' },
  { code: 'SHIFT-2', name: 'Shift Sore', startTime: '15:00', endTime: '23:00' },
  { code: 'SHIFT-3', name: 'Shift Malam', startTime: '23:00', endTime: '07:00' },
];

interface ReasonDef {
  code: string;
  name: string;
  category: MdpReasonCodeCategory;
}

const REASON_DEFS: ReasonDef[] = [
  { code: 'DT-CHANGEOVER', name: 'Changeover / Setup', category: MdpReasonCodeCategory.DOWNTIME },
  { code: 'DT-BREAKDOWN', name: 'Kerusakan Mesin', category: MdpReasonCodeCategory.DOWNTIME },
  { code: 'DT-NO-MATERIAL', name: 'Material Habis', category: MdpReasonCodeCategory.DOWNTIME },
  { code: 'DT-CLEANING', name: 'Pembersihan Terjadwal', category: MdpReasonCodeCategory.DOWNTIME },
  { code: 'SCR-DEFECT', name: 'Cacat Produksi', category: MdpReasonCodeCategory.SCRAP },
  { code: 'SCR-MATERIAL', name: 'Cacat Material', category: MdpReasonCodeCategory.SCRAP },
  { code: 'DLY-WAIT-QC', name: 'Menunggu Inspeksi QC', category: MdpReasonCodeCategory.DELAY },
  { code: 'QUA-REWORK', name: 'Rework Kualitas', category: MdpReasonCodeCategory.QUALITY },
];

interface WorkCalendarDef {
  code: string;
  name: string;
  description?: string;
  plannedMinutesPerDay: number;
  workingDaysPerWeek: number;
}

const WORK_CALENDAR_DEFS: WorkCalendarDef[] = [
  {
    code: 'CAL-DEFAULT',
    name: 'Kalender Default (3 shift)',
    description: 'Plant-wide planned operating time, 24 jam × 6 hari.',
    plannedMinutesPerDay: 1440,
    workingDaysPerWeek: 6,
  },
  {
    code: 'CAL-1SHIFT',
    name: 'Kalender 1 Shift',
    description: 'Plant-wide, 8 jam × 5 hari.',
    plannedMinutesPerDay: 480,
    workingDaysPerWeek: 5,
  },
];

// Nav SSOT (mdp_menus). Ordered roots-first so parentCode resolves via menuMap.
interface MenuDef {
  code: string;
  name: string;
  parentCode?: string;
  path?: string;
  icon?: string;
  moduleKey?: string;
  sequence: number;
}

const MENU_DEFS: MenuDef[] = [
  {
    code: 'mes',
    name: 'Manufacturing Execution',
    path: '/app/mes',
    icon: 'Factory',
    moduleKey: 'mes',
    sequence: 10,
  },
  {
    code: 'mes.orders',
    name: 'Production Orders',
    parentCode: 'mes',
    path: '/app/mes',
    moduleKey: 'mes',
    sequence: 11,
  },
  {
    code: 'mes.operations',
    name: 'Operations',
    parentCode: 'mes',
    path: '/app/mes/operations',
    moduleKey: 'mes',
    sequence: 12,
  },
  {
    code: 'mes.logs',
    name: 'Production Logs',
    parentCode: 'mes',
    path: '/app/mes/logs',
    moduleKey: 'mes',
    sequence: 13,
  },
  {
    code: 'mes.consumptions',
    name: 'Material Consumptions',
    parentCode: 'mes',
    path: '/app/mes/consumptions',
    moduleKey: 'mes',
    sequence: 14,
  },
  {
    code: 'mes.downtime',
    name: 'Downtime Events',
    parentCode: 'mes',
    path: '/app/mes/downtime',
    moduleKey: 'mes',
    sequence: 15,
  },
  {
    code: 'mes.labor',
    name: 'Labor Logs',
    parentCode: 'mes',
    path: '/app/mes/labor',
    moduleKey: 'mes',
    sequence: 16,
  },
  {
    code: 'wms',
    name: 'Eksekusi Gudang',
    path: '/app/wms',
    icon: 'Warehouse',
    moduleKey: 'wms',
    sequence: 20,
  },
  {
    code: 'wms.tasks',
    name: 'Tasks',
    parentCode: 'wms',
    path: '/app/wms',
    moduleKey: 'wms',
    sequence: 21,
  },
  {
    code: 'wms.picks',
    name: 'Picks',
    parentCode: 'wms',
    path: '/app/wms/picks',
    moduleKey: 'wms',
    sequence: 22,
  },
  {
    code: 'wms.movements',
    name: 'Movements',
    parentCode: 'wms',
    path: '/app/wms/movements',
    moduleKey: 'wms',
    sequence: 23,
  },
  {
    code: 'wms.handling-units',
    name: 'Handling Units',
    parentCode: 'wms',
    path: '/app/wms/handling-units',
    moduleKey: 'wms',
    sequence: 24,
  },
  {
    code: 'qms',
    name: 'Kualitas',
    path: '/app/quality',
    icon: 'ShieldCheck',
    moduleKey: 'qms',
    sequence: 30,
  },
  {
    code: 'qms.plans',
    name: 'Inspection Plans',
    parentCode: 'qms',
    path: '/app/quality',
    moduleKey: 'qms',
    sequence: 31,
  },
  {
    code: 'qms.characteristics',
    name: 'Characteristics',
    parentCode: 'qms',
    path: '/app/quality/characteristics',
    moduleKey: 'qms',
    sequence: 32,
  },
  {
    code: 'qms.inspections',
    name: 'Inspections',
    parentCode: 'qms',
    path: '/app/quality/inspections',
    moduleKey: 'qms',
    sequence: 33,
  },
  {
    code: 'qms.results',
    name: 'Inspection Results',
    parentCode: 'qms',
    path: '/app/quality/results',
    moduleKey: 'qms',
    sequence: 34,
  },
  {
    code: 'qms.nonconformances',
    name: 'Nonconformances (NCR)',
    parentCode: 'qms',
    path: '/app/quality/nonconformances',
    moduleKey: 'qms',
    sequence: 35,
  },
  {
    code: 'qms.capa-actions',
    name: 'CAPA Actions',
    parentCode: 'qms',
    path: '/app/quality/capa-actions',
    moduleKey: 'qms',
    sequence: 36,
  },
  {
    code: 'mnt',
    name: 'Pemeliharaan',
    path: '/app/maintenance',
    icon: 'Wrench',
    moduleKey: 'mnt',
    sequence: 40,
  },
  {
    code: 'mnt.work-orders',
    name: 'Work Orders',
    parentCode: 'mnt',
    path: '/app/maintenance',
    moduleKey: 'mnt',
    sequence: 41,
  },
  {
    code: 'mnt.pm-schedules',
    name: 'PM Schedules',
    parentCode: 'mnt',
    path: '/app/maintenance/pm-schedules',
    moduleKey: 'mnt',
    sequence: 42,
  },
  {
    code: 'mnt.spare-parts',
    name: 'Spare Parts',
    parentCode: 'mnt',
    path: '/app/maintenance/spare-parts',
    moduleKey: 'mnt',
    sequence: 43,
  },
  {
    code: 'mnt.failure-codes',
    name: 'Failure Codes',
    parentCode: 'mnt',
    path: '/app/maintenance/failure-codes',
    moduleKey: 'mnt',
    sequence: 44,
  },
  {
    code: 'prt',
    name: 'Problem & Tracking',
    path: '/app/problems',
    icon: 'AlertTriangle',
    moduleKey: 'prt',
    sequence: 50,
  },
  {
    code: 'prt.issues',
    name: 'Issues',
    parentCode: 'prt',
    path: '/app/problems',
    moduleKey: 'prt',
    sequence: 51,
  },
  {
    code: 'prt.escalations',
    name: 'Escalations',
    parentCode: 'prt',
    path: '/app/problems/escalations',
    moduleKey: 'prt',
    sequence: 52,
  },
  {
    code: 'dms',
    name: 'Dokumen',
    path: '/app/documents',
    icon: 'FileText',
    moduleKey: 'dms',
    sequence: 55,
  },
  {
    code: 'dms.documents',
    name: 'Documents',
    parentCode: 'dms',
    path: '/app/documents',
    moduleKey: 'dms',
    sequence: 56,
  },
  {
    code: 'dms.revisions',
    name: 'Revisions',
    parentCode: 'dms',
    path: '/app/documents/revisions',
    moduleKey: 'dms',
    sequence: 57,
  },
  {
    code: 'dms.acknowledgements',
    name: 'Acknowledgements',
    parentCode: 'dms',
    path: '/app/documents/acknowledgements',
    moduleKey: 'dms',
    sequence: 58,
  },
  {
    code: 'ehs',
    name: 'QHSE Terpadu',
    path: '/app/qhse',
    icon: 'HardHat',
    moduleKey: 'ehs',
    sequence: 60,
  },
  {
    code: 'ehs.incidents',
    name: 'Incidents',
    parentCode: 'ehs',
    path: '/app/qhse',
    moduleKey: 'ehs',
    sequence: 61,
  },
  {
    code: 'ehs.audits',
    name: 'Audits',
    parentCode: 'ehs',
    path: '/app/qhse/audits',
    moduleKey: 'ehs',
    sequence: 62,
  },
  {
    code: 'ehs.permits',
    name: 'Permits',
    parentCode: 'ehs',
    path: '/app/qhse/permits',
    moduleKey: 'ehs',
    sequence: 63,
  },
  {
    code: 'lms',
    name: 'Pelatihan',
    path: '/app/training',
    icon: 'GraduationCap',
    moduleKey: 'lms',
    sequence: 65,
  },
  {
    code: 'lms.courses',
    name: 'Courses',
    parentCode: 'lms',
    path: '/app/training',
    moduleKey: 'lms',
    sequence: 66,
  },
  {
    code: 'lms.enrollments',
    name: 'Enrollments',
    parentCode: 'lms',
    path: '/app/training/enrollments',
    moduleKey: 'lms',
    sequence: 67,
  },
  {
    code: 'lms.competencies',
    name: 'Competencies',
    parentCode: 'lms',
    path: '/app/training/competencies',
    moduleKey: 'lms',
    sequence: 68,
  },
  {
    code: 'master',
    name: 'Master Data',
    path: '/app/master',
    icon: 'Database',
    moduleKey: 'mdp',
    sequence: 90,
  },
  {
    code: 'master.work-centers',
    name: 'Work Center',
    parentCode: 'master',
    path: '/app/master/work-centers',
    moduleKey: 'eam',
    sequence: 91,
  },
  {
    code: 'master.assets',
    name: 'Aset / Equipment',
    parentCode: 'master',
    path: '/app/master/assets',
    moduleKey: 'eam',
    sequence: 92,
  },
  {
    code: 'master.shifts',
    name: 'Shift',
    parentCode: 'master',
    path: '/app/master/shifts',
    moduleKey: 'mdp',
    sequence: 93,
  },
  {
    code: 'master.reason-codes',
    name: 'Reason Code',
    parentCode: 'master',
    path: '/app/master/reason-codes',
    moduleKey: 'mdp',
    sequence: 94,
  },
  {
    code: 'master.work-calendars',
    name: 'Work Calendar',
    parentCode: 'master',
    path: '/app/master/work-calendars',
    moduleKey: 'mdp',
    sequence: 95,
  },
  {
    code: 'master.menus',
    name: 'Menu / Navigasi',
    parentCode: 'master',
    path: '/app/master/menus',
    moduleKey: 'mdp',
    sequence: 96,
  },
  {
    code: 'master.role-menus',
    name: 'Akses Menu per Role',
    parentCode: 'master',
    path: '/app/master/role-menus',
    moduleKey: 'mdp',
    sequence: 97,
  },
];

// ─── Helpers ────────────────────────────────────────────────────────────────

async function upsertByCode<TKey extends string>(
  label: string,
  defs: readonly { code: string }[],
  find: (code: string) => Promise<{ id: bigint } | null>,
  create: (def: any) => Promise<{ id: bigint }>,
  update: (id: bigint, def: any) => Promise<unknown>,
  map?: Map<string, bigint>,
): Promise<void> {
  let created = 0;
  let updated = 0;
  for (const def of defs) {
    const existing = await find(def.code);
    if (existing) {
      await update(existing.id, def);
      map?.set(def.code, existing.id);
      updated++;
    } else {
      const row = await create(def);
      map?.set(def.code, row.id);
      created++;
    }
  }
  console.log(`✅ ${label}: ${created} created, ${updated} updated`);
}

// ─── Main ───────────────────────────────────────────────────────────────────

async function main() {
  console.log('=== Seed MDP foundation (mdp/eam) + sample MES orders ===\n');

  const assetMap = new Map<string, bigint>();
  const wcMap = new Map<string, bigint>();

  // 1. Assets (eam)
  await upsertByCode(
    'Assets',
    ASSET_DEFS,
    (code) => prisma.mdpAsset.findFirst({ where: { code }, select: { id: true } }),
    (d: AssetDef) =>
      prisma.mdpAsset.create({ data: { code: d.code, name: d.name, isActive: true } }),
    (id, d: AssetDef) => prisma.mdpAsset.update({ where: { id }, data: { name: d.name } }),
    assetMap,
  );

  // 2. Work centers (eam) — link to assets
  await upsertByCode(
    'Work centers',
    WORK_CENTER_DEFS,
    (code) => prisma.mdpWorkCenter.findFirst({ where: { code }, select: { id: true } }),
    (d: WorkCenterDef) =>
      prisma.mdpWorkCenter.create({
        data: {
          code: d.code,
          name: d.name,
          assetId: d.assetCode ? (assetMap.get(d.assetCode) ?? null) : null,
          idealCycleSeconds: d.idealCycleSeconds ?? null,
          isActive: true,
        },
      }),
    (id, d: WorkCenterDef) =>
      prisma.mdpWorkCenter.update({
        where: { id },
        data: {
          name: d.name,
          assetId: d.assetCode ? (assetMap.get(d.assetCode) ?? null) : null,
          idealCycleSeconds: d.idealCycleSeconds ?? null,
        },
      }),
    wcMap,
  );

  // 3. Shifts (mdp)
  await upsertByCode(
    'Shifts',
    SHIFT_DEFS,
    (code) => prisma.mdpShift.findFirst({ where: { code }, select: { id: true } }),
    (d: ShiftDef) =>
      prisma.mdpShift.create({
        data: {
          code: d.code,
          name: d.name,
          startTime: d.startTime,
          endTime: d.endTime,
          isActive: true,
        },
      }),
    (id, d: ShiftDef) =>
      prisma.mdpShift.update({
        where: { id },
        data: { name: d.name, startTime: d.startTime, endTime: d.endTime },
      }),
  );

  // 4. Reason codes (mdp)
  await upsertByCode(
    'Reason codes',
    REASON_DEFS,
    (code) => prisma.mdpReasonCode.findFirst({ where: { code }, select: { id: true } }),
    (d: ReasonDef) =>
      prisma.mdpReasonCode.create({
        data: { code: d.code, name: d.name, category: d.category, isActive: true },
      }),
    (id, d: ReasonDef) =>
      prisma.mdpReasonCode.update({ where: { id }, data: { name: d.name, category: d.category } }),
  );

  // 5. Work calendars (mdp) — OEE availability basis
  await upsertByCode(
    'Work calendars',
    WORK_CALENDAR_DEFS,
    (code) => prisma.mdpWorkCalendar.findFirst({ where: { code }, select: { id: true } }),
    (d: WorkCalendarDef) =>
      prisma.mdpWorkCalendar.create({
        data: {
          code: d.code,
          name: d.name,
          description: d.description,
          plannedMinutesPerDay: d.plannedMinutesPerDay,
          workingDaysPerWeek: d.workingDaysPerWeek,
          isActive: true,
        },
      }),
    (id, d: WorkCalendarDef) =>
      prisma.mdpWorkCalendar.update({
        where: { id },
        data: {
          name: d.name,
          description: d.description,
          plannedMinutesPerDay: d.plannedMinutesPerDay,
          workingDaysPerWeek: d.workingDaysPerWeek,
        },
      }),
  );

  // 6. Menus (mdp) — nav SSOT; roots first so parentCode resolves
  const menuMap = new Map<string, bigint>();
  let menuCreated = 0;
  let menuUpdated = 0;
  for (const d of MENU_DEFS) {
    const parentId = d.parentCode ? (menuMap.get(d.parentCode) ?? null) : null;
    const data = {
      name: d.name,
      parentId,
      path: d.path ?? null,
      icon: d.icon ?? null,
      moduleKey: d.moduleKey ?? null,
      sequence: d.sequence,
      isActive: true,
    };
    const existing = await prisma.mdpMenu.findFirst({
      where: { code: d.code },
      select: { id: true },
    });
    if (existing) {
      await prisma.mdpMenu.update({ where: { id: existing.id }, data });
      menuMap.set(d.code, existing.id);
      menuUpdated++;
    } else {
      const row = await prisma.mdpMenu.create({ data: { code: d.code, ...data } });
      menuMap.set(d.code, row.id);
      menuCreated++;
    }
  }
  console.log(`✅ Menus: ${menuCreated} created, ${menuUpdated} updated`);

  // 7. Sample production orders (mes) — need real ERP items
  const items = await prisma.erpItem.findMany({
    where: { deletedAt: null },
    select: { id: true, code: true, name: true },
    orderBy: { id: 'asc' },
    take: 3,
  });

  if (items.length === 0) {
    console.warn('\n⚠ No md_items found — skipping sample production orders.');
  } else {
    const wcIds = [...wcMap.values()];
    let created = 0;
    let updated = 0;
    for (let i = 0; i < items.length; i++) {
      const item = items[i];
      const code = `MO-SEED-${String(i + 1).padStart(4, '0')}`;
      const workCenterId = wcIds[i % wcIds.length] ?? null;
      const plannedQty = (i + 1) * 500;
      const data = {
        itemId: item.id,
        workCenterId,
        plannedQty,
        uomCode: 'PCS',
        status: i === 0 ? MdpMesOrderStatus.IN_PROGRESS : MdpMesOrderStatus.RELEASED,
        notes: `Seed order untuk ${item.name ?? item.code}`,
      };
      const existing = await prisma.mdpProductionOrder.findFirst({
        where: { code },
        select: { id: true },
      });
      if (existing) {
        await prisma.mdpProductionOrder.update({ where: { id: existing.id }, data });
        updated++;
      } else {
        await prisma.mdpProductionOrder.create({ data: { code, ...data } });
        created++;
      }
    }
    console.log(
      `✅ Production orders: ${created} created, ${updated} updated (items: ${items.map((i) => i.code).join(', ')})`,
    );
  }

  // 8. Sample WMS rows (handling units + tasks + movements) — idempotent by code
  const HU_DEFS = [
    { code: 'HU-PLT-0001', status: 'OPEN' as const },
    { code: 'HU-PLT-0002', status: 'OPEN' as const },
  ];
  for (const d of HU_DEFS) {
    const existing = await prisma.mdpWmsHandlingUnit.findFirst({
      where: { code: d.code },
      select: { id: true },
    });
    if (existing)
      await prisma.mdpWmsHandlingUnit.update({
        where: { id: existing.id },
        data: { status: d.status },
      });
    else await prisma.mdpWmsHandlingUnit.create({ data: { code: d.code, status: d.status } });
  }
  console.log(`✅ WMS handling units: ${HU_DEFS.length} upserted`);

  if (items.length > 0) {
    const it = items[0];
    const TASK_DEFS = [
      { code: 'WT-SEED-0001', type: 'PICK' as const, qty: 50, priority: 5 },
      { code: 'WT-SEED-0002', type: 'PUTAWAY' as const, qty: 120, priority: 1 },
    ];
    for (const d of TASK_DEFS) {
      const data = {
        type: d.type,
        status: 'OPEN' as const,
        itemId: it.id,
        qty: d.qty,
        uomCode: 'PCS',
        priority: d.priority,
      };
      const existing = await prisma.mdpWmsTask.findFirst({
        where: { code: d.code },
        select: { id: true },
      });
      if (existing) await prisma.mdpWmsTask.update({ where: { id: existing.id }, data });
      else await prisma.mdpWmsTask.create({ data: { code: d.code, ...data } });
    }
    const mvExisting = await prisma.mdpWmsMovement.findFirst({
      where: { code: 'WM-SEED-0001' },
      select: { id: true },
    });
    const mvData = {
      itemId: it.id,
      qty: 50,
      uomCode: 'PCS',
      movedAt: new Date(),
      postingStatus: 'PENDING' as const,
    };
    if (mvExisting)
      await prisma.mdpWmsMovement.update({ where: { id: mvExisting.id }, data: mvData });
    else await prisma.mdpWmsMovement.create({ data: { code: 'WM-SEED-0001', ...mvData } });
    console.log('✅ WMS tasks: 2 upserted · movements: 1 upserted');
  } else {
    console.warn('⚠ No md_items — skipping sample WMS tasks/movements.');
  }

  // 9. Default access map — grant the admin role full access to every menu so the
  // role-filtered nav has a real mapping (instead of falling back to full-tree).
  // Idempotent: upserts by (roleId, menuId). Skipped if no admin role exists.
  const adminRole = await prisma.erpRole.findFirst({
    where: {
      deletedAt: null,
      code: { in: ['SUPERADMIN', 'ADMIN', 'superadmin', 'admin'] },
    },
    select: { id: true, code: true },
    orderBy: { id: 'asc' },
  });
  if (adminRole) {
    const allMenus = await prisma.mdpMenu.findMany({
      where: { deletedAt: null },
      select: { id: true },
    });
    let mapped = 0;
    for (const m of allMenus) {
      const existing = await prisma.mdpRoleMenu.findFirst({
        where: { roleId: adminRole.id, menuId: m.id },
        select: { id: true },
      });
      const data = { canView: true, canEdit: true, deletedAt: null };
      if (existing) {
        await prisma.mdpRoleMenu.update({ where: { id: existing.id }, data });
      } else {
        await prisma.mdpRoleMenu.create({
          data: { roleId: adminRole.id, menuId: m.id, ...data },
        });
      }
      mapped++;
    }
    console.log(`✅ Role access: ${adminRole.code} → ${mapped} menus (canView+canEdit)`);
  } else {
    console.warn('⚠ No admin role (adm_roles) — skipping default access map.');
  }

  console.log('\nDone.');
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());

import { PrismaClient } from '@prisma/client';
import { pbkdf2Sync, randomBytes } from 'crypto';

const prisma = new PrismaClient();

type MenuSeed = {
  key: string;
  title: string;
  path: string | null;
  icon: string | null;
  type: 'GROUP' | 'ITEM' | 'COLLAPSE';
  parentKey: string | null;
  sortOrder: number;
  isVisible?: boolean;
  isActive?: boolean;
  permissionName?: string | null;
};

async function hashPassword(password: string): Promise<string> {
  const salt = randomBytes(16);
  const iterations = 210000;
  const digest = 'sha512';
  const derived = pbkdf2Sync(password, salt, iterations, 64, digest);
  return `pbkdf2$v1$${digest}$${iterations}$${salt.toString('base64')}$${derived.toString('base64')}`;
}

// NOTE: ERP menu seeding lives in `prisma/seed-erp.ts` (canonical, namespaced
// codes `M0`/`M1`/...). A parallel `ERP_MENU_SEEDS` block here used to
// re-introduce stray legacy short-id entries (`master-data`, `administrator`,
// `md-items`, ...) on every `npm run db:seed`, causing duplicate sidebar
// modules. Removed 2026-05-20. Run `npm run db:seed:erp` for ERP.

async function main() {
  console.log('Seeding database...');

  const passwordHash = await hashPassword('Password123!');

  const permissions = [
    { name: 'user:create', module: 'user', action: 'create', description: 'Create users' },
    { name: 'user:read', module: 'user', action: 'read', description: 'Read users' },
    { name: 'user:update', module: 'user', action: 'update', description: 'Update users' },
    { name: 'user:delete', module: 'user', action: 'delete', description: 'Delete users' },
    { name: 'role:manage', module: 'role', action: 'manage', description: 'Manage roles' },
    {
      name: 'department:manage',
      module: 'department',
      action: 'manage',
      description: 'Manage departments',
    },
    { name: 'audit:view', module: 'audit', action: 'view', description: 'View audit logs' },
  ];

  for (const perm of permissions) {
    await prisma.permission.upsert({
      where: { name: perm.name },
      update: {
        module: perm.module,
        action: perm.action,
        description: perm.description,
        deletedAt: null,
        deletedBy: null,
      },
      create: perm,
    });
  }

  const adminRole = await prisma.role.upsert({
    where: { name: 'admin' },
    update: {
      description: 'System Administrator',
      isSystem: true,
      deletedAt: null,
      deletedBy: null,
    },
    create: {
      name: 'admin',
      description: 'System Administrator',
      isSystem: true,
    },
  });

  const managerRole = await prisma.role.upsert({
    where: { name: 'manager' },
    update: {
      description: 'Department Manager',
      isSystem: false,
      deletedAt: null,
      deletedBy: null,
    },
    create: {
      name: 'manager',
      description: 'Department Manager',
      isSystem: false,
    },
  });

  const userRole = await prisma.role.upsert({
    where: { name: 'user' },
    update: {
      description: 'Regular User',
      isSystem: true,
      deletedAt: null,
      deletedBy: null,
    },
    create: {
      name: 'user',
      description: 'Regular User',
      isSystem: true,
    },
  });

  const allPermissions = await prisma.permission.findMany({
    where: { deletedAt: null },
    select: { id: true, name: true },
  });
  const managerPermissionNames = new Set(['user:read', 'user:update', 'department:manage']);

  for (const perm of allPermissions) {
    await prisma.rolePermission.upsert({
      where: { roleId_permissionId: { roleId: adminRole.id, permissionId: perm.id } },
      update: { deletedAt: null, deletedBy: null },
      create: { roleId: adminRole.id, permissionId: perm.id },
    });
    if (managerPermissionNames.has(perm.name)) {
      await prisma.rolePermission.upsert({
        where: { roleId_permissionId: { roleId: managerRole.id, permissionId: perm.id } },
        update: { deletedAt: null, deletedBy: null },
        create: { roleId: managerRole.id, permissionId: perm.id },
      });
    }
  }

  const provinceSeeds = [
    { isoCode: 'ID-SU', name: 'Sumatera Utara' },
    { isoCode: 'ID-KS', name: 'Kalimantan Selatan' },
    { isoCode: 'ID-SS', name: 'Sumatera Selatan' },
    { isoCode: 'ID-SN', name: 'Sulawesi Selatan' },
  ];

  const provinceByIso = new Map<string, number>();
  for (const seed of provinceSeeds) {
    const province = await prisma.masterDataProvince.upsert({
      where: { isoCode: seed.isoCode },
      update: {
        name: seed.name,
        deletedAt: null,
        deletedBy: null,
      },
      create: {
        isoCode: seed.isoCode,
        name: seed.name,
      },
      select: { id: true },
    });
    provinceByIso.set(seed.isoCode, province.id);
  }

  const citySeeds = [
    { provinceIso: 'ID-SU', name: 'Medan', postalCode: '20111' },
    { provinceIso: 'ID-KS', name: 'Banjarmasin', postalCode: '70111' },
    { provinceIso: 'ID-SS', name: 'Palembang', postalCode: '30111' },
    { provinceIso: 'ID-SN', name: 'Makassar', postalCode: '90111' },
  ];

  const cityByName = new Map<string, number>();
  for (const seed of citySeeds) {
    const provinceId = provinceByIso.get(seed.provinceIso);
    if (!provinceId) {
      throw new Error(`Province not found for ${seed.name}`);
    }
    const existing = await prisma.masterDataCity.findFirst({
      where: { provinceId, name: seed.name },
      select: { id: true },
    });

    const city = existing
      ? await prisma.masterDataCity.update({
          where: { id: existing.id },
          data: {
            postalCode: seed.postalCode,
            deletedAt: null,
            deletedBy: null,
          },
          select: { id: true },
        })
      : await prisma.masterDataCity.create({
          data: {
            provinceId,
            name: seed.name,
            postalCode: seed.postalCode,
          },
          select: { id: true },
        });

    cityByName.set(seed.name, city.id);
  }

  const warehouseSeeds = [
    { name: 'Medan', cityName: 'Medan', locationName: 'Gudang Medan' },
    { name: 'Banjarmasin', cityName: 'Banjarmasin', locationName: 'Gudang Banjarmasin' },
    { name: 'Palembang', cityName: 'Palembang', locationName: 'Gudang Palembang' },
    { name: 'Makasar', cityName: 'Makassar', locationName: 'Gudang Makasar' },
  ];

  const warehouseByName = new Map<string, number>();
  for (const seed of warehouseSeeds) {
    const cityId = cityByName.get(seed.cityName);
    if (!cityId) {
      throw new Error(`City not found for warehouse ${seed.name}`);
    }

    const existing = await prisma.masterDataWarehouse.findFirst({
      where: { name: seed.name, cityId },
      select: { id: true },
    });

    const warehouse = existing
      ? await prisma.masterDataWarehouse.update({
          where: { id: existing.id },
          data: {
            locationName: seed.locationName,
            deletedAt: null,
            deletedBy: null,
          },
          select: { id: true },
        })
      : await prisma.masterDataWarehouse.create({
          data: {
            name: seed.name,
            cityId,
            locationName: seed.locationName,
          },
          select: { id: true },
        });

    warehouseByName.set(seed.name, warehouse.id);
  }

  const uomSeeds = [
    { code: 'PCS', name: 'Pieces', type: 'unit' },
    { code: 'KG', name: 'Kilogram', type: 'weight' },
  ];
  for (const seed of uomSeeds) {
    await prisma.masterDataUom.upsert({
      where: { code: seed.code },
      update: {
        name: seed.name,
        type: seed.type,
        deletedAt: null,
        deletedBy: null,
      },
      create: seed,
    });
  }

  const contactSeeds = [
    {
      code: 'SUP-001',
      name: 'PT Supplier Utama',
      type: 'SUPPLIER',
      city: 'Medan',
      province: 'Sumatera Utara',
      contactFirstName: 'Budi',
      contactEmail: 'supplier.utama@example.com',
      contactPhone: '081200000001',
      address: 'Jl. Industri No. 1, Medan',
      tax: '01.234.567.8-901.000',
    },
    {
      code: 'CUS-001',
      name: 'PT Pelanggan Retail',
      type: 'CUSTOMER',
      city: 'Palembang',
      province: 'Sumatera Selatan',
      contactFirstName: 'Sari',
      contactEmail: 'pelanggan.retail@example.com',
      contactPhone: '081200000002',
      address: 'Jl. Niaga No. 99, Palembang',
      tax: '02.345.678.9-012.000',
    },
  ];

  for (const seed of contactSeeds) {
    await prisma.masterDataContact.upsert({
      where: { code: seed.code },
      update: {
        name: seed.name,
        type: seed.type,
        city: seed.city,
        province: seed.province,
        contactFirstName: seed.contactFirstName,
        contactEmail: seed.contactEmail,
        contactPhone: seed.contactPhone,
        address: seed.address,
        tax: seed.tax,
        deletedAt: null,
        deletedBy: null,
      },
      create: {
        code: seed.code,
        name: seed.name,
        type: seed.type,
        city: seed.city,
        province: seed.province,
        contactFirstName: seed.contactFirstName,
        contactEmail: seed.contactEmail,
        contactPhone: seed.contactPhone,
        address: seed.address,
        tax: seed.tax,
      },
    });
  }

  const pcsUom = await prisma.masterDataUom.findUnique({
    where: { code: 'PCS' },
    select: { id: true },
  });
  const kgUom = await prisma.masterDataUom.findUnique({
    where: { code: 'KG' },
    select: { id: true },
  });
  if (!pcsUom || !kgUom) {
    throw new Error('Required UOM seeds (PCS/KG) not found');
  }

  const itemSeeds = [
    {
      code: 'ITEM-001',
      name: 'Gula Pasir Premium 1KG',
      category: 'RAW_MATERIAL',
      itemType: 'RAW',
      uomId: kgUom.id,
    },
    {
      code: 'ITEM-002',
      name: 'Kopi Bubuk 250GR',
      category: 'FINISHED_GOOD',
      itemType: 'FG',
      uomId: pcsUom.id,
    },
  ];

  for (const seed of itemSeeds) {
    await prisma.masterDataItem.upsert({
      where: { code: seed.code },
      update: {
        name: seed.name,
        category: seed.category,
        itemType: seed.itemType,
        uomId: seed.uomId,
        isActive: true,
        deletedAt: null,
        deletedBy: null,
      },
      create: {
        code: seed.code,
        name: seed.name,
        category: seed.category,
        itemType: seed.itemType,
        uomId: seed.uomId,
        isActive: true,
      },
    });
  }

  const divisionSeeds = [
    { code: 'FNB', name: 'Food & Beverage', description: 'Retail food and beverage division' },
    { code: 'INSTI', name: 'Institution', description: 'Institution and B2B division' },
  ];
  for (const seed of divisionSeeds) {
    await prisma.masterDataDivision.upsert({
      where: { code: seed.code },
      update: {
        name: seed.name,
        description: seed.description,
        isActive: true,
        deletedAt: null,
        deletedBy: null,
      },
      create: {
        ...seed,
        isActive: true,
      },
    });
  }

  const rootDepartment = await prisma.department.upsert({
    where: { code: 'root' },
    update: {
      name: 'Headquarters',
      description: 'Global HQ',
      parentId: null,
      deletedAt: null,
      deletedBy: null,
    },
    create: {
      code: 'root',
      name: 'Headquarters',
      description: 'Global HQ',
    },
    select: { id: true },
  });

  const engineeringDepartment = await prisma.department.upsert({
    where: { code: 'eng' },
    update: {
      name: 'Engineering',
      description: 'Engineering and Technology',
      parentId: rootDepartment.id,
      deletedAt: null,
      deletedBy: null,
    },
    create: {
      code: 'eng',
      name: 'Engineering',
      description: 'Engineering and Technology',
      parentId: rootDepartment.id,
    },
    select: { id: true },
  });

  const hrDepartment = await prisma.department.upsert({
    where: { code: 'hr' },
    update: {
      name: 'Human Resources',
      description: 'Human Resources',
      parentId: rootDepartment.id,
      deletedAt: null,
      deletedBy: null,
    },
    create: {
      code: 'hr',
      name: 'Human Resources',
      description: 'Human Resources',
      parentId: rootDepartment.id,
    },
    select: { id: true },
  });

  const users = [
    {
      email: 'admin@example.com',
      username: 'admin',
      fullName: 'System Administrator',
      role: 'admin',
      departmentId: rootDepartment.id,
      warehouseName: 'Medan',
    },
    {
      email: 'administrator@example.com',
      username: 'administrator',
      fullName: 'Administrator',
      role: 'admin',
      departmentId: rootDepartment.id,
      warehouseName: 'Medan',
    },
    {
      email: 'manager.eng@example.com',
      username: 'manager_eng',
      fullName: 'Engineering Manager',
      role: 'manager',
      departmentId: engineeringDepartment.id,
      warehouseName: 'Medan',
    },
    {
      email: 'staff.eng@example.com',
      username: 'staff_eng',
      fullName: 'Engineering Staff',
      role: 'user',
      departmentId: engineeringDepartment.id,
      warehouseName: 'Medan',
    },
    {
      email: 'staff.hr@example.com',
      username: 'staff_hr',
      fullName: 'HR Staff',
      role: 'user',
      departmentId: hrDepartment.id,
      warehouseName: 'Medan',
    },
  ] as const;

  const roleByName = new Map([
    ['admin', adminRole.id],
    ['manager', managerRole.id],
    ['user', userRole.id],
  ]);

  for (const userSeed of users) {
    const warehouseId = warehouseByName.get(userSeed.warehouseName);
    if (!warehouseId) {
      throw new Error(`Warehouse not found for user ${userSeed.email}`);
    }

    const user = await prisma.user.upsert({
      where: { email: userSeed.email },
      update: {
        username: userSeed.username,
        fullName: userSeed.fullName,
        passwordHash,
        isActive: true,
        warehouseId,
        deletedAt: null,
        deletedBy: null,
      },
      create: {
        email: userSeed.email,
        username: userSeed.username,
        fullName: userSeed.fullName,
        passwordHash,
        isActive: true,
        warehouseId,
      },
      select: { id: true, email: true },
    });

    const roleId = roleByName.get(userSeed.role);
    if (!roleId) {
      throw new Error(`Role not found for user ${userSeed.email}`);
    }

    await prisma.userRole.upsert({
      where: { userId_roleId: { userId: user.id, roleId } },
      update: { deletedAt: null, deletedBy: null },
      create: { userId: user.id, roleId },
    });

    await prisma.userDepartment.upsert({
      where: { userId_departmentId: { userId: user.id, departmentId: userSeed.departmentId } },
      update: { deletedAt: null, deletedBy: null },
      create: { userId: user.id, departmentId: userSeed.departmentId },
    });
  }

  const managerDashboardUser = await prisma.user.findUnique({
    where: { email: 'manager.eng@example.com' },
    select: { id: true },
  });
  if (!managerDashboardUser) {
    throw new Error('Manager seed user not found');
  }

  await prisma.managerInsight.deleteMany({});
  await prisma.managerRisk.deleteMany({});
  await prisma.managerDataFreshness.deleteMany({});

  const now = new Date();
  const hourAgo = (hours: number) => new Date(now.getTime() - hours * 60 * 60 * 1000);
  const dayAgo = (days: number, hour = 9, minute = 0) => {
    const d = new Date(now);
    d.setUTCDate(d.getUTCDate() - days);
    d.setUTCHours(hour, minute, 0, 0);
    return d;
  };

  await prisma.managerInsight.createMany({
    data: [
      { title: 'Backlog outbound wave-2', question: 'Kenapa backlog outbound naik pagi ini?', status: 'accepted', managerUserId: managerDashboardUser.id, insightCreatedAt: dayAgo(0, 8, 0), decisionAt: dayAgo(0, 8, 11), decisionNote: 'Tambah picker shift pagi' },
      { title: 'Stockout risk fast moving', question: 'SKU mana paling berisiko stockout?', status: 'accepted', managerUserId: managerDashboardUser.id, insightCreatedAt: dayAgo(0, 9, 10), decisionAt: dayAgo(0, 9, 22), decisionNote: 'Prioritaskan replenishment' },
      { title: 'Receiving slowdown', question: 'Apa penyebab receiving lambat?', status: 'accepted', managerUserId: managerDashboardUser.id, insightCreatedAt: dayAgo(0, 10, 5), decisionAt: dayAgo(0, 10, 16), decisionNote: 'Buka lane tambahan' },
      { title: 'Carrier SLA risk', question: 'Pengiriman mana paling riskan terlambat?', status: 'accepted', managerUserId: managerDashboardUser.id, insightCreatedAt: dayAgo(1, 8, 0), decisionAt: dayAgo(1, 8, 14), decisionNote: 'Re-route order prioritas' },
      { title: 'Picking congestion', question: 'Area picking mana paling padat?', status: 'rejected', managerUserId: managerDashboardUser.id, insightCreatedAt: dayAgo(1, 11, 0), decisionAt: dayAgo(1, 11, 19), decisionNote: 'Data kurang lengkap' },
      { title: 'Cycle count anomaly', question: 'Apakah ada selisih inventori kritis?', status: 'accepted', managerUserId: managerDashboardUser.id, insightCreatedAt: dayAgo(2, 8, 30), decisionAt: dayAgo(2, 8, 45), decisionNote: 'Audit SKU prioritas' },
      { title: 'Inbound dock overload', question: 'Dock mana berpotensi overload?', status: 'accepted', managerUserId: managerDashboardUser.id, insightCreatedAt: dayAgo(3, 9, 0), decisionAt: dayAgo(3, 9, 18), decisionNote: 'Alihkan slot unloading' },
      { title: 'Late dispatch cluster', question: 'Klaster keterlambatan dispatch terbesar?', status: 'accepted', managerUserId: managerDashboardUser.id, insightCreatedAt: dayAgo(4, 10, 0), decisionAt: dayAgo(4, 10, 14), decisionNote: 'Escalate ke supervisor' },
      { title: 'Data quality exception', question: 'Apakah exception master data memengaruhi SLA?', status: 'rejected', managerUserId: managerDashboardUser.id, insightCreatedAt: dayAgo(5, 13, 0), decisionAt: dayAgo(5, 13, 21), decisionNote: 'Perlu validasi manual' },
      { title: 'Replenishment urgency', question: 'Prioritas replenishment hari ini?', status: 'accepted', managerUserId: managerDashboardUser.id, insightCreatedAt: dayAgo(6, 7, 45), decisionAt: dayAgo(6, 7, 59), decisionNote: 'Resequence tasks' },
      { title: 'Last week baseline A', question: 'Baseline pekan lalu A', status: 'accepted', managerUserId: managerDashboardUser.id, insightCreatedAt: dayAgo(7, 8, 0), decisionAt: dayAgo(7, 8, 20), decisionNote: 'Baseline' },
      { title: 'Last week baseline B', question: 'Baseline pekan lalu B', status: 'accepted', managerUserId: managerDashboardUser.id, insightCreatedAt: dayAgo(7, 9, 0), decisionAt: dayAgo(7, 9, 25), decisionNote: 'Baseline' },
      { title: 'Last week baseline C', question: 'Baseline pekan lalu C', status: 'rejected', managerUserId: managerDashboardUser.id, insightCreatedAt: dayAgo(8, 10, 0), decisionAt: dayAgo(8, 10, 32), decisionNote: 'Baseline' },
      { title: 'Last week baseline D', question: 'Baseline pekan lalu D', status: 'accepted', managerUserId: managerDashboardUser.id, insightCreatedAt: dayAgo(9, 11, 0), decisionAt: dayAgo(9, 11, 29), decisionNote: 'Baseline' },
    ],
  });

  await prisma.managerRisk.createMany({
    data: [
      { title: 'Outbound wave-2 backlog', domain: 'outbound', severity: 'critical', status: 'open', managerUserId: managerDashboardUser.id, openedAt: hourAgo(2) },
      { title: 'Stockout SKU AX-44', domain: 'inventory', severity: 'critical', status: 'open', managerUserId: managerDashboardUser.id, openedAt: hourAgo(3) },
      { title: 'Receiving mart stale data', domain: 'inbound', severity: 'critical', status: 'in_progress', managerUserId: managerDashboardUser.id, openedAt: hourAgo(6) },
      { title: 'Late dispatch cluster north', domain: 'delivery', severity: 'critical', status: 'open', managerUserId: managerDashboardUser.id, openedAt: hourAgo(8) },
      { title: 'Cycle count mismatch', domain: 'inventory', severity: 'critical', status: 'in_progress', managerUserId: managerDashboardUser.id, openedAt: hourAgo(12) },
      { title: 'Carrier cut-off breach', domain: 'delivery', severity: 'critical', status: 'open', managerUserId: managerDashboardUser.id, openedAt: hourAgo(18) },
      { title: 'Picking queue saturation', domain: 'outbound', severity: 'critical', status: 'open', managerUserId: managerDashboardUser.id, openedAt: hourAgo(20) },
      { title: 'Resolved dock issue', domain: 'inbound', severity: 'critical', status: 'closed', managerUserId: managerDashboardUser.id, openedAt: dayAgo(1, 7, 0), resolvedAt: dayAgo(0, 1, 0) },
      { title: 'Medium stock variance', domain: 'inventory', severity: 'medium', status: 'open', managerUserId: managerDashboardUser.id, openedAt: hourAgo(4) },
    ],
  });

  await prisma.managerDataFreshness.createMany({
    data: [
      { domain: 'outbound', datasetName: 'outbound_wave_hourly', slaMinutes: 30, lastRefreshAt: hourAgo(0.2) },
      { domain: 'outbound', datasetName: 'picker_queue_live', slaMinutes: 15, lastRefreshAt: hourAgo(0.1) },
      { domain: 'inventory', datasetName: 'stock_position_near_real_time', slaMinutes: 20, lastRefreshAt: hourAgo(0.25) },
      { domain: 'inventory', datasetName: 'cycle_count_variance', slaMinutes: 60, lastRefreshAt: hourAgo(0.5) },
      { domain: 'inbound', datasetName: 'receiving_hourly', slaMinutes: 30, lastRefreshAt: hourAgo(0.3) },
      { domain: 'delivery', datasetName: 'dispatch_sla_hourly', slaMinutes: 30, lastRefreshAt: hourAgo(0.4) },
      { domain: 'delivery', datasetName: 'carrier_delay_prediction', slaMinutes: 45, lastRefreshAt: hourAgo(0.6) },
      { domain: 'quality', datasetName: 'master_data_exceptions', slaMinutes: 120, lastRefreshAt: hourAgo(1.1) },
      { domain: 'inbound', datasetName: 'receiving_mart', slaMinutes: 30, lastRefreshAt: hourAgo(0.9) },
      { domain: 'inventory', datasetName: 'replenishment_priority', slaMinutes: 30, lastRefreshAt: hourAgo(0.2) },
    ],
  });

  const menuSeeds: MenuSeed[] = [
    {
      key: 'dashboard',
      title: 'Dashboard',
      path: null,
      icon: 'LayoutGrid',
      type: 'COLLAPSE',
      parentKey: null,
      sortOrder: 1,
    },
    {
      key: 'dashboard-overview',
      title: 'Overview',
      path: '/app/overview',
      icon: 'LayoutGrid',
      type: 'ITEM',
      parentKey: 'dashboard',
      sortOrder: 1,
    },
    {
      key: 'dashboard-m1',
      title: 'Dashboard M1',
      path: '/app/overview?domain=m1',
      icon: 'BarChart3',
      type: 'ITEM',
      parentKey: 'dashboard',
      sortOrder: 2,
    },
    {
      key: 'dashboard-m',
      title: 'Dashboard M',
      path: '/app/overview?domain=m',
      icon: 'LineChart',
      type: 'ITEM',
      parentKey: 'dashboard',
      sortOrder: 3,
    },
    {
      key: 'dashboard-m2',
      title: 'Finance & Accounting',
      path: '/app/dashboard/finance-accounting',
      icon: 'Wallet',
      type: 'ITEM',
      parentKey: 'dashboard',
      sortOrder: 4,
    },
    {
      key: 'dashboard-so',
      title: 'Sales',
      path: '/app/dashboard/sales',
      icon: 'TrendingUp',
      type: 'ITEM',
      parentKey: 'dashboard',
      sortOrder: 5,
    },
    {
      key: 'dashboard-m2r',
      title: 'Dashboard M2R',
      path: '/app/overview?domain=m2r',
      icon: 'Activity',
      type: 'ITEM',
      parentKey: 'dashboard',
      sortOrder: 6,
    },
    {
      key: 'dashboard-delivery',
      title: 'Delivery',
      path: '/app',
      icon: 'Truck',
      type: 'ITEM',
      parentKey: 'dashboard',
      sortOrder: 7,
    },
    {
      key: 'alerting',
      title: 'Alerting',
      path: null,
      icon: 'BellRing',
      type: 'GROUP',
      parentKey: null,
      sortOrder: 2,
    },
    {
      key: 'alerting-center',
      title: 'Alert Center',
      path: '/app/alerting/center',
      icon: 'BadgeAlert',
      type: 'ITEM',
      parentKey: 'alerting',
      sortOrder: 1,
    },
    {
      key: 'alerting-rules',
      title: 'Alert Rules',
      path: '/app/alerting/rules',
      icon: 'ShieldAlert',
      type: 'ITEM',
      parentKey: 'alerting',
      sortOrder: 2,
    },
    {
      key: 'alerting-templates',
      title: 'Alert Templates',
      path: '/app/alerting/templates',
      icon: 'LayoutTemplate',
      type: 'ITEM',
      parentKey: 'alerting',
      sortOrder: 3,
    },
    {
      key: 'alerting-channels',
      title: 'Notification Channels',
      path: '/app/alerting/channels',
      icon: 'MessageSquareMore',
      type: 'ITEM',
      parentKey: 'alerting',
      sortOrder: 4,
    },
    {
      key: 'alerting-logs',
      title: 'Notification Logs',
      path: '/app/alerting/logs',
      icon: 'History',
      type: 'ITEM',
      parentKey: 'alerting',
      sortOrder: 5,
    },
    {
      key: 'alerting-settings',
      title: 'Settings',
      path: '/app/alerting/settings',
      icon: 'Settings2',
      type: 'ITEM',
      parentKey: 'alerting',
      sortOrder: 6,
    },
    {
      key: 'administrator',
      title: 'Administrator',
      path: null,
      icon: 'Shield',
      type: 'GROUP',
      parentKey: null,
      sortOrder: 3,
    },
    {
      key: 'administrator-users',
      title: 'Users',
      path: '/app/administrator/users',
      icon: 'Users',
      type: 'ITEM',
      parentKey: 'administrator',
      sortOrder: 1,
    },
    {
      key: 'administrator-department',
      title: 'Department',
      path: '/app/administrator/department',
      icon: 'Building',
      type: 'ITEM',
      parentKey: 'administrator',
      sortOrder: 2,
    },
    {
      key: 'administrator-permission',
      title: 'Permission',
      path: '/app/administrator/permission',
      icon: 'Key',
      type: 'ITEM',
      parentKey: 'administrator',
      sortOrder: 3,
    },
    {
      key: 'administrator-menu',
      title: 'Menu',
      path: '/app/administrator/menu',
      icon: 'LayoutGrid',
      type: 'ITEM',
      parentKey: 'administrator',
      sortOrder: 4,
    },
    {
      key: 'administrator-session',
      title: 'Session',
      path: '/app/administrator/session',
      icon: 'Clock',
      type: 'ITEM',
      parentKey: 'administrator',
      sortOrder: 5,
    },
    {
      key: 'administrator-auditlog',
      title: 'Auditlog',
      path: '/app/administrator/auditlog',
      icon: 'FileText',
      type: 'ITEM',
      parentKey: 'administrator',
      sortOrder: 6,
    },
    {
      key: 'administrator-dashboard-manager',
      title: 'Senti AI',
      path: '/app/senti-ai',
      icon: 'LayoutDashboard',
      type: 'ITEM',
      parentKey: 'administrator',
      sortOrder: 7,
    },
    {
      key: 'master-data',
      title: 'Master Data',
      path: null,
      icon: 'Database',
      type: 'GROUP',
      parentKey: null,
      sortOrder: 4,
    },
    {
      key: 'master-data-contact',
      title: 'Contact',
      path: '/app/master/contact',
      icon: 'ContactRound',
      type: 'ITEM',
      parentKey: 'master-data',
      sortOrder: 1,
    },
    {
      key: 'master-data-division',
      title: 'Division',
      path: '/app/master/division',
      icon: 'Building2',
      type: 'ITEM',
      parentKey: 'master-data',
      sortOrder: 2,
    },
    {
      key: 'master-data-item',
      title: 'Item',
      path: '/app/master/item',
      icon: 'Package',
      type: 'ITEM',
      parentKey: 'master-data',
      sortOrder: 4,
    },
    {
      key: 'master-data-province',
      title: 'Province',
      path: '/app/master/province',
      icon: 'Map',
      type: 'ITEM',
      parentKey: 'master-data',
      sortOrder: 5,
    },
    {
      key: 'master-data-city',
      title: 'City',
      path: '/app/master/city',
      icon: 'MapPin',
      type: 'ITEM',
      parentKey: 'master-data',
      sortOrder: 6,
    },
    {
      key: 'master-data-city-sla',
      title: 'City SLA',
      path: '/app/master/city-sla',
      icon: 'Clock',
      type: 'ITEM',
      parentKey: 'master-data',
      sortOrder: 7,
    },
    {
      key: 'master-data-uom',
      title: 'UOM',
      path: '/app/master/uom',
      icon: 'Ruler',
      type: 'ITEM',
      parentKey: 'master-data',
      sortOrder: 8,
    },
    {
      key: 'master-data-warehouse',
      title: 'Warehouse',
      path: '/app/master/warehouse',
      icon: 'Warehouse',
      type: 'ITEM',
      parentKey: 'master-data',
      sortOrder: 9,
    },
    {
      key: 'logistic',
      title: 'Logistic',
      path: null,
      icon: 'Truck',
      type: 'GROUP',
      parentKey: null,
      sortOrder: 5,
    },
    {
      key: 'logistic-inbound',
      title: 'Inbound',
      path: '/app/logistic/inbound',
      icon: 'ArrowDownToLine',
      type: 'ITEM',
      parentKey: 'logistic',
      sortOrder: 1,
    },
    {
      key: 'logistic-outbound',
      title: 'Outbound',
      path: '/app/logistic/outbound',
      icon: 'ArrowRightLeft',
      type: 'ITEM',
      parentKey: 'logistic',
      sortOrder: 2,
    },
    {
      key: 'logistic-report-monitoring-do',
      title: 'Report Monitoring DO',
      path: '/app/logistic/report-monitoring-do',
      icon: 'ClipboardList',
      type: 'ITEM',
      parentKey: 'logistic',
      sortOrder: 3,
    },
    {
      key: 'logistic-report-stock-batch',
      title: 'Report Stock Batch',
      path: '/app/logistic/report-stock-batch',
      icon: 'Boxes',
      type: 'ITEM',
      parentKey: 'logistic',
      sortOrder: 4,
    },
    {
      key: 'logistic-report-stock-mutation',
      title: 'Report Stock Mutation',
      path: '/app/logistic/report-stock-mutation',
      icon: 'Repeat',
      type: 'ITEM',
      parentKey: 'logistic',
      sortOrder: 5,
    },
    {
      key: 'logistic-dashboard-warehouse',
      title: 'Dashboard Warehouse',
      path: '/app/dashboard/warehouse',
      icon: 'Warehouse',
      type: 'ITEM',
      parentKey: 'logistic',
      sortOrder: 6,
    },
  ];

  const menuByKey = new Map<string, number>();
  for (const seed of menuSeeds.filter((m) => m.parentKey === null)) {
    const menu = await prisma.menu.upsert({
      where: { key: seed.key },
      update: {
        title: seed.title,
        path: seed.path,
        icon: seed.icon,
        type: seed.type,
        parentId: null,
        sortOrder: seed.sortOrder,
        isVisible: seed.isVisible ?? true,
        isActive: seed.isActive ?? true,
        permissionName: seed.permissionName ?? null,
        deletedAt: null,
        deletedBy: null,
      },
      create: {
        key: seed.key,
        title: seed.title,
        path: seed.path,
        icon: seed.icon,
        type: seed.type,
        parentId: null,
        sortOrder: seed.sortOrder,
        isVisible: seed.isVisible ?? true,
        isActive: seed.isActive ?? true,
        permissionName: seed.permissionName ?? null,
      },
      select: { id: true },
    });
    menuByKey.set(seed.key, menu.id);
  }

  for (const seed of menuSeeds.filter((m) => m.parentKey !== null)) {
    const parentId = menuByKey.get(seed.parentKey as string);
    if (!parentId) {
      throw new Error(`Parent menu not found for ${seed.key}`);
    }
    const menu = await prisma.menu.upsert({
      where: { key: seed.key },
      update: {
        title: seed.title,
        path: seed.path,
        icon: seed.icon,
        type: seed.type,
        parentId,
        sortOrder: seed.sortOrder,
        isVisible: seed.isVisible ?? true,
        isActive: seed.isActive ?? true,
        permissionName: seed.permissionName ?? null,
        deletedAt: null,
        deletedBy: null,
      },
      create: {
        key: seed.key,
        title: seed.title,
        path: seed.path,
        icon: seed.icon,
        type: seed.type,
        parentId,
        sortOrder: seed.sortOrder,
        isVisible: seed.isVisible ?? true,
        isActive: seed.isActive ?? true,
        permissionName: seed.permissionName ?? null,
      },
      select: { id: true },
    });
    menuByKey.set(seed.key, menu.id);
  }

  const assignMenuToRole = async (roleId: number, menuId: number) => {
    await prisma.roleMenu.upsert({
      where: { roleId_menuId: { roleId, menuId } },
      update: { canView: true, deletedAt: null, deletedBy: null },
      create: { roleId, menuId, canView: true },
    });
  };

  for (const menuId of menuByKey.values()) {
    await assignMenuToRole(adminRole.id, menuId);
  }

  const operationalMenuKeys = [
    'dashboard',
    'dashboard-overview',
    'dashboard-m1',
    'dashboard-m',
    'dashboard-m2',
    'dashboard-so',
    'dashboard-m2r',
    'dashboard-delivery',
    'alerting',
    'alerting-center',
    'alerting-rules',
    'alerting-templates',
    'alerting-channels',
    'alerting-logs',
    'alerting-settings',
    'master-data',
    'master-data-contact',
    'master-data-division',
    'master-data-item',
    'master-data-item-stock',
    'master-data-province',
    'master-data-city',
    'master-data-city-sla',
    'master-data-uom',
    'master-data-warehouse',
    'logistic',
    'logistic-inbound',
    'logistic-outbound',
    'logistic-report-monitoring-do',
    'logistic-report-stock-batch',
    'logistic-report-stock-mutation',
  ] as const;

  for (const key of operationalMenuKeys) {
    const menuId = menuByKey.get(key);
    if (!menuId) {
      continue;
    }
    await assignMenuToRole(managerRole.id, menuId);
    await assignMenuToRole(userRole.id, menuId);
  }

  const managerUser = await prisma.user.findUnique({
    where: { email: 'manager.eng@example.com' },
    select: { id: true },
  });
  if (!managerUser) {
    throw new Error('Manager seed user not found');
  }

  const insightSeeds = [
    {
      title: 'Triage backlog outbound wave 2',
      question: 'Kenapa antrean outbound wave 2 naik pagi ini?',
      status: 'accepted',
      insightCreatedAt: new Date('2026-03-11T08:00:00Z'),
      decisionAt: new Date('2026-03-11T08:09:00Z'),
      decisionNote: 'Tambah picker 1 shift.',
    },
    {
      title: 'Replenishment aisle B',
      question: 'Apakah replenishment aisle B perlu dipindah ke malam?',
      status: 'accepted',
      insightCreatedAt: new Date('2026-03-11T08:25:00Z'),
      decisionAt: new Date('2026-03-11T08:39:00Z'),
      decisionNote: 'Geser ke shift malam.',
    },
    {
      title: 'Receiving dock risk',
      question: 'Apakah receiving dock perlu buka lane tambahan?',
      status: 'accepted',
      insightCreatedAt: new Date('2026-03-11T09:10:00Z'),
      decisionAt: new Date('2026-03-11T09:19:00Z'),
      decisionNote: 'Buka lane 3 untuk prioritas ASN.',
    },
    {
      title: 'Stockout fast moving',
      question: 'Apa penyebab risiko stockout SKU fast moving?',
      status: 'accepted',
      insightCreatedAt: new Date('2026-03-11T09:40:00Z'),
      decisionAt: new Date('2026-03-11T09:53:00Z'),
      decisionNote: 'Prioritaskan cycle count dan top-up.',
    },
    {
      title: 'Picker productivity gap',
      question: 'Kenapa produktivitas picker shift pagi turun?',
      status: 'accepted',
      insightCreatedAt: new Date('2026-03-11T10:05:00Z'),
      decisionAt: new Date('2026-03-11T10:14:00Z'),
      decisionNote: 'Rotasi picker senior ke zone high volume.',
    },
    {
      title: 'Carrier handoff delay',
      question: 'Apakah delay handoff carrier perlu escalation?',
      status: 'accepted',
      insightCreatedAt: new Date('2026-03-11T10:30:00Z'),
      decisionAt: new Date('2026-03-11T10:42:00Z'),
      decisionNote: 'Escalate ke vendor transport.',
    },
    {
      title: 'Data quality receiving',
      question: 'Apakah mismatch ASN memerlukan hold sementara?',
      status: 'rejected',
      insightCreatedAt: new Date('2026-03-11T11:00:00Z'),
      decisionAt: new Date('2026-03-11T11:11:00Z'),
      decisionNote: 'Belum perlu, cukup sampling manual.',
    },
    {
      title: 'Wave planning rebalance',
      question: 'Perlukah rebalance wave planning siang ini?',
      status: 'pending',
      insightCreatedAt: new Date('2026-03-11T11:20:00Z'),
      decisionAt: null,
      decisionNote: null,
    },
    {
      title: 'Slotting review fast pick',
      question: 'Apakah slotting fast pick perlu revisi?',
      status: 'accepted',
      insightCreatedAt: new Date('2026-03-10T08:00:00Z'),
      decisionAt: new Date('2026-03-10T08:19:00Z'),
      decisionNote: 'Re-slot 12 SKU prioritas.',
    },
    {
      title: 'Labor sharing inbound outbound',
      question: 'Bisakah labor sharing antar shift menurunkan backlog?',
      status: 'accepted',
      insightCreatedAt: new Date('2026-03-09T08:15:00Z'),
      decisionAt: new Date('2026-03-09T08:33:00Z'),
      decisionNote: 'Setujui sharing 2 operator.',
    },
    {
      title: 'Cycle count anomaly',
      question: 'Apakah anomali cycle count perlu recount penuh?',
      status: 'rejected',
      insightCreatedAt: new Date('2026-03-08T08:40:00Z'),
      decisionAt: new Date('2026-03-08T09:00:00Z'),
      decisionNote: 'Cukup recount sampel.',
    },
    {
      title: 'Cross-docking candidate',
      question: 'SKU mana yang cocok untuk cross-docking?',
      status: 'accepted',
      insightCreatedAt: new Date('2026-03-07T07:55:00Z'),
      decisionAt: new Date('2026-03-07T08:14:00Z'),
      decisionNote: 'Aktifkan 8 SKU kandidat.',
    },
    {
      title: 'Late ASN supplier',
      question: 'Supplier mana paling sering telat ASN?',
      status: 'accepted',
      insightCreatedAt: new Date('2026-03-06T08:05:00Z'),
      decisionAt: new Date('2026-03-06T08:22:00Z'),
      decisionNote: 'Kirim corrective action request.',
    },
    {
      title: 'Backorder prioritization',
      question: 'Perlu ubah aturan prioritas backorder?',
      status: 'accepted',
      insightCreatedAt: new Date('2026-03-05T08:50:00Z'),
      decisionAt: new Date('2026-03-05T09:08:00Z'),
      decisionNote: 'Prioritaskan VIP customer.',
    },
    {
      title: 'Packing station overtime',
      question: 'Apakah packing station perlu overtime?',
      status: 'rejected',
      insightCreatedAt: new Date('2026-03-04T09:20:00Z'),
      decisionAt: new Date('2026-03-04T09:41:00Z'),
      decisionNote: 'Tidak perlu, cukup ubah sequencing.',
    },
  ] as const;

  for (const insight of insightSeeds) {
    await prisma.managerInsight.upsert({
      where: {
        managerUserId_title_insightCreatedAt: {
          managerUserId: managerDashboardUser.id,
          title: insight.title,
          insightCreatedAt: insight.insightCreatedAt,
        },
      },
      update: {
        question: insight.question,
        status: insight.status,
        decisionAt: insight.decisionAt,
        decisionNote: insight.decisionNote,
      },
      create: {
        managerUserId: managerDashboardUser.id,
        title: insight.title,
        question: insight.question,
        status: insight.status,
        insightCreatedAt: insight.insightCreatedAt,
        decisionAt: insight.decisionAt,
        decisionNote: insight.decisionNote,
      },
    });
  }

  const riskSeeds = [
    ['Predictive bottleneck outbound', 'outbound', 'critical', 'open', '2026-03-11T06:30:00Z', null],
    ['Receiving mart freshness issue', 'inbound', 'critical', 'in_progress', '2026-03-11T07:10:00Z', null],
    ['Fast moving stockout risk', 'inventory', 'critical', 'open', '2026-03-11T07:25:00Z', null],
    ['Wave picking queue overflow', 'outbound', 'critical', 'open', '2026-03-11T08:00:00Z', null],
    ['Carrier late pickup cluster', 'delivery', 'critical', 'in_progress', '2026-03-11T08:20:00Z', null],
    ['Cycle count variance hotspot', 'inventory', 'critical', 'open', '2026-03-11T09:00:00Z', null],
    ['Supplier ASN mismatch', 'inbound', 'critical', 'open', '2026-03-11T09:25:00Z', null],
    ['Packing material low stock', 'inventory', 'warning', 'open', '2026-03-11T09:40:00Z', null],
    ['Forklift battery maintenance', 'warehouse', 'critical', 'resolved', '2026-03-10T06:00:00Z', '2026-03-10T09:00:00Z'],
  ] as const;

  for (const [title, domain, severity, status, openedAt, resolvedAt] of riskSeeds) {
    await prisma.managerRisk.upsert({
      where: {
        title_openedAt: {
          title,
          openedAt: new Date(openedAt),
        },
      },
      update: {
        domain,
        severity,
        status,
        resolvedAt: resolvedAt ? new Date(resolvedAt) : null,
        managerUserId: managerDashboardUser.id,
      },
      create: {
        title,
        domain,
        severity,
        status,
        openedAt: new Date(openedAt),
        resolvedAt: resolvedAt ? new Date(resolvedAt) : null,
        managerUserId: managerDashboardUser.id,
      },
    });
  }

  const freshnessSeeds = [
    ['outbound', 'outbound_wave_dashboard', 30, '2026-03-11T11:46:00Z'],
    ['inventory', 'stockout_risk_model', 60, '2026-03-11T11:18:00Z'],
    ['inventory', 'replenishment_urgency', 45, '2026-03-11T11:32:00Z'],
    ['inbound', 'receiving_control_tower', 30, '2026-03-11T11:40:00Z'],
    ['delivery', 'carrier_handoff_monitor', 30, '2026-03-11T11:44:00Z'],
    ['warehouse', 'labor_productivity_shift', 60, '2026-03-11T11:00:00Z'],
    ['quality', 'asn_data_quality_check', 20, '2026-03-11T11:47:00Z'],
    ['finance', 'cost_to_serve_snapshot', 180, '2026-03-11T10:35:00Z'],
    ['procurement', 'supplier_fill_rate', 120, '2026-03-11T11:15:00Z'],
    ['inbound', 'receiving_mart', 30, '2026-03-11T10:59:00Z'],
    ['outbound', 'pick_queue_monitor', 15, '2026-03-11T11:48:00Z'],
    ['inventory', 'cycle_count_exception', 90, '2026-03-11T10:00:00Z'],
    ['delivery', 'delivery_sla_risk', 45, '2026-03-11T11:30:00Z'],
    ['warehouse', 'dock_utilization', 30, '2026-03-11T11:43:00Z'],
    ['sales', 'priority_order_feed', 20, '2026-03-11T11:46:00Z'],
    ['returns', 'reverse_logistics_queue', 120, '2026-03-11T09:30:00Z'],
    ['planning', 'wave_plan_snapshot', 60, '2026-03-11T11:10:00Z'],
    ['inventory', 'bin_capacity_heatmap', 45, '2026-03-11T11:26:00Z'],
    ['quality', 'damage_claim_tracker', 60, '2026-03-11T11:05:00Z'],
    ['procurement', 'supplier_eta_monitor', 30, '2026-03-11T11:38:00Z'],
    ['delivery', 'route_exception_feed', 30, '2026-03-11T11:41:00Z'],
    ['warehouse', 'equipment_health', 120, '2026-03-11T10:20:00Z'],
    ['outbound', 'order_backlog_ageing', 30, '2026-03-11T11:20:00Z'],
    ['inbound', 'putaway_capacity_tracker', 60, '2026-03-11T11:12:00Z'],
    ['inventory', 'location_accuracy_score', 30, '2026-03-11T10:55:00Z'],
    ['planning', 'labor_rebalance_suggester', 30, '2026-03-11T11:49:00Z'],
    ['returns', 'return_putaway_sla', 90, '2026-03-11T11:14:00Z'],
    ['quality', 'temperature_compliance_feed', 30, '2026-03-11T11:39:00Z'],
  ] as const;

  for (const [domain, datasetName, slaMinutes, lastRefreshAt] of freshnessSeeds) {
    await prisma.managerDataFreshness.upsert({
      where: {
        domain_datasetName: {
          domain,
          datasetName,
        },
      },
      update: {
        slaMinutes,
        lastRefreshAt: new Date(lastRefreshAt),
      },
      create: {
        domain,
        datasetName,
        slaMinutes,
        lastRefreshAt: new Date(lastRefreshAt),
      },
    });
  }

  const adminUser = await prisma.user.findUnique({
    where: { email: 'admin@example.com' },
    select: { id: true },
  });
  if (adminUser) {
    const existingAudit = await prisma.auditLog.findFirst({
      where: {
        userId: adminUser.id,
        action: 'USER_LOGIN',
        entityType: 'AUTH',
        entityId: String(adminUser.id),
        deletedAt: null,
      },
      select: { id: true },
    });

    if (!existingAudit) {
      await prisma.auditLog.create({
        data: {
          userId: adminUser.id,
          action: 'USER_LOGIN',
          entityType: 'AUTH',
          entityId: String(adminUser.id),
          ipAddress: '127.0.0.1',
          userAgent: 'Mozilla/5.0 (Seed)',
          createdBy: adminUser.id,
          updatedBy: adminUser.id,
        },
      });
    }
  }

  // ERP menus seeded separately via `npm run db:seed:erp`. See top of file.

  console.log('Seeding completed.');
  console.log('-------------------------------------------');
  console.log('Admin:   admin@example.com');
  console.log('Manager: manager.eng@example.com');
  console.log('Staff:   staff.eng@example.com');
  console.log('Staff:   staff.hr@example.com');
  console.log('Password for all users: Password123!');
  console.log('-------------------------------------------');
}

main()
  .catch((error) => {
    console.error('Seeding error:', error);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });

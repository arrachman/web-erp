export const PERMISSIONS = [
  { name: 'user:create', module: 'user', action: 'create', description: 'Create users' },
  { name: 'user:read', module: 'user', action: 'read', description: 'Read users' },
  { name: 'user:update', module: 'user', action: 'update', description: 'Update users' },
  { name: 'user:delete', module: 'user', action: 'delete', description: 'Delete users' },
  { name: 'role:manage', module: 'role', action: 'manage', description: 'Manage roles' },
  { name: 'department:manage', module: 'department', action: 'manage', description: 'Manage departments' },
  { name: 'audit:view', module: 'audit', action: 'view', description: 'View audit logs' },
];

export const PROVINCE_SEEDS = [
  { isoCode: 'ID-SU', name: 'Sumatera Utara' },
  { isoCode: 'ID-KS', name: 'Kalimantan Selatan' },
  { isoCode: 'ID-SS', name: 'Sumatera Selatan' },
  { isoCode: 'ID-SN', name: 'Sulawesi Selatan' },
];

export const CITY_SEEDS = [
  { provinceIso: 'ID-SU', name: 'Medan', postalCode: '20111' },
  { provinceIso: 'ID-KS', name: 'Banjarmasin', postalCode: '70111' },
  { provinceIso: 'ID-SS', name: 'Palembang', postalCode: '30111' },
  { provinceIso: 'ID-SN', name: 'Makassar', postalCode: '90111' },
];

export const WAREHOUSE_SEEDS = [
  { name: 'Medan', cityName: 'Medan', locationName: 'Gudang Medan' },
  { name: 'Banjarmasin', cityName: 'Banjarmasin', locationName: 'Gudang Banjarmasin' },
  { name: 'Palembang', cityName: 'Palembang', locationName: 'Gudang Palembang' },
  { name: 'Makasar', cityName: 'Makassar', locationName: 'Gudang Makasar' },
];

export const UOM_SEEDS = [
  { code: 'PCS', name: 'Pieces', type: 'unit' },
  { code: 'KG', name: 'Kilogram', type: 'weight' },
];

export const ITEM_SEEDS = [
  { code: 'ITEM-001', name: 'Gula Pasir Premium 1KG', category: 'RAW_MATERIAL', itemType: 'RAW', uomCode: 'KG' },
  { code: 'ITEM-002', name: 'Kopi Bubuk 250GR', category: 'FINISHED_GOOD', itemType: 'FG', uomCode: 'PCS' },
];

export const CONTACT_SEEDS = [
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

export const DIVISION_SEEDS = [
  { code: 'FNB', name: 'Food & Beverage', description: 'Retail food and beverage division' },
  { code: 'INSTI', name: 'Institution', description: 'Institution and B2B division' },
];

export const OPERATIONAL_MENU_KEYS = [
  'dashboard', 'dashboard-overview', 'dashboard-m1', 'dashboard-m', 'dashboard-m2',
  'dashboard-so', 'dashboard-m2r', 'dashboard-delivery',
  'alerting', 'alerting-center', 'alerting-rules', 'alerting-templates',
  'alerting-channels', 'alerting-logs', 'alerting-settings',
  'master-data', 'master-data-contact', 'master-data-division', 'master-data-item',
  'master-data-item-stock', 'master-data-province', 'master-data-city',
  'master-data-city-sla', 'master-data-uom', 'master-data-warehouse',
  'logistic', 'logistic-inbound', 'logistic-outbound',
  'logistic-report-monitoring-do', 'logistic-report-stock-batch',
  'logistic-report-stock-mutation',
] as const;

export const INSIGHT_SEEDS = [
  { title: 'Triage backlog outbound wave 2', question: 'Kenapa antrean outbound wave 2 naik pagi ini?', status: 'accepted', insightCreatedAt: new Date('2026-03-11T08:00:00Z'), decisionAt: new Date('2026-03-11T08:09:00Z'), decisionNote: 'Tambah picker 1 shift.' },
  { title: 'Replenishment aisle B', question: 'Apakah replenishment aisle B perlu dipindah ke malam?', status: 'accepted', insightCreatedAt: new Date('2026-03-11T08:25:00Z'), decisionAt: new Date('2026-03-11T08:39:00Z'), decisionNote: 'Geser ke shift malam.' },
  { title: 'Receiving dock risk', question: 'Apakah receiving dock perlu buka lane tambahan?', status: 'accepted', insightCreatedAt: new Date('2026-03-11T09:10:00Z'), decisionAt: new Date('2026-03-11T09:19:00Z'), decisionNote: 'Buka lane 3 untuk prioritas ASN.' },
  { title: 'Stockout fast moving', question: 'Apa penyebab risiko stockout SKU fast moving?', status: 'accepted', insightCreatedAt: new Date('2026-03-11T09:40:00Z'), decisionAt: new Date('2026-03-11T09:53:00Z'), decisionNote: 'Prioritaskan cycle count dan top-up.' },
  { title: 'Picker productivity gap', question: 'Kenapa produktivitas picker shift pagi turun?', status: 'accepted', insightCreatedAt: new Date('2026-03-11T10:05:00Z'), decisionAt: new Date('2026-03-11T10:14:00Z'), decisionNote: 'Rotasi picker senior ke zone high volume.' },
  { title: 'Carrier handoff delay', question: 'Apakah delay handoff carrier perlu escalation?', status: 'accepted', insightCreatedAt: new Date('2026-03-11T10:30:00Z'), decisionAt: new Date('2026-03-11T10:42:00Z'), decisionNote: 'Escalate ke vendor transport.' },
  { title: 'Data quality receiving', question: 'Apakah mismatch ASN memerlukan hold sementara?', status: 'rejected', insightCreatedAt: new Date('2026-03-11T11:00:00Z'), decisionAt: new Date('2026-03-11T11:11:00Z'), decisionNote: 'Belum perlu, cukup sampling manual.' },
  { title: 'Wave planning rebalance', question: 'Perlukah rebalance wave planning siang ini?', status: 'pending', insightCreatedAt: new Date('2026-03-11T11:20:00Z'), decisionAt: null, decisionNote: null },
  { title: 'Slotting review fast pick', question: 'Apakah slotting fast pick perlu revisi?', status: 'accepted', insightCreatedAt: new Date('2026-03-10T08:00:00Z'), decisionAt: new Date('2026-03-10T08:19:00Z'), decisionNote: 'Re-slot 12 SKU prioritas.' },
  { title: 'Labor sharing inbound outbound', question: 'Bisakah labor sharing antar shift menurunkan backlog?', status: 'accepted', insightCreatedAt: new Date('2026-03-09T08:15:00Z'), decisionAt: new Date('2026-03-09T08:33:00Z'), decisionNote: 'Setujui sharing 2 operator.' },
  { title: 'Cycle count anomaly', question: 'Apakah anomali cycle count perlu recount penuh?', status: 'rejected', insightCreatedAt: new Date('2026-03-08T08:40:00Z'), decisionAt: new Date('2026-03-08T09:00:00Z'), decisionNote: 'Cukup recount sampel.' },
  { title: 'Cross-docking candidate', question: 'SKU mana yang cocok untuk cross-docking?', status: 'accepted', insightCreatedAt: new Date('2026-03-07T07:55:00Z'), decisionAt: new Date('2026-03-07T08:14:00Z'), decisionNote: 'Aktifkan 8 SKU kandidat.' },
  { title: 'Late ASN supplier', question: 'Supplier mana paling sering telat ASN?', status: 'accepted', insightCreatedAt: new Date('2026-03-06T08:05:00Z'), decisionAt: new Date('2026-03-06T08:22:00Z'), decisionNote: 'Kirim corrective action request.' },
  { title: 'Backorder prioritization', question: 'Perlu ubah aturan prioritas backorder?', status: 'accepted', insightCreatedAt: new Date('2026-03-05T08:50:00Z'), decisionAt: new Date('2026-03-05T09:08:00Z'), decisionNote: 'Prioritaskan VIP customer.' },
  { title: 'Packing station overtime', question: 'Apakah packing station perlu overtime?', status: 'rejected', insightCreatedAt: new Date('2026-03-04T09:20:00Z'), decisionAt: new Date('2026-03-04T09:41:00Z'), decisionNote: 'Tidak perlu, cukup ubah sequencing.' },
] as const;

export const RISK_SEEDS = [
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

export const FRESHNESS_SEEDS = [
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

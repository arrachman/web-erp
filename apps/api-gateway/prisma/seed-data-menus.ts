export type MenuSeed = {
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

export const MENU_SEEDS: MenuSeed[] = [
  { key: 'dashboard', title: 'Dashboard', path: null, icon: 'LayoutGrid', type: 'COLLAPSE', parentKey: null, sortOrder: 1 },
  { key: 'dashboard-overview', title: 'Overview', path: '/app/overview', icon: 'LayoutGrid', type: 'ITEM', parentKey: 'dashboard', sortOrder: 1 },
  { key: 'dashboard-m1', title: 'Dashboard M1', path: '/app/overview?domain=m1', icon: 'BarChart3', type: 'ITEM', parentKey: 'dashboard', sortOrder: 2 },
  { key: 'dashboard-m', title: 'Dashboard M', path: '/app/overview?domain=m', icon: 'LineChart', type: 'ITEM', parentKey: 'dashboard', sortOrder: 3 },
  { key: 'dashboard-m2', title: 'Finance & Accounting', path: '/app/dashboard/finance-accounting', icon: 'Wallet', type: 'ITEM', parentKey: 'dashboard', sortOrder: 4 },
  { key: 'dashboard-so', title: 'Sales', path: '/app/dashboard/sales', icon: 'TrendingUp', type: 'ITEM', parentKey: 'dashboard', sortOrder: 5 },
  { key: 'dashboard-m2r', title: 'Dashboard M2R', path: '/app/overview?domain=m2r', icon: 'Activity', type: 'ITEM', parentKey: 'dashboard', sortOrder: 6 },
  { key: 'dashboard-delivery', title: 'Delivery', path: '/app', icon: 'Truck', type: 'ITEM', parentKey: 'dashboard', sortOrder: 7 },
  { key: 'alerting', title: 'Alerting', path: null, icon: 'BellRing', type: 'GROUP', parentKey: null, sortOrder: 2 },
  { key: 'alerting-center', title: 'Alert Center', path: '/app/alerting/center', icon: 'BadgeAlert', type: 'ITEM', parentKey: 'alerting', sortOrder: 1 },
  { key: 'alerting-rules', title: 'Alert Rules', path: '/app/alerting/rules', icon: 'ShieldAlert', type: 'ITEM', parentKey: 'alerting', sortOrder: 2 },
  { key: 'alerting-templates', title: 'Alert Templates', path: '/app/alerting/templates', icon: 'LayoutTemplate', type: 'ITEM', parentKey: 'alerting', sortOrder: 3 },
  { key: 'alerting-channels', title: 'Notification Channels', path: '/app/alerting/channels', icon: 'MessageSquareMore', type: 'ITEM', parentKey: 'alerting', sortOrder: 4 },
  { key: 'alerting-logs', title: 'Notification Logs', path: '/app/alerting/logs', icon: 'History', type: 'ITEM', parentKey: 'alerting', sortOrder: 5 },
  { key: 'alerting-settings', title: 'Settings', path: '/app/alerting/settings', icon: 'Settings2', type: 'ITEM', parentKey: 'alerting', sortOrder: 6 },
  { key: 'administrator', title: 'Administrator', path: null, icon: 'Shield', type: 'GROUP', parentKey: null, sortOrder: 3 },
  { key: 'administrator-users', title: 'Users', path: '/app/administrator/users', icon: 'Users', type: 'ITEM', parentKey: 'administrator', sortOrder: 1 },
  { key: 'administrator-department', title: 'Department', path: '/app/administrator/department', icon: 'Building', type: 'ITEM', parentKey: 'administrator', sortOrder: 2 },
  { key: 'administrator-permission', title: 'Permission', path: '/app/administrator/permission', icon: 'Key', type: 'ITEM', parentKey: 'administrator', sortOrder: 3 },
  { key: 'administrator-menu', title: 'Menu', path: '/app/administrator/menu', icon: 'LayoutGrid', type: 'ITEM', parentKey: 'administrator', sortOrder: 4 },
  { key: 'administrator-session', title: 'Session', path: '/app/administrator/session', icon: 'Clock', type: 'ITEM', parentKey: 'administrator', sortOrder: 5 },
  { key: 'administrator-auditlog', title: 'Auditlog', path: '/app/administrator/auditlog', icon: 'FileText', type: 'ITEM', parentKey: 'administrator', sortOrder: 6 },
  { key: 'administrator-dashboard-manager', title: 'Senti AI', path: '/app/senti-ai', icon: 'LayoutDashboard', type: 'ITEM', parentKey: 'administrator', sortOrder: 7 },
  { key: 'master-data', title: 'Master Data', path: null, icon: 'Database', type: 'GROUP', parentKey: null, sortOrder: 4 },
  { key: 'master-data-contact', title: 'Contact', path: '/app/master/contact', icon: 'ContactRound', type: 'ITEM', parentKey: 'master-data', sortOrder: 1 },
  { key: 'master-data-division', title: 'Division', path: '/app/master/division', icon: 'Building2', type: 'ITEM', parentKey: 'master-data', sortOrder: 2 },
  { key: 'master-data-item', title: 'Item', path: '/app/master/item', icon: 'Package', type: 'ITEM', parentKey: 'master-data', sortOrder: 4 },
  { key: 'master-data-province', title: 'Province', path: '/app/master/province', icon: 'Map', type: 'ITEM', parentKey: 'master-data', sortOrder: 5 },
  { key: 'master-data-city', title: 'City', path: '/app/master/city', icon: 'MapPin', type: 'ITEM', parentKey: 'master-data', sortOrder: 6 },
  { key: 'master-data-city-sla', title: 'City SLA', path: '/app/master/city-sla', icon: 'Clock', type: 'ITEM', parentKey: 'master-data', sortOrder: 7 },
  { key: 'master-data-uom', title: 'UOM', path: '/app/master/uom', icon: 'Ruler', type: 'ITEM', parentKey: 'master-data', sortOrder: 8 },
  { key: 'master-data-warehouse', title: 'Warehouse', path: '/app/master/warehouse', icon: 'Warehouse', type: 'ITEM', parentKey: 'master-data', sortOrder: 9 },
  { key: 'logistic', title: 'Logistic', path: null, icon: 'Truck', type: 'GROUP', parentKey: null, sortOrder: 5 },
  { key: 'logistic-inbound', title: 'Inbound', path: '/app/logistic/inbound', icon: 'ArrowDownToLine', type: 'ITEM', parentKey: 'logistic', sortOrder: 1 },
  { key: 'logistic-outbound', title: 'Outbound', path: '/app/logistic/outbound', icon: 'ArrowRightLeft', type: 'ITEM', parentKey: 'logistic', sortOrder: 2 },
  { key: 'logistic-report-monitoring-do', title: 'Report Monitoring DO', path: '/app/logistic/report-monitoring-do', icon: 'ClipboardList', type: 'ITEM', parentKey: 'logistic', sortOrder: 3 },
  { key: 'logistic-report-stock-batch', title: 'Report Stock Batch', path: '/app/logistic/report-stock-batch', icon: 'Boxes', type: 'ITEM', parentKey: 'logistic', sortOrder: 4 },
  { key: 'logistic-report-stock-mutation', title: 'Report Stock Mutation', path: '/app/logistic/report-stock-mutation', icon: 'Repeat', type: 'ITEM', parentKey: 'logistic', sortOrder: 5 },
  { key: 'logistic-dashboard-warehouse', title: 'Dashboard Warehouse', path: '/app/dashboard/warehouse', icon: 'Warehouse', type: 'ITEM', parentKey: 'logistic', sortOrder: 6 },
];

import { PrismaClient, ErpUserLevel, ErpMenuType, ErpNumberingReset } from '@prisma/client';
import { pbkdf2Sync, randomBytes } from 'crypto';
import { seedTransactionNotes } from './seed-erp-transaction-notes';
import { seedTransactionGrids } from './seed-erp-transaction-grids';
import { seedSalesForms } from './seed-erp-sales-forms';
import { seedPurchasingForms } from './seed-erp-purchasing-forms';
import { seedInitialSetup } from './seed-erp-initial-setup';
import { iso4217Currencies } from './data/iso-4217-currencies';

const prisma = new PrismaClient();

function hashPassword(password: string): string {
  const salt = randomBytes(16);
  const iterations = 210000;
  const digest = 'sha512';
  const derived = pbkdf2Sync(password, salt, iterations, 64, digest);
  return `pbkdf2$v1$${digest}$${iterations}$${salt.toString('base64')}$${derived.toString('base64')}`;
}

async function seedLanguages() {
  await prisma.erpLanguage.upsert({
    where: { code: 'id' },
    create: { code: 'id', name: 'Indonesian', nativeName: 'Bahasa Indonesia', isDefault: true },
    update: {},
  });
  await prisma.erpLanguage.upsert({
    where: { code: 'en' },
    create: { code: 'en', name: 'English', nativeName: 'English', isDefault: false },
    update: {},
  });
  console.log('✓ sys_languages');
}

async function seedSettings() {
  const settings = [
    // company
    {
      module: 'system',
      group: 'company',
      key: 'name',
      name: 'Nama Perusahaan',
      value: 'PT Sentient Factory',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'company',
      key: 'code',
      name: 'Kode Perusahaan',
      value: 'SF',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'company',
      key: 'address_line1',
      name: 'Alamat Baris 1',
      value: 'Jl. Industri No. 1',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'company',
      key: 'address_line2',
      name: 'Alamat Baris 2',
      value: 'Kawasan Industri MM2100',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'company',
      key: 'city',
      name: 'Kota',
      value: 'Bekasi',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'company',
      key: 'postal_code',
      name: 'Kode Pos',
      value: '17520',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'company',
      key: 'phone',
      name: 'Telepon',
      value: '021-88888888',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'company',
      key: 'fax',
      name: 'Fax',
      value: '021-88888889',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'company',
      key: 'email',
      name: 'Email',
      value: 'info@sentient-factory.com',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'company',
      key: 'website',
      name: 'Website',
      value: 'https://sentient-factory.com',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'company',
      key: 'npwp',
      name: 'NPWP',
      value: '01.234.567.8-901.000',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'company',
      key: 'currency',
      name: 'Mata Uang Dasar',
      value: 'IDR',
      dataType: 'string',
    },
    // accounting
    {
      module: 'system',
      group: 'accounting',
      key: 'fiscal_year_start',
      name: 'Awal Tahun Fiskal (Bulan)',
      value: '1',
      dataType: 'number',
    },
    {
      module: 'system',
      group: 'accounting',
      key: 'decimal_places',
      name: 'Desimal',
      value: '2',
      dataType: 'number',
    },
    {
      module: 'system',
      group: 'accounting',
      key: 'rounding_method',
      name: 'Metode Pembulatan',
      value: 'ROUND',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'accounting',
      key: 'auto_journal',
      name: 'Auto Jurnal',
      value: 'true',
      dataType: 'boolean',
    },
    {
      module: 'system',
      group: 'accounting',
      key: 'ar_account',
      name: 'Akun Piutang Default',
      value: '1130',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'accounting',
      key: 'ap_account',
      name: 'Akun Hutang Default',
      value: '2110',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'accounting',
      key: 'cash_account',
      name: 'Akun Kas Default',
      value: '1110',
      dataType: 'string',
    },
    // bank-accounts
    {
      module: 'system',
      group: 'bank-accounts',
      key: 'default_bank_name',
      name: 'Nama Bank Utama',
      value: 'Bank BCA',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'bank-accounts',
      key: 'default_account_number',
      name: 'Nomor Rekening',
      value: '1234567890',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'bank-accounts',
      key: 'default_account_name',
      name: 'Nama Pemilik Rekening',
      value: 'PT Sentient Factory',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'bank-accounts',
      key: 'swift_code',
      name: 'Kode SWIFT',
      value: 'CENAIDJA',
      dataType: 'string',
    },
    // tax
    {
      module: 'system',
      group: 'tax',
      key: 'ppn_rate',
      name: 'Tarif PPN (%)',
      value: '11',
      dataType: 'number',
    },
    {
      module: 'system',
      group: 'tax',
      key: 'pph_rate',
      name: 'Tarif PPh (%)',
      value: '2',
      dataType: 'number',
    },
    {
      module: 'system',
      group: 'tax',
      key: 'tax_inclusive',
      name: 'Harga Termasuk Pajak',
      value: 'false',
      dataType: 'boolean',
    },
    {
      module: 'system',
      group: 'tax',
      key: 'tax_number',
      name: 'Nomor PKP',
      value: '01.234.567.8-901.000',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'tax',
      key: 'default_tax_code',
      name: 'Kode Pajak Default',
      value: 'PPN11',
      dataType: 'string',
    },
    // description
    {
      module: 'system',
      group: 'description',
      key: 'invoice_header',
      name: 'Header Faktur',
      value: 'Terima kasih atas kepercayaan Anda',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'description',
      key: 'invoice_footer',
      name: 'Footer Faktur',
      value: 'Pembayaran ke rekening PT Sentient Factory',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'description',
      key: 'po_notes',
      name: 'Catatan PO Default',
      value: 'Mohon konfirmasi penerimaan PO ini',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'description',
      key: 'so_notes',
      name: 'Catatan SO Default',
      value: 'Pesanan akan diproses dalam 2 hari kerja',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'description',
      key: 'delivery_notes',
      name: 'Catatan Pengiriman',
      value: 'Mohon diperiksa sebelum diterima',
      dataType: 'string',
    },
    // format
    {
      module: 'system',
      group: 'format',
      key: 'date_format',
      name: 'Format Tanggal',
      value: 'DD/MM/YYYY',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'format',
      key: 'time_format',
      name: 'Format Waktu',
      value: 'HH:mm',
      dataType: 'string',
    },
    // number-format (global number formatting — diakses lewat halaman dedicated /admin/number-format)
    {
      module: 'system',
      group: 'number-format',
      key: 'number_thousands_sep',
      name: 'Pemisah Ribuan',
      value: '.',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'number-format',
      key: 'number_decimal_sep',
      name: 'Pemisah Desimal',
      value: ',',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'number-format',
      key: 'number_decimals',
      name: 'Jumlah Desimal Default',
      value: '2',
      dataType: 'integer',
    },
    {
      module: 'system',
      group: 'format',
      key: 'currency_symbol',
      name: 'Simbol Mata Uang',
      value: 'Rp',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'format',
      key: 'currency_position',
      name: 'Posisi Simbol',
      value: 'before',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'format',
      key: 'timezone',
      name: 'Zona Waktu',
      value: 'Asia/Jakarta',
      dataType: 'string',
    },
    // defaults
    {
      module: 'system',
      group: 'defaults',
      key: 'default_warehouse',
      name: 'Gudang Default',
      value: 'GD-001',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'defaults',
      key: 'default_payment_term',
      name: 'Termin Pembayaran Default',
      value: 'NET30',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'defaults',
      key: 'default_price_category',
      name: 'Kategori Harga Default',
      value: 'RETAIL',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'defaults',
      key: 'default_sales_tax',
      name: 'Pajak Penjualan Default',
      value: 'PPN11',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'defaults',
      key: 'default_purchase_tax',
      name: 'Pajak Pembelian Default',
      value: 'PPN11',
      dataType: 'string',
    },
    // report-defaults
    {
      module: 'system',
      group: 'report-defaults',
      key: 'paper_size',
      name: 'Ukuran Kertas',
      value: 'A4',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'report-defaults',
      key: 'orientation',
      name: 'Orientasi',
      value: 'portrait',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'report-defaults',
      key: 'font_size',
      name: 'Ukuran Font (pt)',
      value: '10',
      dataType: 'number',
    },
    {
      module: 'system',
      group: 'report-defaults',
      key: 'show_logo',
      name: 'Tampilkan Logo',
      value: 'true',
      dataType: 'boolean',
    },
    {
      module: 'system',
      group: 'report-defaults',
      key: 'show_stamp',
      name: 'Tampilkan Cap',
      value: 'false',
      dataType: 'boolean',
    },
    // signature
    {
      module: 'system',
      group: 'signature',
      key: 'authorized_by',
      name: 'Disetujui Oleh',
      value: 'Direktur Utama',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'signature',
      key: 'prepared_by',
      name: 'Disiapkan Oleh',
      value: 'Staff Administrasi',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'signature',
      key: 'checked_by',
      name: 'Diperiksa Oleh',
      value: 'Manajer Keuangan',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'signature',
      key: 'show_signature_line',
      name: 'Tampilkan Baris TTD',
      value: 'true',
      dataType: 'boolean',
    },
    // options
    {
      module: 'system',
      group: 'options',
      key: 'auto_increment_code',
      name: 'Auto Kode',
      value: 'true',
      dataType: 'boolean',
    },
    {
      module: 'system',
      group: 'options',
      key: 'confirm_before_delete',
      name: 'Konfirmasi Sebelum Hapus',
      value: 'true',
      dataType: 'boolean',
    },
    {
      module: 'system',
      group: 'options',
      key: 'show_audit_log',
      name: 'Tampilkan Audit Log',
      value: 'true',
      dataType: 'boolean',
    },
    {
      module: 'system',
      group: 'options',
      key: 'enable_multi_currency',
      name: 'Multi Mata Uang',
      value: 'false',
      dataType: 'boolean',
    },
    {
      module: 'system',
      group: 'options',
      key: 'enable_batch',
      name: 'Enable Batch',
      value: 'false',
      dataType: 'boolean',
    },
    // home
    {
      module: 'system',
      group: 'home',
      key: 'welcome_message',
      name: 'Pesan Selamat Datang',
      value: 'Selamat datang di Sentient ERP',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'home',
      key: 'dashboard_layout',
      name: 'Tata Letak Dashboard',
      value: 'grid',
      dataType: 'string',
    },
    {
      module: 'system',
      group: 'home',
      key: 'show_quick_access',
      name: 'Tampilkan Quick Access',
      value: 'true',
      dataType: 'boolean',
    },
    // approval
    {
      module: 'system',
      group: 'approval',
      key: 'require_po_approval',
      name: 'Persetujuan PO',
      value: 'false',
      dataType: 'boolean',
    },
    {
      module: 'system',
      group: 'approval',
      key: 'require_so_approval',
      name: 'Persetujuan SO',
      value: 'false',
      dataType: 'boolean',
    },
    {
      module: 'system',
      group: 'approval',
      key: 'auto_approve_below',
      name: 'Auto Approve di Bawah (Rp)',
      value: '0',
      dataType: 'number',
    },
    {
      module: 'system',
      group: 'approval',
      key: 'approval_levels',
      name: 'Tingkat Persetujuan',
      value: '1',
      dataType: 'number',
    },
    // account-code (CoA format — diakses lewat halaman dedicated /admin/account-code-format)
    {
      module: 'system',
      group: 'account-code',
      key: 'account_code_segments',
      name: 'Segmen Kode Akun',
      value: '[4,2,3]',
      dataType: 'json',
    },
    {
      module: 'system',
      group: 'account-code',
      key: 'account_code_separator',
      name: 'Pemisah Segmen Kode Akun',
      value: '.',
      dataType: 'string',
    },
  ];
  // Cleanup deprecated single-key number_format (replaced by group `number-format` with 3 keys, 2026-05-28).
  await prisma.erpSetting.deleteMany({
    where: { module: 'system', group: 'format', key: 'number_format' },
  });
  for (const s of settings) {
    await prisma.erpSetting.upsert({
      where: { module_group_key: { module: s.module, group: s.group, key: s.key } },
      create: s,
      update: {},
    });
  }
  console.log('✓ sys_settings');
}

async function seedCurrency() {
  // Full ISO 4217 active currency list (incl. IDR). Data: prisma/data/iso-4217-currencies.ts.
  for (const c of iso4217Currencies) {
    await prisma.erpCurrency.upsert({
      where: { code: c.code },
      create: { code: c.code, name: c.name, symbol: c.symbol, isActive: true },
      update: { name: c.name, symbol: c.symbol },
    });
  }
  console.log(`✓ md_currencies (${iso4217Currencies.length} entries)`);
}

async function seedBranch() {
  const branches = [
    {
      code: 'HQ',
      name: 'Head Quarter',
      addressLine1: 'Jl. Jend. Sudirman No. 1',
      city: 'Jakarta Pusat',
      postalCode: '10220',
      phone: '021-5700001',
      fax: '021-5700002',
      notes: 'Kantor pusat perusahaan',
      isActive: true,
    },
    {
      code: 'BDG',
      name: 'Cabang Bandung',
      addressLine1: 'Jl. Asia Afrika No. 45',
      addressLine2: 'Gedung Landmark Lt. 3',
      city: 'Bandung',
      postalCode: '40111',
      phone: '022-4200101',
      fax: '022-4200102',
      notes: 'Cabang wilayah Jawa Barat',
      isActive: true,
    },
    {
      code: 'SBY',
      name: 'Cabang Surabaya',
      addressLine1: 'Jl. Pemuda No. 27',
      addressLine2: 'Ruko Pemuda Square Blok B-5',
      city: 'Surabaya',
      postalCode: '60271',
      phone: '031-5350201',
      fax: '031-5350202',
      notes: 'Cabang wilayah Jawa Timur',
      isActive: true,
    },
    {
      code: 'MKS',
      name: 'Cabang Makassar',
      addressLine1: 'Jl. Sam Ratulangi No. 8',
      city: 'Makassar',
      postalCode: '90111',
      phone: '0411-871301',
      fax: '0411-871302',
      notes: 'Cabang wilayah Sulawesi Selatan',
      isActive: true,
    },
    {
      code: 'MDN',
      name: 'Cabang Medan',
      addressLine1: 'Jl. Gatot Subroto No. 12',
      addressLine2: 'Kompleks Bisnis Center Blok A-2',
      city: 'Medan',
      postalCode: '20112',
      phone: '061-4521401',
      fax: '061-4521402',
      notes: 'Cabang wilayah Sumatera Utara',
      isActive: true,
    },
    {
      code: 'SMG',
      name: 'Cabang Semarang',
      addressLine1: 'Jl. Pandanaran No. 55',
      city: 'Semarang',
      postalCode: '50134',
      phone: '024-8310501',
      fax: '024-8310502',
      notes: 'Cabang wilayah Jawa Tengah',
      isActive: true,
    },
    {
      code: 'YGY',
      name: 'Cabang Yogyakarta',
      addressLine1: 'Jl. Malioboro No. 20',
      addressLine2: 'Ruko Malioboro Blok C-1',
      city: 'Yogyakarta',
      postalCode: '55213',
      phone: '0274-561601',
      fax: '0274-561602',
      notes: 'Cabang wilayah DIY',
      isActive: true,
    },
    {
      code: 'PLM',
      name: 'Cabang Palembang',
      addressLine1: 'Jl. Jend. Sudirman No. 88',
      city: 'Palembang',
      postalCode: '30126',
      phone: '0711-357701',
      fax: '0711-357702',
      notes: 'Cabang wilayah Sumatera Selatan',
      isActive: true,
    },
    {
      code: 'DPS',
      name: 'Cabang Denpasar',
      addressLine1: 'Jl. Raya Puputan No. 33',
      addressLine2: 'Gedung Renon Center Lt. 2',
      city: 'Denpasar',
      postalCode: '80235',
      phone: '0361-225801',
      fax: '0361-225802',
      notes: 'Cabang wilayah Bali',
      isActive: true,
    },
    {
      code: 'BPN',
      name: 'Cabang Balikpapan',
      addressLine1: 'Jl. Jend. Sudirman No. 15',
      city: 'Balikpapan',
      postalCode: '76114',
      phone: '0542-736901',
      fax: '0542-736902',
      notes: 'Cabang wilayah Kalimantan Timur',
      isActive: true,
    },
    {
      code: 'PKU',
      name: 'Cabang Pekanbaru',
      addressLine1: 'Jl. Jend. Ahmad Yani No. 7',
      city: 'Pekanbaru',
      postalCode: '28156',
      phone: '0761-857001',
      fax: '0761-857002',
      notes: 'Cabang wilayah Riau',
      isActive: false,
    },
    {
      code: 'PNK',
      name: 'Cabang Pontianak',
      addressLine1: 'Jl. Tanjungpura No. 66',
      addressLine2: 'Ruko Tanjungpura Bisnis Center No. 5',
      city: 'Pontianak',
      postalCode: '78112',
      phone: '0561-748101',
      fax: '0561-748102',
      notes: 'Cabang wilayah Kalimantan Barat',
      isActive: false,
    },
  ];

  for (const branch of branches) {
    await prisma.erpBranch.upsert({
      where: { code: branch.code },
      create: branch,
      update: {},
    });
  }
  console.log(`✓ md_branches (${branches.length} entries)`);
}

async function seedPasswordPolicy() {
  await prisma.erpPasswordPolicy.upsert({
    where: { code: 'DEFAULT' },
    create: {
      code: 'DEFAULT',
      name: 'Default Policy',
      minLength: 8,
      requireUppercase: true,
      requireLowercase: true,
      requireNumber: true,
      requireSymbol: false,
      historyCount: 3,
      maxFailedAttempts: 5,
      lockoutDurationMin: 30,
      sessionTimeoutMin: 480,
      isDefault: true,
      isActive: true,
    },
    update: {},
  });
  console.log('✓ adm_password_policies');
}

async function seedMenus(): Promise<Map<string, bigint>> {
  const menuIds = new Map<string, bigint>();

  async function upsertMenu(p: {
    code: string;
    title: string;
    type: ErpMenuType;
    parentId?: bigint;
    path?: string;
    icon?: string;
    sortOrder: number;
    legacyCode?: string;
  }) {
    const m = await prisma.erpMenu.upsert({
      where: { code: p.code },
      create: {
        code: p.code,
        title: p.title,
        type: p.type,
        parentId: p.parentId,
        path: p.path,
        icon: p.icon,
        sortOrder: p.sortOrder,
        legacyCode: p.legacyCode,
        isActive: true,
      },
      update: {
        title: p.title,
        type: p.type,
        parentId: p.parentId ?? null,
        path: p.path ?? null,
        icon: p.icon ?? null,
        sortOrder: p.sortOrder,
        legacyCode: p.legacyCode ?? undefined,
      },
    });
    menuIds.set(p.code, m.id);
    return m;
  }

  async function upsertItems(
    items: Array<{ code: string; title: string; path: string; legacyCode?: string }>,
    parentId: bigint,
  ) {
    for (const [i, item] of items.entries()) {
      await upsertMenu({ ...item, type: ErpMenuType.ITEM, parentId, sortOrder: i + 1 });
    }
  }

  // ── M0: Administrator ─────────────────────────────────────────────────────
  const m0 = await upsertMenu({
    code: 'M0',
    title: 'Administrator',
    icon: 'gear',
    type: ErpMenuType.MODULE,
    sortOrder: 1,
    legacyCode: '0-1',
  });

  // M0.CFG — Pengaturan Awal (legacy 0-2): all initial config
  const cfgGrp = await upsertMenu({
    code: 'M0.CFG',
    title: 'Initial Setup',
    type: ErpMenuType.GROUP,
    parentId: m0.id,
    sortOrder: 1,
    legacyCode: '0-2',
  });
  await upsertItems(
    [
      { code: 'M0.CFG.PREFERENCES', title: 'Preferensi', path: '/admin/preferences' },
      {
        code: 'M0.CFG.COMPANY',
        title: 'Company Settings',
        path: '/admin/settings/company',
        legacyCode: '0-27',
      },
      {
        code: 'M0.CFG.ACCOUNTING',
        title: 'Accounting Settings',
        path: '/admin/settings/accounting',
        legacyCode: '0-29',
      },
      {
        code: 'M0.CFG.DOCNUM',
        title: 'Document Numbering',
        path: '/admin/document-numbering',
        legacyCode: '0-30',
      },
      {
        code: 'M0.CFG.BANK-ACCOUNTS',
        title: 'Bank Accounts',
        path: '/admin/settings/bank-accounts',
        legacyCode: '0-31',
      },
      {
        code: 'M0.CFG.TAX',
        title: 'Tax Settings',
        path: '/admin/settings/tax',
        legacyCode: '0-32',
      },
      {
        code: 'M0.CFG.DESCRIPTION',
        title: 'Description Presets',
        path: '/admin/settings/description',
        legacyCode: '0-33',
      },
      {
        code: 'M0.CFG.FORMAT',
        title: 'Format Settings',
        path: '/admin/settings/format',
        legacyCode: '0-34',
      },
      {
        code: 'M0.CFG.DEFAULTS',
        title: 'Default Settings',
        path: '/admin/settings/defaults',
        legacyCode: '0-35',
      },
      {
        code: 'M0.CFG.REPORT-DEFAULT',
        title: 'Report Defaults',
        path: '/admin/settings/report-defaults',
        legacyCode: '0-36',
      },
      {
        code: 'M0.CFG.SIGNATURE',
        title: 'Signature Settings',
        path: '/admin/settings/signature',
        legacyCode: '0-37',
      },
      {
        code: 'M0.CFG.OPTIONS',
        title: 'Options',
        path: '/admin/settings/options',
        legacyCode: '0-38',
      },
      {
        code: 'M0.CFG.HOME',
        title: 'Home Layout',
        path: '/admin/settings/home',
        legacyCode: '0-39',
      },
      {
        code: 'M0.CFG.APPROVAL',
        title: 'Approval Settings',
        path: '/admin/settings/approval',
        legacyCode: '0-46',
      },
      {
        code: 'M0.CFG.DOC-POLICY',
        title: 'Doc Creation Policy',
        path: '/admin/settings/doc-creation-policies',
      },
      { code: 'M0.CFG.IMPORT', title: 'Import Data', path: '/admin/import', legacyCode: '0-20' },
    ],
    cfgGrp.id,
  );

  // M0.ADM — Tool Administrator (legacy 0-25): identity & admin tools
  const admGrp = await upsertMenu({
    code: 'M0.ADM',
    title: 'Administration',
    type: ErpMenuType.GROUP,
    parentId: m0.id,
    sortOrder: 2,
    legacyCode: '0-25',
  });
  await upsertItems(
    [
      { code: 'M0.ADM.USERS', title: 'User Management', path: '/admin/users', legacyCode: '0-4' },
      { code: 'M0.ADM.ROLES', title: 'Role Management', path: '/admin/roles', legacyCode: '0-5' },
      { code: 'M0.ADM.PERMISSIONS', title: 'Permissions', path: '/admin/permissions' },
      { code: 'M0.ADM.LANG', title: 'Language', path: '/admin/language', legacyCode: '0-6' },
      {
        code: 'M0.ADM.ONLINE-USERS',
        title: 'Online Users',
        path: '/admin/users/online',
        legacyCode: '0-18',
      },
      { code: 'M0.ADM.USER-LOG', title: 'User Log', path: '/admin/audit-logs', legacyCode: '0-21' },
      {
        code: 'M0.ADM.CLOSE-PERIOD',
        title: 'Close Fiscal Period',
        path: '/admin/fiscal-periods/close',
        legacyCode: '0-17',
      },
      {
        code: 'M0.ADM.RECALC-COGS',
        title: 'Recalculate COGS',
        path: '/admin/tools/recalc-cogs',
        legacyCode: '0-12',
      },
      {
        code: 'M0.ADM.REPOST-JOURNAL',
        title: 'Repost Journals',
        path: '/admin/tools/repost-journal',
        legacyCode: '0-13',
      },
      {
        code: 'M0.ADM.DATA-VALIDITY',
        title: 'Data Validity Check',
        path: '/admin/tools/data-validity',
        legacyCode: '0-23',
      },
    ],
    admGrp.id,
  );

  // M0.SYS — Sistem (legacy 0-26): system management (keep existing codes)
  const sysGrp = await upsertMenu({
    code: 'M0.SYS',
    title: 'System',
    type: ErpMenuType.GROUP,
    parentId: m0.id,
    sortOrder: 3,
    legacyCode: '0-26',
  });
  await upsertItems(
    [
      { code: 'M0.SYS.MENUS', title: 'Menu Manager', path: '/admin/menus', legacyCode: '0-9' },
      {
        code: 'M0.SYS.SETTINGS',
        title: 'Settings Manager',
        path: '/admin/settings',
        legacyCode: '0-11',
      },
      {
        code: 'M0.SYS.ACCT-CODE',
        title: 'Account Code Format',
        path: '/admin/account-code-format',
      },
      { code: 'M0.SYS.NUM-FORMAT', title: 'Number Format', path: '/admin/number-format' },
      { code: 'M0.SYS.DATE-FORMAT', title: 'Date Format', path: '/admin/date-format' },
      {
        code: 'M0.SYS.DOCNUM',
        title: 'Document Numbering',
        path: '/admin/document-numbering',
        legacyCode: '0-30',
      },
      {
        code: 'M0.SYS.FISCAL',
        title: 'Fiscal Periods',
        path: '/admin/fiscal-periods',
        legacyCode: '0-17',
      },
      { code: 'M0.SYS.AUDIT', title: 'Audit Log', path: '/admin/audit-logs', legacyCode: '0-21' },
      { code: 'M0.SYS.GRID', title: 'Kustomisasi Grid', path: '/admin/grid-customization' },
      { code: 'M0.SYS.FORM', title: 'Form Builder', path: '/admin/form-builder' },
      { code: 'M0.SYS.APPEARANCE', title: 'Appearance', path: '/settings/appearance' },
      { code: 'M0.SYS.REPORT', title: 'Report Designer', path: '/admin/report-designer' },
    ],
    sysGrp.id,
  );

  // ── M1: Master Data ───────────────────────────────────────────────────────
  const m1 = await upsertMenu({
    code: 'M1',
    title: 'Master Data',
    icon: 'database',
    type: ErpMenuType.MODULE,
    sortOrder: 2,
    legacyCode: '1-1',
  });

  // M1.ORG — Organization (branch, location, warehouse, divisions, etc.)
  const orgGrp = await upsertMenu({
    code: 'M1.ORG',
    title: 'Organization',
    type: ErpMenuType.GROUP,
    parentId: m1.id,
    sortOrder: 1,
  });
  await upsertItems(
    [
      { code: 'M1.ORG.BRANCH', title: 'Branch', path: '/org/branches', legacyCode: '1-13' },
      { code: 'M1.ORG.LOCATION', title: 'Location', path: '/org/locations', legacyCode: '1-14' },
      { code: 'M1.ORG.WAREHOUSE', title: 'Warehouse', path: '/org/warehouses', legacyCode: '1-15' },
      { code: 'M1.ORG.DIVISION', title: 'Division', path: '/org/divisions', legacyCode: '1-16' },
      {
        code: 'M1.ORG.SUBDIVISION',
        title: 'Sub Division',
        path: '/org/sub-divisions',
        legacyCode: '1-17',
      },
      { code: 'M1.ORG.PROJECT', title: 'Project', path: '/org/projects', legacyCode: '1-18' },
      {
        code: 'M1.ORG.COST-CENTER',
        title: 'Cost Center',
        path: '/org/cost-centers',
        legacyCode: '1-19',
      },
      {
        code: 'M1.ORG.DEPARTMENT',
        title: 'Department',
        path: '/org/departments',
        legacyCode: '1-84',
      },
      {
        code: 'M1.ORG.SUBDEPT',
        title: 'Sub Department',
        path: '/org/sub-departments',
        legacyCode: '1-85',
      },
    ],
    orgGrp.id,
  );

  // M1.ITEM — Items & product classification
  const itemsGrp = await upsertMenu({
    code: 'M1.ITEM',
    title: 'Items',
    type: ErpMenuType.GROUP,
    parentId: m1.id,
    sortOrder: 2,
  });
  await upsertItems(
    [
      { code: 'M1.ITEM.ITEMS', title: 'Items', path: '/master/items', legacyCode: '1-11' },
      {
        code: 'M1.ITEM.CATEGORIES',
        title: 'Item Categories',
        path: '/master/item-categories',
        legacyCode: '1-8',
      },
      { code: 'M1.ITEM.TYPES', title: 'Item Types', path: '/master/item-types', legacyCode: '1-9' },
      { code: 'M1.ITEM.UNITS', title: 'Units', path: '/master/units', legacyCode: '1-10' },
      {
        code: 'M1.ITEM.LOCATIONS',
        title: 'Lokasi Gudang',
        path: '/master/storage-bins',
        legacyCode: '1-7',
      },
      {
        code: 'M1.ITEM.INFO',
        title: 'Item Information',
        path: '/master/item-info',
        legacyCode: '1-72',
      },
      {
        code: 'M1.ITEM.PRODUCT-CLASS',
        title: 'Product Class',
        path: '/master/product-classes',
        legacyCode: '1-80',
      },
      {
        code: 'M1.ITEM.PRICE-INDEX',
        title: 'Price Index',
        path: '/master/price-indices',
        legacyCode: '1-81',
      },
      {
        code: 'M1.ITEM.PRICE-CATEGORY',
        title: 'Price Category',
        path: '/master/price-categories',
        legacyCode: '1-90',
      },
      {
        code: 'M1.ITEM.COMMISSION',
        title: 'Commission',
        path: '/master/commissions',
        legacyCode: '1-88',
      },
      { code: 'M1.ITEM.CLASS', title: 'Class', path: '/master/classes', legacyCode: '1-96' },
      { code: 'M1.ITEM.MODEL', title: 'Model', path: '/master/models', legacyCode: '1-97' },
      { code: 'M1.ITEM.SIZE', title: 'Size', path: '/master/sizes', legacyCode: '1-98' },
      { code: 'M1.ITEM.COLOR', title: 'Color', path: '/master/colors', legacyCode: '1-99' },
      { code: 'M1.ITEM.NOZZLE', title: 'Nozzle', path: '/master/nozzles', legacyCode: '1-901' },
      { code: 'M1.ITEM.OEM', title: 'OEM', path: '/master/oems', legacyCode: '1-902' },
      { code: 'M1.ITEM.BRAND', title: 'Brand', path: '/master/brands', legacyCode: '1-101' },
      {
        code: 'M1.ITEM.MATERIAL',
        title: 'Material',
        path: '/master/materials',
        legacyCode: '1-102',
      },
      { code: 'M1.ITEM.SECTION', title: 'Section', path: '/master/sections', legacyCode: '1-103' },
    ],
    itemsGrp.id,
  );

  // M1.PARTNER — Partners & contacts
  const partnerGrp = await upsertMenu({
    code: 'M1.PARTNER',
    title: 'Partners',
    type: ErpMenuType.GROUP,
    parentId: m1.id,
    sortOrder: 3,
  });
  await upsertItems(
    [
      {
        code: 'M1.PARTNER.PARTNERS',
        title: 'Partners',
        path: '/master/partners',
        legacyCode: '1-6',
      },
      {
        code: 'M1.PARTNER.CATEGORIES',
        title: 'Partner Categories',
        path: '/master/partner-categories',
        legacyCode: '1-3',
      },
      {
        code: 'M1.PARTNER.TYPES',
        title: 'Partner Types',
        path: '/master/partner-types',
        legacyCode: '1-3b',
      },
      {
        code: 'M1.PARTNER.CUSTOMER-CAT',
        title: 'Customer Categories',
        path: '/master/customer-categories',
        legacyCode: '1-4',
      },
      {
        code: 'M1.PARTNER.SUPPLIER-CAT',
        title: 'Supplier Categories',
        path: '/master/supplier-categories',
        legacyCode: '1-70',
      },
      {
        code: 'M1.PARTNER.SALESMAN-CAT',
        title: 'Salesman Categories',
        path: '/master/salesman-categories',
        legacyCode: '1-5',
      },
      { code: 'M1.PARTNER.BANK', title: 'Bank', path: '/master/banks', legacyCode: '1-23' },
      {
        code: 'M1.PARTNER.EXPEDITION',
        title: 'Expedition',
        path: '/master/expeditions',
        legacyCode: '1-29',
      },
      { code: 'M1.PARTNER.VENDOR', title: 'Vendor', path: '/master/vendors', legacyCode: '1-105' },
    ],
    partnerGrp.id,
  );

  // M1.FIN — Finance Masters
  const finGrp = await upsertMenu({
    code: 'M1.FIN',
    title: 'Finance Masters',
    type: ErpMenuType.GROUP,
    parentId: m1.id,
    sortOrder: 4,
  });
  await upsertItems(
    [
      {
        code: 'M1.FIN.ACCOUNTS',
        title: 'Chart of Accounts',
        path: '/master/accounts',
        legacyCode: '1-12',
      },
      { code: 'M1.FIN.TAXES', title: 'Taxes', path: '/master/taxes', legacyCode: '1-21' },
      {
        code: 'M1.FIN.CURRENCIES',
        title: 'Currencies',
        path: '/master/currencies',
        legacyCode: '1-22',
      },
      {
        code: 'M1.FIN.TERMS',
        title: 'Payment Terms',
        path: '/master/payment-terms',
        legacyCode: '1-20',
      },
      {
        code: 'M1.FIN.OTHER-COSTS',
        title: 'Other Costs',
        path: '/master/other-costs',
        legacyCode: '1-31',
      },
    ],
    finGrp.id,
  );

  // M1.REF — Reference / Misc
  const refGrp = await upsertMenu({
    code: 'M1.REF',
    title: 'Reference',
    type: ErpMenuType.GROUP,
    parentId: m1.id,
    sortOrder: 5,
  });
  await upsertItems(
    [
      { code: 'M1.REF.COUNTRY', title: 'Country', path: '/master/countries', legacyCode: '1-25' },
      { code: 'M1.REF.PROVINCE', title: 'Province', path: '/master/provinces', legacyCode: '1-26' },
      { code: 'M1.REF.CITY', title: 'City', path: '/master/cities', legacyCode: '1-27' },
      { code: 'M1.REF.AREA', title: 'Area', path: '/master/areas', legacyCode: '1-28' },
      {
        code: 'M1.REF.TXN-NOTE',
        title: 'Transaction Notes',
        path: '/master/transaction-notes',
        legacyCode: '1-24',
      },
      {
        code: 'M1.REF.TXN-NOTE-DETAIL',
        title: 'Txn Note Detail',
        path: '/master/txn-note-details',
        legacyCode: '1-33',
      },
      {
        code: 'M1.REF.ITEM-TXN-TYPE',
        title: 'Stock Adjustment Types',
        path: '/master/stock-adjustment-types',
        legacyCode: '1-30',
      },
      {
        code: 'M1.REF.PRODUCTION-CAT',
        title: 'Production Categories',
        path: '/master/production-categories',
        legacyCode: '1-35',
      },
      {
        code: 'M1.REF.WORK-ESTIMATE',
        title: 'Work Estimate',
        path: '/master/work-estimates',
        legacyCode: '1-34',
      },
      {
        code: 'M1.REF.POINT-CAT',
        title: 'Point Categories',
        path: '/master/point-categories',
        legacyCode: '1-78',
      },
      {
        code: 'M1.REF.MISC',
        title: 'Miscellaneous',
        path: '/master/miscellaneous',
        legacyCode: '1-32',
      },
    ],
    refGrp.id,
  );

  // M1.PROD — Production Masters
  const prodGrp = await upsertMenu({
    code: 'M1.PROD',
    title: 'Production',
    type: ErpMenuType.GROUP,
    parentId: m1.id,
    sortOrder: 6,
  });
  await upsertItems(
    [
      { code: 'M1.PROD.LABOR', title: 'Labor', path: '/master/labors', legacyCode: '1-94' },
      { code: 'M1.PROD.MACHINE', title: 'Machine', path: '/master/machines', legacyCode: '1-95' },
      {
        code: 'M1.PROD.DESIGNER',
        title: 'Designer',
        path: '/master/designers',
        legacyCode: '1-104',
      },
      {
        code: 'M1.PROD.ACTIVITY',
        title: 'Production Activity',
        path: '/master/production-activities',
        legacyCode: '1-120',
      },
      {
        code: 'M1.PROD.ROUTE',
        title: 'Production Route',
        path: '/master/production-routes',
        legacyCode: '1-122',
      },
      {
        code: 'M1.PROD.SUBCLASS',
        title: 'Sub Class',
        path: '/master/sub-classes',
        legacyCode: '1-106',
      },
    ],
    prodGrp.id,
  );

  // SET module removed — Appearance moved to M0.SYS.APPEARANCE (System group).

  // ── M2–M14: Module stubs (legacy MyERP+ modules) ─────────────────────────
  // Roots only — items belum di-port. Path null = sidebar tampil tapi belum
  // navigable; expand per modul saat fitur dibangun. Tidak ada m9 di legacy.
  // Module roots Senti ERP. Dashboard pinned to top. M10 (HR & Payroll),
  // M11 (Hospital — Althea vertical), M13 (Academic), M14 (Cooperative)
  // dihapus 2026-05-20: bukan scope ERP manufaktur, belum di module-roadmap.
  // M11 secara eksplisit milik apps/web-althea (CLAUDE.md §1).
  const moduleStubs: Array<{
    code: string;
    title: string;
    icon: string;
    legacyCode: string;
    sortOrder: number;
  }> = [
    { code: 'M8', title: 'Dashboard', icon: 'layout-dashboard', legacyCode: '8-1', sortOrder: 0 },
    // M0/M1 sudah di-upsert di atas dengan sortOrder 1 & 2.
    {
      code: 'M2',
      title: 'Finance & Accounting',
      icon: 'calculator',
      legacyCode: '2-1',
      sortOrder: 3,
    },
    { code: 'M3', title: 'Warehouse & Inventory', icon: 'boxes', legacyCode: '3-1', sortOrder: 4 },
    { code: 'M4', title: 'Purchasing', icon: 'shopping-cart', legacyCode: '4-1', sortOrder: 5 },
    { code: 'M5', title: 'Sales', icon: 'trending-up', legacyCode: '5-1', sortOrder: 6 },
    { code: 'M6', title: 'Production', icon: 'factory', legacyCode: '6-1', sortOrder: 7 },
    { code: 'M7', title: 'Fixed Assets', icon: 'building', legacyCode: '7-1', sortOrder: 8 },
    { code: 'M12', title: 'Point of Sale', icon: 'shopping-bag', legacyCode: '12-1', sortOrder: 9 },
  ];
  for (const s of moduleStubs) {
    await upsertMenu({
      code: s.code,
      title: s.title,
      icon: s.icon,
      type: ErpMenuType.MODULE,
      sortOrder: s.sortOrder,
      legacyCode: s.legacyCode,
    });
  }

  // M2 Finance — paritas dengan legacy m2-finance (Keuangan ▸ Transaksi + Laporan).
  // Legacy "Transaksi" codes (lihat menu legacy):
  //   CR=Kas Masuk (Cash Receipt), CD=Kas Keluar (Cash Disbursement),
  //   RM=Bank Masuk (Bank Receipt → fin_ar_receipts), SM=Bank Keluar (Bank Payment → fin_ap_payments),
  //   GJ=Jurnal Umum (General Journal), RG/SG=Giro Masuk/Keluar, RGC/SGC=Giro …Batal (clearing),
  //   CB=Saldo Awal Coa (Opening Balance → JournalType.OPENING_BALANCE).
  //   Tambahan modern (tak ada di menu legacy ini): BD=Bank Disbursement, AJ=Adjustment Journal.
  // Title English (keputusan 2026-05-20); CB & RM/SM dikoreksi 2026-05-31 (§ finance-menu-parity).
  const m2Id = menuIds.get('M2');
  if (m2Id) {
    const m2TxGrp = await upsertMenu({
      code: 'M2.TX',
      title: 'Transactions',
      type: ErpMenuType.GROUP,
      parentId: m2Id,
      sortOrder: 1,
    });
    await upsertItems(
      [
        {
          code: 'M2.TX.CASH-RECEIPT',
          title: 'Cash Receipt',
          path: '/finance/cash-receipts',
          legacyCode: 'CR',
        },
        {
          code: 'M2.TX.CASH-DISBURSEMENT',
          title: 'Cash Disbursement',
          path: '/finance/cash-disbursements',
          legacyCode: 'CD',
        },
        {
          code: 'M2.TX.BANK-RECEIPT',
          title: 'Bank Receipt',
          path: '/finance/bank-receipts',
          legacyCode: 'RM',
        },
        // SM=Bank Keluar lives on the shared cash-bank table (kind=BANK, direction=DISBURSEMENT)
        // at /finance/bank-disbursements. The old standalone BANK-PAYMENT (SM→fin_ap_payments)
        // entry was removed (2026-05-31) — it duplicated this page (§ Bank Keluar SM).
        {
          code: 'M2.TX.BANK-DISBURSEMENT',
          title: 'Bank Keluar',
          path: '/finance/bank-disbursements',
          legacyCode: 'SM',
        },
        {
          code: 'M2.TX.GENERAL-JOURNAL',
          title: 'General Journal',
          path: '/finance/general-journals',
          legacyCode: 'GJ',
        },
        {
          code: 'M2.TX.ADJUSTMENT-JOURNAL',
          title: 'Adjustment Journal',
          path: '/finance/adjustment-journals',
          legacyCode: 'AJ',
        },
        {
          code: 'M2.TX.RECEIPT-GIRO',
          title: 'Receipt Giro',
          path: '/finance/receipt-giros',
          legacyCode: 'RG',
        },
        {
          code: 'M2.TX.SEND-GIRO',
          title: 'Send Giro',
          path: '/finance/send-giros',
          legacyCode: 'SG',
        },
        {
          code: 'M2.TX.RECEIPT-GIRO-CLR',
          title: 'Receipt Giro Clearing',
          path: '/finance/receipt-giro-clearings',
          legacyCode: 'RGC',
        },
        {
          code: 'M2.TX.SEND-GIRO-CLR',
          title: 'Send Giro Clearing',
          path: '/finance/send-giro-clearings',
          legacyCode: 'SGC',
        },
        {
          code: 'M2.TX.MEMORIAL-JOURNAL',
          title: 'Memorial Journal',
          path: '/finance/memorial-journals',
          legacyCode: 'JM',
        },
        {
          code: 'M2.TX.REVALUATION',
          title: 'FX Revaluation',
          path: '/finance/revaluations',
          legacyCode: 'RV',
        },
        {
          code: 'M2.TX.RECEIPT-MEMO',
          title: 'Receipt Memo',
          path: '/finance/receipt-memos',
          legacyCode: 'RM2',
        },
        {
          code: 'M2.TX.SEND-MEMO',
          title: 'Send Memo',
          path: '/finance/send-memos',
          legacyCode: 'SM2',
        },
        {
          code: 'M2.TX.CASHBANK-TRANSFER',
          title: 'Cash/Bank Transfer',
          path: '/finance/cashbank-transfers',
          legacyCode: 'CBT',
        },
        {
          code: 'M2.TX.OPENING-BALANCE',
          title: 'Opening Balance (CoA)',
          path: '/finance/opening-balances',
          legacyCode: 'CB',
        },
      ],
      m2TxGrp.id,
    );
    // Prune legacy duplicate "Bank Payment" (SM → fin_ap_payments) menu — superseded by
    // "Bank Keluar" on the shared cash-bank table at /finance/bank-disbursements (2026-05-31).
    const legacyBankPayment = await prisma.erpMenu.findUnique({
      where: { code: 'M2.TX.BANK-PAYMENT' },
    });
    if (legacyBankPayment) {
      await prisma.erpRoleMenu.deleteMany({ where: { menuId: legacyBankPayment.id } });
      await prisma.erpMenu.delete({ where: { id: legacyBankPayment.id } });
    }
    // Laporan keuangan inti — legacy MODULEID=2 (Report.vb) punya ~150 varian;
    // di sini parent report kanonik yang dipenuhi oleh fin_ledger_entries / fin_budget_realizations / fin_giros.
    const m2RptGrp = await upsertMenu({
      code: 'M2.RPT',
      title: 'Reports',
      type: ErpMenuType.GROUP,
      parentId: m2Id,
      sortOrder: 2,
    });
    await upsertItems(
      [
        {
          code: 'M2.RPT.LEDGER',
          title: 'General Ledger',
          path: '/finance/ledger',
          legacyCode: '2-41',
        },
        {
          code: 'M2.RPT.TRIAL-BALANCE',
          title: 'Trial Balance',
          path: '/finance/trial-balance',
          legacyCode: '2-42',
        },
        {
          code: 'M2.RPT.MOVEMENT-BALANCE',
          title: 'Neraca Mutasi',
          path: '/finance/movement-balance',
          legacyCode: '2-44',
        },
        {
          code: 'M2.RPT.BALANCE-SHEET',
          title: 'Balance Sheet',
          path: '/finance/balance-sheet',
          legacyCode: '2-45',
        },
        {
          code: 'M2.RPT.INCOME-STATEMENT',
          title: 'Income Statement',
          path: '/finance/income-statement',
          legacyCode: '2-46',
        },
        {
          code: 'M2.RPT.EQUITY-CHANGES',
          title: 'Perubahan Modal',
          path: '/finance/equity-changes',
          legacyCode: '2-48',
        },
        {
          code: 'M2.RPT.CASH-FLOW',
          title: 'Cash Flow',
          path: '/finance/cash-flow',
          legacyCode: '2-104',
        },
        {
          code: 'M2.RPT.DAILY-CASH-BANK',
          title: 'Daily Cash & Bank',
          path: '/finance/daily-cash-bank',
          legacyCode: '2-43',
        },
        { code: 'M2.RPT.AR-CARD', title: 'AR Card', path: '/finance/ar-card', legacyCode: '2-47' },
        {
          code: 'M2.RPT.AR-AGING',
          title: 'AR Aging',
          path: '/finance/ar-aging',
          legacyCode: '2-92',
        },
        { code: 'M2.RPT.AP-CARD', title: 'AP Card', path: '/finance/ap-card', legacyCode: '2-50' },
        {
          code: 'M2.RPT.AP-AGING',
          title: 'AP Aging',
          path: '/finance/ap-aging',
          legacyCode: '2-93',
        },
        {
          code: 'M2.RPT.GIRO-MATURITY',
          title: 'Giro Maturity',
          path: '/finance/giro-maturity',
          legacyCode: '2-9',
        },
        {
          code: 'M2.RPT.BUDGET-REALIZATION',
          title: 'Budget vs Realization',
          path: '/finance/budget-realization',
          legacyCode: '2-135',
        },
      ],
      m2RptGrp.id,
    );
  }

  // M3 Warehouse & Inventory — paritas dengan legacy m3 (10 transaksi, data, laporan, statistik).
  // Legacy mntype: 2=group, 3=data-list, 4=report, 6=stat-group, 7=stat-item.
  // Path prefix: /warehouse/* (belum navigable — frontend M3 belum dibangun).
  const m3Id = menuIds.get('M3');
  if (m3Id) {
    const m3TxGrp = await upsertMenu({
      code: 'M3.TX',
      title: 'Transactions',
      type: ErpMenuType.GROUP,
      parentId: m3Id,
      sortOrder: 1,
      legacyCode: '3-2',
    });
    await upsertItems(
      [
        {
          code: 'M3.TX.MR',
          title: 'Material Request (MR)',
          path: '/warehouse/material-requests',
          legacyCode: '3-3',
        },
        {
          code: 'M3.TX.TS',
          title: 'Stock Transfer (TS)',
          path: '/warehouse/transfers',
          legacyCode: '3-4',
        },
        {
          code: 'M3.TX.RS',
          title: 'Transfer Receipt (RS)',
          path: '/warehouse/transfer-receipts',
          legacyCode: '3-5',
        },
        {
          code: 'M3.TX.SP',
          title: 'Stock Count (SP)',
          path: '/warehouse/stock-counts',
          legacyCode: '3-7',
        },
        {
          code: 'M3.TX.SA',
          title: 'Stock Adjustment (SA)',
          path: '/warehouse/stock-adjustments',
          legacyCode: '3-6',
        },
        {
          code: 'M3.TX.PA',
          title: 'Price Adjustment (PA)',
          path: '/warehouse/price-adjustments',
          legacyCode: '3-8',
        },
        {
          code: 'M3.TX.IB',
          title: 'Opening Stock (IB)',
          path: '/warehouse/opening-stocks',
          legacyCode: '3-23',
        },
        {
          code: 'M3.TX.RF',
          title: 'Fuel Refill (RF)',
          path: '/warehouse/fuel-refills',
          legacyCode: '3-40',
        },
        {
          code: 'M3.TX.DC',
          title: 'Time Sheet/Daily Check (DC)',
          path: '/warehouse/daily-checks',
          legacyCode: '3-41',
        },
        {
          code: 'M3.TX.RW',
          title: 'Receipt Weigher (RW)',
          path: '/warehouse/receipt-weighers',
          legacyCode: '3-51',
        },
      ],
      m3TxGrp.id,
    );

    const m3DataGrp = await upsertMenu({
      code: 'M3.DATA',
      title: 'Data',
      type: ErpMenuType.GROUP,
      parentId: m3Id,
      sortOrder: 2,
      legacyCode: '3-9',
    });
    await upsertItems(
      [
        {
          code: 'M3.DATA.MR',
          title: 'Material Request (MR)',
          path: '/warehouse/data/material-requests',
          legacyCode: '3-10',
        },
        {
          code: 'M3.DATA.TS',
          title: 'Stock Transfer (TS)',
          path: '/warehouse/data/transfers',
          legacyCode: '3-11',
        },
        {
          code: 'M3.DATA.RS',
          title: 'Transfer Receipt (RS)',
          path: '/warehouse/data/transfer-receipts',
          legacyCode: '3-12',
        },
        {
          code: 'M3.DATA.SP',
          title: 'Stock Count (SP)',
          path: '/warehouse/data/stock-counts',
          legacyCode: '3-14',
        },
        {
          code: 'M3.DATA.SA',
          title: 'Stock Adjustment (SA)',
          path: '/warehouse/data/stock-adjustments',
          legacyCode: '3-13',
        },
        {
          code: 'M3.DATA.PA',
          title: 'Price Adjustment (PA)',
          path: '/warehouse/data/price-adjustments',
          legacyCode: '3-15',
        },
        {
          code: 'M3.DATA.IB',
          title: 'Opening Stock (IB)',
          path: '/warehouse/data/opening-stocks',
          legacyCode: '3-24',
        },
        {
          code: 'M3.DATA.RF',
          title: 'Fuel Refill (RF)',
          path: '/warehouse/data/fuel-refills',
          legacyCode: '3-42',
        },
        {
          code: 'M3.DATA.DC',
          title: 'Time Sheet/Daily Check (DC)',
          path: '/warehouse/data/daily-checks',
          legacyCode: '3-43',
        },
        {
          code: 'M3.DATA.RW',
          title: 'Receipt Weigher (RW)',
          path: '/warehouse/data/receipt-weighers',
          legacyCode: '3-52',
        },
        {
          code: 'M3.DATA.SERIAL',
          title: 'Serial Item Cards',
          path: '/warehouse/data/serial-cards',
          legacyCode: '3-54',
        },
      ],
      m3DataGrp.id,
    );

    const m3RptGrp = await upsertMenu({
      code: 'M3.RPT',
      title: 'Reports',
      type: ErpMenuType.GROUP,
      parentId: m3Id,
      sortOrder: 3,
      legacyCode: '3-16',
    });
    await upsertItems(
      [
        {
          code: 'M3.RPT.MR',
          title: 'Material Request (MR)',
          path: '/warehouse/reports/material-requests',
          legacyCode: '3-17',
        },
        {
          code: 'M3.RPT.TS',
          title: 'Stock Transfer (TS)',
          path: '/warehouse/reports/transfers',
          legacyCode: '3-18',
        },
        {
          code: 'M3.RPT.RS',
          title: 'Transfer Receipt (RS)',
          path: '/warehouse/reports/transfer-receipts',
          legacyCode: '3-19',
        },
        {
          code: 'M3.RPT.SP',
          title: 'Stock Count (SP)',
          path: '/warehouse/reports/stock-counts',
          legacyCode: '3-21',
        },
        {
          code: 'M3.RPT.SA',
          title: 'Stock Adjustment (SA)',
          path: '/warehouse/reports/stock-adjustments',
          legacyCode: '3-20',
        },
        {
          code: 'M3.RPT.PA',
          title: 'Price Adjustment (PA)',
          path: '/warehouse/reports/price-adjustments',
          legacyCode: '3-22',
        },
        {
          code: 'M3.RPT.IB',
          title: 'Opening Stock (IB)',
          path: '/warehouse/reports/opening-stocks',
          legacyCode: '3-25',
        },
        {
          code: 'M3.RPT.RF',
          title: 'Fuel Refill (RF)',
          path: '/warehouse/reports/fuel-refills',
          legacyCode: '3-44',
        },
        {
          code: 'M3.RPT.DC',
          title: 'Time Sheet/Daily Check (DC)',
          path: '/warehouse/reports/daily-checks',
          legacyCode: '3-45',
        },
        {
          code: 'M3.RPT.RW',
          title: 'Receipt Weigher (RW)',
          path: '/warehouse/reports/receipt-weighers',
          legacyCode: '3-53',
        },
        {
          code: 'M3.RPT.STOCK-CARD',
          title: 'Stock Card',
          path: '/warehouse/reports/stock-cards',
          legacyCode: '3-36',
        },
        {
          code: 'M3.RPT.STOCK-MUT',
          title: 'Stock Mutation',
          path: '/warehouse/reports/stock-mutations',
          legacyCode: '3-37',
        },
        {
          code: 'M3.RPT.STOCK',
          title: 'Stock',
          path: '/warehouse/reports/stock',
          legacyCode: '3-47',
        },
        {
          code: 'M3.RPT.RETURNS',
          title: 'Return Items',
          path: '/warehouse/reports/returns',
          legacyCode: '3-48',
        },
        {
          code: 'M3.RPT.BELOW-MIN',
          title: 'Below Minimum Stock',
          path: '/warehouse/reports/below-minimum',
          legacyCode: '3-26',
        },
        {
          code: 'M3.RPT.BATCH',
          title: 'Batch Items',
          path: '/warehouse/reports/batch-items',
          legacyCode: '3-39',
        },
        {
          code: 'M3.RPT.BATCH-CARD',
          title: 'Batch Item Cards',
          path: '/warehouse/reports/batch-cards',
          legacyCode: '3-38',
        },
        {
          code: 'M3.RPT.SERIAL',
          title: 'Serial Items',
          path: '/warehouse/reports/serial-items',
          legacyCode: '3-35',
        },
        {
          code: 'M3.RPT.SERIAL-CARD',
          title: 'Serial Item Cards',
          path: '/warehouse/reports/serial-cards',
          legacyCode: '3-34',
        },
        {
          code: 'M3.RPT.COGS-BAL',
          title: 'COGS Balance',
          path: '/warehouse/reports/cogs-balance',
          legacyCode: '3-49',
        },
        {
          code: 'M3.RPT.CONSIGNMENT',
          title: 'Consignment Summary',
          path: '/warehouse/reports/consignment',
          legacyCode: '3-50',
        },
        {
          code: 'M3.RPT.DAILY-STOCK',
          title: 'Daily Available Stock',
          path: '/warehouse/reports/daily-stock',
          legacyCode: '3-55',
        },
        {
          code: 'M3.RPT.STOCK-MINUS',
          title: 'Stock Minus',
          path: '/warehouse/reports/stock-minus',
          legacyCode: '3-56',
        },
      ],
      m3RptGrp.id,
    );

    const m3StatGrp = await upsertMenu({
      code: 'M3.STAT',
      title: 'Statistics',
      type: ErpMenuType.GROUP,
      parentId: m3Id,
      sortOrder: 4,
      legacyCode: '3-27',
    });
    await upsertItems(
      [
        {
          code: 'M3.STAT.TOP-REVENUE',
          title: 'Top Revenue Products',
          path: '/warehouse/stats/top-revenue',
          legacyCode: '3-29',
        },
        {
          code: 'M3.STAT.PROFITABLE',
          title: 'Most Profitable Products',
          path: '/warehouse/stats/most-profitable',
          legacyCode: '3-32',
        },
        {
          code: 'M3.STAT.BEST-SELLING',
          title: 'Best Selling Products',
          path: '/warehouse/stats/best-selling',
          legacyCode: '3-33',
        },
        {
          code: 'M3.STAT.BELOW-MIN',
          title: 'Below Minimum Stock',
          path: '/warehouse/stats/below-minimum',
          legacyCode: '3-30',
        },
        {
          code: 'M3.STAT.APPROVAL',
          title: 'Need Approval (Warehouse)',
          path: '/warehouse/stats/approvals',
          legacyCode: '3-46',
        },
        {
          code: 'M3.STAT.KPI',
          title: 'KPI Warehouse',
          path: '/warehouse/stats/kpi',
          legacyCode: '3-31',
        },
      ],
      m3StatGrp.id,
    );
  }

  // M4 Purchasing — active items only (skip CS=Lembar Biaya, IPC=Kalkulasi Import — mnactive=0 in legacy).
  // TX/DATA/RPT mirror legacy 3-group pattern. Payments (VPP/VP) kept in M4 for discoverability
  // even though posting goes via fin_ap_payments.
  const m4Id = menuIds.get('M4');
  if (m4Id) {
    const m4TxGrp = await upsertMenu({
      code: 'M4.TX',
      title: 'Transactions',
      type: ErpMenuType.GROUP,
      parentId: m4Id,
      sortOrder: 1,
      legacyCode: '4-2',
    });
    await upsertItems(
      [
        {
          code: 'M4.TX.PR',
          title: 'Purchase Requisition (PR)',
          path: '/purchasing/purchase-requisitions',
          legacyCode: '4-3',
        },
        {
          code: 'M4.TX.RFQ',
          title: 'Request for Quotation (RFQ)',
          path: '/purchasing/rfqs',
          legacyCode: '4-5',
        },
        {
          code: 'M4.TX.BS',
          title: 'Bid Comparison (BS)',
          path: '/purchasing/bid-comparisons',
          legacyCode: '4-6',
        },
        {
          code: 'M4.TX.PO',
          title: 'Purchase Order (PO)',
          path: '/purchasing/purchase-orders',
          legacyCode: '4-7',
        },
        {
          code: 'M4.TX.AP',
          title: 'Vendor Advance (AP)',
          path: '/purchasing/vendor-advances',
          legacyCode: '4-8',
        },
        {
          code: 'M4.TX.GRN',
          title: 'Goods Receipt (GRN)',
          path: '/purchasing/goods-receipts',
          legacyCode: '4-10',
        },
        {
          code: 'M4.TX.PI',
          title: 'Purchase Invoice (PI)',
          path: '/purchasing/purchase-invoices',
          legacyCode: '4-11',
        },
        {
          code: 'M4.TX.PP',
          title: 'Freight Payable (PP)',
          path: '/purchasing/freight-payables',
          legacyCode: '4-44',
        },
        {
          code: 'M4.TX.DNR',
          title: 'Return Shipment (DNR)',
          path: '/purchasing/return-shipments',
          legacyCode: '4-12',
        },
        {
          code: 'M4.TX.PRT',
          title: 'Purchase Return (PRT)',
          path: '/purchasing/purchase-returns',
          legacyCode: '4-13',
        },
        {
          code: 'M4.TX.VPP',
          title: 'Payment Schedule (VPP)',
          path: '/purchasing/payment-schedules',
          legacyCode: '4-14',
        },
        {
          code: 'M4.TX.VP',
          title: 'Vendor Payment (VP)',
          path: '/purchasing/vendor-payments',
          legacyCode: '4-15',
        },
        {
          code: 'M4.TX.BALANCE',
          title: 'Opening AP Balance',
          path: '/purchasing/opening-ap-balance',
          legacyCode: '4-51',
        },
      ],
      m4TxGrp.id,
    );

    const m4DataGrp = await upsertMenu({
      code: 'M4.DATA',
      title: 'Data',
      type: ErpMenuType.GROUP,
      parentId: m4Id,
      sortOrder: 2,
      legacyCode: '4-16',
    });
    await upsertItems(
      [
        {
          code: 'M4.DATA.PR',
          title: 'Purchase Requisition (PR)',
          path: '/purchasing/data/purchase-requisitions',
          legacyCode: '4-17',
        },
        {
          code: 'M4.DATA.RFQ',
          title: 'Request for Quotation (RFQ)',
          path: '/purchasing/data/rfqs',
          legacyCode: '4-19',
        },
        {
          code: 'M4.DATA.BS',
          title: 'Bid Comparison (BS)',
          path: '/purchasing/data/bid-comparisons',
          legacyCode: '4-20',
        },
        {
          code: 'M4.DATA.PO',
          title: 'Purchase Order (PO)',
          path: '/purchasing/data/purchase-orders',
          legacyCode: '4-21',
        },
        {
          code: 'M4.DATA.AP',
          title: 'Vendor Advance (AP)',
          path: '/purchasing/data/vendor-advances',
          legacyCode: '4-22',
        },
        {
          code: 'M4.DATA.GRN',
          title: 'Goods Receipt (GRN)',
          path: '/purchasing/data/goods-receipts',
          legacyCode: '4-24',
        },
        {
          code: 'M4.DATA.PI',
          title: 'Purchase Invoice (PI)',
          path: '/purchasing/data/purchase-invoices',
          legacyCode: '4-25',
        },
        {
          code: 'M4.DATA.PP',
          title: 'Freight Payable (PP)',
          path: '/purchasing/data/freight-payables',
          legacyCode: '4-45',
        },
        {
          code: 'M4.DATA.DNR',
          title: 'Return Shipment (DNR)',
          path: '/purchasing/data/return-shipments',
          legacyCode: '4-26',
        },
        {
          code: 'M4.DATA.PRT',
          title: 'Purchase Return (PRT)',
          path: '/purchasing/data/purchase-returns',
          legacyCode: '4-27',
        },
        {
          code: 'M4.DATA.VPP',
          title: 'Payment Schedule (VPP)',
          path: '/purchasing/data/payment-schedules',
          legacyCode: '4-28',
        },
        {
          code: 'M4.DATA.VP',
          title: 'Vendor Payment (VP)',
          path: '/purchasing/data/vendor-payments',
          legacyCode: '4-29',
        },
        {
          code: 'M4.DATA.BALANCE',
          title: 'Opening AP Balance',
          path: '/purchasing/data/opening-ap-balance',
          legacyCode: '4-51',
        },
      ],
      m4DataGrp.id,
    );

    const m4RptGrp = await upsertMenu({
      code: 'M4.RPT',
      title: 'Reports',
      type: ErpMenuType.GROUP,
      parentId: m4Id,
      sortOrder: 3,
      legacyCode: '4-30',
    });
    await upsertItems(
      [
        {
          code: 'M4.RPT.PR',
          title: 'Purchase Requisition (PR)',
          path: '/purchasing/reports/purchase-requisitions',
          legacyCode: '4-31',
        },
        {
          code: 'M4.RPT.RFQ',
          title: 'Request for Quotation (RFQ)',
          path: '/purchasing/reports/rfqs',
          legacyCode: '4-33',
        },
        {
          code: 'M4.RPT.BS',
          title: 'Bid Comparison (BS)',
          path: '/purchasing/reports/bid-comparisons',
          legacyCode: '4-34',
        },
        {
          code: 'M4.RPT.PO',
          title: 'Purchase Order (PO)',
          path: '/purchasing/reports/purchase-orders',
          legacyCode: '4-35',
        },
        {
          code: 'M4.RPT.AP',
          title: 'Vendor Advance (AP)',
          path: '/purchasing/reports/vendor-advances',
          legacyCode: '4-36',
        },
        {
          code: 'M4.RPT.GRN',
          title: 'Goods Receipt (GRN)',
          path: '/purchasing/reports/goods-receipts',
          legacyCode: '4-38',
        },
        {
          code: 'M4.RPT.PI',
          title: 'Purchase Invoice (PI)',
          path: '/purchasing/reports/purchase-invoices',
          legacyCode: '4-39',
        },
        {
          code: 'M4.RPT.PP',
          title: 'Freight Payable (PP)',
          path: '/purchasing/reports/freight-payables',
          legacyCode: '4-46',
        },
        {
          code: 'M4.RPT.DNR',
          title: 'Return Shipment (DNR)',
          path: '/purchasing/reports/return-shipments',
          legacyCode: '4-40',
        },
        {
          code: 'M4.RPT.PRT',
          title: 'Purchase Return (PRT)',
          path: '/purchasing/reports/purchase-returns',
          legacyCode: '4-41',
        },
        {
          code: 'M4.RPT.VPP',
          title: 'Payment Schedule (VPP)',
          path: '/purchasing/reports/payment-schedules',
          legacyCode: '4-42',
        },
        {
          code: 'M4.RPT.VP',
          title: 'Vendor Payment (VP)',
          path: '/purchasing/reports/vendor-payments',
          legacyCode: '4-43',
        },
        {
          code: 'M4.RPT.SUMMARY',
          title: 'Purchases',
          path: '/purchasing/reports/summary',
          legacyCode: '4-50',
        },
      ],
      m4RptGrp.id,
    );
  }

  // M5 Sales — active items only (skip SPA=Penyesuaian Poin, CL=Complain — mnactive=0 in legacy).
  // IC (Penagihan Piutang) and PV (Pembayaran Piutang) kept in M5 for discoverability;
  // actual posting goes via fin_ar_receipts.
  const m5Id = menuIds.get('M5');
  if (m5Id) {
    const m5TxGrp = await upsertMenu({
      code: 'M5.TX',
      title: 'Transactions',
      type: ErpMenuType.GROUP,
      parentId: m5Id,
      sortOrder: 1,
      legacyCode: '5-2',
    });
    await upsertItems(
      [
        {
          code: 'M5.TX.SQ',
          title: 'Sales Quotation (SQ)',
          path: '/sales/quotations',
          legacyCode: '5-3',
        },
        { code: 'M5.TX.SO', title: 'Sales Order (SO)', path: '/sales/orders', legacyCode: '5-4' },
        {
          code: 'M5.TX.AS',
          title: 'Customer Advance (AS)',
          path: '/sales/customer-advances',
          legacyCode: '5-5',
        },
        {
          code: 'M5.TX.IP',
          title: 'Payment Receipt (IP)',
          path: '/sales/payment-receipts',
          legacyCode: '5-44',
        },
        {
          code: 'M5.TX.PI',
          title: 'Proforma Invoice (PI)',
          path: '/sales/proforma-invoices',
          legacyCode: '5-9',
        },
        {
          code: 'M5.TX.PL',
          title: 'Packing List (PL)',
          path: '/sales/packing-lists',
          legacyCode: '5-6',
        },
        {
          code: 'M5.TX.DO',
          title: 'Delivery Order (DO)',
          path: '/sales/delivery-orders',
          legacyCode: '5-7',
        },
        {
          code: 'M5.TX.DR',
          title: 'Delivery Report (DR)',
          path: '/sales/delivery-reports',
          legacyCode: '5-8',
        },
        {
          code: 'M5.TX.SI',
          title: 'Sales Invoice (SI)',
          path: '/sales/invoices',
          legacyCode: '5-10',
        },
        {
          code: 'M5.TX.RP',
          title: 'Freight Receivable (RP)',
          path: '/sales/freight-receivables',
          legacyCode: '5-41',
        },
        {
          code: 'M5.TX.RNR',
          title: 'Return Receipt (RNR)',
          path: '/sales/return-receipts',
          legacyCode: '5-11',
        },
        {
          code: 'M5.TX.SR',
          title: 'Sales Return (SR)',
          path: '/sales/returns',
          legacyCode: '5-12',
        },
        {
          code: 'M5.TX.IC',
          title: 'AR Collection (IC)',
          path: '/sales/ar-collections',
          legacyCode: '5-13',
        },
        {
          code: 'M5.TX.PV',
          title: 'AR Payment (PV)',
          path: '/sales/ar-payments',
          legacyCode: '5-14',
        },
        {
          code: 'M5.TX.SIE',
          title: 'Invoice Swap (SIE)',
          path: '/sales/invoice-swaps',
          legacyCode: '5-68',
        },
        {
          code: 'M5.TX.BALANCE',
          title: 'Opening AR Balance',
          path: '/sales/opening-ar-balance',
          legacyCode: '5-51',
        },
      ],
      m5TxGrp.id,
    );

    const m5DataGrp = await upsertMenu({
      code: 'M5.DATA',
      title: 'Data',
      type: ErpMenuType.GROUP,
      parentId: m5Id,
      sortOrder: 2,
      legacyCode: '5-15',
    });
    await upsertItems(
      [
        {
          code: 'M5.DATA.SQ',
          title: 'Sales Quotation (SQ)',
          path: '/sales/data/quotations',
          legacyCode: '5-16',
        },
        {
          code: 'M5.DATA.SO',
          title: 'Sales Order (SO)',
          path: '/sales/data/orders',
          legacyCode: '5-17',
        },
        {
          code: 'M5.DATA.AS',
          title: 'Customer Advance (AS)',
          path: '/sales/data/customer-advances',
          legacyCode: '5-18',
        },
        {
          code: 'M5.DATA.IP',
          title: 'Payment Receipt (IP)',
          path: '/sales/data/payment-receipts',
          legacyCode: '5-45',
        },
        {
          code: 'M5.DATA.PI',
          title: 'Proforma Invoice (PI)',
          path: '/sales/data/proforma-invoices',
          legacyCode: '5-22',
        },
        {
          code: 'M5.DATA.PL',
          title: 'Packing List (PL)',
          path: '/sales/data/packing-lists',
          legacyCode: '5-19',
        },
        {
          code: 'M5.DATA.DO',
          title: 'Delivery Order (DO)',
          path: '/sales/data/delivery-orders',
          legacyCode: '5-20',
        },
        {
          code: 'M5.DATA.DR',
          title: 'Delivery Report (DR)',
          path: '/sales/data/delivery-reports',
          legacyCode: '5-21',
        },
        {
          code: 'M5.DATA.SI',
          title: 'Sales Invoice (SI)',
          path: '/sales/data/invoices',
          legacyCode: '5-23',
        },
        {
          code: 'M5.DATA.RP',
          title: 'Freight Receivable (RP)',
          path: '/sales/data/freight-receivables',
          legacyCode: '5-42',
        },
        {
          code: 'M5.DATA.RNR',
          title: 'Return Receipt (RNR)',
          path: '/sales/data/return-receipts',
          legacyCode: '5-24',
        },
        {
          code: 'M5.DATA.SR',
          title: 'Sales Return (SR)',
          path: '/sales/data/returns',
          legacyCode: '5-25',
        },
        {
          code: 'M5.DATA.IC',
          title: 'AR Collection (IC)',
          path: '/sales/data/ar-collections',
          legacyCode: '5-26',
        },
        {
          code: 'M5.DATA.PV',
          title: 'AR Payment (PV)',
          path: '/sales/data/ar-payments',
          legacyCode: '5-27',
        },
        {
          code: 'M5.DATA.SIE',
          title: 'Invoice Swap (SIE)',
          path: '/sales/data/invoice-swaps',
          legacyCode: '5-69',
        },
        {
          code: 'M5.DATA.BALANCE',
          title: 'Opening AR Balance',
          path: '/sales/data/opening-ar-balance',
          legacyCode: '5-52',
        },
      ],
      m5DataGrp.id,
    );

    const m5RptGrp = await upsertMenu({
      code: 'M5.RPT',
      title: 'Reports',
      type: ErpMenuType.GROUP,
      parentId: m5Id,
      sortOrder: 3,
      legacyCode: '5-28',
    });
    await upsertItems(
      [
        {
          code: 'M5.RPT.SQ',
          title: 'Sales Quotation (SQ)',
          path: '/sales/reports/quotations',
          legacyCode: '5-29',
        },
        {
          code: 'M5.RPT.SO',
          title: 'Sales Order (SO)',
          path: '/sales/reports/orders',
          legacyCode: '5-30',
        },
        {
          code: 'M5.RPT.AS',
          title: 'Customer Advance (AS)',
          path: '/sales/reports/customer-advances',
          legacyCode: '5-31',
        },
        {
          code: 'M5.RPT.IP',
          title: 'Payment Receipt (IP)',
          path: '/sales/reports/payment-receipts',
          legacyCode: '5-46',
        },
        {
          code: 'M5.RPT.PI',
          title: 'Proforma Invoice (PI)',
          path: '/sales/reports/proforma-invoices',
          legacyCode: '5-35',
        },
        {
          code: 'M5.RPT.PL',
          title: 'Packing List (PL)',
          path: '/sales/reports/packing-lists',
          legacyCode: '5-32',
        },
        {
          code: 'M5.RPT.DO',
          title: 'Delivery Order (DO)',
          path: '/sales/reports/delivery-orders',
          legacyCode: '5-33',
        },
        {
          code: 'M5.RPT.DR',
          title: 'Delivery Report (DR)',
          path: '/sales/reports/delivery-reports',
          legacyCode: '5-34',
        },
        {
          code: 'M5.RPT.SI',
          title: 'Sales Invoice (SI)',
          path: '/sales/reports/invoices',
          legacyCode: '5-36',
        },
        {
          code: 'M5.RPT.RP',
          title: 'Freight Receivable (RP)',
          path: '/sales/reports/freight-receivables',
          legacyCode: '5-43',
        },
        {
          code: 'M5.RPT.RNR',
          title: 'Return Receipt (RNR)',
          path: '/sales/reports/return-receipts',
          legacyCode: '5-37',
        },
        {
          code: 'M5.RPT.SR',
          title: 'Sales Return (SR)',
          path: '/sales/reports/returns',
          legacyCode: '5-38',
        },
        {
          code: 'M5.RPT.IC',
          title: 'AR Collection (IC)',
          path: '/sales/reports/ar-collections',
          legacyCode: '5-39',
        },
        {
          code: 'M5.RPT.PV',
          title: 'AR Payment (PV)',
          path: '/sales/reports/ar-payments',
          legacyCode: '5-40',
        },
        {
          code: 'M5.RPT.SIE',
          title: 'Invoice Swap (SIE)',
          path: '/sales/reports/invoice-swaps',
          legacyCode: '5-70',
        },
        {
          code: 'M5.RPT.BALANCE',
          title: 'Opening AR Balance',
          path: '/sales/reports/opening-ar-balance',
          legacyCode: '5-53',
        },
        {
          code: 'M5.RPT.SUMMARY',
          title: 'Sales Summary',
          path: '/sales/reports/summary',
          legacyCode: '5-50',
        },
        {
          code: 'M5.RPT.BY-CUSTOMER',
          title: 'Sales by Customer',
          path: '/sales/reports/by-customer',
          legacyCode: '5-60',
        },
        {
          code: 'M5.RPT.BY-SALESMAN',
          title: 'Sales by Salesman',
          path: '/sales/reports/by-salesman',
          legacyCode: '5-61',
        },
        {
          code: 'M5.RPT.BY-ITEM',
          title: 'Sales by Item',
          path: '/sales/reports/by-item',
          legacyCode: '5-62',
        },
        {
          code: 'M5.RPT.BY-PROJECT',
          title: 'Sales by Project',
          path: '/sales/reports/by-project',
          legacyCode: '5-63',
        },
        {
          code: 'M5.RPT.BY-DIVISION',
          title: 'Sales by Division',
          path: '/sales/reports/by-division',
          legacyCode: '5-64',
        },
        {
          code: 'M5.RPT.BY-CC',
          title: 'Sales by Cost Center',
          path: '/sales/reports/by-cost-center',
          legacyCode: '5-65',
        },
        {
          code: 'M5.RPT.BY-CATEGORY',
          title: 'Sales by Item Category',
          path: '/sales/reports/by-item-category',
          legacyCode: '5-66',
        },
        {
          code: 'M5.RPT.REVENUE',
          title: 'Revenue & Collection',
          path: '/sales/reports/revenue-collection',
          legacyCode: '5-72',
        },
        {
          code: 'M5.RPT.BY-GROUP',
          title: 'Sales by Group',
          path: '/sales/reports/by-group',
          legacyCode: '5-73',
        },
      ],
      m5RptGrp.id,
    );
  }

  // M6 Production — BOM + Work Order (frontend & backend live, 2026-06-03)
  const m6Id = menuIds.get('M6');
  if (m6Id) {
    const m6TxGrp = await upsertMenu({
      code: 'M6.TX',
      title: 'Transaksi',
      type: ErpMenuType.GROUP,
      parentId: m6Id,
      sortOrder: 1,
      legacyCode: '6-2',
    });
    await upsertItems(
      [
        {
          code: 'M6.TX.BOM',
          title: 'Bill of Materials (BOM)',
          path: '/manufacturing/boms',
          legacyCode: '6-3',
        },
        {
          code: 'M6.TX.WO',
          title: 'Work Order (WO)',
          path: '/manufacturing/work-orders',
          legacyCode: '6-4',
        },
      ],
      m6TxGrp.id,
    );
  }

  console.log(`✓ sys_menus (${menuIds.size} entries)`);
  return menuIds;
}

async function seedDocumentNumberings() {
  for (const n of [
    { documentCode: 'GRN', name: 'Goods Receipt Note', prefix: 'GRN', digitCount: 6 },
    { documentCode: 'PO', name: 'Purchase Order', prefix: 'PO', digitCount: 6 },
    { documentCode: 'SO', name: 'Sales Order', prefix: 'SO', digitCount: 6 },
    { documentCode: 'INV', name: 'Sales Invoice', prefix: 'INV', digitCount: 6 },
    { documentCode: 'JV', name: 'Journal Voucher', prefix: 'JV', digitCount: 6 },
    { documentCode: 'RCP', name: 'Receipt', prefix: 'RCP', digitCount: 6 },
    { documentCode: 'PAY', name: 'Payment', prefix: 'PAY', digitCount: 6 },
    { documentCode: 'CASH_RECEIPT', name: 'Kas Masuk (Cash Receipt)', prefix: 'CR', digitCount: 6 },
    {
      documentCode: 'CASH_DISBURSEMENT',
      name: 'Kas Keluar (Cash Disbursement)',
      prefix: 'CD',
      digitCount: 6,
    },
    {
      documentCode: 'BANK_RECEIPT',
      name: 'Bank Masuk (Bank Receipt)',
      prefix: 'RM',
      digitCount: 6,
    },
    {
      documentCode: 'BANK_DISBURSEMENT',
      name: 'Bank Keluar (Bank Disbursement)',
      prefix: 'SM',
      digitCount: 6,
    },
    { documentCode: 'BOM', name: 'Bill of Materials', prefix: 'BOM', digitCount: 6 },
    { documentCode: 'WO', name: 'Work Order', prefix: 'WO', digitCount: 6 },
    // Purchasing advance / freight / schedule + AR collection (Group B — reuse fin_ap/ar_receipts)
    { documentCode: 'AP', name: 'Vendor Advance (AP)', prefix: 'AP', digitCount: 6 },
    { documentCode: 'PP', name: 'Freight Payable (PP)', prefix: 'PP', digitCount: 6 },
    { documentCode: 'VPP', name: 'Payment Schedule (VPP)', prefix: 'VPP', digitCount: 6 },
    { documentCode: 'IC', name: 'AR Collection (IC)', prefix: 'IC', digitCount: 6 },
  ]) {
    await prisma.erpDocumentNumbering.upsert({
      where: { documentCode: n.documentCode },
      create: { ...n, resetPolicy: ErpNumberingReset.YEARLY, nextNumber: 1 },
      update: {},
    });
  }
  console.log('✓ sys_document_numberings');
}

async function seedPermissions(): Promise<Map<string, bigint>> {
  const permIds = new Map<string, bigint>();
  for (const p of [
    { code: 'user.read', name: 'View Users', group: 'admin' },
    { code: 'user.create', name: 'Create Users', group: 'admin' },
    { code: 'user.edit', name: 'Edit Users', group: 'admin' },
    { code: 'user.delete', name: 'Delete Users', group: 'admin' },
    { code: 'role.read', name: 'View Roles', group: 'admin' },
    { code: 'role.create', name: 'Create Roles', group: 'admin' },
    { code: 'role.edit', name: 'Edit Roles', group: 'admin' },
    { code: 'role.delete', name: 'Delete Roles', group: 'admin' },
    { code: 'setting.read', name: 'View Settings', group: 'admin' },
    { code: 'setting.edit', name: 'Edit Settings', group: 'admin' },
    { code: 'fiscal.read', name: 'View Fiscal Periods', group: 'admin' },
    { code: 'fiscal.edit', name: 'Edit Fiscal Periods', group: 'admin' },
    { code: 'branch.read', name: 'View Branches', group: 'master' },
    { code: 'branch.create', name: 'Create Branches', group: 'master' },
    { code: 'branch.edit', name: 'Edit Branches', group: 'master' },
    { code: 'branch.delete', name: 'Delete Branches', group: 'master' },
    { code: 'item.read', name: 'View Items', group: 'master' },
    { code: 'item.create', name: 'Create Items', group: 'master' },
    { code: 'item.edit', name: 'Edit Items', group: 'master' },
    { code: 'item.delete', name: 'Delete Items', group: 'master' },
    { code: 'partner.read', name: 'View Partners', group: 'master' },
    { code: 'partner.create', name: 'Create Partners', group: 'master' },
    { code: 'partner.edit', name: 'Edit Partners', group: 'master' },
    { code: 'partner.delete', name: 'Delete Partners', group: 'master' },
    { code: 'account.read', name: 'View Accounts', group: 'finance' },
    { code: 'account.create', name: 'Create Accounts', group: 'finance' },
    { code: 'account.edit', name: 'Edit Accounts', group: 'finance' },
    { code: 'account.delete', name: 'Delete Accounts', group: 'finance' },
    { code: 'currency.read', name: 'View Currencies', group: 'finance' },
    { code: 'currency.edit', name: 'Edit Currencies', group: 'finance' },
    { code: 'tax.read', name: 'View Taxes', group: 'finance' },
    { code: 'tax.create', name: 'Create Taxes', group: 'finance' },
    { code: 'tax.edit', name: 'Edit Taxes', group: 'finance' },
    { code: 'tax.delete', name: 'Delete Taxes', group: 'finance' },
  ]) {
    const perm = await prisma.erpPermission.upsert({
      where: { code: p.code },
      create: p,
      update: {},
    });
    permIds.set(p.code, perm.id);
  }
  console.log(`✓ adm_permissions (${permIds.size} entries)`);
  return permIds;
}

async function seedRoleAndUser(menuIds: Map<string, bigint>, permIds: Map<string, bigint>) {
  const role = await prisma.erpRole.upsert({
    where: { code: 'SUPERADMIN' },
    create: {
      code: 'SUPERADMIN',
      name: 'Super Administrator',
      description: 'Full system access',
      isActive: true,
    },
    update: {},
  });

  for (const [, permId] of permIds) {
    await prisma.erpRolePermission.upsert({
      where: { roleId_permissionId: { roleId: role.id, permissionId: permId } },
      create: { roleId: role.id, permissionId: permId },
      update: {},
    });
  }

  for (const [, menuId] of menuIds) {
    await prisma.erpRoleMenu.upsert({
      where: { roleId_menuId: { roleId: role.id, menuId } },
      create: {
        roleId: role.id,
        menuId,
        canView: true,
        canCreate: true,
        canEdit: true,
        canDelete: true,
        canApprove: true,
        canPrint: true,
        canExport: true,
        canImport: true,
        isFavorite: false,
      },
      update: {},
    });
  }

  const devUsers = [
    { code: 'ADMIN', name: 'Administrator', email: 'admin@senti-erp.local', password: 'Admin123!' },
    { code: 'rania', name: 'Rania', email: 'rania@senti-erp.local', password: 'sentient' },
  ];

  for (const u of devUsers) {
    const passwordHash = hashPassword(u.password);
    const user = await prisma.erpUser.upsert({
      where: { code: u.code },
      create: {
        code: u.code,
        name: u.name,
        email: u.email,
        passwordHash,
        level: ErpUserLevel.CENTRAL,
        language: 'id',
        isActive: true,
      },
      update: {},
    });
    await prisma.erpUserRole.upsert({
      where: { userId_roleId: { userId: user.id, roleId: role.id } },
      create: { userId: user.id, roleId: role.id },
      update: {},
    });
  }

  // Super admin can create any document starting at any status.
  const ALL_DOC_TYPES = [
    'CASH_RECEIPT',
    'CASH_DISBURSEMENT',
    'BANK_RECEIPT',
    'BANK_DISBURSEMENT',
    'JOURNAL_ENTRY',
    'ADJUSTING_JOURNAL',
    'JOURNAL_MEMO',
    'BANK_BALANCE',
    'REVERSAL',
    'INCOMING_GIRO',
    'OUTGOING_GIRO',
    'GIRO_CLEARING_IN',
    'GIRO_CLEARING_OUT',
    'PUR.PO',
    'PUR.PR',
    'PUR.PI',
    'PUR.GRN',
    'PUR.DNR',
    'PUR.PRT',
    'SLS.SO',
    'SLS.SI',
    'SLS.DO',
    'SLS.RO',
    'SLS.CR',
    'INV.OS',
    'INV.SA',
    'INV.SC',
    'INV.SM',
    'INV.PA',
    'MFG.WO',
  ];
  const SUPERADMIN_STATUSES = ['DRAFT', 'NEED_APPROVE', 'APPROVED'];
  for (const documentType of ALL_DOC_TYPES) {
    await prisma.erpRoleDocPolicy.upsert({
      where: { roleId_documentType: { roleId: role.id, documentType } },
      create: {
        roleId: role.id,
        documentType,
        allowedStatuses: SUPERADMIN_STATUSES,
        isActive: true,
      },
      update: { allowedStatuses: SUPERADMIN_STATUSES, isActive: true },
    });
  }
  console.log('✓ adm_role_doc_policies (SUPERADMIN — all doc types, all creation statuses)');

  console.log('✓ adm_roles, adm_users, adm_user_roles, adm_role_permissions, adm_role_menus');
}

async function seedNotifications() {
  const admin = await prisma.erpUser.findUnique({ where: { email: 'admin@senti-erp.local' } });
  if (!admin) {
    console.warn('  ! admin user not found, skip notifications seed');
    return;
  }
  const existing = await prisma.erpNotification.count({ where: { recipientId: admin.id } });
  if (existing > 0) {
    console.log(`✓ sys_notifications (skip, ${existing} sudah ada untuk admin)`);
    return;
  }
  const now = Date.now();
  const items = [
    {
      type: 'warn',
      title: 'Menunggu persetujuan',
      body: 'CR-2605-2398 (Rp 4.250.000) perlu Anda setujui.',
      referenceEntity: 'fin_ar_receipts',
      actionUrl: '/finance/cash-receipts',
      minutesAgo: 2,
      read: false,
    },
    {
      type: 'success',
      title: 'Dokumen diposting',
      body: 'RM-2605-0871 berhasil diposting oleh fitri.h.',
      referenceEntity: 'fin_receipt_memos',
      actionUrl: '/finance/receipt-memos',
      minutesAgo: 14,
      read: false,
    },
    {
      type: 'danger',
      title: 'Transaksi ditolak',
      body: 'CD-2605-1640 ditolak oleh maya.p — cek catatan.',
      referenceEntity: 'fin_ap_payments',
      actionUrl: '/finance/cash-disbursements',
      minutesAgo: 38,
      read: false,
    },
    {
      type: 'info',
      title: 'Stok menipis',
      body: 'Bearing 6204 di bawah minimum (sisa 12 PCS).',
      referenceEntity: 'md_items',
      actionUrl: '/master/items',
      minutesAgo: 60,
      read: true,
    },
    {
      type: 'info',
      title: 'Giro jatuh tempo',
      body: 'RG-2605-0231 jatuh tempo besok (20/06/2026).',
      referenceEntity: 'fin_giros',
      actionUrl: '/finance/receipt-giros',
      minutesAgo: 180,
      read: true,
    },
    {
      type: 'success',
      title: 'Backup harian selesai',
      body: 'Backup database 02:00 WIB sukses.',
      referenceEntity: null,
      actionUrl: null,
      minutesAgo: 60 * 20,
      read: true,
    },
  ];
  for (const it of items) {
    const createdAt = new Date(now - it.minutesAgo * 60_000);
    await prisma.erpNotification.create({
      data: {
        recipientId: admin.id,
        type: it.type,
        title: it.title,
        body: it.body,
        referenceEntity: it.referenceEntity ?? undefined,
        actionUrl: it.actionUrl ?? undefined,
        createdAt,
        readAt: it.read ? createdAt : null,
      },
    });
  }
  console.log(`✓ sys_notifications (${items.length} contoh untuk admin)`);
}

async function seedDivisions() {
  const divisions = [
    { code: 'PROD', name: 'Produksi' },
    { code: 'QC', name: 'Quality Control' },
    { code: 'GUDLOG', name: 'Gudang & Logistik' },
    { code: 'PURC', name: 'Pengadaan' },
    { code: 'SALES', name: 'Penjualan & Pemasaran' },
    { code: 'FIN', name: 'Keuangan & Akuntansi' },
    { code: 'HR', name: 'Sumber Daya Manusia' },
    { code: 'ENG', name: 'Engineering & R&D' },
    { code: 'IT', name: 'Teknologi Informasi' },
    { code: 'GA', name: 'General Affairs' },
  ];
  const subdivisionsByDivCode: Record<string, { code: string; name: string }[]> = {
    PROD: [
      { code: 'PROD-FAB', name: 'Fabrikasi' },
      { code: 'PROD-ASM', name: 'Perakitan (Assembly)' },
      { code: 'PROD-FIN', name: 'Finishing' },
      { code: 'PROD-MNT', name: 'Pemeliharaan & Perawatan' },
    ],
    QC: [
      { code: 'QC-IQC', name: 'Incoming Quality Control' },
      { code: 'QC-IPQC', name: 'In-Process Quality Control' },
      { code: 'QC-FQC', name: 'Final Quality Control' },
    ],
    GUDLOG: [
      { code: 'GUDLOG-BB', name: 'Gudang Bahan Baku' },
      { code: 'GUDLOG-BJ', name: 'Gudang Barang Jadi' },
      { code: 'GUDLOG-SP', name: 'Gudang Spare Parts' },
      { code: 'GUDLOG-EXP', name: 'Ekspedisi & Pengiriman' },
    ],
    PURC: [
      { code: 'PURC-LOK', name: 'Pembelian Lokal' },
      { code: 'PURC-IMP', name: 'Pembelian Impor' },
    ],
    SALES: [
      { code: 'SALES-DOM', name: 'Sales Domestik' },
      { code: 'SALES-EXP', name: 'Sales Ekspor' },
      { code: 'SALES-CS', name: 'Customer Service' },
    ],
    FIN: [
      { code: 'FIN-AKT', name: 'Akuntansi' },
      { code: 'FIN-KAU', name: 'Keuangan' },
      { code: 'FIN-TAX', name: 'Perpajakan' },
    ],
    HR: [
      { code: 'HR-RKT', name: 'Rekrutmen & Seleksi' },
      { code: 'HR-PAY', name: 'Payroll & Kompensasi' },
      { code: 'HR-TRN', name: 'Pelatihan & Pengembangan' },
    ],
    ENG: [
      { code: 'ENG-RD', name: 'Riset & Pengembangan' },
      { code: 'ENG-DSN', name: 'Desain & Perencanaan' },
      { code: 'ENG-PE', name: 'Process Engineering' },
    ],
    IT: [
      { code: 'IT-INF', name: 'Infrastruktur & Jaringan' },
      { code: 'IT-DEV', name: 'Pengembangan Sistem' },
    ],
    GA: [
      { code: 'GA-UMM', name: 'Umum & Kantor' },
      { code: 'GA-K3', name: 'Keamanan & K3' },
    ],
  };

  const divMap: Record<string, bigint> = {};
  for (const d of divisions) {
    const row = await prisma.erpDivision.upsert({
      where: { code: d.code },
      create: { code: d.code, name: d.name, isActive: true },
      update: { name: d.name, isActive: true, deletedAt: null },
    });
    divMap[d.code] = row.id;
  }
  for (const [divCode, subs] of Object.entries(subdivisionsByDivCode)) {
    const divisionId = divMap[divCode];
    for (const s of subs) {
      await prisma.erpSubdivision.upsert({
        where: { code: s.code },
        create: { code: s.code, name: s.name, divisionId, isActive: true },
        update: { name: s.name, divisionId, isActive: true, deletedAt: null },
      });
    }
  }
  console.log(
    `✓ md_divisions (${divisions.length}) + md_subdivisions (${Object.values(subdivisionsByDivCode).flat().length})`,
  );
}

async function seedLocations() {
  const branchCodes = [
    'HQ',
    'BDG',
    'SBY',
    'MKS',
    'MDN',
    'SMG',
    'YGY',
    'PLM',
    'DPS',
    'BPN',
    'PKU',
    'PNK',
  ];
  const branchMap = new Map<string, bigint>();
  for (const code of branchCodes) {
    const b = await prisma.erpBranch.findUnique({ where: { code }, select: { id: true } });
    if (b) branchMap.set(code, b.id);
  }

  // Hapus child records (FK non-nullable) sebelum delete locations
  await prisma.erpWarehouse.deleteMany({});
  await prisma.erpLocation.deleteMany({});

  const locs: {
    code: string;
    name: string;
    bc: string;
    addr?: string;
    city?: string;
    postal?: string;
    phone?: string;
    notes?: string;
    active?: boolean;
  }[] = [
    // ── HQ Jakarta (12) ──────────────────────────────────────────────
    {
      code: 'HQ-KTR-01',
      name: 'Kantor Pusat - Gedung Utama',
      bc: 'HQ',
      addr: 'Jl. Jend. Sudirman No. 1',
      city: 'Jakarta Pusat',
      postal: '10220',
      phone: '021-5700001',
      notes: 'Lantai 1-5 operasional',
    },
    {
      code: 'HQ-KTR-02',
      name: 'Kantor Pusat - Gedung Annex',
      bc: 'HQ',
      addr: 'Jl. Jend. Sudirman No. 1A',
      city: 'Jakarta Pusat',
      postal: '10220',
      phone: '021-5700010',
      notes: 'Lantai 6-10 divisi keuangan',
    },
    {
      code: 'HQ-GDG-01',
      name: 'Gudang Pusat Jakarta',
      bc: 'HQ',
      addr: 'Jl. Raya Cakung No. 88',
      city: 'Jakarta Timur',
      postal: '13910',
      phone: '021-4601101',
      notes: 'Gudang stok utama',
    },
    {
      code: 'HQ-GDG-02',
      name: 'Gudang Distribusi Jakarta Utara',
      bc: 'HQ',
      addr: 'Jl. Industri Raya No. 12',
      city: 'Jakarta Utara',
      postal: '14430',
      phone: '021-6901201',
      notes: 'Gudang area Tanjung Priok',
    },
    {
      code: 'HQ-GDG-03',
      name: 'Gudang Logistik Cakung',
      bc: 'HQ',
      addr: 'Kawasan Industri Cakung Blok C-5',
      city: 'Jakarta Timur',
      postal: '13930',
      phone: '021-4608301',
      notes: 'Penyimpanan bahan baku',
    },
    {
      code: 'HQ-DCT-01',
      name: 'Distribution Center Marunda',
      bc: 'HQ',
      addr: 'Jl. Marunda Raya No. 45',
      city: 'Jakarta Utara',
      postal: '14150',
      phone: '021-4403401',
      notes: 'Pusat distribusi wilayah Jawa',
    },
    {
      code: 'HQ-SHR-01',
      name: 'Showroom Jakarta Selatan',
      bc: 'HQ',
      addr: 'Jl. TB Simatupang No. 20',
      city: 'Jakarta Selatan',
      postal: '12430',
      phone: '021-7884501',
      notes: 'Showroom produk premium',
    },
    {
      code: 'HQ-SHR-02',
      name: 'Showroom Kelapa Gading',
      bc: 'HQ',
      addr: 'Jl. Boulevard Raya No. 33',
      city: 'Jakarta Utara',
      postal: '14240',
      phone: '021-4506601',
      notes: 'Showroom produk reguler',
    },
    {
      code: 'HQ-WRK-01',
      name: 'Workshop Cakung',
      bc: 'HQ',
      addr: 'Jl. Penggilingan Raya No. 7',
      city: 'Jakarta Timur',
      postal: '13940',
      phone: '021-4601701',
      notes: 'Bengkel dan servis',
    },
    {
      code: 'HQ-CTR-01',
      name: 'Counter Sales Mangga Dua',
      bc: 'HQ',
      addr: 'Jl. Mangga Dua Raya No. 5',
      city: 'Jakarta Utara',
      postal: '14430',
      phone: '021-6012801',
      notes: 'Counter penjualan grosir',
    },
    {
      code: 'HQ-CTR-02',
      name: 'Counter Sales Glodok',
      bc: 'HQ',
      addr: 'Jl. Hayam Wuruk No. 108',
      city: 'Jakarta Barat',
      postal: '11180',
      phone: '021-6008901',
      notes: 'Counter elektronik & spare part',
    },
    {
      code: 'HQ-CTR-03',
      name: 'Counter Sales Puri Indah',
      bc: 'HQ',
      addr: 'Puri Indah Business Park Blok A-12',
      city: 'Jakarta Barat',
      postal: '11610',
      phone: '021-5839001',
      notes: 'Counter wilayah Jakarta Barat',
    },
    // ── SBY Surabaya (10) ─────────────────────────────────────────────
    {
      code: 'SBY-KTR-01',
      name: 'Kantor Cabang Surabaya',
      bc: 'SBY',
      addr: 'Jl. Pemuda No. 27',
      city: 'Surabaya',
      postal: '60271',
      phone: '031-5350201',
      notes: 'Kantor utama cabang',
    },
    {
      code: 'SBY-KTR-02',
      name: 'Kantor Area Jawa Timur',
      bc: 'SBY',
      addr: 'Jl. Basuki Rachmat No. 55',
      city: 'Surabaya',
      postal: '60261',
      phone: '031-5476301',
      notes: 'Kantor koordinasi area Jatim',
    },
    {
      code: 'SBY-GDG-01',
      name: 'Gudang Utama Surabaya',
      bc: 'SBY',
      addr: 'Jl. Margomulyo No. 18',
      city: 'Surabaya',
      postal: '60185',
      phone: '031-7490401',
      notes: 'Gudang stok utama Surabaya',
    },
    {
      code: 'SBY-GDG-02',
      name: 'Gudang Distribusi Surabaya Barat',
      bc: 'SBY',
      addr: 'Jl. Raya Banjaran No. 99',
      city: 'Surabaya',
      postal: '60211',
      phone: '031-7402501',
      notes: 'Gudang distribusi barat',
    },
    {
      code: 'SBY-GDG-03',
      name: 'Gudang Transit Juanda',
      bc: 'SBY',
      addr: 'Jl. Raya Juanda No. 5',
      city: 'Sidoarjo',
      postal: '61253',
      phone: '031-8685601',
      notes: 'Transit area bandara Juanda',
    },
    {
      code: 'SBY-SHR-01',
      name: 'Showroom Surabaya',
      bc: 'SBY',
      addr: 'Jl. Tunjungan No. 40',
      city: 'Surabaya',
      postal: '60275',
      phone: '031-5340701',
      notes: 'Showroom utama Surabaya',
    },
    {
      code: 'SBY-WRK-01',
      name: 'Workshop Rungkut',
      bc: 'SBY',
      addr: 'Kawasan Industri SIER Blok D-3',
      city: 'Surabaya',
      postal: '60291',
      phone: '031-8494801',
      notes: 'Workshop & service center',
    },
    {
      code: 'SBY-CTR-01',
      name: 'Counter Sales ITC Surabaya',
      bc: 'SBY',
      addr: 'Jl. Gembong No. 20 Lt. 2 Kios 35',
      city: 'Surabaya',
      postal: '60133',
      phone: '031-3534901',
      notes: 'Counter ITC Surabaya',
    },
    {
      code: 'SBY-CTR-02',
      name: 'Counter Sales Pakuwon',
      bc: 'SBY',
      addr: 'Jl. Lontar No. 2 Ruko Pakuwon B-7',
      city: 'Surabaya',
      postal: '60213',
      phone: '031-7401001',
      notes: 'Counter wilayah Surabaya Barat',
    },
    {
      code: 'SBY-DCT-01',
      name: 'Distribution Center Sidoarjo',
      bc: 'SBY',
      addr: 'Jl. Raya Betro No. 101',
      city: 'Sidoarjo',
      postal: '61219',
      phone: '031-8056101',
      notes: 'DC untuk distribusi Jatim',
    },
    // ── BDG Bandung (9) ───────────────────────────────────────────────
    {
      code: 'BDG-KTR-01',
      name: 'Kantor Cabang Bandung',
      bc: 'BDG',
      addr: 'Jl. Asia Afrika No. 45',
      city: 'Bandung',
      postal: '40111',
      phone: '022-4200101',
      notes: 'Kantor utama cabang Bandung',
    },
    {
      code: 'BDG-GDG-01',
      name: 'Gudang Utama Bandung',
      bc: 'BDG',
      addr: 'Jl. Soekarno Hatta No. 123',
      city: 'Bandung',
      postal: '40223',
      phone: '022-6030201',
      notes: 'Gudang stok utama Bandung',
    },
    {
      code: 'BDG-GDG-02',
      name: 'Gudang Distribusi Cimahi',
      bc: 'BDG',
      addr: 'Jl. Industri No. 17',
      city: 'Cimahi',
      postal: '40534',
      phone: '022-6641301',
      notes: 'Gudang distribusi barat Bandung',
    },
    {
      code: 'BDG-SHR-01',
      name: 'Showroom Bandung',
      bc: 'BDG',
      addr: 'Jl. Braga No. 30',
      city: 'Bandung',
      postal: '40111',
      phone: '022-4234401',
      notes: 'Showroom pusat kota Bandung',
    },
    {
      code: 'BDG-WRK-01',
      name: 'Workshop Dayeuhkolot',
      bc: 'BDG',
      addr: 'Jl. Raya Dayeuhkolot No. 88',
      city: 'Bandung',
      postal: '40256',
      phone: '022-5202501',
      notes: 'Bengkel dan servis Bandung',
    },
    {
      code: 'BDG-CTR-01',
      name: 'Counter Sales BTC Bandung',
      bc: 'BDG',
      addr: 'Jl. Purnawarman No. 13 Lt. 1',
      city: 'Bandung',
      postal: '40117',
      phone: '022-4218601',
      notes: 'Counter BTC Bandung',
    },
    {
      code: 'BDG-CTR-02',
      name: 'Counter Sales Paris Van Java',
      bc: 'BDG',
      addr: 'Jl. Sukajadi No. 137 Kios B-24',
      city: 'Bandung',
      postal: '40162',
      phone: '022-2033701',
      notes: 'Counter Paris Van Java Mall',
    },
    {
      code: 'BDG-DCT-01',
      name: 'Distribution Center Gedebage',
      bc: 'BDG',
      addr: 'Kawasan Industri Gedebage Blok E-2',
      city: 'Bandung',
      postal: '40294',
      phone: '022-7832801',
      notes: 'DC distribusi Jawa Barat Timur',
    },
    {
      code: 'BDG-CTR-03',
      name: 'Counter Sales Cimahi',
      bc: 'BDG',
      addr: 'Jl. Jend. H. Amir Machmud No. 55',
      city: 'Cimahi',
      postal: '40526',
      phone: '022-6630901',
      notes: 'Counter wilayah Cimahi',
    },
    // ── MKS Makassar (9) ──────────────────────────────────────────────
    {
      code: 'MKS-KTR-01',
      name: 'Kantor Cabang Makassar',
      bc: 'MKS',
      addr: 'Jl. Sam Ratulangi No. 8',
      city: 'Makassar',
      postal: '90111',
      phone: '0411-871301',
      notes: 'Kantor utama cabang Makassar',
    },
    {
      code: 'MKS-KTR-02',
      name: 'Kantor Area Sulawesi Selatan',
      bc: 'MKS',
      addr: 'Jl. Pettarani No. 50',
      city: 'Makassar',
      postal: '90222',
      phone: '0411-452201',
      notes: 'Koordinasi area Sulsel',
    },
    {
      code: 'MKS-GDG-01',
      name: 'Gudang Utama Makassar',
      bc: 'MKS',
      addr: 'Jl. Kima Raya No. 33',
      city: 'Makassar',
      postal: '90241',
      phone: '0411-511301',
      notes: 'Gudang stok utama Makassar',
    },
    {
      code: 'MKS-GDG-02',
      name: 'Gudang Distribusi Maros',
      bc: 'MKS',
      addr: 'Jl. Poros Makassar-Maros Km. 25',
      city: 'Maros',
      postal: '90514',
      phone: '0411-373401',
      notes: 'Gudang distribusi Sulsel Utara',
    },
    {
      code: 'MKS-SHR-01',
      name: 'Showroom Makassar',
      bc: 'MKS',
      addr: 'Jl. A.P. Pettarani No. 22',
      city: 'Makassar',
      postal: '90222',
      phone: '0411-456501',
      notes: 'Showroom utama Makassar',
    },
    {
      code: 'MKS-WRK-01',
      name: 'Workshop Antang',
      bc: 'MKS',
      addr: 'Jl. Antang Raya No. 77',
      city: 'Makassar',
      postal: '90231',
      phone: '0411-492601',
      notes: 'Bengkel dan servis Makassar',
    },
    {
      code: 'MKS-CTR-01',
      name: 'Counter Sales Mall Ratu Indah',
      bc: 'MKS',
      addr: 'Jl. Urip Sumoharjo Kios C-12',
      city: 'Makassar',
      postal: '90232',
      phone: '0411-443701',
      notes: 'Counter Mall Ratu Indah',
    },
    {
      code: 'MKS-CTR-02',
      name: 'Counter Sales Somba Opu',
      bc: 'MKS',
      addr: 'Jl. Somba Opu No. 140',
      city: 'Makassar',
      postal: '90111',
      phone: '0411-363801',
      notes: 'Counter kawasan bisnis Somba Opu',
    },
    {
      code: 'MKS-DCT-01',
      name: 'Distribution Center Maros',
      bc: 'MKS',
      addr: 'Kawasan Industri Maros Blok A-8',
      city: 'Maros',
      postal: '90512',
      phone: '0411-374901',
      notes: 'DC distribusi Indonesia Timur',
    },
    // ── MDN Medan (8) ─────────────────────────────────────────────────
    {
      code: 'MDN-KTR-01',
      name: 'Kantor Cabang Medan',
      bc: 'MDN',
      addr: 'Jl. Gatot Subroto No. 12',
      city: 'Medan',
      postal: '20112',
      phone: '061-4521401',
      notes: 'Kantor utama cabang Medan',
    },
    {
      code: 'MDN-GDG-01',
      name: 'Gudang Utama Medan',
      bc: 'MDN',
      addr: 'Jl. Glugur Raya No. 66',
      city: 'Medan',
      postal: '20214',
      phone: '061-6626501',
      notes: 'Gudang stok utama Medan',
    },
    {
      code: 'MDN-GDG-02',
      name: 'Gudang Distribusi Belawan',
      bc: 'MDN',
      addr: 'Jl. Pelabuhan Raya No. 30',
      city: 'Medan',
      postal: '20411',
      phone: '061-6943601',
      notes: 'Gudang area pelabuhan Belawan',
    },
    {
      code: 'MDN-SHR-01',
      name: 'Showroom Medan',
      bc: 'MDN',
      addr: 'Jl. Iskandar Muda No. 44',
      city: 'Medan',
      postal: '20154',
      phone: '061-4155701',
      notes: 'Showroom utama Medan',
    },
    {
      code: 'MDN-WRK-01',
      name: 'Workshop Helvetia',
      bc: 'MDN',
      addr: 'Jl. Asrama No. 88',
      city: 'Medan',
      postal: '20124',
      phone: '061-8468801',
      notes: 'Bengkel dan servis Medan',
    },
    {
      code: 'MDN-CTR-01',
      name: 'Counter Sales Plaza Medan Fair',
      bc: 'MDN',
      addr: 'Jl. Gatot Subroto No. 30 Lt. 1',
      city: 'Medan',
      postal: '20112',
      phone: '061-4527901',
      notes: 'Counter Plaza Medan Fair',
    },
    {
      code: 'MDN-CTR-02',
      name: 'Counter Sales Sun Plaza',
      bc: 'MDN',
      addr: 'Jl. K.H. Zainul Arifin No. 7 Lt. 2',
      city: 'Medan',
      postal: '20153',
      phone: '061-4574001',
      notes: 'Counter Sun Plaza Medan',
    },
    {
      code: 'MDN-DCT-01',
      name: 'Distribution Center Belawan',
      bc: 'MDN',
      addr: 'Kawasan Industri Belawan Blok C-3',
      city: 'Medan',
      postal: '20411',
      phone: '061-6942101',
      notes: 'DC distribusi Sumatera Utara',
    },
    // ── SMG Semarang (8) ──────────────────────────────────────────────
    {
      code: 'SMG-KTR-01',
      name: 'Kantor Cabang Semarang',
      bc: 'SMG',
      addr: 'Jl. Pandanaran No. 55',
      city: 'Semarang',
      postal: '50134',
      phone: '024-8310501',
      notes: 'Kantor utama cabang Semarang',
    },
    {
      code: 'SMG-GDG-01',
      name: 'Gudang Utama Semarang',
      bc: 'SMG',
      addr: 'Jl. Kaligawe Raya No. 101',
      city: 'Semarang',
      postal: '50112',
      phone: '024-6583601',
      notes: 'Gudang stok utama Semarang',
    },
    {
      code: 'SMG-GDG-02',
      name: 'Gudang Distribusi Kaligawe',
      bc: 'SMG',
      addr: 'Kawasan Industri Semarang Blok B-4',
      city: 'Semarang',
      postal: '50119',
      phone: '024-6584701',
      notes: 'Gudang distribusi Jateng',
    },
    {
      code: 'SMG-SHR-01',
      name: 'Showroom Semarang',
      bc: 'SMG',
      addr: 'Jl. Ahmad Yani No. 150',
      city: 'Semarang',
      postal: '50174',
      phone: '024-8314801',
      notes: 'Showroom utama Semarang',
    },
    {
      code: 'SMG-WRK-01',
      name: 'Workshop Terboyo',
      bc: 'SMG',
      addr: 'Jl. Terboyo Industri No. 22',
      city: 'Semarang',
      postal: '50115',
      phone: '024-6592901',
      notes: 'Bengkel dan servis Semarang',
    },
    {
      code: 'SMG-CTR-01',
      name: 'Counter Sales DP Mall',
      bc: 'SMG',
      addr: 'Jl. Pemuda No. 150 Lt. 2 Kios 14',
      city: 'Semarang',
      postal: '50132',
      phone: '024-3520001',
      notes: 'Counter DP Mall Semarang',
    },
    {
      code: 'SMG-CTR-02',
      name: 'Counter Sales Paragon Mall',
      bc: 'SMG',
      addr: 'Jl. Pemuda No. 118 Lt. 3 Kios 21',
      city: 'Semarang',
      postal: '50132',
      phone: '024-3551101',
      notes: 'Counter Paragon City Mall',
    },
    {
      code: 'SMG-DCT-01',
      name: 'Distribution Center Kaligawe',
      bc: 'SMG',
      addr: 'Jl. Raya Semarang-Demak Km. 8',
      city: 'Semarang',
      postal: '50118',
      phone: '024-6595201',
      notes: 'DC distribusi Jawa Tengah',
    },
    // ── YGY Yogyakarta (7) ────────────────────────────────────────────
    {
      code: 'YGY-KTR-01',
      name: 'Kantor Cabang Yogyakarta',
      bc: 'YGY',
      addr: 'Jl. Malioboro No. 20',
      city: 'Yogyakarta',
      postal: '55213',
      phone: '0274-561601',
      notes: 'Kantor utama cabang Yogyakarta',
    },
    {
      code: 'YGY-GDG-01',
      name: 'Gudang Utama Yogyakarta',
      bc: 'YGY',
      addr: 'Jl. Ringroad Utara No. 77',
      city: 'Yogyakarta',
      postal: '55283',
      phone: '0274-868701',
      notes: 'Gudang stok utama Yogyakarta',
    },
    {
      code: 'YGY-GDG-02',
      name: 'Gudang Distribusi Maguwoharjo',
      bc: 'YGY',
      addr: 'Jl. Raya Maguwoharjo No. 45',
      city: 'Sleman',
      postal: '55282',
      phone: '0274-496801',
      notes: 'Gudang distribusi DIY',
    },
    {
      code: 'YGY-SHR-01',
      name: 'Showroom Yogyakarta',
      bc: 'YGY',
      addr: 'Jl. Solo No. 55',
      city: 'Yogyakarta',
      postal: '55281',
      phone: '0274-580901',
      notes: 'Showroom utama Yogyakarta',
    },
    {
      code: 'YGY-WRK-01',
      name: 'Workshop Condongcatur',
      bc: 'YGY',
      addr: 'Jl. Seturan Raya No. 12',
      city: 'Sleman',
      postal: '55281',
      phone: '0274-487001',
      notes: 'Bengkel dan servis Yogyakarta',
    },
    {
      code: 'YGY-CTR-01',
      name: 'Counter Sales Ambarrukmo Plaza',
      bc: 'YGY',
      addr: 'Jl. Laksda Adisucipto No. 32 Lt. 1',
      city: 'Yogyakarta',
      postal: '55281',
      phone: '0274-488101',
      notes: 'Counter Ambarrukmo Plaza',
    },
    {
      code: 'YGY-CTR-02',
      name: 'Counter Sales Malioboro Mall',
      bc: 'YGY',
      addr: 'Jl. Malioboro No. 52 Lt. 1 Kios 8',
      city: 'Yogyakarta',
      postal: '55213',
      phone: '0274-562201',
      notes: 'Counter Malioboro Mall',
    },
    // ── PLM Palembang (7) ─────────────────────────────────────────────
    {
      code: 'PLM-KTR-01',
      name: 'Kantor Cabang Palembang',
      bc: 'PLM',
      addr: 'Jl. Jend. Sudirman No. 88',
      city: 'Palembang',
      postal: '30126',
      phone: '0711-357701',
      notes: 'Kantor utama cabang Palembang',
    },
    {
      code: 'PLM-GDG-01',
      name: 'Gudang Utama Palembang',
      bc: 'PLM',
      addr: 'Jl. Mayor Ruslan No. 50',
      city: 'Palembang',
      postal: '30137',
      phone: '0711-711801',
      notes: 'Gudang stok utama Palembang',
    },
    {
      code: 'PLM-GDG-02',
      name: 'Gudang Distribusi Boom Baru',
      bc: 'PLM',
      addr: 'Jl. Rumah Bari No. 15',
      city: 'Palembang',
      postal: '30121',
      phone: '0711-352901',
      notes: 'Gudang area pelabuhan Boom Baru',
    },
    {
      code: 'PLM-SHR-01',
      name: 'Showroom Palembang',
      bc: 'PLM',
      addr: 'Jl. Demang Lebar Daun No. 25',
      city: 'Palembang',
      postal: '30137',
      phone: '0711-813001',
      notes: 'Showroom utama Palembang',
    },
    {
      code: 'PLM-WRK-01',
      name: 'Workshop Jakabaring',
      bc: 'PLM',
      addr: 'Jl. Jakabaring Raya No. 8',
      city: 'Palembang',
      postal: '30262',
      phone: '0711-511101',
      notes: 'Bengkel dan servis Palembang',
    },
    {
      code: 'PLM-CTR-01',
      name: 'Counter Sales Palembang Trade Center',
      bc: 'PLM',
      addr: 'Jl. Basuki Rahmat No. 1 Lt. 1',
      city: 'Palembang',
      postal: '30126',
      phone: '0711-356201',
      notes: 'Counter PTC Palembang',
    },
    {
      code: 'PLM-DCT-01',
      name: 'Distribution Center Boom Baru',
      bc: 'PLM',
      addr: 'Kawasan Industri Boom Baru Blok D-2',
      city: 'Palembang',
      postal: '30121',
      phone: '0711-353301',
      notes: 'DC distribusi Sumatera Selatan',
    },
    // ── DPS Denpasar (8) ──────────────────────────────────────────────
    {
      code: 'DPS-KTR-01',
      name: 'Kantor Cabang Denpasar',
      bc: 'DPS',
      addr: 'Jl. Raya Puputan No. 33',
      city: 'Denpasar',
      postal: '80235',
      phone: '0361-225801',
      notes: 'Kantor utama cabang Denpasar',
    },
    {
      code: 'DPS-KTR-02',
      name: 'Kantor Area Bali',
      bc: 'DPS',
      addr: 'Jl. Teuku Umar No. 66',
      city: 'Denpasar',
      postal: '80113',
      phone: '0361-264901',
      notes: 'Koordinasi area Bali-Nusra',
    },
    {
      code: 'DPS-GDG-01',
      name: 'Gudang Utama Denpasar',
      bc: 'DPS',
      addr: 'Jl. Cargo Raya No. 20',
      city: 'Denpasar',
      postal: '80115',
      phone: '0361-412001',
      notes: 'Gudang stok utama Denpasar',
    },
    {
      code: 'DPS-GDG-02',
      name: 'Gudang Distribusi Sanur',
      bc: 'DPS',
      addr: 'Jl. By Pass Ngurah Rai No. 55',
      city: 'Denpasar',
      postal: '80222',
      phone: '0361-285101',
      notes: 'Gudang distribusi Bali Selatan',
    },
    {
      code: 'DPS-SHR-01',
      name: 'Showroom Denpasar',
      bc: 'DPS',
      addr: 'Jl. Diponegoro No. 12',
      city: 'Denpasar',
      postal: '80113',
      phone: '0361-227201',
      notes: 'Showroom utama Denpasar',
    },
    {
      code: 'DPS-WRK-01',
      name: 'Workshop Pemogan',
      bc: 'DPS',
      addr: 'Jl. Raya Pemogan No. 40',
      city: 'Denpasar',
      postal: '80221',
      phone: '0361-723301',
      notes: 'Bengkel dan servis Denpasar',
    },
    {
      code: 'DPS-CTR-01',
      name: 'Counter Sales Kuta Square',
      bc: 'DPS',
      addr: 'Jl. Bakungsari No. 1 Kios D-5',
      city: 'Badung',
      postal: '80361',
      phone: '0361-753401',
      notes: 'Counter Kuta Square Bali',
    },
    {
      code: 'DPS-DCT-01',
      name: 'Distribution Center Sanur',
      bc: 'DPS',
      addr: 'Jl. Hang Tuah No. 77',
      city: 'Denpasar',
      postal: '80227',
      phone: '0361-289501',
      notes: 'DC distribusi Bali & NTB',
    },
    // ── BPN Balikpapan (7) ────────────────────────────────────────────
    {
      code: 'BPN-KTR-01',
      name: 'Kantor Cabang Balikpapan',
      bc: 'BPN',
      addr: 'Jl. Jend. Sudirman No. 15',
      city: 'Balikpapan',
      postal: '76114',
      phone: '0542-736901',
      notes: 'Kantor utama cabang Balikpapan',
    },
    {
      code: 'BPN-GDG-01',
      name: 'Gudang Utama Balikpapan',
      bc: 'BPN',
      addr: 'Jl. Soekarno Hatta KM. 7 No. 30',
      city: 'Balikpapan',
      postal: '76126',
      phone: '0542-777001',
      notes: 'Gudang stok utama Balikpapan',
    },
    {
      code: 'BPN-GDG-02',
      name: 'Gudang Distribusi Kariangau',
      bc: 'BPN',
      addr: 'Kawasan Industri Kariangau Blok B-3',
      city: 'Balikpapan',
      postal: '76117',
      phone: '0542-884101',
      notes: 'Gudang distribusi Kaltim',
    },
    {
      code: 'BPN-SHR-01',
      name: 'Showroom Balikpapan',
      bc: 'BPN',
      addr: 'Jl. Ahmad Yani No. 40',
      city: 'Balikpapan',
      postal: '76112',
      phone: '0542-733201',
      notes: 'Showroom utama Balikpapan',
    },
    {
      code: 'BPN-WRK-01',
      name: 'Workshop Klandasan',
      bc: 'BPN',
      addr: 'Jl. Letjen S. Parman No. 12',
      city: 'Balikpapan',
      postal: '76111',
      phone: '0542-427301',
      notes: 'Bengkel dan servis Balikpapan',
    },
    {
      code: 'BPN-CTR-01',
      name: 'Counter Sales Mall Fantasi',
      bc: 'BPN',
      addr: 'Jl. Ahmad Yani No. 5 Lt. 2 Kios 8',
      city: 'Balikpapan',
      postal: '76112',
      phone: '0542-735401',
      notes: 'Counter Mall Fantasi Balikpapan',
    },
    {
      code: 'BPN-DCT-01',
      name: 'Distribution Center Kariangau',
      bc: 'BPN',
      addr: 'Kawasan Industri Kariangau Blok A-7',
      city: 'Balikpapan',
      postal: '76117',
      phone: '0542-884501',
      notes: 'DC distribusi Kalimantan Timur',
    },
    // ── PKU Pekanbaru (8, branch non-aktif) ───────────────────────────
    {
      code: 'PKU-KTR-01',
      name: 'Kantor Cabang Pekanbaru',
      bc: 'PKU',
      addr: 'Jl. Jend. Ahmad Yani No. 7',
      city: 'Pekanbaru',
      postal: '28156',
      phone: '0761-857001',
      notes: 'Kantor utama cabang Pekanbaru',
      active: false,
    },
    {
      code: 'PKU-GDG-01',
      name: 'Gudang Utama Pekanbaru',
      bc: 'PKU',
      addr: 'Jl. Soekarno Hatta No. 55',
      city: 'Pekanbaru',
      postal: '28289',
      phone: '0761-673101',
      notes: 'Gudang stok utama Pekanbaru',
      active: false,
    },
    {
      code: 'PKU-GDG-02',
      name: 'Gudang Distribusi Rumbai',
      bc: 'PKU',
      addr: 'Jl. Sekolah No. 40',
      city: 'Pekanbaru',
      postal: '28262',
      phone: '0761-551201',
      notes: 'Gudang distribusi Riau',
      active: false,
    },
    {
      code: 'PKU-SHR-01',
      name: 'Showroom Pekanbaru',
      bc: 'PKU',
      addr: 'Jl. Tuanku Tambusai No. 20',
      city: 'Pekanbaru',
      postal: '28282',
      phone: '0761-856301',
      notes: 'Showroom utama Pekanbaru',
      active: false,
    },
    {
      code: 'PKU-WRK-01',
      name: 'Workshop Marpoyan',
      bc: 'PKU',
      addr: 'Jl. Soekarno Hatta KM. 3 No. 15',
      city: 'Pekanbaru',
      postal: '28284',
      phone: '0761-689401',
      notes: 'Bengkel dan servis Pekanbaru',
      active: false,
    },
    {
      code: 'PKU-CTR-01',
      name: 'Counter Sales Metropolitan Pekanbaru',
      bc: 'PKU',
      addr: 'Jl. Tuanku Tambusai Ujung No. 1',
      city: 'Pekanbaru',
      postal: '28282',
      phone: '0761-858501',
      notes: 'Counter Metropolitan Mall',
      active: false,
    },
    {
      code: 'PKU-CTR-02',
      name: 'Counter Sales Plaza Senapelan',
      bc: 'PKU',
      addr: 'Jl. Ahmad Yani No. 60 Lt. 1',
      city: 'Pekanbaru',
      postal: '28156',
      phone: '0761-857601',
      notes: 'Counter Plaza Senapelan',
      active: false,
    },
    {
      code: 'PKU-DCT-01',
      name: 'Distribution Center Rumbai',
      bc: 'PKU',
      addr: 'Kawasan Industri Rumbai Blok C-1',
      city: 'Pekanbaru',
      postal: '28266',
      phone: '0761-554701',
      notes: 'DC distribusi Riau',
      active: false,
    },
    // ── PNK Pontianak (7, branch non-aktif) ───────────────────────────
    {
      code: 'PNK-KTR-01',
      name: 'Kantor Cabang Pontianak',
      bc: 'PNK',
      addr: 'Jl. Tanjungpura No. 66',
      city: 'Pontianak',
      postal: '78112',
      phone: '0561-748101',
      notes: 'Kantor utama cabang Pontianak',
      active: false,
    },
    {
      code: 'PNK-GDG-01',
      name: 'Gudang Utama Pontianak',
      bc: 'PNK',
      addr: 'Jl. Khatulistiwa No. 88',
      city: 'Pontianak',
      postal: '78243',
      phone: '0561-884201',
      notes: 'Gudang stok utama Pontianak',
      active: false,
    },
    {
      code: 'PNK-GDG-02',
      name: 'Gudang Distribusi Siantan',
      bc: 'PNK',
      addr: 'Jl. Siantan Hulu No. 30',
      city: 'Pontianak',
      postal: '78244',
      phone: '0561-881301',
      notes: 'Gudang distribusi Kalbar',
      active: false,
    },
    {
      code: 'PNK-SHR-01',
      name: 'Showroom Pontianak',
      bc: 'PNK',
      addr: 'Jl. Ahmad Yani No. 98',
      city: 'Pontianak',
      postal: '78124',
      phone: '0561-732401',
      notes: 'Showroom utama Pontianak',
      active: false,
    },
    {
      code: 'PNK-WRK-01',
      name: 'Workshop Sei Ambawang',
      bc: 'PNK',
      addr: 'Jl. Tanjung Raya II No. 12',
      city: 'Pontianak',
      postal: '78241',
      phone: '0561-882501',
      notes: 'Bengkel dan servis Pontianak',
      active: false,
    },
    {
      code: 'PNK-CTR-01',
      name: 'Counter Sales Ayani Mega Mall',
      bc: 'PNK',
      addr: 'Jl. Ahmad Yani No. 49 Lt. 1 Kios 6',
      city: 'Pontianak',
      postal: '78124',
      phone: '0561-737601',
      notes: 'Counter Ayani Mega Mall',
      active: false,
    },
    {
      code: 'PNK-DCT-01',
      name: 'Distribution Center Siantan',
      bc: 'PNK',
      addr: 'Kawasan Industri Siantan Blok A-4',
      city: 'Pontianak',
      postal: '78244',
      phone: '0561-883701',
      notes: 'DC distribusi Kalimantan Barat',
      active: false,
    },
  ];

  for (const { bc, addr, active, postal, ...d } of locs) {
    const branchId = branchMap.get(bc);
    if (!branchId) continue;
    const payload = {
      ...d,
      branchId,
      addressLine1: addr,
      postalCode: postal,
      isActive: active ?? true,
    };
    await prisma.erpLocation.upsert({
      where: { code: d.code },
      create: payload,
      update: payload,
    });
  }
  console.log(`✓ md_locations (${locs.length} entries)`);
}

async function seedWarehouses() {
  type WH = {
    lc: string;
    code: string;
    name: string;
    neg?: boolean;
    notes?: string;
    active?: boolean;
  };
  const rows: WH[] = [
    // ── HQ-GDG-01: Gudang Pusat Jakarta (8) ───────────────────────────────────────
    {
      lc: 'HQ-GDG-01',
      code: 'HQ-GDG-01-RM-A',
      name: 'Bahan Baku A — Logam & Profil',
      notes: 'Plat baja, besi hollow, aluminium profil',
    },
    {
      lc: 'HQ-GDG-01',
      code: 'HQ-GDG-01-RM-B',
      name: 'Bahan Baku B — Plastik & Kimia',
      notes: 'Resin, compound, bahan kimia proses',
    },
    {
      lc: 'HQ-GDG-01',
      code: 'HQ-GDG-01-WIP',
      name: 'Barang Setengah Jadi (WIP)',
      notes: 'Buffer produksi antar lini',
    },
    {
      lc: 'HQ-GDG-01',
      code: 'HQ-GDG-01-FG-A',
      name: 'Barang Jadi A — Produk Reguler',
      notes: 'Produk standar siap jual',
    },
    {
      lc: 'HQ-GDG-01',
      code: 'HQ-GDG-01-FG-B',
      name: 'Barang Jadi B — Produk Premium',
      notes: 'Produk premium & custom order',
    },
    {
      lc: 'HQ-GDG-01',
      code: 'HQ-GDG-01-PKG',
      name: 'Kemasan & Label',
      notes: 'Karton, bubble wrap, label barcode',
    },
    {
      lc: 'HQ-GDG-01',
      code: 'HQ-GDG-01-QR',
      name: 'Karantina & Hold',
      neg: false,
      notes: 'Produk pending QC, retur supplier',
    },
    {
      lc: 'HQ-GDG-01',
      code: 'HQ-GDG-01-RET',
      name: 'Retur Pelanggan',
      neg: false,
      notes: 'Barang retur dari pelanggan, sorting & rework',
    },

    // ── HQ-GDG-02: Gudang Distribusi Jakarta Utara (3) ───────────────────────────
    {
      lc: 'HQ-GDG-02',
      code: 'HQ-GDG-02-FG',
      name: 'Barang Jadi Distribusi Utara',
      notes: 'Stok distribusi wilayah Jakarta Utara',
    },
    {
      lc: 'HQ-GDG-02',
      code: 'HQ-GDG-02-STG',
      name: 'Staging Area Pengiriman Utara',
      notes: 'Barang siap muat & antrian ekspedisi',
    },
    {
      lc: 'HQ-GDG-02',
      code: 'HQ-GDG-02-RET',
      name: 'Retur Distribusi Utara',
      neg: false,
      notes: 'Barang retur dari agen/distributor area utara',
    },

    // ── HQ-GDG-03: Gudang Logistik Cakung (3) ────────────────────────────────────
    {
      lc: 'HQ-GDG-03',
      code: 'HQ-GDG-03-RM',
      name: 'Bahan Baku Impor Cakung',
      notes: 'Buffer bahan baku impor & bonded',
    },
    {
      lc: 'HQ-GDG-03',
      code: 'HQ-GDG-03-CONS',
      name: 'Bahan Pendukung Produksi',
      notes: 'Oli, pelumas, majun, consumables lini produksi',
    },
    {
      lc: 'HQ-GDG-03',
      code: 'HQ-GDG-03-SCRP',
      name: 'Scrap & Limbah Produksi',
      neg: false,
      notes: 'Scrap metal, plastik off-spec, limbah proses',
    },

    // ── HQ-DCT-01: Distribution Center Marunda (5) ───────────────────────────────
    {
      lc: 'HQ-DCT-01',
      code: 'HQ-DCT-01-FG-A',
      name: 'Finished Goods DC Marunda — Zona A',
      notes: 'Zona A: produk kategori berat & bulk',
    },
    {
      lc: 'HQ-DCT-01',
      code: 'HQ-DCT-01-FG-B',
      name: 'Finished Goods DC Marunda — Zona B',
      notes: 'Zona B: produk kategori ringan & kecil',
    },
    {
      lc: 'HQ-DCT-01',
      code: 'HQ-DCT-01-STG',
      name: 'Staging & Dispatch Marunda',
      notes: 'Antrian loading dock & verifikasi DO',
    },
    {
      lc: 'HQ-DCT-01',
      code: 'HQ-DCT-01-QR',
      name: 'Karantina DC Marunda',
      neg: false,
      notes: 'Produk pending inspeksi sebelum dispatch',
    },
    {
      lc: 'HQ-DCT-01',
      code: 'HQ-DCT-01-RET',
      name: 'Retur Processing Marunda',
      neg: false,
      notes: 'Sorting & repackaging retur dari distributor',
    },

    // ── HQ-WRK-01: Workshop Cakung (3) ───────────────────────────────────────────
    {
      lc: 'HQ-WRK-01',
      code: 'HQ-WRK-01-SP',
      name: 'Spare Parts Mesin Workshop',
      notes: 'Suku cadang mesin produksi & utilitas',
    },
    {
      lc: 'HQ-WRK-01',
      code: 'HQ-WRK-01-CONS',
      name: 'Consumables Workshop',
      notes: 'Elektroda las, mata bor, amplas, perkakas habis',
    },
    {
      lc: 'HQ-WRK-01',
      code: 'HQ-WRK-01-SCRP',
      name: 'Scrap Workshop',
      neg: false,
      notes: 'Scrap hasil repair & overhaul mesin',
    },

    // ── SBY-GDG-01: Gudang Utama Surabaya (6) ────────────────────────────────────
    {
      lc: 'SBY-GDG-01',
      code: 'SBY-GDG-01-RM',
      name: 'Bahan Baku Surabaya',
      notes: 'Stok bahan baku wilayah Jawa Timur',
    },
    {
      lc: 'SBY-GDG-01',
      code: 'SBY-GDG-01-FG-A',
      name: 'Barang Jadi A Surabaya',
      notes: 'Produk reguler stok cabang Surabaya',
    },
    {
      lc: 'SBY-GDG-01',
      code: 'SBY-GDG-01-FG-B',
      name: 'Barang Jadi B Surabaya',
      notes: 'Produk project & tender Jawa Timur',
    },
    {
      lc: 'SBY-GDG-01',
      code: 'SBY-GDG-01-PKG',
      name: 'Kemasan Surabaya',
      notes: 'Kemasan repack & relabeling cabang Surabaya',
    },
    {
      lc: 'SBY-GDG-01',
      code: 'SBY-GDG-01-QR',
      name: 'Karantina Surabaya',
      neg: false,
      notes: 'Hold QC & investigasi keluhan Surabaya',
    },
    {
      lc: 'SBY-GDG-01',
      code: 'SBY-GDG-01-RET',
      name: 'Retur Surabaya',
      neg: false,
      notes: 'Retur pelanggan & distributor Jawa Timur',
    },

    // ── SBY-GDG-02: Gudang Distribusi Surabaya Barat (2) ─────────────────────────
    {
      lc: 'SBY-GDG-02',
      code: 'SBY-GDG-02-FG',
      name: 'Barang Jadi Distribusi Barat SBY',
      notes: 'Stok untuk area Surabaya Barat & Gresik',
    },
    {
      lc: 'SBY-GDG-02',
      code: 'SBY-GDG-02-STG',
      name: 'Staging Distribusi Barat SBY',
      notes: 'Antrian muat truk distribusi harian',
    },

    // ── SBY-GDG-03: Gudang Transit Juanda (2) ────────────────────────────────────
    {
      lc: 'SBY-GDG-03',
      code: 'SBY-GDG-03-TRAN',
      name: 'Transit Kargo Udara Juanda',
      notes: 'Buffer kargo masuk/keluar via Bandara Juanda',
    },
    {
      lc: 'SBY-GDG-03',
      code: 'SBY-GDG-03-FG',
      name: 'Transit Barang Jadi Juanda',
      notes: 'Konsolidasi pengiriman lintas pulau',
    },

    // ── SBY-DCT-01: DC Sidoarjo (3) ──────────────────────────────────────────────
    {
      lc: 'SBY-DCT-01',
      code: 'SBY-DCT-01-FG',
      name: 'Finished Goods DC Sidoarjo',
      notes: 'Stok distribusi area Sidoarjo & Pasuruan',
    },
    {
      lc: 'SBY-DCT-01',
      code: 'SBY-DCT-01-STG',
      name: 'Staging DC Sidoarjo',
      notes: 'Persiapan muat & sortir delivery order',
    },
    {
      lc: 'SBY-DCT-01',
      code: 'SBY-DCT-01-QR',
      name: 'Karantina DC Sidoarjo',
      neg: false,
      notes: 'Produk pending inspeksi & retur Sidoarjo',
    },

    // ── SBY-WRK-01: Workshop Rungkut (2) ─────────────────────────────────────────
    {
      lc: 'SBY-WRK-01',
      code: 'SBY-WRK-01-SP',
      name: 'Spare Parts Workshop Rungkut',
      notes: 'Suku cadang mesin di kawasan industri SIER',
    },
    {
      lc: 'SBY-WRK-01',
      code: 'SBY-WRK-01-CONS',
      name: 'Consumables Workshop Rungkut',
      notes: 'Bahan habis pakai maintenance Surabaya',
    },

    // ── BDG-GDG-01: Gudang Utama Bandung (5) ─────────────────────────────────────
    {
      lc: 'BDG-GDG-01',
      code: 'BDG-GDG-01-RM',
      name: 'Bahan Baku Bandung',
      notes: 'Tekstil, benang, bahan kimia tekstil & garmen',
    },
    {
      lc: 'BDG-GDG-01',
      code: 'BDG-GDG-01-FG',
      name: 'Barang Jadi Bandung',
      notes: 'Produk jadi stok cabang Bandung',
    },
    {
      lc: 'BDG-GDG-01',
      code: 'BDG-GDG-01-PKG',
      name: 'Kemasan Bandung',
      notes: 'Kemasan produk wilayah Jawa Barat',
    },
    {
      lc: 'BDG-GDG-01',
      code: 'BDG-GDG-01-QR',
      name: 'Karantina Bandung',
      neg: false,
      notes: 'Hold QC & retur produk wilayah Bandung',
    },
    {
      lc: 'BDG-GDG-01',
      code: 'BDG-GDG-01-RET',
      name: 'Retur Bandung',
      neg: false,
      notes: 'Retur pelanggan wilayah Jawa Barat',
    },

    // ── BDG-GDG-02: Gudang Distribusi Cimahi (2) ─────────────────────────────────
    {
      lc: 'BDG-GDG-02',
      code: 'BDG-GDG-02-FG',
      name: 'Barang Jadi Distribusi Cimahi',
      notes: 'Stok distribusi Cimahi & Bandung Barat',
    },
    {
      lc: 'BDG-GDG-02',
      code: 'BDG-GDG-02-STG',
      name: 'Staging Distribusi Cimahi',
      notes: 'Area muat harian ekspedisi Bandung Barat',
    },

    // ── BDG-DCT-01: DC Gedebage (2) ──────────────────────────────────────────────
    {
      lc: 'BDG-DCT-01',
      code: 'BDG-DCT-01-FG',
      name: 'Finished Goods DC Gedebage',
      notes: 'Stok DC untuk distribusi Jawa Barat Timur',
    },
    {
      lc: 'BDG-DCT-01',
      code: 'BDG-DCT-01-STG',
      name: 'Staging DC Gedebage',
      notes: 'Loading dock & verifikasi delivery order Jabar Timur',
    },

    // ── BDG-WRK-01: Workshop Dayeuhkolot (2) ─────────────────────────────────────
    {
      lc: 'BDG-WRK-01',
      code: 'BDG-WRK-01-SP',
      name: 'Spare Parts Workshop Dayeuhkolot',
      notes: 'Suku cadang mesin cabang Bandung',
    },
    {
      lc: 'BDG-WRK-01',
      code: 'BDG-WRK-01-CONS',
      name: 'Consumables Workshop Dayeuhkolot',
      notes: 'Bahan habis pakai maintenance Bandung',
    },

    // ── MKS-GDG-01: Gudang Utama Makassar (4) ────────────────────────────────────
    {
      lc: 'MKS-GDG-01',
      code: 'MKS-GDG-01-FG',
      name: 'Barang Jadi Makassar',
      notes: 'Stok produk jadi cabang Makassar',
    },
    {
      lc: 'MKS-GDG-01',
      code: 'MKS-GDG-01-RM',
      name: 'Bahan Baku Makassar',
      notes: 'Buffer bahan baku untuk wilayah Sulawesi Selatan',
    },
    {
      lc: 'MKS-GDG-01',
      code: 'MKS-GDG-01-QR',
      name: 'Karantina Makassar',
      neg: false,
      notes: 'Produk hold & investigasi mutu Makassar',
    },
    {
      lc: 'MKS-GDG-01',
      code: 'MKS-GDG-01-RET',
      name: 'Retur Makassar',
      neg: false,
      notes: 'Retur pelanggan & agen Sulawesi',
    },

    // ── MKS-GDG-02: Gudang Distribusi Maros (2) ──────────────────────────────────
    {
      lc: 'MKS-GDG-02',
      code: 'MKS-GDG-02-FG',
      name: 'Barang Jadi Distribusi Maros',
      notes: 'Stok distribusi Sulsel Utara & Sulawesi Tengah',
    },
    {
      lc: 'MKS-GDG-02',
      code: 'MKS-GDG-02-STG',
      name: 'Staging Distribusi Maros',
      notes: 'Antrian muat & pencocokan DO area Maros',
    },

    // ── MKS-DCT-01: DC Maros (2) ─────────────────────────────────────────────────
    {
      lc: 'MKS-DCT-01',
      code: 'MKS-DCT-01-FG',
      name: 'Finished Goods DC Maros',
      notes: 'DC distribusi Indonesia Timur via pelabuhan',
    },
    {
      lc: 'MKS-DCT-01',
      code: 'MKS-DCT-01-STG',
      name: 'Staging DC Maros',
      notes: 'Persiapan pengiriman kapal Indonesia Timur',
    },

    // ── MKS-WRK-01: Workshop Antang (2) ──────────────────────────────────────────
    {
      lc: 'MKS-WRK-01',
      code: 'MKS-WRK-01-SP',
      name: 'Spare Parts Workshop Antang',
      notes: 'Suku cadang & tooling workshop Makassar',
    },
    {
      lc: 'MKS-WRK-01',
      code: 'MKS-WRK-01-CONS',
      name: 'Consumables Workshop Antang',
      notes: 'Bahan habis pakai servis kendaraan & mesin Makassar',
    },

    // ── MDN-GDG-01: Gudang Utama Medan (4) ───────────────────────────────────────
    {
      lc: 'MDN-GDG-01',
      code: 'MDN-GDG-01-FG',
      name: 'Barang Jadi Medan',
      notes: 'Stok produk jadi cabang Medan',
    },
    {
      lc: 'MDN-GDG-01',
      code: 'MDN-GDG-01-RM',
      name: 'Bahan Baku Medan',
      notes: 'Buffer bahan baku untuk wilayah Sumatera Utara',
    },
    {
      lc: 'MDN-GDG-01',
      code: 'MDN-GDG-01-QR',
      name: 'Karantina Medan',
      neg: false,
      notes: 'Hold QC & investigasi produk wilayah Medan',
    },
    {
      lc: 'MDN-GDG-01',
      code: 'MDN-GDG-01-RET',
      name: 'Retur Medan',
      neg: false,
      notes: 'Retur agen & pelanggan Sumatera Utara',
    },

    // ── MDN-GDG-02: Gudang Distribusi Belawan (2) ────────────────────────────────
    {
      lc: 'MDN-GDG-02',
      code: 'MDN-GDG-02-FG',
      name: 'Barang Jadi Pelabuhan Belawan',
      notes: 'Stok siap muat kapal wilayah Sumatera',
    },
    {
      lc: 'MDN-GDG-02',
      code: 'MDN-GDG-02-STG',
      name: 'Staging Pelabuhan Belawan',
      notes: 'Antrian muat & cek fisik ekspor/antar pulau',
    },

    // ── MDN-DCT-01: DC Belawan (2) ────────────────────────────────────────────────
    {
      lc: 'MDN-DCT-01',
      code: 'MDN-DCT-01-FG',
      name: 'Finished Goods DC Belawan',
      notes: 'DC distribusi Sumatera Utara & Aceh',
    },
    {
      lc: 'MDN-DCT-01',
      code: 'MDN-DCT-01-STG',
      name: 'Staging DC Belawan',
      notes: 'Loading dock & verifikasi delivery order Sumut',
    },

    // ── MDN-WRK-01: Workshop Helvetia (2) ────────────────────────────────────────
    {
      lc: 'MDN-WRK-01',
      code: 'MDN-WRK-01-SP',
      name: 'Spare Parts Workshop Helvetia',
      notes: 'Suku cadang mesin & kendaraan cabang Medan',
    },
    {
      lc: 'MDN-WRK-01',
      code: 'MDN-WRK-01-CONS',
      name: 'Consumables Workshop Helvetia',
      notes: 'Bahan habis pakai maintenance Medan',
    },

    // ── SMG-GDG-01: Gudang Utama Semarang (4) ────────────────────────────────────
    {
      lc: 'SMG-GDG-01',
      code: 'SMG-GDG-01-FG',
      name: 'Barang Jadi Semarang',
      notes: 'Stok produk jadi cabang Semarang',
    },
    {
      lc: 'SMG-GDG-01',
      code: 'SMG-GDG-01-RM',
      name: 'Bahan Baku Semarang',
      notes: 'Buffer bahan baku untuk wilayah Jawa Tengah',
    },
    {
      lc: 'SMG-GDG-01',
      code: 'SMG-GDG-01-QR',
      name: 'Karantina Semarang',
      neg: false,
      notes: 'Hold QC & investigasi produk Semarang',
    },
    {
      lc: 'SMG-GDG-01',
      code: 'SMG-GDG-01-RET',
      name: 'Retur Semarang',
      neg: false,
      notes: 'Retur pelanggan wilayah Jawa Tengah',
    },

    // ── SMG-GDG-02: Gudang Distribusi Kaligawe (2) ───────────────────────────────
    {
      lc: 'SMG-GDG-02',
      code: 'SMG-GDG-02-FG',
      name: 'Barang Jadi Distribusi Kaligawe',
      notes: 'Stok distribusi kawasan industri Semarang',
    },
    {
      lc: 'SMG-GDG-02',
      code: 'SMG-GDG-02-STG',
      name: 'Staging Distribusi Kaligawe',
      notes: 'Antrian muat harian area Kaligawe',
    },

    // ── SMG-DCT-01: DC Semarang-Demak (2) ────────────────────────────────────────
    {
      lc: 'SMG-DCT-01',
      code: 'SMG-DCT-01-FG',
      name: 'Finished Goods DC Semarang-Demak',
      notes: 'DC distribusi Jawa Tengah & DIY',
    },
    {
      lc: 'SMG-DCT-01',
      code: 'SMG-DCT-01-STG',
      name: 'Staging DC Semarang-Demak',
      notes: 'Loading dock & sortir delivery order Jateng',
    },

    // ── SMG-WRK-01: Workshop Terboyo (2) ─────────────────────────────────────────
    {
      lc: 'SMG-WRK-01',
      code: 'SMG-WRK-01-SP',
      name: 'Spare Parts Workshop Terboyo',
      notes: 'Suku cadang mesin & kendaraan Semarang',
    },
    {
      lc: 'SMG-WRK-01',
      code: 'SMG-WRK-01-CONS',
      name: 'Consumables Workshop Terboyo',
      notes: 'Bahan habis pakai maintenance Semarang',
    },

    // ── YGY-GDG-01: Gudang Utama Yogyakarta (4) ──────────────────────────────────
    {
      lc: 'YGY-GDG-01',
      code: 'YGY-GDG-01-FG',
      name: 'Barang Jadi Yogyakarta',
      notes: 'Stok produk jadi cabang Yogyakarta',
    },
    {
      lc: 'YGY-GDG-01',
      code: 'YGY-GDG-01-RM',
      name: 'Bahan Baku Yogyakarta',
      notes: 'Buffer bahan baku DIY & Jateng Selatan',
    },
    {
      lc: 'YGY-GDG-01',
      code: 'YGY-GDG-01-QR',
      name: 'Karantina Yogyakarta',
      neg: false,
      notes: 'Hold QC produk wilayah Yogyakarta',
    },
    {
      lc: 'YGY-GDG-01',
      code: 'YGY-GDG-01-RET',
      name: 'Retur Yogyakarta',
      neg: false,
      notes: 'Retur pelanggan wilayah DIY',
    },

    // ── YGY-GDG-02: Gudang Distribusi Maguwoharjo (2) ────────────────────────────
    {
      lc: 'YGY-GDG-02',
      code: 'YGY-GDG-02-FG',
      name: 'Barang Jadi Distribusi Maguwoharjo',
      notes: 'Stok distribusi Sleman & Gunung Kidul',
    },
    {
      lc: 'YGY-GDG-02',
      code: 'YGY-GDG-02-STG',
      name: 'Staging Distribusi Maguwoharjo',
      notes: 'Antrian muat & sortir DO area Sleman',
    },

    // ── YGY-WRK-01: Workshop Condongcatur (2) ────────────────────────────────────
    {
      lc: 'YGY-WRK-01',
      code: 'YGY-WRK-01-SP',
      name: 'Spare Parts Workshop Condongcatur',
      notes: 'Suku cadang mesin & kendaraan Yogyakarta',
    },
    {
      lc: 'YGY-WRK-01',
      code: 'YGY-WRK-01-CONS',
      name: 'Consumables Workshop Condongcatur',
      notes: 'Bahan habis pakai maintenance Yogyakarta',
    },

    // ── PLM-GDG-01: Gudang Utama Palembang (4) ───────────────────────────────────
    {
      lc: 'PLM-GDG-01',
      code: 'PLM-GDG-01-FG',
      name: 'Barang Jadi Palembang',
      notes: 'Stok produk jadi cabang Palembang',
    },
    {
      lc: 'PLM-GDG-01',
      code: 'PLM-GDG-01-RM',
      name: 'Bahan Baku Palembang',
      notes: 'Buffer bahan baku untuk wilayah Sumatera Selatan',
    },
    {
      lc: 'PLM-GDG-01',
      code: 'PLM-GDG-01-QR',
      name: 'Karantina Palembang',
      neg: false,
      notes: 'Hold QC produk Palembang',
    },
    {
      lc: 'PLM-GDG-01',
      code: 'PLM-GDG-01-RET',
      name: 'Retur Palembang',
      neg: false,
      notes: 'Retur pelanggan Sumatera Selatan & Bangka',
    },

    // ── PLM-GDG-02: Gudang Distribusi Boom Baru (2) ──────────────────────────────
    {
      lc: 'PLM-GDG-02',
      code: 'PLM-GDG-02-FG',
      name: 'Barang Jadi Pelabuhan Boom Baru',
      notes: 'Stok siap muat kapal pelabuhan Palembang',
    },
    {
      lc: 'PLM-GDG-02',
      code: 'PLM-GDG-02-STG',
      name: 'Staging Pelabuhan Boom Baru',
      notes: 'Antrian muat & cek fisik antar pulau Palembang',
    },

    // ── PLM-DCT-01: DC Boom Baru (1) ─────────────────────────────────────────────
    {
      lc: 'PLM-DCT-01',
      code: 'PLM-DCT-01-FG',
      name: 'Finished Goods DC Boom Baru',
      notes: 'DC distribusi Sumatera Selatan & Lampung',
    },
    {
      lc: 'PLM-DCT-01',
      code: 'PLM-DCT-01-STG',
      name: 'Staging DC Boom Baru',
      notes: 'Sortir & muat distribusi wilayah Sumsel',
    },

    // ── PLM-WRK-01: Workshop Jakabaring (2) ──────────────────────────────────────
    {
      lc: 'PLM-WRK-01',
      code: 'PLM-WRK-01-SP',
      name: 'Spare Parts Workshop Jakabaring',
      notes: 'Suku cadang mesin & alat berat Palembang',
    },
    {
      lc: 'PLM-WRK-01',
      code: 'PLM-WRK-01-CONS',
      name: 'Consumables Workshop Jakabaring',
      notes: 'Bahan habis pakai maintenance Palembang',
    },

    // ── DPS-GDG-01: Gudang Utama Denpasar (4) ────────────────────────────────────
    {
      lc: 'DPS-GDG-01',
      code: 'DPS-GDG-01-FG',
      name: 'Barang Jadi Denpasar',
      notes: 'Stok produk jadi cabang Denpasar',
    },
    {
      lc: 'DPS-GDG-01',
      code: 'DPS-GDG-01-RM',
      name: 'Bahan Baku Denpasar',
      notes: 'Buffer bahan baku Bali & Nusa Tenggara',
    },
    {
      lc: 'DPS-GDG-01',
      code: 'DPS-GDG-01-QR',
      name: 'Karantina Denpasar',
      neg: false,
      notes: 'Hold QC produk wilayah Denpasar',
    },
    {
      lc: 'DPS-GDG-01',
      code: 'DPS-GDG-01-RET',
      name: 'Retur Denpasar',
      neg: false,
      notes: 'Retur pelanggan Bali & Nusa Tenggara',
    },

    // ── DPS-GDG-02: Gudang Distribusi Sanur (2) ──────────────────────────────────
    {
      lc: 'DPS-GDG-02',
      code: 'DPS-GDG-02-FG',
      name: 'Barang Jadi Distribusi Sanur',
      notes: 'Stok distribusi Bali Selatan & Nusra',
    },
    {
      lc: 'DPS-GDG-02',
      code: 'DPS-GDG-02-STG',
      name: 'Staging Distribusi Sanur',
      notes: 'Antrian muat & pencocokan DO Bali Selatan',
    },

    // ── DPS-DCT-01: DC Sanur (2) ─────────────────────────────────────────────────
    {
      lc: 'DPS-DCT-01',
      code: 'DPS-DCT-01-FG',
      name: 'Finished Goods DC Sanur',
      notes: 'DC distribusi Bali & Nusa Tenggara Barat',
    },
    {
      lc: 'DPS-DCT-01',
      code: 'DPS-DCT-01-STG',
      name: 'Staging DC Sanur',
      notes: 'Loading dock & verifikasi DO Bali-NTB',
    },

    // ── DPS-WRK-01: Workshop Pemogan (2) ─────────────────────────────────────────
    {
      lc: 'DPS-WRK-01',
      code: 'DPS-WRK-01-SP',
      name: 'Spare Parts Workshop Pemogan',
      notes: 'Suku cadang mesin & kendaraan Bali',
    },
    {
      lc: 'DPS-WRK-01',
      code: 'DPS-WRK-01-CONS',
      name: 'Consumables Workshop Pemogan',
      notes: 'Bahan habis pakai maintenance Denpasar',
    },

    // ── BPN-GDG-01: Gudang Utama Balikpapan (4) ──────────────────────────────────
    {
      lc: 'BPN-GDG-01',
      code: 'BPN-GDG-01-FG',
      name: 'Barang Jadi Balikpapan',
      notes: 'Stok produk jadi cabang Balikpapan',
    },
    {
      lc: 'BPN-GDG-01',
      code: 'BPN-GDG-01-RM',
      name: 'Bahan Baku Balikpapan',
      notes: 'Buffer bahan baku Kaltim & proyek IKN',
    },
    {
      lc: 'BPN-GDG-01',
      code: 'BPN-GDG-01-QR',
      name: 'Karantina Balikpapan',
      neg: false,
      notes: 'Hold QC produk wilayah Balikpapan',
    },
    {
      lc: 'BPN-GDG-01',
      code: 'BPN-GDG-01-RET',
      name: 'Retur Balikpapan',
      neg: false,
      notes: 'Retur pelanggan wilayah Kalimantan Timur',
    },

    // ── BPN-GDG-02: Gudang Distribusi Kariangau (2) ──────────────────────────────
    {
      lc: 'BPN-GDG-02',
      code: 'BPN-GDG-02-FG',
      name: 'Barang Jadi Distribusi Kariangau',
      notes: 'Stok distribusi kawasan industri Kariangau',
    },
    {
      lc: 'BPN-GDG-02',
      code: 'BPN-GDG-02-STG',
      name: 'Staging Distribusi Kariangau',
      notes: 'Antrian muat & verifikasi DO Kaltim',
    },

    // ── BPN-DCT-01: DC Kariangau (2) ─────────────────────────────────────────────
    {
      lc: 'BPN-DCT-01',
      code: 'BPN-DCT-01-FG',
      name: 'Finished Goods DC Kariangau',
      notes: 'DC distribusi Kalimantan Timur & proyek IKN',
    },
    {
      lc: 'BPN-DCT-01',
      code: 'BPN-DCT-01-STG',
      name: 'Staging DC Kariangau',
      notes: 'Loading dock & sortir DO Kalimantan',
    },

    // ── BPN-WRK-01: Workshop Klandasan (2) ───────────────────────────────────────
    {
      lc: 'BPN-WRK-01',
      code: 'BPN-WRK-01-SP',
      name: 'Spare Parts Workshop Klandasan',
      notes: 'Suku cadang alat berat & mesin Balikpapan',
    },
    {
      lc: 'BPN-WRK-01',
      code: 'BPN-WRK-01-CONS',
      name: 'Consumables Workshop Klandasan',
      notes: 'Bahan habis pakai maintenance Balikpapan',
    },

    // ── PKU Pekanbaru — non-aktif (4) ─────────────────────────────────────────────
    {
      lc: 'PKU-GDG-01',
      code: 'PKU-GDG-01-FG',
      name: 'Barang Jadi Pekanbaru',
      notes: 'Stok produk jadi cabang Pekanbaru (hibernasi)',
      active: false,
    },
    {
      lc: 'PKU-GDG-01',
      code: 'PKU-GDG-01-RM',
      name: 'Bahan Baku Pekanbaru',
      notes: 'Buffer bahan baku wilayah Riau (hibernasi)',
      active: false,
    },
    {
      lc: 'PKU-GDG-02',
      code: 'PKU-GDG-02-FG',
      name: 'Barang Jadi Distribusi Rumbai',
      notes: 'Stok distribusi Riau (hibernasi)',
      active: false,
    },
    {
      lc: 'PKU-DCT-01',
      code: 'PKU-DCT-01-FG',
      name: 'Finished Goods DC Rumbai',
      notes: 'DC distribusi Riau — non-aktif',
      active: false,
    },

    // ── PNK Pontianak — non-aktif (4) ─────────────────────────────────────────────
    {
      lc: 'PNK-GDG-01',
      code: 'PNK-GDG-01-FG',
      name: 'Barang Jadi Pontianak',
      notes: 'Stok produk jadi cabang Pontianak (hibernasi)',
      active: false,
    },
    {
      lc: 'PNK-GDG-01',
      code: 'PNK-GDG-01-RM',
      name: 'Bahan Baku Pontianak',
      notes: 'Buffer bahan baku Kalimantan Barat (hibernasi)',
      active: false,
    },
    {
      lc: 'PNK-GDG-02',
      code: 'PNK-GDG-02-FG',
      name: 'Barang Jadi Distribusi Siantan',
      notes: 'Stok distribusi Kalimantan Barat (hibernasi)',
      active: false,
    },
    {
      lc: 'PNK-DCT-01',
      code: 'PNK-DCT-01-FG',
      name: 'Finished Goods DC Siantan',
      notes: 'DC distribusi Kalimantan Barat — non-aktif',
      active: false,
    },
  ];

  // Build locationCode → id map
  const allLocCodes = [...new Set(rows.map((r) => r.lc))];
  const locMap = new Map<string, bigint>();
  for (const code of allLocCodes) {
    const loc = await prisma.erpLocation.findUnique({ where: { code }, select: { id: true } });
    if (loc) locMap.set(code, loc.id);
  }

  let count = 0;
  for (const { lc, active, neg, ...d } of rows) {
    const locationId = locMap.get(lc);
    if (!locationId) {
      console.warn(`  ⚠ Location ${lc} not found, skip ${d.code}`);
      continue;
    }
    const payload = {
      ...d,
      locationId,
      isActive: active ?? true,
      allowNegativeStock: neg ?? false,
    };
    await prisma.erpWarehouse.upsert({
      where: { code: d.code },
      create: payload,
      update: payload,
    });
    count++;
  }
  console.log(`✓ md_warehouses (${count} entries)`);
}

async function seedPartnerCategories() {
  const categories: { code: string; name: string; salesTier?: number; legacyCode?: string }[] = [
    // ── Industri Manufaktur ───────────────────────────────────────────────────
    { code: 'MFG-01', name: 'Manufaktur Otomotif & Komponen', salesTier: 1 },
    { code: 'MFG-02', name: 'Manufaktur Elektronik & Listrik', salesTier: 1 },
    { code: 'MFG-03', name: 'Manufaktur Mesin & Peralatan Industri', salesTier: 1 },
    { code: 'MFG-04', name: 'Manufaktur Baja & Logam', salesTier: 1 },
    { code: 'MFG-05', name: 'Manufaktur Plastik & Karet', salesTier: 2 },
    { code: 'MFG-06', name: 'Manufaktur Tekstil & Garmen', salesTier: 2 },
    { code: 'MFG-07', name: 'Manufaktur Kertas & Kemasan', salesTier: 2 },
    { code: 'MFG-08', name: 'Manufaktur Farmasi & Alat Kesehatan', salesTier: 1 },
    { code: 'MFG-09', name: 'Manufaktur Makanan & Minuman', salesTier: 2 },
    { code: 'MFG-10', name: 'Manufaktur Furnitur & Interior', salesTier: 2 },
    { code: 'MFG-11', name: 'Manufaktur Kimia & Petrokimia', salesTier: 1 },
    { code: 'MFG-12', name: 'Manufaktur Semen & Material Bangunan', salesTier: 2 },
    { code: 'MFG-13', name: 'Manufaktur Kayu & Turunannya', salesTier: 3 },
    { code: 'MFG-14', name: 'Manufaktur Keramik & Kaca', salesTier: 2 },
    { code: 'MFG-15', name: 'Manufaktur Alat Pertanian', salesTier: 3 },
    { code: 'MFG-16', name: 'Manufaktur Peralatan Rumah Tangga', salesTier: 2 },
    { code: 'MFG-17', name: 'Manufaktur Percetakan & Penerbitan', salesTier: 3 },
    { code: 'MFG-18', name: 'Manufaktur Alat Tulis & Kantor', salesTier: 3 },
    { code: 'MFG-19', name: 'Manufaktur Produk Karet Teknik', salesTier: 2 },
    { code: 'MFG-20', name: 'Manufaktur Pengolahan Bahan Tambang', salesTier: 1 },
    { code: 'MFG-21', name: 'Manufaktur Produk Beton Pracetak', salesTier: 2 },
    { code: 'MFG-22', name: 'Manufaktur Kabel & Kawat', salesTier: 2 },
    { code: 'MFG-23', name: 'Manufaktur Sepatu & Alas Kaki', salesTier: 2 },
    { code: 'MFG-24', name: 'Manufaktur Kosmetik & Perawatan', salesTier: 2 },
    { code: 'MFG-25', name: 'Manufaktur Produk Kulit', salesTier: 3 },

    // ── Distribusi & Perdagangan ──────────────────────────────────────────────
    { code: 'DAG-01', name: 'Distributor Tunggal Nasional', salesTier: 1, legacyCode: 'D1' },
    { code: 'DAG-02', name: 'Distributor Regional', salesTier: 1, legacyCode: 'D2' },
    { code: 'DAG-03', name: 'Sub-Distributor', salesTier: 2, legacyCode: 'D3' },
    { code: 'DAG-04', name: 'Pedagang Besar / Grosir', salesTier: 2 },
    { code: 'DAG-05', name: 'Importir & Eksportir', salesTier: 1 },
    { code: 'DAG-06', name: 'Agen Tunggal Pemegang Merek (ATPM)', salesTier: 1 },
    { code: 'DAG-07', name: 'Retailer Modern (Supermarket / Hypermarket)', salesTier: 2 },
    { code: 'DAG-08', name: 'Retailer Tradisional', salesTier: 3 },
    { code: 'DAG-09', name: 'Toko Bangunan & Material', salesTier: 3 },
    { code: 'DAG-10', name: 'Toko Suku Cadang & Spare Part', salesTier: 3 },
    { code: 'DAG-11', name: 'Toko Online / E-Commerce Seller', salesTier: 2 },
    { code: 'DAG-12', name: 'Dealer Resmi', salesTier: 2 },
    { code: 'DAG-13', name: 'Agen Pemasaran / Trading House', salesTier: 2 },
    { code: 'DAG-14', name: 'Koperasi Distribusi', salesTier: 3 },
    { code: 'DAG-15', name: 'Marketplace Platform (Agregator)', salesTier: 1 },

    // ── Jasa & Proyek ─────────────────────────────────────────────────────────
    { code: 'JSA-01', name: 'Kontraktor Konstruksi Besar', salesTier: 1 },
    { code: 'JSA-02', name: 'Kontraktor Konstruksi Menengah', salesTier: 2 },
    { code: 'JSA-03', name: 'Kontraktor Mekanikal & Elektrikal (ME)', salesTier: 2 },
    { code: 'JSA-04', name: 'Perusahaan Jasa Pemeliharaan & MRO', salesTier: 2 },
    { code: 'JSA-05', name: 'Perusahaan Jasa Engineering & Konsultasi', salesTier: 1 },
    { code: 'JSA-06', name: 'Perusahaan Jasa Pengiriman & Logistik', salesTier: 2 },
    { code: 'JSA-07', name: 'Perusahaan Jasa Pertambangan', salesTier: 1 },
    { code: 'JSA-08', name: 'Perusahaan Perkebunan & Agribisnis', salesTier: 2 },
    { code: 'JSA-09', name: 'Perusahaan Jasa IT & Teknologi', salesTier: 2 },
    { code: 'JSA-10', name: 'Perusahaan Properti & Developer', salesTier: 1 },
    { code: 'JSA-11', name: 'Perusahaan Energi & Kelistrikan', salesTier: 1 },
    { code: 'JSA-12', name: 'Hotel, Restoran & Katering (HoReCa)', salesTier: 2 },
    { code: 'JSA-13', name: 'Rumah Sakit & Klinik', salesTier: 2 },
    { code: 'JSA-14', name: 'Perusahaan Media & Periklanan', salesTier: 3 },
    { code: 'JSA-15', name: 'Perusahaan Asuransi & Keuangan', salesTier: 2 },

    // ── Pemerintah & Institusi ────────────────────────────────────────────────
    { code: 'GOV-01', name: 'BUMN / Perusahaan Negara', salesTier: 1 },
    { code: 'GOV-02', name: 'BUMD / Perusahaan Daerah', salesTier: 2 },
    { code: 'GOV-03', name: 'Kementerian & Lembaga Pemerintah Pusat', salesTier: 1 },
    { code: 'GOV-04', name: 'Pemerintah Daerah (Pemda / Dinas)', salesTier: 2 },
    { code: 'GOV-05', name: 'TNI & Kepolisian', salesTier: 1 },
    { code: 'GOV-06', name: 'Lembaga Pendidikan Negeri (Universitas/Poltek)', salesTier: 2 },
    { code: 'GOV-07', name: 'Lembaga Penelitian & Pengembangan Negeri', salesTier: 2 },
    { code: 'GOV-08', name: 'Badan Usaha Pelabuhan & Bandara', salesTier: 1 },
    { code: 'GOV-09', name: 'Organisasi Internasional & NGO', salesTier: 2 },
    { code: 'GOV-10', name: 'Koperasi Primer & Sekunder', salesTier: 3 },

    // ── Skala & Tier Usaha ────────────────────────────────────────────────────
    { code: 'SKL-01', name: 'Pelanggan Korporat (Enterprise > 1.000 karyawan)', salesTier: 1 },
    { code: 'SKL-02', name: 'Pelanggan Besar (Large 500–1.000 karyawan)', salesTier: 1 },
    { code: 'SKL-03', name: 'Pelanggan Menengah Atas (Mid-Upper 100–499)', salesTier: 2 },
    { code: 'SKL-04', name: 'Pelanggan Menengah (Mid 50–99 karyawan)', salesTier: 2 },
    { code: 'SKL-05', name: 'Pelanggan Kecil (Small < 50 karyawan)', salesTier: 3 },
    { code: 'SKL-06', name: 'UMKM Mikro (< 10 karyawan)', salesTier: 3 },
    { code: 'SKL-07', name: 'Perorangan / Individual (B2C)', salesTier: 3 },
    { code: 'SKL-08', name: 'Grup Usaha / Holding Company', salesTier: 1 },
    { code: 'SKL-09', name: 'Perusahaan Multinasional (PMA)', salesTier: 1 },
    { code: 'SKL-10', name: 'Startup & Perusahaan Rintisan', salesTier: 2 },

    // ── Kanal & Pola Transaksi ────────────────────────────────────────────────
    { code: 'KAN-01', name: 'Penjualan Langsung (Direct Sales)', salesTier: 1 },
    { code: 'KAN-02', name: 'Penjualan via Tender / Lelang', salesTier: 1 },
    { code: 'KAN-03', name: 'Kontrak Jangka Panjang (Annual Contract)', salesTier: 1 },
    { code: 'KAN-04', name: 'Penjualan Proyek (Project-Based)', salesTier: 2 },
    { code: 'KAN-05', name: 'Penjualan COD (Cash on Delivery)', salesTier: 3 },
    { code: 'KAN-06', name: 'Penjualan Kredit Jangka Pendek (< 30 hari)', salesTier: 2 },
    { code: 'KAN-07', name: 'Penjualan Kredit Jangka Panjang (> 60 hari)', salesTier: 2 },
    { code: 'KAN-08', name: 'Penjualan via Platform Digital', salesTier: 2 },
    { code: 'KAN-09', name: 'Pelanggan Walk-in / Over the Counter (OTC)', salesTier: 3 },
    { code: 'KAN-10', name: 'Penjualan via Mitra / Referral Partner', salesTier: 2 },

    // ── Regional & Ekspor ─────────────────────────────────────────────────────
    { code: 'REG-01', name: 'Pelanggan Lokal (Kota/Kabupaten)', salesTier: 3 },
    { code: 'REG-02', name: 'Pelanggan Regional Jawa', salesTier: 2 },
    { code: 'REG-03', name: 'Pelanggan Regional Sumatera', salesTier: 2 },
    { code: 'REG-04', name: 'Pelanggan Regional Kalimantan', salesTier: 2 },
    { code: 'REG-05', name: 'Pelanggan Regional Sulawesi', salesTier: 2 },
    { code: 'REG-06', name: 'Pelanggan Regional Bali & Nusa Tenggara', salesTier: 2 },
    { code: 'REG-07', name: 'Pelanggan Regional Maluku & Papua', salesTier: 2 },
    { code: 'REG-08', name: 'Ekspor ASEAN (Malaysia, Singapura, Thailand, dll)', salesTier: 1 },
    { code: 'REG-09', name: 'Ekspor Asia Timur (Jepang, Korea, Tiongkok)', salesTier: 1 },
    { code: 'REG-10', name: 'Ekspor Eropa & Amerika', salesTier: 1 },

    // ── Segmen Khusus & Loyalitas ─────────────────────────────────────────────
    { code: 'SPL-01', name: 'Pelanggan VIP / Key Account', salesTier: 1 },
    { code: 'SPL-02', name: 'Pelanggan Prioritas (Top 20 Revenue)', salesTier: 1 },
    { code: 'SPL-03', name: 'Pelanggan Loyal (> 5 tahun bertransaksi)', salesTier: 2 },
    { code: 'SPL-04', name: 'Pelanggan Baru (< 1 tahun)', salesTier: 3 },
    { code: 'SPL-05', name: 'Pelanggan Potensial / Prospek Aktif', salesTier: 3 },
    { code: 'SPL-06', name: 'Pelanggan Reaktivasi (Kembali Bertransaksi)', salesTier: 3 },
    { code: 'SPL-07', name: 'Pelanggan Musiman (Seasonal Buyer)', salesTier: 3 },
    { code: 'SPL-08', name: 'Pelanggan Konsinyasi / Titip Jual', salesTier: 2 },
    { code: 'SPL-09', name: 'Pelanggan Sampel / Demo Unit', salesTier: 3 },
    { code: 'SPL-10', name: 'Pelanggan Internal (Antar Divisi / Grup)', salesTier: 1 },
  ];

  let count = 0;
  for (const cat of categories) {
    await prisma.erpPartnerCategory.upsert({
      where: { code: cat.code },
      create: {
        code: cat.code,
        name: cat.name,
        kind: 'CUSTOMER',
        salesTier: cat.salesTier ?? null,
        isActive: true,
        legacyCode: cat.legacyCode ?? null,
      },
      update: {},
    });
    count++;
  }
  console.log(`✓ md_partner_categories — CUSTOMER (${count} entries)`);
}

async function seedPartnerTypes(prisma: any) {
  const types: { code: string; name: string; kind: string }[] = [
    { code: 'CUST', name: 'Customer', kind: 'CUSTOMER' },
    { code: 'SUP', name: 'Supplier', kind: 'SUPPLIER' },
    { code: 'SLS', name: 'Salesman', kind: 'SALESMAN' },
    { code: 'GEN', name: 'General', kind: 'GENERAL' },
  ];

  let count = 0;
  for (const t of types) {
    await prisma.erpPartnerType.upsert({
      where: { code: t.code },
      create: {
        code: t.code,
        name: t.name,
        kind: t.kind,
        isActive: true,
      },
      update: {},
    });
    count++;
  }
  console.log(`✓ md_partner_types (${count} entries)`);
}

async function main() {
  console.log('Seeding ERP MVP (m0 + m1)...\n');
  await seedLanguages();
  await seedSettings();
  await seedCurrency();
  await seedBranch();
  await seedLocations();
  await seedWarehouses();
  await seedPasswordPolicy();
  const menuIds = await seedMenus();
  await seedDocumentNumberings();
  const permIds = await seedPermissions();
  await seedRoleAndUser(menuIds, permIds);
  await seedDivisions();
  await seedNotifications();
  await seedPartnerCategories();
  await seedPartnerTypes(prisma);
  await seedTransactionNotes(prisma);
  await seedTransactionGrids(prisma);
  await seedSalesForms(prisma);
  await seedPurchasingForms(prisma);
  await seedInitialSetup(prisma);
  console.log('\n✅ ERP seed complete.');
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());

/**
 * Seed 100 vendor (supplier) realistis untuk manufaktur Indonesia.
 * Code format: VND-NNNN (VND-0001 … VND-0100).
 * Idempotent: upsert on code.
 * Run: npx ts-node prisma/seed-erp-md-vendors.ts
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

// ─── Data master vendor realistis (manufaktur Indonesia) ──────────────────────

const VENDOR_DATA: Array<{ name: string; taxNumber: string; isTaxable: boolean }> = [
  // Bahan baku logam & baja
  { name: 'PT Krakatau Steel Tbk',                taxNumber: '01.000.182.3-093.000', isTaxable: true },
  { name: 'PT Gunawan Dianjaya Steel Tbk',        taxNumber: '01.001.344.2-611.000', isTaxable: true },
  { name: 'PT Steel Pipe Industry of Indonesia',  taxNumber: '01.001.799.6-054.000', isTaxable: true },
  { name: 'PT Jaya Pari Steel Tbk',               taxNumber: '01.001.847.8-054.000', isTaxable: true },
  { name: 'PT Harapan Widyatama Pertiwi',         taxNumber: '01.002.115.4-054.000', isTaxable: true },
  { name: 'CV Logam Makmur Jaya',                 taxNumber: '02.114.567.8-541.000', isTaxable: true },
  { name: 'UD Sumber Besi Utama',                 taxNumber: '03.223.441.5-611.000', isTaxable: false },
  { name: 'PT Pelat Timah Nusantara Tbk',         taxNumber: '01.001.982.3-054.000', isTaxable: true },
  { name: 'PT Ispat Indo',                        taxNumber: '01.002.001.4-611.000', isTaxable: true },
  { name: 'CV Baja Sentosa Mandiri',              taxNumber: '02.334.112.6-611.000', isTaxable: true },

  // Bahan kimia & plastik
  { name: 'PT Chandra Asri Petrochemical Tbk',   taxNumber: '01.001.557.9-054.000', isTaxable: true },
  { name: 'PT Lotte Chemical Titan Tbk',          taxNumber: '01.001.778.2-054.000', isTaxable: true },
  { name: 'PT Asahimas Chemical',                 taxNumber: '01.001.889.5-054.000', isTaxable: true },
  { name: 'PT BASF Indonesia',                    taxNumber: '01.003.224.1-054.000', isTaxable: true },
  { name: 'PT Dow Chemical Indonesia',            taxNumber: '01.003.445.7-054.000', isTaxable: true },
  { name: 'PT Clariant Indonesia',                taxNumber: '01.002.998.3-054.000', isTaxable: true },
  { name: 'CV Kimia Prima Nusantara',             taxNumber: '02.445.881.2-611.000', isTaxable: true },
  { name: 'PT Evonik Indonesia',                  taxNumber: '01.004.112.8-054.000', isTaxable: true },
  { name: 'PT Ineos Indonesia',                   taxNumber: '01.003.678.4-054.000', isTaxable: true },
  { name: 'UD Plastik Sentosa',                   taxNumber: '03.112.334.9-542.000', isTaxable: false },

  // Kemasan & packaging
  { name: 'PT Indah Kiat Pulp & Paper Tbk',       taxNumber: '01.001.002.3-054.000', isTaxable: true },
  { name: 'PT Pabrik Kertas Tjiwi Kimia Tbk',     taxNumber: '01.001.113.6-054.000', isTaxable: true },
  { name: 'PT Fajar Surya Wisesa Tbk',            taxNumber: '01.001.557.2-054.000', isTaxable: true },
  { name: 'PT Tunas Baru Lampung Tbk',            taxNumber: '01.001.669.8-322.000', isTaxable: true },
  { name: 'CV Kemasan Unggul Sejahtera',          taxNumber: '02.556.223.7-611.000', isTaxable: true },
  { name: 'PT Berlina Tbk',                       taxNumber: '01.001.889.1-054.000', isTaxable: true },
  { name: 'PT Dynaplast Tbk',                     taxNumber: '01.001.997.4-054.000', isTaxable: true },
  { name: 'CV Karton Prima Jaya',                 taxNumber: '02.667.334.5-611.000', isTaxable: false },
  { name: 'PT Primavista Solusi',                 taxNumber: '01.004.556.3-054.000', isTaxable: true },
  { name: 'UD Kemasan Mandiri Abadi',             taxNumber: '03.334.556.8-611.000', isTaxable: false },

  // Komponen elektronik & elektrikal
  { name: 'PT Schneider Electric Manufacturing',  taxNumber: '01.002.334.9-054.000', isTaxable: true },
  { name: 'PT Siemens Indonesia',                 taxNumber: '01.001.445.3-054.000', isTaxable: true },
  { name: 'PT ABB Sakti Industri',                taxNumber: '01.002.778.6-054.000', isTaxable: true },
  { name: 'PT Omron Manufacturing Indonesia',     taxNumber: '01.003.112.5-054.000', isTaxable: true },
  { name: 'PT Panasonic Manufacturing Indonesia', taxNumber: '01.002.889.4-054.000', isTaxable: true },
  { name: 'CV Komponen Elektrik Pratama',         taxNumber: '02.778.445.3-611.000', isTaxable: true },
  { name: 'PT Fuji Electric Indonesia',           taxNumber: '01.003.556.2-054.000', isTaxable: true },
  { name: 'PT Delta Djakarta Tbk',                taxNumber: '01.001.334.7-054.000', isTaxable: true },
  { name: 'UD Elektrikal Nusantara Jaya',         taxNumber: '03.445.667.1-611.000', isTaxable: false },
  { name: 'CV Sinar Elektrindo',                  taxNumber: '02.889.112.4-542.000', isTaxable: true },

  // Mesin & peralatan industri
  { name: 'PT Komatsu Indonesia Tbk',             taxNumber: '01.001.223.5-054.000', isTaxable: true },
  { name: 'PT United Tractors Tbk',               taxNumber: '01.000.891.4-054.000', isTaxable: true },
  { name: 'PT Astra Otoparts Tbk',                taxNumber: '01.001.002.7-054.000', isTaxable: true },
  { name: 'PT Kubota Indonesia',                  taxNumber: '01.002.445.1-054.000', isTaxable: true },
  { name: 'PT Yanmar Agricultural Machinery',     taxNumber: '01.003.334.6-054.000', isTaxable: true },
  { name: 'CV Mesin Industri Sejahtera',          taxNumber: '02.990.223.8-611.000', isTaxable: true },
  { name: 'PT Fanuc Indonesia',                   taxNumber: '01.004.001.3-054.000', isTaxable: true },
  { name: 'UD Alat Berat Prima',                  taxNumber: '03.556.778.2-611.000', isTaxable: false },
  { name: 'PT Trumpf Indonesia',                  taxNumber: '01.005.223.9-054.000', isTaxable: true },
  { name: 'CV Teknik Mesin Mandiri',              taxNumber: '02.112.889.5-542.000', isTaxable: true },

  // MRO, perkakas & oli
  { name: 'PT Total Oil Indonesia',               taxNumber: '01.002.113.4-054.000', isTaxable: true },
  { name: 'PT Pertamina Lubricants',              taxNumber: '01.000.334.8-054.000', isTaxable: true },
  { name: 'PT Shell Indonesia',                   taxNumber: '01.001.778.9-054.000', isTaxable: true },
  { name: 'CV Perkakas Teknik Jaya',              taxNumber: '02.334.001.6-611.000', isTaxable: false },
  { name: 'PT Mobil Lubricants Indonesia',        taxNumber: '01.003.667.2-054.000', isTaxable: true },
  { name: 'UD Oli & Pelumas Prima',               taxNumber: '03.667.889.4-611.000', isTaxable: false },
  { name: 'PT Castrol Indonesia',                 taxNumber: '01.004.112.7-054.000', isTaxable: true },
  { name: 'CV Sumber Perkakas Mandiri',           taxNumber: '02.445.113.9-542.000', isTaxable: true },
  { name: 'PT Ansell Indonesia',                  taxNumber: '01.003.889.1-054.000', isTaxable: true },
  { name: 'CV MRO Andalan Sejahtera',             taxNumber: '02.778.334.3-611.000', isTaxable: false },

  // Jasa teknik & subkon
  { name: 'PT Rekayasa Industri',                 taxNumber: '01.001.556.4-054.000', isTaxable: true },
  { name: 'PT Waskita Karya Tbk',                 taxNumber: '01.000.445.6-054.000', isTaxable: true },
  { name: 'PT Adhi Karya Tbk',                    taxNumber: '01.000.334.1-054.000', isTaxable: true },
  { name: 'CV Konstruksi Maju Jaya',              taxNumber: '02.001.556.7-611.000', isTaxable: true },
  { name: 'PT Jasa Marga Tbk',                    taxNumber: '01.000.223.9-054.000', isTaxable: true },
  { name: 'CV Subkon Teknik Pratama',             taxNumber: '02.889.447.1-542.000', isTaxable: true },
  { name: 'PT Elnusa Tbk',                        taxNumber: '01.001.001.2-054.000', isTaxable: true },
  { name: 'UD Jasa Instalasi Listrik Utama',      taxNumber: '03.778.001.5-611.000', isTaxable: false },
  { name: 'PT Medco Energi Internasional Tbk',    taxNumber: '01.000.667.3-054.000', isTaxable: true },
  { name: 'CV Mekanikal Elektrikal Andalan',      taxNumber: '02.334.778.6-611.000', isTaxable: true },

  // Tekstil & bahan baku garmen
  { name: 'PT Sri Rejeki Isman Tbk (Sritex)',     taxNumber: '01.001.334.2-501.000', isTaxable: true },
  { name: 'PT Pan Brothers Tbk',                  taxNumber: '01.001.445.5-054.000', isTaxable: true },
  { name: 'PT Argo Manunggal Group',              taxNumber: '01.001.667.8-054.000', isTaxable: true },
  { name: 'PT Delta Dunia Makmur Tbk',            taxNumber: '01.001.889.2-054.000', isTaxable: true },
  { name: 'CV Tekstil Jaya Abadi',                taxNumber: '02.556.001.4-611.000', isTaxable: true },
  { name: 'PT Sinar Mas Agro Resources Tbk',      taxNumber: '01.000.778.7-054.000', isTaxable: true },
  { name: 'UD Kain Prima Nusantara',              taxNumber: '03.889.223.6-611.000', isTaxable: false },
  { name: 'PT Apaci Tbk',                         taxNumber: '01.001.112.4-054.000', isTaxable: true },
  { name: 'CV Benang & Serat Mandiri',            taxNumber: '02.001.889.8-542.000', isTaxable: false },
  { name: 'PT Primissima Tbk',                    taxNumber: '01.001.223.7-054.000', isTaxable: true },

  // Bahan baku pertanian & agro-industri
  { name: 'PT Salim Ivomas Pratama Tbk',          taxNumber: '01.001.334.3-054.000', isTaxable: true },
  { name: 'PT Astra Agro Lestari Tbk',            taxNumber: '01.000.889.1-054.000', isTaxable: true },
  { name: 'PT Smart Tbk',                         taxNumber: '01.001.112.6-054.000', isTaxable: true },
  { name: 'PT Bakrie Sumatera Plantations Tbk',   taxNumber: '01.000.556.9-054.000', isTaxable: true },
  { name: 'CV Agro Sumber Prima',                 taxNumber: '02.667.112.3-611.000', isTaxable: false },
  { name: 'PT Dharma Satya Nusantara Tbk',        taxNumber: '01.001.778.4-054.000', isTaxable: true },
  { name: 'UD Hasil Bumi Andalan',                taxNumber: '03.001.445.7-611.000', isTaxable: false },
  { name: 'PT PP London Sumatra Indonesia Tbk',   taxNumber: '01.000.668.2-054.000', isTaxable: true },
  { name: 'CV Perkebunan Mandiri Jaya',           taxNumber: '02.445.334.9-542.000', isTaxable: true },
  { name: 'PT Socfin Indonesia',                  taxNumber: '01.001.556.5-054.000', isTaxable: true },

  // IT & perangkat lunak
  { name: 'PT IBM Indonesia',                     taxNumber: '01.003.001.8-054.000', isTaxable: true },
  { name: 'PT Microsoft Indonesia',               taxNumber: '01.003.223.4-054.000', isTaxable: true },
  { name: 'PT SAP Indonesia',                     taxNumber: '01.003.445.1-054.000', isTaxable: true },
  { name: 'PT Multipolar Technology Tbk',         taxNumber: '01.001.667.6-054.000', isTaxable: true },
  { name: 'PT Telkom Indonesia Tbk',              taxNumber: '01.000.112.3-054.000', isTaxable: true },
  { name: 'CV Solusi Teknologi Andalan',          taxNumber: '02.889.556.2-611.000', isTaxable: true },
  { name: 'PT Oracle Indonesia',                  taxNumber: '01.004.334.7-054.000', isTaxable: true },
  { name: 'CV Infosys Nusantara',                 taxNumber: '02.001.112.5-542.000', isTaxable: true },
  { name: 'PT Tata Consultancy Services Indonesia', taxNumber: '01.005.001.2-054.000', isTaxable: true },
  { name: 'UD Sistem Informasi Prima',            taxNumber: '03.223.556.4-611.000', isTaxable: false },
];

const pad = (n: number, w = 4) => String(n).padStart(w, '0');

async function main() {
  console.log(`Seeding ${VENDOR_DATA.length} vendors (md_partners, partnerType=SUPPLIER)...`);
  const supplierType = await prisma.erpPartnerType.findUnique({ where: { code: 'SUP' } });
  if (!supplierType) throw new Error('Missing md_partner_types SUP seed');
  let created = 0;
  let skipped = 0;

  for (let i = 0; i < VENDOR_DATA.length; i++) {
    const v = VENDOR_DATA[i];
    const code = `VND-${pad(i + 1)}`;

    const existing = await prisma.erpPartner.findUnique({ where: { code } });
    if (existing) {
      skipped++;
      continue;
    }

    await prisma.erpPartner.create({
      data: {
        code,
        name: v.name,
        partnerTypeId: supplierType.id,
        taxNumber: v.taxNumber,
        isTaxable: v.isTaxable,
        isActive: true,
      },
    });
    created++;
  }

  console.log(`Done. Created: ${created}, Skipped (already exists): ${skipped}.`);
}

main()
  .catch((e) => { console.error(e); process.exit(1); })
  .finally(() => prisma.$disconnect());

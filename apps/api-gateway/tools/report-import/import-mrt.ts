/**
 * .mrt → template_json v2 batch importer (Wave G0).
 *
 * Usage (from apps/api-gateway):
 *   npx ts-node tools/report-import/import-mrt.ts --module m1
 *   npx ts-node tools/report-import/import-mrt.ts --code RPT-M1-ITEM --force
 *   npx ts-node tools/report-import/import-mrt.ts --module m1 --dry-run
 *
 * Worklist comes from m0_reports (design §3.2), source files from the
 * legacy mrt tree. Requires DATABASE_URL (loaded from apps/api-gateway/.env
 * when not already in the environment).
 */

import * as fs from 'fs';
import * as path from 'path';
import { PrismaClient } from '@prisma/client';
import { parseXml, sanitizeMrtXml, type XmlEl } from './mrt-xml';
import { convertMrt } from './mrt-map';
import { loadWorklist, saveConversion } from './import-report';

const MRT_ROOT = '/opt/erp-bahtera-madani/preferensi/Backened - myerpplus/report/mrt';

function loadDatabaseUrl(): void {
  if (process.env.DATABASE_URL) return;
  const envPath = path.resolve(__dirname, '../../.env');
  if (fs.existsSync(envPath)) {
    for (const line of fs.readFileSync(envPath, 'utf8').split('\n')) {
      const m = /^DATABASE_URL=(.*)$/.exec(line.trim());
      if (m) {
        process.env.DATABASE_URL = m[1].replace(/^"|"$/g, '');
        return;
      }
    }
  }
  throw new Error('DATABASE_URL tidak ditemukan (env / apps/api-gateway/.env)');
}

interface Args {
  module?: string;
  code?: string;
  all: boolean;
  dryRun: boolean;
  force: boolean;
  includeConverted: boolean;
}

function parseArgs(argv: string[]): Args {
  const args: Args = { all: false, dryRun: false, force: false, includeConverted: false };
  for (let i = 0; i < argv.length; i++) {
    const a = argv[i];
    if (a === '--module') args.module = argv[++i];
    else if (a === '--code') args.code = argv[++i];
    else if (a === '--all') args.all = true;
    else if (a === '--dry-run') args.dryRun = true;
    else if (a === '--force') {
      args.force = true;
      args.includeConverted = true;
    } else if (a === '--include-converted') args.includeConverted = true;
  }
  return args;
}

/** Regex fallback for files whose XML cannot be parsed even after sanitizing. */
function regexFallbackParse(raw: string): XmlEl {
  const text = (re: RegExp) => raw.match(re)?.[1] ?? '';
  const texts: XmlEl[] = [];
  const textRe = /<(\w+) Ref="\d+" type="Text"[^>]*>([\s\S]*?)<\/\1>/g;
  let m: RegExpExecArray | null;
  while ((m = textRe.exec(raw)) !== null) {
    const body = m[2];
    const get = (tag: string) => body.match(new RegExp(`<${tag}>([\\s\\S]*?)</${tag}>`))?.[1] ?? '';
    texts.push({
      tag: m[1],
      attrs: { type: 'Text' },
      children: [
        { tag: 'Name', attrs: {}, children: [], text: m[1] },
        { tag: 'Text', attrs: {}, children: [], text: get('Text') },
        { tag: 'ClientRectangle', attrs: {}, children: [], text: get('ClientRectangle') || '0,0,2,0.2' },
        { tag: 'Font', attrs: {}, children: [], text: get('Font') || 'Lao UI,9' },
      ],
      text: '',
    });
  }
  const dataBand: XmlEl = {
    tag: 'DataBand1',
    attrs: { type: 'DataBand' },
    children: [
      { tag: 'Name', attrs: {}, children: [], text: 'DataBand1' },
      { tag: 'DataSourceName', attrs: {}, children: [], text: 'DS1' },
      { tag: 'ClientRectangle', attrs: {}, children: [], text: '0,0,8,0.2' },
      { tag: 'Components', attrs: {}, children: texts, text: '' },
    ],
    text: '',
  };
  const page: XmlEl = {
    tag: 'Page1',
    attrs: { type: 'Page' },
    children: [
      { tag: 'PageWidth', attrs: {}, children: [], text: text(/<PageWidth>([\d.]+)<\/PageWidth>/) || '8.5' },
      { tag: 'PageHeight', attrs: {}, children: [], text: text(/<PageHeight>([\d.]+)<\/PageHeight>/) || '11' },
      { tag: 'Margins', attrs: {}, children: [], text: text(/<Margins>([\d.,]+)<\/Margins>/) || '0.3,0.1,0.2,0.15' },
      { tag: 'Components', attrs: {}, children: [dataBand], text: '' },
    ],
    text: '',
  };
  return {
    tag: 'StiSerializer',
    attrs: {},
    children: [
      { tag: 'ReportUnit', attrs: {}, children: [], text: 'Inches' },
      { tag: 'Pages', attrs: {}, children: [page], text: '' },
    ],
    text: '',
  };
}

async function main(): Promise<void> {
  const args = parseArgs(process.argv.slice(2));
  if (!args.module && !args.code && !args.all) {
    console.error('Pakai --module <m1|m2|…>, --code <RPT-…>, atau --all');
    process.exit(2);
  }
  loadDatabaseUrl();
  const prisma = new PrismaClient();
  try {
    const worklist = await loadWorklist(prisma, {
      module: args.module,
      code: args.code,
      includeConverted: args.includeConverted || args.force,
    });
    console.log(`Worklist: ${worklist.length} laporan`);
    let converted = 0;
    let skipped = 0;
    const failures: Array<{ code: string; error: string }> = [];
    for (const row of worklist) {
      const filePath = path.join(MRT_ROOT, row.sourceRelPath);
      try {
        if (!fs.existsSync(filePath)) throw new Error(`File tidak ditemukan: ${row.sourceRelPath}`);
        const raw = fs.readFileSync(filePath, 'utf8');
        let root: XmlEl;
        let fallback = false;
        try {
          root = parseXml(sanitizeMrtXml(raw));
        } catch {
          root = regexFallbackParse(raw);
          fallback = true;
        }
        const { template, warnings } = convertMrt(root, { sourceFile: row.filename, registryCode: row.code });
        if (fallback) warnings.push('XML tidak valid — dipakai regex fallback (konversi parsial)');
        if (args.dryRun) {
          console.log(
            `[dry-run] ${row.code}: bands=${template.bands.length} datasets=${template.datasets.map((d) => d.name).join(',')} warnings=${warnings.length}`,
          );
          converted++;
          continue;
        }
        const result = await saveConversion(prisma, row, template, args.force);
        if (result === 'skipped-verified') {
          skipped++;
          console.log(`SKIP (VERIFIED) ${row.code}`);
        } else {
          converted++;
          if (warnings.length) console.log(`OK ${row.code} (${warnings.length} warning)`);
        }
      } catch (err) {
        failures.push({ code: row.code, error: err instanceof Error ? err.message : String(err) });
        console.error(`GAGAL ${row.code}: ${err instanceof Error ? err.message : String(err)}`);
      }
    }
    console.log(`\nSelesai: converted=${converted} skipped=${skipped} failed=${failures.length}`);
    if (failures.length) process.exitCode = 1;
  } finally {
    await prisma.$disconnect();
  }
}

void main();

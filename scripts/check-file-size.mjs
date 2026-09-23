#!/usr/bin/env node
import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join, relative } from 'node:path';

const ROOT = new URL('..', import.meta.url).pathname;
const MAX_LINES = 400;

const INCLUDE_DIRS = ['app', 'components', 'lib', 'shared'];
const SOURCE_EXT = /\.(ts|tsx|js|jsx|mjs|cjs)$/;

const SKIP_DIR = new Set(['node_modules', '.next', 'dist', 'build', 'prototype', 'preferensi', 'db-design', '__fixtures__', '__mocks__', 'generated']);
const SKIP_FILE = /\.(test|spec|seed|mock|fixture|feed)\.[cm]?[jt]sx?$/;

const offenders = [];

function walk(dir) {
  for (const entry of readdirSync(dir)) {
    if (SKIP_DIR.has(entry)) continue;
    const full = join(dir, entry);
    const st = statSync(full);
    if (st.isDirectory()) {
      walk(full);
    } else if (SOURCE_EXT.test(entry) && !SKIP_FILE.test(entry)) {
      const lines = readFileSync(full, 'utf8').split('\n').length;
      if (lines > MAX_LINES) offenders.push({ file: relative(ROOT, full), lines });
    }
  }
}

for (const d of INCLUDE_DIRS) {
  try { walk(join(ROOT, d)); } catch { /* dir not present */ }
}

if (offenders.length === 0) {
  console.log(`OK — no source file exceeds ${MAX_LINES} lines.`);
  process.exit(0);
}

offenders.sort((a, b) => b.lines - a.lines);
console.error(`\nFAIL — ${offenders.length} file(s) exceed ${MAX_LINES} lines:\n`);
for (const { file, lines } of offenders) {
  console.error(`  ${lines.toString().padStart(5)}  ${file}`);
}
console.error('\nSplit each file into modules < 400 lines (see apps/web-erp/CLAUDE.md §3).\n');
process.exit(1);

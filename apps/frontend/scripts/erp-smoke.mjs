#!/usr/bin/env node
// Playwright smoke for the 19 ERP pages. Logs in once, then for each route
// rewrites the workspace tab in localStorage + reloads, captures console
// errors + a screenshot. All requests to the prod API are routed to local.

import { chromium } from 'playwright';
import { mkdir, writeFile } from 'node:fs/promises';
import { dirname } from 'node:path';

const FRONT = 'http://localhost:3219';
const PROD_API = 'https://erp.fr-labs.my.id/api/erp';
const LOCAL_API = 'http://localhost:3203/api/erp';
const OUT_DIR = '/home/rania/.claude/jobs/20fecc7f/smoke';

const ROUTES = [
  ['admin-users', '/admin/users', 'Users'],
  ['admin-roles', '/admin/roles', 'Roles'],
  ['admin-settings', '/admin/settings', 'System Settings'],
  ['admin-permissions', '/admin/permissions', 'Hak Akses'],
  ['admin-menus', '/admin/menus', 'Menu'],
  ['admin-document-numbering', '/admin/document-numbering', 'Document'],
  ['admin-fiscal-periods', '/admin/fiscal-periods', 'Fiscal'],
  ['master-branches', '/master/branches', 'Branch'],
  ['master-locations', '/master/locations', 'Location'],
  ['master-warehouses', '/master/warehouses', 'Warehouse'],
  ['master-items', '/master/items', 'Item'],
  ['master-item-categories', '/master/item-categories', 'Kategori'],
  ['master-units', '/master/units', 'Satuan'],
  ['master-partners', '/master/partners', 'Partner'],
  ['master-partner-categories', '/master/partner-categories', 'Partner Categor'],
  ['master-accounts', '/master/accounts', 'Account'],
  ['master-currencies', '/master/currencies', 'Curren'],
  ['master-taxes', '/master/taxes', 'Tax'],
  ['master-payment-terms', '/master/payment-terms', 'Payment'],
  // m2 Finance (skeleton CRUD)
  ['fin-journal-entries', '/keuangan/journal-entries', 'Journal'],
  ['fin-ar-receipts', '/keuangan/ar-receipts', 'AR Receipt'],
  ['fin-ap-payments', '/keuangan/ap-payments', 'AP Payment'],
  ['fin-giros', '/keuangan/giros', 'Giro'],
  ['fin-ledger', '/keuangan/ledger', 'Ledger'],
];

async function main() {
  await mkdir(OUT_DIR, { recursive: true });

  // Obtain JWT once, inject as Authorization header for all intercepted calls
  const loginResp = await fetch(`${LOCAL_API}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ login: 'admin', password: 'Admin123!' }),
  });
  const loginJson = await loginResp.json();
  const TOKEN = loginJson?.data?.accessToken;
  if (!TOKEN) throw new Error('No accessToken from local login: ' + JSON.stringify(loginJson));
  console.log('▶ token obtained, len=', TOKEN.length);

  const browser = await chromium.launch({ headless: true });
  const ctx = await browser.newContext({ viewport: { width: 1440, height: 900 } });

  // Reroute prod API calls to local; inject Bearer (avoids SameSite=Lax cookie loss)
  await ctx.route(`${PROD_API}/**`, async (route) => {
    const req = route.request();
    const localUrl = req.url().replace(PROD_API, LOCAL_API);
    try {
      const headers = { ...req.headers(), authorization: `Bearer ${TOKEN}` };
      const resp = await route.fetch({ url: localUrl, headers });
      await route.fulfill({ response: resp });
    } catch (e) {
      await route.abort();
    }
  });

  const page = await ctx.newPage();
  const errLog = [];
  page.on('console', (m) => {
    if (m.type() === 'error') errLog.push({ when: 'console', text: m.text() });
  });
  page.on('pageerror', (e) => errLog.push({ when: 'pageerror', text: e.message }));
  page.on('requestfailed', (r) => {
    const u = r.url();
    if (u.includes('/api/erp/')) errLog.push({ when: 'reqfail', text: `${r.failure()?.errorText} ${u}` });
  });

  console.log('▶ goto', FRONT);
  try {
    await page.goto(FRONT, { waitUntil: 'domcontentloaded', timeout: 30000 });
  } catch (e) { console.log('goto warn:', e.message); }
  await page.waitForTimeout(2500); // let client redirect to /ws1 + render
  console.log('▶ url =', page.url());
  await page.screenshot({ path: `${OUT_DIR}/00-landing.png` });

  try {
    await page.waitForSelector('input[autocomplete="username"]', { timeout: 20000 });
  } catch (e) {
    console.log('login form not visible:', e.message);
    console.log('first 500 chars:', (await page.content()).slice(0, 500));
    throw e;
  }
  console.log('▶ login');
  await page.locator('input[autocomplete="username"]').fill('admin');
  await page.locator('input[autocomplete="current-password"]').fill('Admin123!');
  await page.locator('button:has-text("Masuk"), button[type="submit"]').first().click();

  // Wait for shell — sidebar element
  await page.waitForSelector('.app, [class*="sidebar"]', { timeout: 15000 });
  await page.waitForTimeout(1500); // settle nav fetch
  console.log('▶ logged in, shell visible');
  await page.screenshot({ path: `${OUT_DIR}/00-shell.png`, fullPage: false });

  const results = [];
  for (const [slug, route, expectInTitle] of ROUTES) {
    errLog.length = 0;
    // Mutate workspace state to open exactly this route as the sole tab
    await page.evaluate((r) => {
      const key = 'erp-ws-ws1';
      const raw = localStorage.getItem(key);
      const ws = raw ? JSON.parse(raw) : { meta: { id: 'ws1', name: 'Workspace 1', createdAt: new Date().toISOString() } };
      ws.tabs = [{ id: 't1', route: r }];
      ws.activeTabId = 't1';
      localStorage.setItem(key, JSON.stringify(ws));
      if (!localStorage.getItem('erp-ws-list')) localStorage.setItem('erp-ws-list', JSON.stringify(['ws1']));
    }, route);
    await page.reload({ waitUntil: 'networkidle' });
    await page.waitForTimeout(1500);
    const html = await page.content();
    const titleMatch = html.toLowerCase().includes(expectInTitle.toLowerCase());
    const isComingSoon = html.includes('Coming Soon') || html.includes('coming-soon');
    const status = errLog.length === 0 && titleMatch && !isComingSoon
      ? 'OK'
      : (isComingSoon ? 'COMING_SOON_FALLBACK' : (errLog.length ? 'ERROR' : 'TITLE_MISS'));
    results.push({ route, status, errors: [...errLog] });
    await page.screenshot({ path: `${OUT_DIR}/${slug}.png`, fullPage: false });
    console.log(`  ${status.padEnd(22)} ${route}${errLog.length ? '  (' + errLog.length + ' errs)' : ''}`);
  }

  await browser.close();
  const report = {
    pages: ROUTES.length,
    ok: results.filter((r) => r.status === 'OK').length,
    errors: results.filter((r) => r.status === 'ERROR').length,
    comingSoon: results.filter((r) => r.status === 'COMING_SOON_FALLBACK').length,
    titleMiss: results.filter((r) => r.status === 'TITLE_MISS').length,
    results,
  };
  await writeFile(`${OUT_DIR}/report.json`, JSON.stringify(report, null, 2));
  console.log('\n=== SMOKE SUMMARY ===');
  console.log(`OK: ${report.ok}/${report.pages}  ERR: ${report.errors}  COMING_SOON: ${report.comingSoon}  TITLE_MISS: ${report.titleMiss}`);
  if (report.errors || report.comingSoon || report.titleMiss) {
    for (const r of results.filter((x) => x.status !== 'OK')) {
      console.log(`  - ${r.route} [${r.status}]`);
      for (const e of r.errors.slice(0, 3)) console.log(`      ${e.when}: ${e.text.slice(0, 200)}`);
    }
  }
}

main().catch((e) => { console.error(e); process.exit(1); });

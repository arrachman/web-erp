#!/usr/bin/env python3
"""Buat invoice penjualan DRAFT dari sheet INVOICE lewat API ERP. Pakai: python3 import-invoices.py [N|all] [--apply]  (tanpa --apply = dry-run/rollback). Perlu login API tidak lagi; tulis via Prisma di container."""
import json, os, re, subprocess, sys, urllib.request, http.cookiejar, datetime, collections
TMP = os.environ.get('CLAUDE_JOB_DIR', '/tmp') + '/tmp'
API = 'http://localhost:3203/api/erp'
exec(open(os.path.dirname(os.path.abspath(__file__)) + '/import-master.py').read().split('def rows(wb')[0].split("FLAGS = collections.defaultdict(list)")[1].split('def sq')[0]) if False else None

SCHOOL_ALIAS = {'TL ABA 5': 'TK ABA 5', 'ABA 5': 'TK ABA 5', 'ABA 24': 'TK ABA 24', 'TK A YANI': 'TK AHMAD YANI',
    'WONDERFULL KIDS': 'TK WONDERFULL KIDS', 'TK MASLIMAT NU 34': 'TK MUSLIMAT NU 34', 'PP PERMATA KASIH': 'TK PERMATA KASIH',
    'PP. ROSALIA': 'POS PAUD ROSALIA', 'RA NA NAYARA': 'RA AN NAYARA', 'TK NAYARA': 'RA AN NAYARA',
    'TK MUSLIMAT NU 28': 'TK MUSLIMAT NU 28 HASYIM ASYARI', "TK HASYIM ASY'ARI": 'TK MUSLIMAT NU 28 HASYIM ASYARI'}
ITEM_ALIAS = {'ENSIKLOPEDIA SI KECIL ISI 12 BUKU': 'ENSIKLOPEDIA SI KECIL ISI 12 BUKU KB', 'SEJARAH 25 NABI DAN RASAUL': 'SEJARAH 25 NABI DAN RASUL',
    'TANTANGAN JAGARAGA': 'TANTANGAN JAGA RAGA', 'BAHASA JAWA A': 'BUKU BAHASA JAWA A', 'BAHSA JAWA B': 'BUKU BAHASA JAWA B',
    'BAHASA JAWA B': 'BUKU BAHASA JAWA B'}

def ns(r):
    n = re.sub(r'\s+', ' ', r).strip().upper(); return SCHOOL_ALIAS.get(n, n)
def ni(r):
    n = re.sub(r'\s+', ' ', r).strip().upper(); n = re.sub(r'\bBHS\b', 'BAHASA', n).replace('BAHSA', 'BAHASA'); return ITEM_ALIAS.get(n, n)

def psql(sql):
    p = subprocess.run(['docker', 'exec', '-i', 'sentient-postgres-core', 'sh', '-c', 'psql -U "${POSTGRES_USER:-postgres}" -d sentient_factory -Atq -F"|"'], input=sql, capture_output=True, text=True)
    if p.returncode: sys.exit(p.stderr)
    return [l.split('|') for l in p.stdout.strip().splitlines()]

cust = {r[1]: r[0] for r in psql("select id,name from md_partners where legacy_code='data-client' and code like 'CUST-%'")}
item = {r[1]: (r[0], r[2], r[3]) for r in psql("select id,name,base_unit_id,sale_price from md_items where legacy_code='data-client'")}

cj = http.cookiejar.CookieJar(); op = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj))
def call(path, body=None):
    req = urllib.request.Request(API + path, data=json.dumps(body).encode() if body is not None else None, headers={'content-type': 'application/json'})
    try:
        return json.loads(op.open(req).read())
    except urllib.error.HTTPError as e:
        return {'_http': e.code, **json.loads(e.read() or b'{}')}
r = call('/auth/login', {'login': 'admin@senti-erp.local', 'password': 'Admin123!'})
assert r.get('success'), r

invs = json.load(open(TMP + '/invoices.json'))
seen = collections.Counter(i['no'] for i in invs); idx = collections.Counter(); out = []
want = next((a for a in sys.argv[1:] if not a.startswith('--')), '1')
for n, inv in enumerate(invs):
    if want != 'all' and n >= int(want): break
    no = inv['no']
    if seen[no] > 1:
        idx[no] += 1; no = f"{no}-{'AB'[idx[no]-1]}"
    d, m, y = inv['date'].split('-'); date = f'{y}-{m}-{d}'
    sc = ns(inv['school']); cid = cust.get(sc)
    lines = []
    for ln, (nm, qty, price, tot) in enumerate(inv['lines'], 1):
        it = item.get(ni(nm))
        if not it: sys.exit(f'item tidak ditemukan: {nm} -> {ni(nm)}')
        p = price if price is not None else (tot / qty if tot else float(it[2]))
        lines.append({'itemId': it[0], 'unitId': it[1], 'quantity': f'{qty:.4f}', 'unitPrice': f'{p:.4f}', 'lineNo': ln})
    body = {'auto': False, 'docNumber': no, 'docDate': date, 'branchId': '1', 'currencyId': '1', 'exchangeRate': '1', 'priceMode': 'TAX_INCLUSIVE',
            'status': 'DRAFT', 'lines': lines,
            'customFields': {'sourcePO': inv.get('po'), 'sourceSJ': inv.get('sj'), 'sourceSchool': inv['school'], 'marketing': inv.get('mkt'), 'legacyInvoiceNo': inv['no'], 'origin': 'data-client'}}
    if os.environ.get('NO_CF'): body.pop('customFields')
    if cid: body['customerId'] = cid
    meta = body.pop('customFields', None)
    out.append({'dto': body, 'meta': meta, 'school': sc, 'hasCustomer': bool(cid)})
json.dump(out, open(TMP + '/invs-prepared.json', 'w'))
NODE = open(os.path.dirname(os.path.abspath(__file__)) + '/create-invoices.js').read()
subprocess.run(['docker', 'cp', TMP + '/invs-prepared.json', 'sentient-infra-api-gateway:/tmp/invs.json'], check=True)
subprocess.run(['docker', 'cp', os.path.dirname(os.path.abspath(__file__)) + '/create-invoices.js', 'sentient-infra-api-gateway:/tmp/create-invoices.js'], check=True)
r = subprocess.run(['docker', 'exec', '-w', '/app', 'sentient-infra-api-gateway', 'node', '/tmp/create-invoices.js'] + (['--apply'] if '--apply' in sys.argv else []), capture_output=True, text=True)
print(r.stdout, r.stderr)
subprocess.run(['docker', 'exec', 'sentient-infra-api-gateway', 'rm', '-f', '/tmp/invs.json', '/tmp/create-invoices.js'])

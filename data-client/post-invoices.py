#!/usr/bin/env python3
"""Posting invoice data-client lewat API workflow (SUBMIT->APPROVE->POST). Pakai: python3 post-invoices.py 09/0004 [09/0001 ...|all]"""
import json, subprocess, sys, urllib.request, http.cookiejar
import os
API = os.environ.get('ERP_API', 'http://localhost:3203/api/erp')
cj = http.cookiejar.CookieJar(); op = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj))
def call(path, body):
    req = urllib.request.Request(API + path, data=json.dumps(body).encode(), headers={'content-type': 'application/json'})
    try: return json.loads(op.open(req).read())
    except urllib.error.HTTPError as e: return {'_http': e.code, **json.loads(e.read() or b'{}')}
def psql(sql):
    p = subprocess.run(['docker','exec','-i','sentient-postgres-core','sh','-c','psql -U "${POSTGRES_USER:-postgres}" -d sentient_factory -Atq -F"|"'], input=sql, capture_output=True, text=True)
    if p.returncode: sys.exit(p.stderr)
    return [l.split('|') for l in p.stdout.strip().splitlines()]
assert call('/auth/login', {'login': 'admin@senti-erp.local', 'password': 'Admin123!'}).get('success')
rows = psql("select doc_number,id,status from sls_invoices where legacy_code='data-client' and customer_id is not null and deleted_at is null order by id")
want = sys.argv[1:]
for no, iid, st in rows:
    if want != ['all'] and no not in want: continue
    for act in ('SUBMIT', 'APPROVE', 'POST'):
        cur = psql(f"select status from sls_invoices where id={iid}")[0][0]
        if (act, cur) not in (('SUBMIT','DRAFT'), ('APPROVE','NEED_APPROVE'), ('POST','APPROVED')): continue
        r = call(f'/sls/invoices/{iid}/transition', {'action': act})
        if not r.get('success', False) and r.get('_http'):
            print(no, act, 'GAGAL', json.dumps(r, ensure_ascii=False)[:300]); sys.exit(1)
    print(no, 'status ->', psql(f"select status||'/'||posting_status from sls_invoices where id={iid}")[0][0])

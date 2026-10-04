import json, urllib.request, urllib.error

BASE = "http://localhost:3203/api/erp"

def login():
    req = urllib.request.Request(BASE + "/auth/login", method="POST")
    req.add_header("Content-Type", "application/json")
    data = json.dumps({"login": "admin@senti-erp.local", "password": "Admin123!"}).encode()
    with urllib.request.urlopen(req, data=data, timeout=30) as r:
        return json.loads(r.read())["data"]["accessToken"]

TOKEN = login()

def call(method, path, body=None):
    req = urllib.request.Request(BASE + path, method=method)
    req.add_header("Authorization", "Bearer " + TOKEN)
    data = None
    if body is not None:
        data = json.dumps(body).encode()
        req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req, data=data, timeout=30) as r:
            return r.status, json.loads(r.read())
    except urllib.error.HTTPError as e:
        try:
            return e.code, json.loads(e.read())
        except Exception:
            return e.code, {}

def line(no, item, qty, price):
    return {"lineNo": no, "itemId": str(item), "unitId": "115", "quantity": str(qty), "unitPrice": str(price)}

HDR = {"branchId": "1033", "currencyId": "1", "exchangeRate": "1", "docDate": "2026-10-01", "channel": "ADMIN"}

so1 = {**HDR, "customerId": "1035", "lines": [
    line(1, 834, 9, 50000), line(2, 835, 25, 25000), line(3, 836, 20, 21000),
    line(4, 837, 2, 50000), line(5, 838, 25, 25000),
]}
so2 = {**HDR, "customerId": "1025", "lines": [line(1, 838, 79, 15000)]}

for label, payload in [("ROSALIA", so1), ("AHMAD YANI", so2)]:
    s, r = call("POST", "/sls/orders", payload)
    d = r.get("data", {})
    print(label, s, d.get("docNumber"), "id", d.get("id"), "status", d.get("status"), "total", d.get("grandTotal"), str(r.get("message"))[:140])

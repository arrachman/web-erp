# API Gateway — Refactor Plan (Clean Code & 400-Line Cap)

**Goal:** Setiap file di `apps/api-gateway/src` ≤ 400 baris, satu tanggung jawab per file, **tanpa mengubah behavior** (refactor murni).
**Aturan emas:** Public API (controller routes, exported service methods) tidak berubah. Test harus tetap hijau di setiap commit.

## Ringkasan

12 file > 400 baris, total ~19.700 baris (≈58% codebase TS).

| Prioritas | File | Baris | Domain |
|:--:|---|---:|---|
| P0 | dashboard/dashboard.service.ts | 8.252 | alerting + dashboard + ETL (god class) |
| P0 | hr-attendance/hr-attendance.service.ts | 2.565 | face enrollment + worksite + attendance |
| P0 | outbound/outbound.service.ts | 1.947 | DO CRUD + monitoring + batch options |
| P1 | dashboard/alerting-config.service.ts | 1.233 | channels + templates |
| P1 | dashboard/alerting-rule.service.ts | 944 | rules + insights + saved queries |
| P1 | inbounds/inbounds.service.ts | 936 | inbound CRUD + stock guard |
| P1 | clinic-psikolog/clinic-psikolog.service.ts | 932 | psikolog + stats + dashboard |
| P1 | dashboard/dashboard.controller.ts | 898 | dashboard routes (split per domain) |
| P2 | users/users.service.ts | 666 | user CRUD + role assign |
| P2 | menus/menus.service.ts | 640 | menu tree CRUD |
| P2 | master-data-roles/master-data-roles.service.ts | 465 | role CRUD + permission map |
| P2 | dashboard/dashboard-mysql.service.ts | 458 | MyERP+ adapter |

---

## Strategi umum (terapkan di tiap file)

1. **Identifikasi sub-domain** dari method clustering (lihat peta per file di bawah).
2. **Ekstrak helper non-stateful** lebih dulu (formatter, validator, query builder) → file `*.utils.ts` / `*.formatter.ts` — risiko terendah.
3. **Pecah service besar** menjadi beberapa sub-service ber-`@Injectable()`. Service utama jadi **facade tipis** yang mendelegasikan — public method signature **persis sama**.
4. **Module update**: daftarkan sub-service baru di `*.module.ts` (providers + exports bila dipakai modul lain).
5. **Test gate per commit**: `npm run typecheck && npm test --workspace=apps/api-gateway --filter=<domain>` harus hijau.
6. **Conventional commit**: `refactor(api-gateway/<domain>): split X into Y` per langkah kecil.

**Pattern split khas NestJS service**:
```
domain/
├── domain.module.ts          # register sub-services + facade
├── domain.controller.ts      # tetap, atau di-split per sub-route
├── domain.service.ts         # facade: ≤ 200 baris, delegate ke sub-services
├── services/
│   ├── domain-query.service.ts
│   ├── domain-mutation.service.ts
│   └── domain-stats.service.ts
├── utils/
│   ├── domain.formatter.ts
│   └── domain.validator.ts
└── types/domain.types.ts
```

---

## Peta split per file

### P0-1 · `dashboard/dashboard.service.ts` (8.252 → ~10 file ≤ 400)

File ini menggabungkan **alerting engine + dashboard query + scheduler + MyERP+ pin targets**. Sudah ada `alerting-config.service.ts` dan `alerting-rule.service.ts` tapi masih banyak nyangkut di service utama.

Cluster yang teridentifikasi (dari method signature):
- **Scheduler/timers** (constructor + onModuleInit/Destroy + `runAlertingSchedulerCycle`, `runAlertDeliveryCycle`, triage escalation) → `services/alerting-scheduler.service.ts`
- **Alerting rules CRUD** (`createAlertingRule`, `updateAlertingRule`, `updateAlertingRuleState`, `deleteAlertingRule`, `runAlertingRule`, `alertingRuleDetail`) → **pindahkan ke `alerting-rule.service.ts` yang sudah ada** (saat ini cuma read).
- **Alerting metrics & insights** (`alertingBusinessMetrics`, `alertingSystemMetrics`, `alertingMetricBuilderContext`, `alertingInsights`, `alertingSavedQueries`) → **pindahkan ke `alerting-rule.service.ts`** atau buat `alerting-metrics.service.ts` baru.
- **Alerting events & delivery logs** (`alertingEvents`, `alertingDeliveryLogs`, `requeueAlertingDeliveryLog`, dead-letter triage) → `services/alerting-delivery.service.ts`
- **Alerting analytics/ops** (`alertingAnalytics`, `alertingDeliveryObservability`, `alertingOpsOverview`, `alertingProviderHealth`) → `services/alerting-observability.service.ts`
- **Dashboard pin targets & domains** (`customDbPinTargets`, `listDomains`, `health`, `managerKpis`) → `services/dashboard-meta.service.ts`
- **SQL helpers** (semua raw SQL bertumpuk) → `utils/alerting.sql.ts` + `utils/dashboard.sql.ts` (template literal returning strings)
- **Formatter** (jsonb_agg post-processing) → `utils/alerting.formatter.ts`

**Urutan eksekusi** (≈10 commit):
1. Ekstrak SQL templates ke `utils/*.sql.ts`. ⟶ ~−800 baris.
2. Pindah CRUD rule + metric read ke `alerting-rule.service.ts` (sudah ada). ⟶ ~−1.500 baris.
3. Pindah config detail ke `alerting-config.service.ts` (sudah ada). ⟶ ~−500 baris.
4. Buat `alerting-scheduler.service.ts` (timers + run cycles). ⟶ ~−1.200 baris.
5. Buat `alerting-delivery.service.ts` (events, logs, requeue, triage). ⟶ ~−1.500 baris.
6. Buat `alerting-observability.service.ts` (analytics, ops, health). ⟶ ~−800 baris.
7. Buat `dashboard-meta.service.ts`. ⟶ ~−400 baris.
8. Sisa `dashboard.service.ts` jadi **facade ≤ 250 baris**.
9. Update `dashboard.module.ts` (register semua sub-service).
10. Smoke test endpoint dashboard + alerting.

### P0-2 · `hr-attendance/hr-attendance.service.ts` (2.565 → ~6 file)

Cluster:
- **Worksite assignment** (`getAssignedWorksites`, `getAssignedWorksiteMap`, `syncAssignedWorksites`) → `services/worksite-assignment.service.ts`
- **Face enrollment** (`createFaceEnrollment` + helpers) → `services/face-enrollment.service.ts`
- **Attendance check-in/out** (cluster method clock-in/out — verifikasi lewat grep) → `services/attendance-tracking.service.ts`
- **Reporting / queries** → `services/attendance-query.service.ts`
- **Date normalization** (`normalizeHrDates`) → `utils/hr-dates.ts`
- Facade `hr-attendance.service.ts` ≤ 250 baris.

### P0-3 · `outbound/outbound.service.ts` (1.947 → ~5 file)

Cluster:
- **DO CRUD** (`create`, `findAll`, `findOne`, `update`, `remove`) → tetap di service utama (cek size, kemungkinan ~700 baris → masih > 400).
- **Batch options** (`getBatchOptions`) → `services/outbound-batch.service.ts` (kompleks, banyak pair matching).
- **Monitoring report** (`findMonitoringReport`) → `services/outbound-monitoring.service.ts`.
- **Warehouse scope helper** + duplicate handlers → `utils/outbound.helpers.ts`.
- **SQL untuk monitoring** → `utils/outbound.sql.ts`.

### P1-1 · `dashboard/alerting-config.service.ts` (1.233 → 3 file)

- **Channels** (`alertingChannels` + CRUD + `validateAlertChannelTarget`) → tetap di service ini.
- **Templates** (`alertingTemplates`, `createAlertingTemplate`, detail, update) → `alerting-template.service.ts` baru.
- **Validators** (`validateAlertTemplateSource`, `validateAlertChannelTarget`) → `utils/alerting-validators.ts` (pure functions).

### P1-2 · `dashboard/alerting-rule.service.ts` (944, akan bertambah dari P0-1)

Setelah P0-1 selesai, file ini akan lebih besar lagi. Pecah lagi:
- **Rule CRUD** → tetap di sini.
- **Metrics read** (`alertingBusinessMetrics`, `alertingSystemMetrics`, `alertingMetricBuilderContext`) → `alerting-metrics.service.ts`.
- **Insights & saved queries** → `alerting-insights.service.ts`.

### P1-3 · `inbounds/inbounds.service.ts` (936 → 3 file)

- **CRUD** (`create`, `findAll`, `findOne`, `update`, `remove`) → tetap (≈600 baris → masih > 400, pecah `findAll` filter builder ke utils).
- **Stock guard** (`ensureInboundDeleteWillNotCauseNegativeStock`) → `services/inbound-stock-guard.service.ts`.
- **Transaction-no helpers** (`resolveTransactionNo`, `ensureTransactionNoAvailable`) → `utils/inbound-transaction.ts`.
- **Detail mapper** → `utils/inbound-detail.mapper.ts`.

### P1-4 · `clinic-psikolog/clinic-psikolog.service.ts` (932 → 3 file)

- **CRUD** (`create`, `findAll`, `findOne`, `findByUserId`, `updateMe`) → tetap.
- **Dashboard & stats** (`getMyStats`, `getDashboardStats`) → `services/psikolog-dashboard.service.ts`.
- **Avatar validator** + service mapping → `utils/psikolog.helpers.ts`.

### P1-5 · `dashboard/dashboard.controller.ts` (898 → 4 controller)

Pecah per sub-route prefix (tetap di module yang sama):
- `dashboard-meta.controller.ts` — `/dashboard/domains`, `/health`, `/manager/kpis`, `/custom-db/pin-targets`.
- `alerting-rules.controller.ts` — `/alerting/rules*`, `/alerting/business-metrics`, `/alerting/system-metrics`, `/alerting/insights`, `/alerting/saved-queries`.
- `alerting-delivery.controller.ts` — `/alerting/events`, `/alerting/delivery-logs*`, `/alerting/dead-letter*`, scheduler/delivery cycles.
- `alerting-ops.controller.ts` — `/alerting/analytics`, `/observability`, `/ops-overview`, `/provider-health`, `/delivery-status`.

### P2-1 · `users/users.service.ts` (666 → 2 file)

- **Lookup** (`findOneBy*`, `hasWarehouse`, `getWarehouseMetaByUserUuid`, `getActiveRoleNamesByUserId`) → tetap.
- **Admin mutations** (`createFromAdmin`, `update`, role assignment) → `services/user-admin.service.ts`.

### P2-2 · `menus/menus.service.ts` (640 → 2 file)

- **CRUD** → tetap.
- **Tree builder & traversal** (`findAll` rekursif + helpers) → `utils/menu-tree.ts`.

### P2-3 · `master-data-roles/master-data-roles.service.ts` (465 → 2 file)

- **CRUD + listing** → tetap.
- **Permission mapping** (assign/revoke permission ke role) → `services/role-permissions.service.ts`.

### P2-4 · `dashboard/dashboard-mysql.service.ts` (458 → 2 file)

- **Adapter & connection** → tetap.
- **Query templates** → `utils/myerp.sql.ts`.

---

## Eksekusi & gating

Per file target, satu PR (atau satu deret commit di branch `refactor/api-gateway-<file>`):

```bash
# 1. Branch
git checkout -b refactor/api-gateway-dashboard-service

# 2. Per langkah extract:
#    - move code
#    - update imports
#    - update module providers
git add -p && git commit -m "refactor(api-gateway/dashboard): extract <slice> into <file>"

# 3. Gate per commit (WAJIB hijau sebelum lanjut)
npm run typecheck
npm test --workspace=apps/api-gateway

# 4. Setelah file target ≤ 400, smoke test endpoint
npm run dev
# manual curl ke endpoint terdampak
```

**Stop rule**: jika test merah dan tidak bisa diperbaiki ≤ 15 menit, `git reset --hard HEAD~1` dan pecah commit lebih kecil. Jangan lanjut menumpuk perubahan.

## Risiko & mitigasi

| Risiko | Mitigasi |
|---|---|
| Circular import antar sub-service | Sub-service hanya bergantung ke `PrismaService` + util murni; facade yang orkestrasi. |
| Timer/scheduler bocor (dashboard.service) | Pindahkan `onModuleInit/Destroy` bareng timer-nya ke `alerting-scheduler.service.ts` — satu lifecycle owner. |
| Behavior berubah karena helper tidak murni | Audit setiap fungsi yang baca/tulis `this.*` sebelum pindah ke utils — kalau ada state, jadikan sub-service. |
| Test cakupan tipis | Sebelum split P0, tambahkan integration test endpoint kritis (alerting CRUD, hr face enrollment, outbound monitoring). |
| Module providers ketinggalan daftar | Setelah split, `nest start` dev — error DI muncul instan kalau lupa register. |

## Definition of Done

- [ ] Semua 12 file target ≤ 400 baris (cek: `find apps/api-gateway/src -name "*.ts" | xargs wc -l | awk '$1>400'` → kosong).
- [ ] Public API tidak berubah (`apps/api-gateway/src/**/*.controller.ts` diff hanya berisi pemindahan, bukan perubahan route/signature).
- [ ] `npm run typecheck` + `npm test --workspace=apps/api-gateway` hijau.
- [ ] Smoke test manual: dashboard load, alerting CRUD, hr attendance check-in, outbound DO create+monitoring.
- [ ] CHANGELOG entry per fase besar.

## Estimasi

| Prioritas | Estimasi sesi |
|---|---|
| P0 (3 file) | 4–6 sesi |
| P1 (5 file) | 3–4 sesi |
| P2 (4 file) | 1–2 sesi |
| **Total** | **8–12 sesi** |

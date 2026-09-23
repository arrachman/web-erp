# Manufacturing / Production: Entity Catalog (m6 → `mfg`)

> Legacy "Production" (m6) maps to the **`mfg_*`** semantic domain per
> [web-erp/CLAUDE.md §1](../CLAUDE.md). No `erp_` prefix, no numeric `m<n>` segment.
> Depth = modern **core subset** (resolved [README §8](README.md#8-resolved-decisions-2026-05-17)
> #15). **Source note:** m6 is **not** in `semantic-schema.json` (that covers m0–m5
> only) — this catalog is derived from the legacy schema in
> `/home/rania/apps/myerpplus_serenity.sql` (read-only) + the m6 Flex screens.

Field-level model. Types Prisma/Postgres (PK/FK = **`BigInt`**, resolved §8 #2).
Global **audit + soft-delete** ([README §3](README.md#3-global-conventions)) and
`legacyCode String?` on transaction masters — omitted per-row. `*custom{text,int,
dbl,date}*` → `metadata Json?` (resolved §8 conventions). Money/qty
`Decimal(19,4)`, rate `Decimal(19,6)`.

> **Production flow:** BOM (recipe) → WorkOrder → {MaterialIssue (to floor) /
> MaterialReturn (from floor)} → ProductionEntry (consume in / yield out) →
> ProductionRework. Every doc has **input** (materials consumed) and **output**
> (goods produced) line sets. Upstream links via `bomId`/`workOrderId`/
> `productionReworkId`/`materialIssueId`/`materialReturnId` (legacy
> `*idbom/idwo/idpdr/idmrs/idmrn` + line-level `idbomin`/`idwoin`/…).
> Period FK = `fiscalPeriodId → sys_fiscal_periods`; dimensions reuse m2 `md_*`.

---

## Common shapes (defined once, referenced per entity)

### «MfgDocHeader» — shared header columns

| Field | Type | Legacy | Notes |
| --- | --- | --- | --- |
| id | BigInt PK | `*id` | |
| docNumber 🔑 | String unique | `*notransaksi` | |
| autoNumber ○ | String | `*autonotransaksi` | via `sys_document_numberings` |
| docType ◆ | `MfgDocType` | (per entity) | BOM/WORK_ORDER/… |
| kind ○ | String | `*jenis` | sub-type |
| branchId ➜ | BigInt → Branch | `*cabang` | |
| locationId ○ ➜ | BigInt → Location | `*lokasi` | |
| sourceWarehouseId ○ ➜ | BigInt → Warehouse | `*gudangasal` | |
| productionWarehouseId ○ ➜ | BigInt → Warehouse | `*gudangproduksi` | WIP |
| destinationWarehouseId ○ ➜ | BigInt → Warehouse | `*gudangtujuan` | FG |
| docDate | Date | `*tgl` | |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `*kodepa` | |
| requestedById ○ ➜ | BigInt → ErpUser | `*pembuat`/`*dimintaoleh`/`*bagian*` | |
| requestedPartnerId ○ ➜ | BigInt → Partner | `*pembuatkontak`/`*…kontak` | |
| neededDate ○ | Date | `*tgldipakai` | |
| workEstimate ○ | Decimal(19,4) | `*estimasikerja` | est. effort |
| currencyId ➜ | BigInt → Currency | `*matauang` | |
| exchangeRate | Decimal(19,6) | `*kurs` | |
| inputTotalPrice ○ | Decimal(19,4) | `*totalhargain` | |
| outputTotalPrice ○ | Decimal(19,4) | `*totalhargaout` | |
| inputTotalCost ○ | Decimal(19,4) | `*totalhppin` | |
| outputTotalCost ○ | Decimal(19,4) | `*totalhppout` | |
| description ○ | String | `*uraian` | |
| notes ○ | String | `*catatan` | |
| referenceNo ○ | String | `*noref` | |
| referenceDate ○ | Date | `*tglnoref` | |
| sourceDocType ○ | String | `*sumber` | |
| status ◆ | `DocumentStatus` | `*status` | |
| previousStatus ○ ◆ | `DocumentStatus` | `*statussebelumnya` | |
| revisionCount | Int | `*jmlrevisi` | |
| printCount | Int | `*cetakanke` | |
| postingStatus ◆ | `PostingStatus` | `*posting` | |
| postedAt ○ | DateTime | `*postingtgl` | |
| metadata ○ | Json | `*custom*`, `*aktivitas` | |

Plus **upstream links** (nullable FK; only the meaningful ones per entity):
`bomId`, `workOrderId`, `productionReworkId`, `materialIssueId`,
`materialReturnId` (legacy `*idbom/idwo/idpdr/idmrs/idmrn`).

### «MfgInputLine» / «MfgOutputLine» — shared line columns

Both share: `id` PK, `«parent»Id ➜`, `itemId ➜ Item` (`idbarang`; echoes
dropped), `quantity` (`jml`)/`unitId ➜`/`unitValue` (`nilaisatuan`)/
`baseQuantity` (`jmlbarang`)/`baseUnitId ➜`, `currencyId ➜`/`exchangeRate`,
`unitPrice` (`harga`), `unitCost` (`hpp`), `inventoryAccountId ➜ Account`
(`rekpersediaan`), warehouse trio (`gudangasal/produksi/tujuan`), dimension FKs
(`costCenterId/divisionId/subdivisionId/projectId → md_*`), `notes` (`catatan`),
`lineNo` (`urutan`), `metadata Json?` (`custom*`), and chain line links
(`bomLineId`/`workOrderLineId`/… from `idbomin`/`idwoin`/`idpdrin`/`idmrsin`/
`idmrnin`).

- **«MfgInputLine»** adds: `costPercent ○` (`hpppersen` — overhead allocation %).
- **«MfgOutputLine»** adds: `costLayerInId ○` / `costLayerFifoId ○`
  (`idhppkhususmasuk`/`idhppfifomasuk` — valuation-layer refs for COGS).

> Per-step fulfilment counters (`jmlmrs`/`statusmrs`, `jmlpd`/`statuspd`, …) are
> **derived** from downstream lines, not stored.

---

## Entities (`mfg_*`)

Each = «MfgDocHeader» + `inputs: «MfgInputLine»[]` + `outputs: «MfgOutputLine»[]`,
with the deltas below.

### ErpMfgBom → `mfg_boms` (+ `mfg_bom_inputs` / `mfg_bom_outputs`)  (legacy `m6_bom` + `m6_bom_in`/`out`)
- The **recipe**: which inputs produce which outputs. Versioned doc
  (`revisionCount`, `status`). Legacy `m6_itembom_in`/`itembom_out` (the standing
  per-item default BOM) = the same input/output line tables with the BOM bound to
  its primary output item — no separate tables. `docType = BOM`.

### ErpMfgWorkOrder → `mfg_work_orders` (+ inputs/outputs)  (legacy `m6_wo`)
- Production order against a BOM. Adds children:
  - `mfg_work_order_activities` (legacy `m6_wo_activity`): `workOrderId ➜`,
    `priceAdjId ○` (`idpa`), `activityName` (`namaaktivitas`),
    `machineCode ○` (`kodemesin`), dim FKs, `notes`, `lineNo`, `metadata`.
  - `mfg_work_order_route_cards` (legacy `m6_wo_route_card`): `workOrderId ➜`,
    `docNumber` (`notransaksi`), `quantity`/`unitId ➜`, dim FKs, `notes`,
    `lineNo`, `metadata`.
  Upstream: `bomId`, `productionReworkId`. `docType = WORK_ORDER`.

### ErpMfgMaterialIssue → `mfg_material_issues` (+ inputs/outputs)  (legacy `m6_mrs`)
- Material requisition slip — issue raw materials **to** the production floor.
  Upstream: `bomId`/`workOrderId`/`productionReworkId`. `docType = MATERIAL_ISSUE`.
  Posting moves `inv` (store → WIP).

### ErpMfgMaterialReturn → `mfg_material_returns` (+ inputs/outputs)  (legacy `m6_mrn`)
- Return unused materials **from** the floor. Upstream:
  `bomId`/`workOrderId`/`materialIssueId`/`productionReworkId`.
  `docType = MATERIAL_RETURN`. Posting moves `inv` (WIP → store).

### ErpMfgProductionEntry → `mfg_production_entries` (+ inputs/outputs)  (legacy `m6_pd`)
- Production execution: consume inputs (WIP/raw), yield outputs (FG/by-product).
  Adds child `mfg_production_boms` (legacy `m6_pd_bom`) — the **actual** BOM
  consumed per produced item: `productionEntryId ➜`,
  `producedItemId ➜ Item` (`idbaranghasil`), `itemId ➜ Item` (consumed),
  qty/unit, `unitCost`, `inventoryAccountId ➜`, `bomId ○ ➜`,
  `bomOutputLineId ○ ➜`, dims, `lineNo`, `metadata`. Upstream:
  `bomId`/`workOrderId`/`materialIssueId`/`materialReturnId`/
  `productionReworkId`. `docType = PRODUCTION`. Posting: COGM/COGS into `fin`,
  stock into `inv`.

### ErpMfgProductionRework → `mfg_production_reworks` (+ inputs/outputs)  (legacy `m6_pdr`)
- Rework / disassembly (reverse a production: break FG back to components, or
  re-process). Upstream: `bomId`. `docType = REWORK`.

---

## Enums (added to [README §4](README.md#4-enum-catalog))

| Enum | Values | Legacy source |
| --- | --- | --- |
| `MfgDocType` | `BOM`, `WORK_ORDER`, `MATERIAL_ISSUE`, `MATERIAL_RETURN`, `PRODUCTION`, `REWORK` | the m6 doc set |

Reused: `DocumentStatus`, `PostingStatus`.

### Flagged / not modeled
- `m6_files` / `m6_notes` — generic attachment/note (app concern), not modeled.
- All `*_history` shadow tables → `sys_audit_logs`.
- Legacy machine master is `m1_machine` (+`_kapasitas`/`m1_kategori_mesin`),
  still **deferred** (legacy-mapping §3) — `mfg_work_order_activities.machineCode`
  stays a string until a `mfg`/equipment master phase. **Flagged** (also where
  `m3_dc` equipment hour-meter log lands — see entities-m3-inventory.md).

---

**Count:** 6 Manufacturing (`mfg_*`) doc entities, each with input + output line
tables, + WO activities/route-cards + production-BOM ≈ **~17 tables**. Period
reuses `sys_fiscal_periods`; dimensions reuse `md_*`; posting flows to `inv`
(stock) + `fin` (COGM/COGS). Machine master deferred.

Legacy field-mapping appendix: **[legacy-mapping.md](legacy-mapping.md)** ·
Roadmap context: **[module-roadmap.md](module-roadmap.md)**.

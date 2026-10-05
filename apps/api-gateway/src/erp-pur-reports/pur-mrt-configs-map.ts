/**
 * Merged Purchasing (M4) .mrt dataset configs (Wave G5).
 *
 * Coverage of the 169 pur.* report keys:
 *  - 162 keys get real builders (documents, lists, outstanding,
 *    analytics, vendor payments — see the per-family files).
 *  - 5 keys are honest-empty (no ERP entity exists):
 *      pur.kontrakbeli, pur.pfdetail  — legacy m4_pf purchase
 *        contracts; the ERP has no purchase-contract entity (the
 *        school contract-pricing module is a different semantic).
 *      pur.piexchange, pur.piexchange2 — legacy PI exchange-rate
 *        adjustment documents; no ERP entity.
 *      pur.poambilsi — legacy PO↔SI premium linkage (m4_po_trans);
 *        not modeled in the ERP.
 *  - 2 keys are intentionally NOT claimed here (deferred waves):
 *      pur.labelpci2 → G8 (labels), pur.voucherpiutangpersalesman →
 *      G6 (sales-domain packaging-unit conversion).
 */

import { emptyConfig, type PurReportConfig } from './pur-mrt-configs';
import { PUR_PO_CONFIGS } from './pur-mrt-configs-docs-po';
import { PUR_PRE_DOC_CONFIGS } from './pur-mrt-configs-docs-pre';
import { PUR_GRN_CONFIGS } from './pur-mrt-configs-docs-grn';
import { PUR_RI_CONFIGS } from './pur-mrt-configs-docs-ri';
import { PUR_RET_CONFIGS } from './pur-mrt-configs-docs-ret';
import { PUR_LIST_CONFIGS } from './pur-mrt-configs-lists';
import { PUR_OUTSTANDING_CONFIGS } from './pur-mrt-configs-outstanding';
import { PUR_ANALYTICS_CONFIGS } from './pur-mrt-configs-analytics';
import { PUR_PAY_CONFIGS } from './pur-mrt-configs-pay';
import { PUR_PAY_LIST_CONFIGS } from './pur-mrt-configs-pay-lists';

const HONEST_EMPTY: Record<string, PurReportConfig> = {
  'pur.kontrakbeli': {
    datasets: { DS1: emptyConfig('m4_pf purchase contracts — no ERP entity') },
  },
  'pur.pfdetail': {
    datasets: { DS1: emptyConfig('m4_pf purchase contracts — no ERP entity') },
  },
  'pur.piexchange': {
    datasets: { DS1: emptyConfig('PI exchange adjustment — no ERP entity') },
  },
  'pur.piexchange2': {
    datasets: { DS1: emptyConfig('PI exchange adjustment — no ERP entity') },
  },
  'pur.poambilsi': {
    datasets: { DS1: emptyConfig('PO↔SI premium linkage (m4_po_trans) — not modeled') },
  },
};

export const PUR_MRT_CONFIGS: Record<string, PurReportConfig> = {
  ...PUR_PO_CONFIGS,
  ...PUR_PRE_DOC_CONFIGS,
  ...PUR_GRN_CONFIGS,
  ...PUR_RI_CONFIGS,
  ...PUR_RET_CONFIGS,
  ...PUR_LIST_CONFIGS,
  ...PUR_OUTSTANDING_CONFIGS,
  ...PUR_ANALYTICS_CONFIGS,
  ...PUR_PAY_CONFIGS,
  ...PUR_PAY_LIST_CONFIGS,
  ...HONEST_EMPTY,
};

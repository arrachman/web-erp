/**
 * Read-only "Data register" registry — merges per-module register configs and
 * derives route metadata. Consumed by `shell-route-renderer` (page lookup) and
 * `erp-route-meta` (breadcrumb/tab labels).
 *
 * Add a new register by editing the relevant module file (`inv`/`pur`/`sls`),
 * not here — this index only composes them.
 */

import type { IconName } from '@/components/ui/icons';
import type { AnyDocumentRegisterConfig } from './register-config';
import { INV_REGISTERS } from './inv-registers';
import { PUR_REGISTERS } from './pur-registers';
import { SLS_REGISTERS } from './sls-registers';

/** All Data registers keyed by canonical `sys_menus.path`. */
export const REGISTER_CONFIGS: Record<string, AnyDocumentRegisterConfig> = {
  ...INV_REGISTERS,
  ...PUR_REGISTERS,
  ...SLS_REGISTERS,
};

/** Route metadata derived from register configs (group/title/icon) for breadcrumbs. */
export const REGISTER_ROUTE_META: Record<string, { group: string; title: string; icon: IconName }> =
  Object.fromEntries(
    Object.entries(REGISTER_CONFIGS).map(([path, cfg]) => [
      path,
      { group: cfg.group, title: cfg.title, icon: cfg.icon },
    ]),
  );

export type { DocumentRegisterConfig, AnyDocumentRegisterConfig } from './register-config';

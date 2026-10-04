import { ErpOrderHubStatus } from '@prisma/client';

/**
 * A2 Order Hub — unified business stage of a sales order, derived from its
 * document chain (order → delivery order → delivery report/BAST → invoice →
 * settlement). Mirrors the derived-pipeline idea of A1 CRM Sekolah: the stage
 * is never stored on the order itself; `sls_order_status_logs` only records
 * the transitions this module observes (audit trail).
 *
 *   BARU        order still DRAFT
 *   DIKONFIRMASI order left DRAFT (submitted/approved/posted)
 *   DISIAPKAN   a delivery order exists (being prepared)
 *   DIKIRIM     a delivery order is posted (goods shipped)
 *   DITERIMA    a delivery report is posted (BAST / handover accepted)
 *   DITAGIH     a sales invoice exists for the order
 *   LUNAS       an invoice of the order is fully settled (PAID)
 */
export interface HubFacts {
  orderStatus: string;
  hasDeliveryOrder: boolean;
  hasPostedDeliveryOrder: boolean;
  hasPostedDeliveryReport: boolean;
  hasInvoice: boolean;
  hasPaidInvoice: boolean;
}

export function deriveHubStage(f: HubFacts): ErpOrderHubStatus {
  if (f.hasPaidInvoice) return ErpOrderHubStatus.LUNAS;
  if (f.hasInvoice) return ErpOrderHubStatus.DITAGIH;
  if (f.hasPostedDeliveryReport) return ErpOrderHubStatus.DITERIMA;
  if (f.hasPostedDeliveryOrder) return ErpOrderHubStatus.DIKIRIM;
  if (f.hasDeliveryOrder) return ErpOrderHubStatus.DISIAPKAN;
  if (f.orderStatus !== 'DRAFT') return ErpOrderHubStatus.DIKONFIRMASI;
  return ErpOrderHubStatus.BARU;
}

export const HUB_STAGE_ORDER: ErpOrderHubStatus[] = [
  ErpOrderHubStatus.BARU,
  ErpOrderHubStatus.DIKONFIRMASI,
  ErpOrderHubStatus.DISIAPKAN,
  ErpOrderHubStatus.DIKIRIM,
  ErpOrderHubStatus.DITERIMA,
  ErpOrderHubStatus.DITAGIH,
  ErpOrderHubStatus.LUNAS,
];

export const HUB_STAGE_LABELS: Record<ErpOrderHubStatus, string> = {
  BARU: 'Baru',
  DIKONFIRMASI: 'Dikonfirmasi',
  DISIAPKAN: 'Disiapkan',
  DIKIRIM: 'Dikirim',
  DITERIMA: 'Diterima (BAST)',
  DITAGIH: 'Ditagih',
  LUNAS: 'Lunas',
};

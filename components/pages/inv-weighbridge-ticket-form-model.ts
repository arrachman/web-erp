// Form-data model for the inventory weighbridge-ticket transaction form.
// Header-only (no item lines). netWeight = grossWeight - tareWeight, computed
// server-side; displayed as read-only in the form.

import { TODAY_DEFAULT, type ErpFormField } from '@/lib/api/form-fields';
import type {
  CreateInvWeighbridgeTicketPayload,
  ErpDocumentStatus,
  ErpInvWeighbridgeTicket,
} from '@/lib/api/inv-weighbridge-tickets';

export interface InvWeighbridgeTicketFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  ticketDate: string;
  fiscalPeriodId: string;
  branchId: string;
  branchLabel?: string;
  locationId: string;
  locationLabel?: string;
  vehiclePlate: string;
  driverName: string;
  partnerId: string;
  partnerLabel?: string;
  itemId: string;
  itemLabel?: string;
  grossWeight: string;
  tareWeight: string;
  unitPrice: string;
  description: string;
  notes: string;
  status: ErpDocumentStatus;
  postedAt?: string | null;
}

export function defaultInvWeighbridgeTicketForm(): InvWeighbridgeTicketFormData {
  return {
    docNumber: '',
    auto: true,
    ticketDate: '',
    fiscalPeriodId: '',
    branchId: '',
    locationId: '',
    vehiclePlate: '',
    driverName: '',
    partnerId: '',
    itemId: '',
    grossWeight: '0',
    tareWeight: '0',
    unitPrice: '',
    description: '',
    notes: '',
    status: 'DRAFT',
  };
}

/**
 * Fallback header layout until Form Builder config loads.
 * LEFT: partnerId, vehiclePlate, driverName, itemId.
 * CENTER: branchId (req), locationId, grossWeight, tareWeight.
 * RIGHT: ticketDate (req, today), docNumber, unitPrice.
 */
export const DEFAULT_INV_RW_FORM_FIELDS: ErpFormField[] = [
  { fieldKey: 'partnerId', kind: 'STRUCTURAL', label: 'Partner', fieldType: 'PARTNER', isRequired: false, isVisible: true, sortOrder: 0, columnSlot: 'LEFT' },
  { fieldKey: 'vehiclePlate', kind: 'STRUCTURAL', label: 'No Kendaraan', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'LEFT' },
  { fieldKey: 'driverName', kind: 'STRUCTURAL', label: 'Nama Pengemudi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'LEFT' },
  { fieldKey: 'itemId', kind: 'STRUCTURAL', label: 'Item', fieldType: 'LOOKUP', lookupSource: 'items', isRequired: false, isVisible: true, sortOrder: 3, columnSlot: 'LEFT' },
  { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'locationId', kind: 'STRUCTURAL', label: 'Lokasi', fieldType: 'LOCATION', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'CENTER' },
  { fieldKey: 'grossWeight', kind: 'STRUCTURAL', label: 'Berat Kotor', fieldType: 'NUMBER', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'CENTER' },
  { fieldKey: 'tareWeight', kind: 'STRUCTURAL', label: 'Berat Tara', fieldType: 'NUMBER', isRequired: false, isVisible: true, sortOrder: 3, columnSlot: 'CENTER' },
  { fieldKey: 'ticketDate', kind: 'STRUCTURAL', label: 'Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT', defaultValue: TODAY_DEFAULT },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
  { fieldKey: 'unitPrice', kind: 'STRUCTURAL', label: 'Harga Satuan', fieldType: 'NUMBER', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'RIGHT' },
];

export function fromInvWeighbridgeTicket(
  r: ErpInvWeighbridgeTicket,
): InvWeighbridgeTicketFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: false,
    ticketDate: r.ticketDate.slice(0, 10),
    fiscalPeriodId: r.fiscalPeriodId ?? '',
    branchId: r.branchId,
    branchLabel: r.branch?.name,
    locationId: r.locationId ?? '',
    locationLabel: r.location?.name,
    vehiclePlate: r.vehiclePlate ?? '',
    driverName: r.driverName ?? '',
    partnerId: r.partnerId ?? '',
    partnerLabel: r.partner?.name,
    itemId: r.itemId ?? '',
    itemLabel: r.item?.name,
    grossWeight: r.grossWeight,
    tareWeight: r.tareWeight,
    unitPrice: r.unitPrice ?? '',
    description: r.description ?? '',
    notes: r.notes ?? '',
    status: r.status,
    postedAt: r.postedAt,
  };
}

export function toInvWeighbridgeTicketPayload(
  d: InvWeighbridgeTicketFormData,
): CreateInvWeighbridgeTicketPayload {
  return {
    docNumber: d.auto ? undefined : d.docNumber || undefined,
    ticketDate: d.ticketDate,
    fiscalPeriodId: d.fiscalPeriodId || undefined,
    branchId: d.branchId,
    locationId: d.locationId || undefined,
    vehiclePlate: d.vehiclePlate || undefined,
    driverName: d.driverName || undefined,
    partnerId: d.partnerId || undefined,
    itemId: d.itemId || undefined,
    grossWeight: d.grossWeight,
    tareWeight: d.tareWeight,
    unitPrice: d.unitPrice || undefined,
    description: d.description || undefined,
    notes: d.notes || undefined,
  };
}

/** Compute netWeight as a display string from form data. */
export function computeNetWeight(d: InvWeighbridgeTicketFormData): string {
  const gross = parseFloat(d.grossWeight) || 0;
  const tare = parseFloat(d.tareWeight) || 0;
  return String(gross - tare);
}

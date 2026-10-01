'use client';

import * as React from 'react';
import { CREATION_STATUSES } from '@/lib/api/role-doc-policies';

const STATUS_SHORT: Record<string, string> = {
  DRAFT: 'Draft',
  NEED_APPROVE: 'Need Appr.',
  APPROVE_1: 'Appr. 1',
  APPROVE_2: 'Appr. 2',
  APPROVE_3: 'Appr. 3',
  APPROVE_4: 'Appr. 4',
  APPROVED: 'Approved',
  REJECTED: 'Rejected',
};

export const DOC_TYPE_LABELS: Record<string, string> = {
  // Administrator
  'ADM.USER': 'User', 'ADM.ROLE': 'Role & Permission',
  'ADM.BRANCH': 'Cabang', 'ADM.DEPT': 'Departemen',
  'ADM.APPROVAL': 'Aturan Approval',
  // Master Data
  'MD.PARTNER': 'Partner (Pelanggan / Pemasok)', 'MD.ITEM': 'Item / Produk',
  'MD.ACCOUNT': 'Akun Rekening', 'MD.WAREHOUSE': 'Gudang',
  'MD.CATEGORY': 'Kategori Item', 'MD.UNIT': 'Satuan',
  'MD.PRICE_LIST': 'Daftar Harga', 'MD.TAX': 'Pajak',
  // Finance
  CASH_RECEIPT: 'Kas Masuk', CASH_DISBURSEMENT: 'Kas Keluar',
  BANK_RECEIPT: 'Bank Masuk', BANK_DISBURSEMENT: 'Bank Keluar',
  JOURNAL_ENTRY: 'Jurnal Umum', ADJUSTING_JOURNAL: 'Jurnal Penyesuaian',
  JOURNAL_MEMO: 'Jurnal Memorial', BANK_BALANCE: 'Bank Balance',
  REVERSAL: 'Reversal', INCOMING_GIRO: 'Giro Masuk',
  OUTGOING_GIRO: 'Giro Keluar', GIRO_CLEARING_IN: 'Penagihan Giro Masuk',
  GIRO_CLEARING_OUT: 'Penagihan Giro Keluar',
  'PUR.PO': 'Purchase Order', 'PUR.PR': 'Purchase Request',
  'PUR.PI': 'Purchase Invoice', 'PUR.GRN': 'Goods Receipt Note',
  'PUR.DNR': 'Debit Note Return', 'PUR.PRT': 'Purchase Return',
  'PUR.RFQ': 'Request for Quotation', 'PUR.BS': 'Blanket Order',
  'SLS.SO': 'Sales Order', 'SLS.SI': 'Sales Invoice',
  'SLS.DO': 'Delivery Order', 'SLS.RO': 'Sales Return',
  'SLS.CR': 'Credit Note (Sales)', 'SLS.SQ': 'Sales Quotation',
  'SLS.PL': 'Packing List', 'SLS.RP': 'Receive Payment',
  'SLS.DR': 'Delivery Report', 'SLS.RNR': 'Return Receipt',
  'SLS.PI': 'Proforma Invoice',
  'INV.OS': 'Opening Stock', 'INV.SA': 'Stock Adjustment',
  'INV.SC': 'Stock Count', 'INV.SM': 'Stock Movement',
  'INV.PA': 'Price Adjustment', 'MFG.WO': 'Work Order',
};

export const DOC_GROUPS: Array<{ label: string; items: string[] }> = [
  { label: 'Administrator', items: ['ADM.USER', 'ADM.ROLE', 'ADM.BRANCH', 'ADM.DEPT', 'ADM.APPROVAL'] },
  { label: 'Master Data', items: ['MD.PARTNER', 'MD.ITEM', 'MD.ACCOUNT', 'MD.WAREHOUSE', 'MD.CATEGORY', 'MD.UNIT', 'MD.PRICE_LIST', 'MD.TAX'] },
  { label: 'Keuangan — Kas & Bank', items: ['CASH_RECEIPT', 'CASH_DISBURSEMENT', 'BANK_RECEIPT', 'BANK_DISBURSEMENT'] },
  { label: 'Keuangan — Jurnal', items: ['JOURNAL_ENTRY', 'ADJUSTING_JOURNAL', 'JOURNAL_MEMO', 'BANK_BALANCE', 'REVERSAL'] },
  { label: 'Keuangan — Giro', items: ['INCOMING_GIRO', 'OUTGOING_GIRO', 'GIRO_CLEARING_IN', 'GIRO_CLEARING_OUT'] },
  { label: 'Pembelian', items: ['PUR.PO', 'PUR.PR', 'PUR.PI', 'PUR.GRN', 'PUR.DNR', 'PUR.PRT', 'PUR.RFQ', 'PUR.BS'] },
  { label: 'Penjualan', items: ['SLS.SO', 'SLS.SI', 'SLS.DO', 'SLS.RO', 'SLS.CR', 'SLS.SQ', 'SLS.PL', 'SLS.RP', 'SLS.DR', 'SLS.RNR', 'SLS.PI'] },
  { label: 'Inventori', items: ['INV.OS', 'INV.SA', 'INV.SC', 'INV.SM', 'INV.PA'] },
  { label: 'Manufaktur', items: ['MFG.WO'] },
];

// ─── Indeterminate checkbox ───────────────────────────────────────────────────

function IndeterminateCheckbox({
  checked,
  indeterminate,
  onChange,
}: {
  checked: boolean;
  indeterminate: boolean;
  onChange: (checked: boolean) => void;
}) {
  const ref = React.useRef<HTMLInputElement>(null);
  React.useEffect(() => {
    if (ref.current) ref.current.indeterminate = indeterminate;
  }, [indeterminate]);
  return (
    <input
      ref={ref}
      type="checkbox"
      checked={checked}
      onChange={(e) => onChange(e.target.checked)}
      style={{ cursor: 'pointer', width: 14, height: 14 }}
    />
  );
}

// ─── Matrix ───────────────────────────────────────────────────────────────────

interface Props {
  matrix: Record<string, Set<string>>;
  onToggle: (docType: string, status: string) => void;
  onToggleGroup: (docTypes: string[], status: string, checked: boolean) => void;
}

const TH: React.CSSProperties = {
  padding: '6px 4px',
  textAlign: 'center',
  minWidth: 72,
  fontSize: 'calc(11px * var(--font-scale, 1))',
  fontWeight: 600,
  whiteSpace: 'nowrap',
};

const TD_CENTER: React.CSSProperties = { textAlign: 'center', padding: '4px 2px' };

export function RoleDocPoliciesMatrix({ matrix, onToggle, onToggleGroup }: Props) {
  const [expanded, setExpanded] = React.useState<Record<string, boolean>>({});
  const isOpen = (label: string) => expanded[label] !== false;

  return (
    <div style={{ overflowX: 'auto' }}>
      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 'calc(12px * var(--font-scale, 1))' }}>
        <thead>
          <tr style={{ background: 'var(--bg-muted)', borderBottom: '2px solid var(--border)' }}>
            <th style={{ textAlign: 'left', padding: '6px 12px', position: 'sticky', left: 0, background: 'var(--bg-muted)', zIndex: 2, minWidth: 240 }}>
              Nama
            </th>
            {CREATION_STATUSES.map((s) => (
              <th key={s} style={TH}>{STATUS_SHORT[s] ?? s}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {DOC_GROUPS.map((group) => {
            const open = isOpen(group.label);
            return (
              <React.Fragment key={group.label}>
                {/* Group header */}
                <tr style={{ background: 'hsl(var(--primary, 217 91% 60%) / 0.08)', borderTop: '1px solid var(--border)', borderBottom: '1px solid var(--border)' }}>
                  <td
                    style={{ padding: '5px 12px', fontWeight: 700, color: 'var(--primary, #2563eb)', position: 'sticky', left: 0, background: 'hsl(var(--primary, 217 91% 60%) / 0.08)', cursor: 'pointer', userSelect: 'none' }}
                    onClick={() => setExpanded((p) => ({ ...p, [group.label]: !open }))}
                  >
                    <span style={{ display: 'inline-block', width: 16, fontFamily: 'monospace', fontWeight: 700 }}>
                      {open ? '−' : '+'}
                    </span>
                    {group.label}
                  </td>
                  {CREATION_STATUSES.map((s) => {
                    const all = group.items.every((dt) => matrix[dt]?.has(s));
                    const some = group.items.some((dt) => matrix[dt]?.has(s));
                    return (
                      <td key={s} style={TD_CENTER}>
                        <IndeterminateCheckbox
                          checked={all}
                          indeterminate={!all && some}
                          onChange={(chk) => onToggleGroup(group.items, s, chk)}
                        />
                      </td>
                    );
                  })}
                </tr>
                {/* Doc type rows */}
                {open && group.items.map((dt, idx) => (
                  <tr
                    key={dt}
                    style={{ borderBottom: '1px solid var(--border)', background: idx % 2 === 1 ? 'var(--bg-muted, rgba(0,0,0,.02))' : 'transparent' }}
                  >
                    <td style={{ padding: '4px 12px 4px 28px', position: 'sticky', left: 0, background: idx % 2 === 1 ? 'var(--card, #fff)' : 'var(--card, #fff)' }}>
                      {DOC_TYPE_LABELS[dt] ?? dt}
                      <span style={{ marginLeft: 6, color: 'var(--fg-muted)', fontSize: '0.85em', opacity: 0.7 }}>({dt})</span>
                    </td>
                    {CREATION_STATUSES.map((s) => (
                      <td key={s} style={TD_CENTER}>
                        <input
                          type="checkbox"
                          checked={matrix[dt]?.has(s) ?? false}
                          onChange={() => onToggle(dt, s)}
                          style={{ cursor: 'pointer', width: 14, height: 14 }}
                        />
                      </td>
                    ))}
                  </tr>
                ))}
              </React.Fragment>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}

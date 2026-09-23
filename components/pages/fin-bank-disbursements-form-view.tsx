import * as React from 'react';

/**
 * Bank Keluar (Bank Disbursement / SM) — form view branch (presentational).
 * State/logic stays in the page; receives slots + callbacks via props.
 */
export interface BankDisbursementFormViewProps {
  title: string;
  code: string;
  formReady: boolean;
  onBack: () => void;
  children: React.ReactNode;
}

export function BankDisbursementFormView({
  title,
  code,
  formReady,
  onBack,
  children,
}: BankDisbursementFormViewProps) {
  return (
    <div className="page">
      <div className="page-header">
        <h1 className="page-title flex items-center gap-2">
          <button
            className="iconbtn"
            onClick={onBack}
            title="Kembali"
            style={{ fontSize: 18, lineHeight: 1 }}
          >
            ←
          </button>
          {title}
          <span className="code-tag">{code}</span>
        </h1>
      </div>
      <div className="page-body overflow-auto p-4">
        {formReady ? (
          children
        ) : (
          <div className="p-8 text-center text-muted">Memuat…</div>
        )}
      </div>
    </div>
  );
}
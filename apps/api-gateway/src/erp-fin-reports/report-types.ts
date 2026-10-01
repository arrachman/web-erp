export type ReportFormat = 'json' | 'xlsx' | 'pdf' | 'docx';

export type CellAlign = 'left' | 'right' | 'center';

export interface ReportColumn {
  key: string;
  label: string;
  align?: CellAlign;
  type?: 'text' | 'number' | 'date';
  width?: number;
}

export interface ReportRow {
  cells: Record<string, string | number | null>;
  bold?: boolean;
  indent?: number;
}

export interface ReportSection {
  heading?: string;
  rows: ReportRow[];
  subtotal?: ReportRow;
}

export interface ReportDocument {
  key: string;
  title: string;
  subtitle?: string;
  meta: { label: string; value: string }[];
  columns: ReportColumn[];
  sections: ReportSection[];
  grandTotal?: ReportRow;
}

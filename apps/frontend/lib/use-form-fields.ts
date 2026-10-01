import * as React from 'react';
import { getFormFields, type ErpFormField, type FormColumnSlot } from './api/form-fields';

export interface FormFieldsConfig {
  /** Every field (structural + custom) keyed by fieldKey. */
  byKey: Record<string, ErpFormField>;
  /** All fields grouped per column slot, ordered by sortOrder. Drives rendering. */
  slotFields: Record<FormColumnSlot, ErpFormField[]>;
}

const EMPTY: FormFieldsConfig = {
  byKey: {},
  slotFields: { LEFT: [], CENTER: [], RIGHT: [] },
};

/** Build a render config from a flat field list (sorted by sortOrder per slot). */
export function buildFormConfig(fields: ErpFormField[]): FormFieldsConfig {
  const byKey: Record<string, ErpFormField> = {};
  const slotFields: Record<FormColumnSlot, ErpFormField[]> = { LEFT: [], CENTER: [], RIGHT: [] };

  const ordered = fields.slice().sort((a, b) => a.sortOrder - b.sortOrder);
  for (const f of ordered) {
    byKey[f.fieldKey] = f;
    slotFields[f.columnSlot].push(f);
  }
  return { byKey, slotFields };
}

/** Loads form field config for a transaction type. Returns empty config while loading. */
export function useFormFields(transactionCode?: string): FormFieldsConfig {
  const [config, setConfig] = React.useState<FormFieldsConfig>(EMPTY);

  React.useEffect(() => {
    if (!transactionCode) return;
    let cancelled = false;
    getFormFields(transactionCode)
      .then((r) => { if (!cancelled) setConfig(buildFormConfig(r.fields)); })
      .catch(() => {});
    return () => { cancelled = true; };
  }, [transactionCode]);

  return config;
}

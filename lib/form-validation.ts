/** Per-field error map. Keys match form data keys. */
export type FormErrors<F> = Partial<Record<keyof F, string>>;

export interface FieldRule<F, K extends keyof F = keyof F> {
  field: K;
  label: string;
  required?: boolean;
  validate?: (value: F[K], form: F) => string | undefined;
}

/**
 * Validate a form object against a list of rules.
 * Returns an error map; empty object means valid.
 */
export function validateForm<F>(
  form: F,
  rules: FieldRule<F>[],
): FormErrors<F> {
  const errors: FormErrors<F> = {};
  for (const rule of rules) {
    const value = form[rule.field];
    if (rule.required && (value === '' || value === null || value === undefined)) {
      (errors as Record<keyof F, string>)[rule.field] = `${rule.label} wajib diisi`;
      continue;
    }
    if (rule.validate) {
      const msg = rule.validate(value, form);
      if (msg) (errors as Record<keyof F, string>)[rule.field] = msg;
    }
  }
  return errors;
}

export function hasErrors<F>(errors: FormErrors<F>): boolean {
  return Object.keys(errors).length > 0;
}

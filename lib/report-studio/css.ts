import type * as React from 'react';

/**
 * Parse an inline CSS string ("a:b;c:d") into a React style object.
 * Preserves CSS custom properties (`--x`) verbatim; camelCases the rest.
 * Splits each declaration on the FIRST colon only so values containing
 * colons/commas (gradients, url(), etc.) survive intact.
 */
export function s(css?: string): React.CSSProperties {
  const out: Record<string, string> = {};
  if (!css) return out as React.CSSProperties;
  for (const decl of css.split(';')) {
    const i = decl.indexOf(':');
    if (i < 0) continue;
    const rawKey = decl.slice(0, i).trim();
    const val = decl.slice(i + 1).trim();
    if (!rawKey || !val) continue;
    const key = rawKey.startsWith('--')
      ? rawKey
      : rawKey.replace(/-([a-z])/g, (_m, c: string) => c.toUpperCase());
    out[key] = val;
  }
  return out as React.CSSProperties;
}

/** Merge several inline CSS strings into one style object (later wins). */
export function sm(...css: Array<string | undefined>): React.CSSProperties {
  return s(css.filter(Boolean).join(';'));
}

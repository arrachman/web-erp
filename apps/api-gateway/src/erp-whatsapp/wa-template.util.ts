/**
 * Renderer template Mustache sederhana: ganti `{{variabel}}` dengan nilai dari
 * map. Variabel yang tidak ada dibiarkan apa adanya (memudahkan debug).
 */
export function renderTemplate(
  body: string,
  variables: Record<string, string | number | undefined | null> = {},
): string {
  return body.replace(/\{\{(\w+)\}\}/g, (match, key: string) => {
    const value = variables[key];
    if (value === undefined || value === null) return match;
    return String(value);
  });
}

/** Daftar nama variabel yang dipakai sebuah template. */
export function extractVariables(body: string): string[] {
  const matches = body.matchAll(/\{\{(\w+)\}\}/g);
  return Array.from(new Set([...matches].map((m) => m[1])));
}

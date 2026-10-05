/**
 * Minimal tolerant XML parser for legacy Stimulsoft .mrt files (Wave G0
 * importer). The corpus uses a simple serialization dialect (elements,
 * attributes, text, CDATA) and 10 files are not well-formed (unescaped
 * `<`/`&` inside SqlCommand), so the importer sanitizes first:
 *  - SqlCommand bodies are entity-escaped wholesale (they are data specs,
 *    never markup).
 *  - Bare `&` not starting an entity is escaped.
 *  - If parsing still fails, a regex fallback extracts the pieces the
 *    mapper needs (page, bands, texts, dictionary) approximately.
 */

export interface XmlEl {
  tag: string;
  attrs: Record<string, string>;
  children: XmlEl[];
  text: string;
}

const ENTITIES: Record<string, string> = {
  amp: '&',
  lt: '<',
  gt: '>',
  quot: '"',
  apos: "'",
};

export function decodeEntities(s: string): string {
  return s.replace(/&(#x?[0-9a-fA-F]+|[a-zA-Z]+);/g, (m, body: string) => {
    if (body.startsWith('#x') || body.startsWith('#X')) {
      return String.fromCharCode(parseInt(body.slice(2), 16));
    }
    if (body.startsWith('#')) return String.fromCharCode(parseInt(body.slice(1), 10));
    return ENTITIES[body] ?? m;
  });
}

/** Decode Stimulsoft's _xHHHH_ escaping used inside serialized list values. */
export function decodeStiEscapes(s: string): string {
  return s.replace(/_x([0-9a-fA-F]{4})_/g, (_, hex: string) => String.fromCharCode(parseInt(hex, 16)));
}

export function sanitizeMrtXml(raw: string): string {
  let out = raw;
  // Escape SqlCommand bodies wholesale (protect existing entities first).
  out = out.replace(/<SqlCommand>([\s\S]*?)<\/SqlCommand>/gi, (_, body: string) => {
    const protectedBody = body
      .replace(/&(amp|lt|gt|quot|apos);/g, '\u0001$1;')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/\u0001(amp|lt|gt|quot|apos);/g, '&$1;');
    return `<SqlCommand>${protectedBody}</SqlCommand>`;
  });
  // Escape bare ampersands elsewhere.
  out = out.replace(/&(?!(amp|lt|gt|quot|apos|#\d+|#x[0-9a-fA-F]+);)/g, '&amp;');
  // Repair nameless component elements (3 corpus files, e.g.
  // receivegirocanceldetail1.mrt): a lost element name leaves
  // `< Ref="11" type="Text" isKey="true">` closed by `</>`. Both shapes
  // are invalid XML anywhere else, so renaming them is safe. Runs after
  // the SqlCommand escaping above, so SQL text can never match.
  out = out.replace(/< Ref=/g, '<Recovered Ref=');
  out = out.replace(/<\/>/g, '</Recovered>');
  return out;
}

export function parseXml(raw: string): XmlEl {
  const input = raw.charCodeAt(0) === 0xfeff ? raw.slice(1) : raw;
  let i = 0;
  const n = input.length;

  const fail = (msg: string): never => {
    throw new Error(`XML parse error @${i}: ${msg}`);
  };

  function skipMisc(): void {
    for (;;) {
      while (i < n && /\s/.test(input[i])) i++;
      if (input.startsWith('<?', i)) {
        const end = input.indexOf('?>', i);
        i = end === -1 ? n : end + 2;
        continue;
      }
      if (input.startsWith('<!--', i)) {
        const end = input.indexOf('-->', i);
        i = end === -1 ? n : end + 3;
        continue;
      }
      if (input.startsWith('<!DOCTYPE', i) || input.startsWith('<!doctype', i)) {
        const end = input.indexOf('>', i);
        i = end === -1 ? n : end + 1;
        continue;
      }
      break;
    }
  }

  function parseElement(): XmlEl {
    // input[i] === '<'
    i++;
    let tag = '';
    while (i < n && !/[\s/>]/.test(input[i])) tag += input[i++];
    if (!tag) fail('empty tag');
    const attrs: Record<string, string> = {};
    for (;;) {
      while (i < n && /\s/.test(input[i])) i++;
      if (input.startsWith('/>', i)) {
        i += 2;
        return { tag, attrs, children: [], text: '' };
      }
      if (input[i] === '>') {
        i++;
        break;
      }
      let name = '';
      while (i < n && !/[\s=/>]/.test(input[i])) name += input[i++];
      while (i < n && /\s/.test(input[i])) i++;
      let value = '';
      if (input[i] === '=') {
        i++;
        while (i < n && /\s/.test(input[i])) i++;
        const quote = input[i];
        if (quote === '"' || quote === "'") {
          i++;
          const end = input.indexOf(quote, i);
          value = input.slice(i, end === -1 ? n : end);
          i = end === -1 ? n : end + 1;
        } else {
          while (i < n && !/[\s>]/.test(input[i])) value += input[i++];
        }
      }
      if (name) attrs[name] = decodeEntities(value);
    }
    const children: XmlEl[] = [];
    let text = '';
    for (;;) {
      if (i >= n) fail(`unclosed <${tag}>`);
      if (input.startsWith('</', i)) {
        const end = input.indexOf('>', i);
        i = end === -1 ? n : end + 1;
        break;
      }
      if (input.startsWith('<![CDATA[', i)) {
        const end = input.indexOf(']]>', i);
        text += input.slice(i + 9, end === -1 ? n : end);
        i = end === -1 ? n : end + 3;
        continue;
      }
      if (input.startsWith('<!--', i) || input.startsWith('<?', i)) {
        skipMisc();
        continue;
      }
      if (input[i] === '<') {
        children.push(parseElement());
        continue;
      }
      const next = input.indexOf('<', i);
      text += decodeEntities(input.slice(i, next === -1 ? n : next));
      i = next === -1 ? n : next;
    }
    return { tag, attrs, children, text };
  }

  skipMisc();
  if (input[i] !== '<') fail('root element expected');
  return parseElement();
}

/* ---------------- tree helpers ---------------- */

export function child(el: XmlEl, tag: string): XmlEl | undefined {
  return el.children.find((c) => c.tag === tag);
}

export function childText(el: XmlEl, tag: string): string | undefined {
  const c = child(el, tag);
  return c ? c.text.trim() : undefined;
}

export function propMap(el: XmlEl): Record<string, string> {
  const out: Record<string, string> = {};
  for (const c of el.children) {
    if (c.children.length === 0) out[c.tag] = c.text.trim();
  }
  return out;
}

export function findAll(el: XmlEl, pred: (e: XmlEl) => boolean, acc: XmlEl[] = []): XmlEl[] {
  if (pred(el)) acc.push(el);
  for (const c of el.children) findAll(c, pred, acc);
  return acc;
}

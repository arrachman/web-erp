/**
 * Tokenizer + recursive-descent parser for the Stimulsoft expression
 * dialect (the text inside { … } in .mrt templates). Produces an AST —
 * no eval/new Function anywhere. Supports: dotted refs (DS1.field,
 * formatNominal.fromat, c.companyName), function calls incl. dotted
 * names (double.Parse, System.Math.Round), arithmetic, comparison,
 * logic (&&/||/AND/OR/NOT), ternary ?:, string index r.paramLines[0],
 * string literals ('…' or "…"), numbers, true/false/null.
 */

export type AstNode =
  | { t: 'lit'; v: unknown }
  | { t: 'ref'; path: string[] }
  | { t: 'call'; name: string; args: AstNode[] }
  | { t: 'un'; op: string; e: AstNode }
  | { t: 'bin'; op: string; l: AstNode; r: AstNode }
  | { t: 'cond'; c: AstNode; a: AstNode; b: AstNode }
  | { t: 'idx'; obj: AstNode; idx: AstNode };

export class StiParseError extends Error {}

type Tok =
  | { k: 'num'; v: number }
  | { k: 'str'; v: string }
  | { k: 'name'; v: string }
  | { k: 'op'; v: string }
  | { k: 'eof' };

const TWO_CHAR_OPS = ['==', '!=', '<>', '<=', '>=', '&&', '||'];
const ONE_CHAR_OPS = new Set(['(', ')', '[', ']', ',', '?', ':', '+', '-', '*', '/', '%', '<', '>', '!', '=', '&', '|', '.']);

export function tokenize(input: string): Tok[] {
  const toks: Tok[] = [];
  let i = 0;
  const n = input.length;
  while (i < n) {
    const ch = input[i];
    if (ch === ' ' || ch === '\t' || ch === '\r' || ch === '\n') {
      i++;
      continue;
    }
    if (ch === '"' || ch === "'") {
      const quote = ch;
      let j = i + 1;
      let out = '';
      while (j < n) {
        const cj = input[j];
        if (cj === quote) {
          if (input[j + 1] === quote) {
            out += quote;
            j += 2;
            continue;
          }
          break;
        }
        if (cj === '\\' && quote === '"' && j + 1 < n) {
          const esc = input[j + 1];
          out += esc === 'n' ? '\n' : esc === 't' ? '\t' : esc;
          j += 2;
          continue;
        }
        out += cj;
        j++;
      }
      toks.push({ k: 'str', v: out });
      i = j + 1;
      continue;
    }
    if (/[0-9]/.test(ch)) {
      let j = i;
      let seenDot = false;
      while (j < n && (/[0-9]/.test(input[j]) || (input[j] === '.' && !seenDot))) {
        if (input[j] === '.') seenDot = true;
        j++;
      }
      toks.push({ k: 'num', v: Number(input.slice(i, j)) });
      i = j;
      continue;
    }
    if (/[A-Za-z_$]/.test(ch)) {
      // Greedy dotted name: DS1.bkode / System.Math.Round / c.companyName
      let j = i;
      while (j < n && /[A-Za-z0-9_$]/.test(input[j])) j++;
      while (input[j] === '.' && /[A-Za-z_$]/.test(input[j + 1] ?? '')) {
        j++;
        while (j < n && /[A-Za-z0-9_$]/.test(input[j])) j++;
      }
      toks.push({ k: 'name', v: input.slice(i, j) });
      i = j;
      continue;
    }
    const two = input.slice(i, i + 2);
    if (TWO_CHAR_OPS.includes(two)) {
      toks.push({ k: 'op', v: two });
      i += 2;
      continue;
    }
    if (ONE_CHAR_OPS.has(ch)) {
      toks.push({ k: 'op', v: ch });
      i++;
      continue;
    }
    throw new StiParseError(`Karakter tak dikenal '${ch}' pada posisi ${i}`);
  }
  toks.push({ k: 'eof' });
  return toks;
}

class Parser {
  private pos = 0;
  constructor(private readonly toks: Tok[]) {}

  private peek(): Tok {
    return this.toks[this.pos];
  }
  private next(): Tok {
    return this.toks[this.pos++];
  }
  private isOp(v: string): boolean {
    const t = this.peek();
    return t.k === 'op' && t.v === v;
  }
  private isKw(word: string): boolean {
    const t = this.peek();
    return t.k === 'name' && t.v.toLowerCase() === word;
  }
  private expectOp(v: string): void {
    if (!this.isOp(v)) throw new StiParseError(`Mengharapkan '${v}'`);
    this.pos++;
  }

  parse(): AstNode {
    const node = this.parseTernary();
    if (this.peek().k !== 'eof') throw new StiParseError('Sisa token tak terduga');
    return node;
  }

  private parseTernary(): AstNode {
    const c = this.parseOr();
    if (this.isOp('?')) {
      this.next();
      const a = this.parseTernary();
      this.expectOp(':');
      const b = this.parseTernary();
      return { t: 'cond', c, a, b };
    }
    return c;
  }

  private parseOr(): AstNode {
    let l = this.parseAnd();
    while (this.isOp('||') || this.isOp('|') || this.isKw('or')) {
      this.next();
      l = { t: 'bin', op: '||', l, r: this.parseAnd() };
    }
    return l;
  }

  private parseAnd(): AstNode {
    let l = this.parseEquality();
    while (this.isOp('&&') || this.isOp('&') || this.isKw('and')) {
      this.next();
      l = { t: 'bin', op: '&&', l, r: this.parseEquality() };
    }
    return l;
  }

  private parseEquality(): AstNode {
    let l = this.parseRelational();
    for (;;) {
      const t = this.peek();
      if (t.k === 'op' && (t.v === '==' || t.v === '!=' || t.v === '<>' || t.v === '=')) {
        this.next();
        l = { t: 'bin', op: t.v === '<>' ? '!=' : t.v, l, r: this.parseRelational() };
      } else break;
    }
    return l;
  }

  private parseRelational(): AstNode {
    let l = this.parseAdditive();
    for (;;) {
      const t = this.peek();
      if (t.k === 'op' && (t.v === '<' || t.v === '<=' || t.v === '>' || t.v === '>=')) {
        this.next();
        l = { t: 'bin', op: t.v, l, r: this.parseAdditive() };
      } else break;
    }
    return l;
  }

  private parseAdditive(): AstNode {
    let l = this.parseMultiplicative();
    for (;;) {
      const t = this.peek();
      if (t.k === 'op' && (t.v === '+' || t.v === '-')) {
        this.next();
        l = { t: 'bin', op: t.v, l, r: this.parseMultiplicative() };
      } else break;
    }
    return l;
  }

  private parseMultiplicative(): AstNode {
    let l = this.parseUnary();
    for (;;) {
      const t = this.peek();
      if (t.k === 'op' && (t.v === '*' || t.v === '/' || t.v === '%')) {
        this.next();
        l = { t: 'bin', op: t.v, l, r: this.parseUnary() };
      } else if (this.isKw('mod')) {
        this.next();
        l = { t: 'bin', op: '%', l, r: this.parseUnary() };
      } else break;
    }
    return l;
  }

  private parseUnary(): AstNode {
    if (this.isOp('!') || this.isKw('not')) {
      this.next();
      return { t: 'un', op: '!', e: this.parseUnary() };
    }
    if (this.isOp('-')) {
      this.next();
      return { t: 'un', op: '-', e: this.parseUnary() };
    }
    if (this.isOp('+')) {
      this.next();
      return this.parseUnary();
    }
    return this.parsePostfix();
  }

  private parsePostfix(): AstNode {
    let node = this.parsePrimary();
    for (;;) {
      if (this.isOp('[')) {
        this.next();
        const idx = this.parseTernary();
        this.expectOp(']');
        node = { t: 'idx', obj: node, idx };
        continue;
      }
      if (this.isOp('.')) {
        // Member access after ')' or ']' (dotted names are tokenized whole).
        this.next();
        const t = this.next();
        if (t.k !== 'name') throw new StiParseError('Nama member diharapkan setelah .');
        if (node.t === 'ref') node = { t: 'ref', path: [...node.path, ...t.v.split('.')] };
        else if (this.isOp('(')) {
          const args = this.parseArgs();
          node = { t: 'call', name: '@method:' + t.v, args: [node, ...args] };
        } else {
          node = { t: 'idx', obj: node, idx: { t: 'lit', v: t.v } };
        }
        continue;
      }
      break;
    }
    return node;
  }

  private parseArgs(): AstNode[] {
    this.expectOp('(');
    const args: AstNode[] = [];
    if (!this.isOp(')')) {
      args.push(this.parseTernary());
      while (this.isOp(',')) {
        this.next();
        args.push(this.parseTernary());
      }
    }
    this.expectOp(')');
    return args;
  }

  private parsePrimary(): AstNode {
    const t = this.next();
    if (t.k === 'num') return { t: 'lit', v: t.v };
    if (t.k === 'str') return { t: 'lit', v: t.v };
    if (t.k === 'op' && t.v === '(') {
      const inner = this.parseTernary();
      this.expectOp(')');
      return inner;
    }
    if (t.k === 'name') {
      const lower = t.v.toLowerCase();
      if (lower === 'true') return { t: 'lit', v: true };
      if (lower === 'false') return { t: 'lit', v: false };
      if (lower === 'null' || lower === 'nothing') return { t: 'lit', v: null };
      if (this.isOp('(')) {
        const args = this.parseArgs();
        return { t: 'call', name: t.v, args };
      }
      return { t: 'ref', path: t.v.split('.') };
    }
    throw new StiParseError('Token tak terduga');
  }
}

const astCache = new Map<string, AstNode>();

export function parseStiExpression(source: string): AstNode {
  const cached = astCache.get(source);
  if (cached) return cached;
  const ast = new Parser(tokenize(source)).parse();
  if (astCache.size > 5000) astCache.clear();
  astCache.set(source, ast);
  return ast;
}

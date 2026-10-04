/** Indonesian number-to-words (terbilang) for document totals. */
const UNITS = [
  '',
  'Satu',
  'Dua',
  'Tiga',
  'Empat',
  'Lima',
  'Enam',
  'Tujuh',
  'Delapan',
  'Sembilan',
  'Sepuluh',
  'Sebelas',
];

function spellBelowThousand(n: number): string {
  let out = '';
  const hundreds = Math.floor(n / 100);
  const rest = n % 100;
  if (hundreds > 0) out += hundreds === 1 ? 'Seratus' : `${UNITS[hundreds]} Ratus`;
  if (rest > 0) {
    if (out) out += ' ';
    if (rest < 12) out += UNITS[rest];
    else if (rest < 20) out += `${UNITS[rest - 10]} Belas`;
    else {
      const tens = Math.floor(rest / 10);
      const ones = rest % 10;
      out += `${UNITS[tens]} Puluh${ones ? ` ${UNITS[ones]}` : ''}`;
    }
  }
  return out;
}

export function terbilang(value: number | string): string {
  let n = Math.floor(Math.abs(Number(value) || 0));
  if (n === 0) return 'Nol';
  const scales: Array<[number, string]> = [
    [1_000_000_000_000, 'Triliun'],
    [1_000_000_000, 'Miliar'],
    [1_000_000, 'Juta'],
    [1_000, 'Ribu'],
  ];
  const parts: string[] = [];
  for (const [scale, label] of scales) {
    const count = Math.floor(n / scale);
    if (count > 0) {
      const words = scale === 1000 && count === 1 ? 'Seribu' : `${spellBelowThousand(count)} ${label}`;
      parts.push(words);
      n -= count * scale;
    }
  }
  if (n > 0) parts.push(spellBelowThousand(n));
  return parts.join(' ');
}

export function terbilangRupiah(value: number | string): string {
  return `${terbilang(value)} Rupiah`;
}

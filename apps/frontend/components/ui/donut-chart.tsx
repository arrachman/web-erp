import * as React from 'react';

/**
 * Ring/donut chart. Ported 1:1 from prototype `Donut` in ui.jsx —
 * stroke-dasharray arcs over a `--panel-2` track. Each slice carries
 * its own `color` (pass tokens like `var(--success)`).
 */
export interface DonutSlice {
  v: number;
  color: string;
  label?: string;
}

export interface DonutChartProps
  extends Omit<React.SVGProps<SVGSVGElement>, 'width' | 'height' | 'stroke'> {
  slices: DonutSlice[];
  size?: number;
  stroke?: number;
}

export function DonutChart({
  slices,
  size = 100,
  stroke = 14,
  ...props
}: DonutChartProps) {
  const r = (size - stroke) / 2;
  const c = 2 * Math.PI * r;
  const total = slices.reduce((s, x) => s + x.v, 0) || 1;
  const lens = slices.map((s) => (s.v / total) * c);
  const offsets = lens.map(
    (_, i) => c - lens.slice(0, i).reduce((a, b) => a + b, 0),
  );

  return (
    <svg
      width={size}
      height={size}
      viewBox={`0 0 ${size} ${size}`}
      {...props}
    >
      <circle
        cx={size / 2}
        cy={size / 2}
        r={r}
        fill="none"
        stroke="var(--panel-2)"
        strokeWidth={stroke}
      />
      {slices.map((s, i) => (
        <circle
          // eslint-disable-next-line react/no-array-index-key
          key={i}
          cx={size / 2}
          cy={size / 2}
          r={r}
          fill="none"
          stroke={s.color}
          strokeWidth={stroke}
          strokeDasharray={`${lens[i]} ${c}`}
          strokeDashoffset={offsets[i]}
          transform={`rotate(-90 ${size / 2} ${size / 2})`}
        />
      ))}
    </svg>
  );
}

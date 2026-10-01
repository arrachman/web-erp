import * as React from 'react';
import { cn } from '@/lib/utils';

/**
 * Inline area sparkline. Ported 1:1 from prototype `Spark` in ui.jsx
 * (120-wide viewBox, soft area fill + 1.5px stroke). `color` accepts
 * any CSS color — pass a token like `var(--primary)` to stay on-theme.
 */
export interface SparklineProps
  extends Omit<React.SVGProps<SVGSVGElement>, 'height' | 'fill'> {
  data: number[];
  color?: string;
  height?: number;
  fill?: boolean;
}

export function Sparkline({
  data,
  color = 'var(--primary)',
  height = 28,
  fill = true,
  className,
  style,
  ...props
}: SparklineProps) {
  const w = 120;
  const h = height;
  const min = Math.min(...data);
  const max = Math.max(...data);
  const range = max - min || 1;
  const step = w / (data.length - 1 || 1);
  const points = data.map((v, i) => [
    i * step,
    h - 4 - ((v - min) / range) * (h - 8),
  ]);
  const d = points
    .map((p, i) => (i === 0 ? 'M' : 'L') + p[0].toFixed(1) + ' ' + p[1].toFixed(1))
    .join(' ');
  const fillD = `${d} L ${w} ${h} L 0 ${h} Z`;

  return (
    <svg
      viewBox={`0 0 ${w} ${h}`}
      preserveAspectRatio="none"
      className={cn('w-full', className)}
      style={{ height, ...style }}
      {...props}
    >
      {fill && <path d={fillD} fill={color} opacity="0.12" />}
      <path
        d={d}
        fill="none"
        stroke={color}
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

import * as React from 'react';
import { cn } from '@/lib/utils';

/**
 * Compact diverging mini bar chart. Ported 1:1 from prototype `Bars`
 * in ui.jsx (zero baseline, positive vs negative tint via `negColor`).
 */
export interface BarChartProps
  extends Omit<React.SVGProps<SVGSVGElement>, 'height'> {
  data: number[];
  color?: string;
  negColor?: string;
  height?: number;
}

export function BarChart({
  data,
  color = 'var(--primary)',
  negColor,
  height = 64,
  className,
  style,
  ...props
}: BarChartProps) {
  const w = 400;
  const h = height;
  const max = Math.max(...data.map(Math.abs)) || 1;
  const bw = (w - data.length * 3) / data.length;

  return (
    <svg
      viewBox={`0 0 ${w} ${h}`}
      preserveAspectRatio="none"
      className={cn('w-full', className)}
      style={{ height, ...style }}
      {...props}
    >
      <line
        x1="0"
        y1={h / 2}
        x2={w}
        y2={h / 2}
        stroke="currentColor"
        strokeWidth="0.5"
        opacity="0.18"
      />
      {data.map((v, i) => {
        const x = i * (bw + 3);
        const barH = (Math.abs(v) / max) * (h / 2 - 4);
        const y = v >= 0 ? h / 2 - barH : h / 2;
        return (
          <rect
            // eslint-disable-next-line react/no-array-index-key
            key={i}
            x={x}
            y={y}
            width={bw}
            height={barH}
            rx="1.5"
            fill={v >= 0 ? color : (negColor ?? color)}
            opacity={v >= 0 ? 1 : 0.65}
          />
        );
      })}
    </svg>
  );
}

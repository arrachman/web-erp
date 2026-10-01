'use client';

import * as React from 'react';
import { s } from '@/lib/report-studio/css';

interface HovProps {
  base: string;
  hover?: string;
  focus?: string;
  as?: React.ElementType;
  children?: React.ReactNode;
  [key: string]: unknown;
}

/**
 * Renders an element whose inline style swaps to `hover`/`focus` variants on
 * pointer/focus — the React equivalent of the source's `style-hover`/`style-focus`.
 */
export function Hov({ base, hover, focus, as, children, ...rest }: HovProps) {
  const [h, setH] = React.useState(false);
  const [f, setF] = React.useState(false);
  const Tag = (as || 'div') as React.ElementType;
  const style = s([base, h && hover ? hover : '', f && focus ? focus : ''].filter(Boolean).join(';'));
  return (
    <Tag
      {...rest}
      style={style}
      onMouseEnter={() => setH(true)}
      onMouseLeave={() => setH(false)}
      onFocus={focus ? () => setF(true) : undefined}
      onBlur={focus ? () => setF(false) : undefined}
    >
      {children}
    </Tag>
  );
}

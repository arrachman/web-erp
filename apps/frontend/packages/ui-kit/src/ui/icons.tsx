import * as React from 'react';
import { cn } from '../utils';

/**
 * Outline icon set ported 1:1 from prototype `icons.jsx` (16-unit
 * viewBox, currentColor stroke, round caps/joins). `name` is typed —
 * `IconName` is the exhaustive union of available glyphs.
 */
const ICON_PATHS = {
  home: (
    <>
      <path d="M3 10.5 8 6l5 4.5" />
      <path d="M4.5 9.5V13h7V9.5" />
    </>
  ),
  stats: (
    <>
      <path d="M3 12.5h10" />
      <path d="M5 10v2" />
      <path d="M8 7v5" />
      <path d="M11 4v8" />
    </>
  ),
  database: (
    <>
      <ellipse cx="8" cy="4" rx="4.5" ry="1.6" />
      <path d="M3.5 4v3.5c0 .9 2 1.6 4.5 1.6s4.5-.7 4.5-1.6V4" />
      <path d="M3.5 7.5V11c0 .9 2 1.6 4.5 1.6s4.5-.7 4.5-1.6V7.5" />
    </>
  ),
  coins: (
    <>
      <circle cx="6" cy="6" r="3.5" />
      <circle cx="10" cy="10" r="3.5" />
    </>
  ),
  box: (
    <>
      <path d="m2.5 5 5.5-2.5L13.5 5" />
      <path d="M2.5 5v6L8 13.5 13.5 11V5" />
      <path d="m2.5 5 5.5 2.5L13.5 5" />
      <path d="M8 7.5v6" />
    </>
  ),
  cart: (
    <>
      <path d="M2 3h2l1.5 8h7l1.2-5H4.5" />
      <circle cx="6" cy="13" r="0.8" />
      <circle cx="11.5" cy="13" r="0.8" />
    </>
  ),
  tag: (
    <>
      <path d="m2.5 2.5 5-0.2 6 6-5.2 5.2-6-6z" />
      <circle cx="5" cy="5" r="0.8" />
    </>
  ),
  factory: (
    <>
      <path d="M2 13V7l3 1.5V7l3 1.5V7l3 1.5V13H2z" />
      <path d="M2 13h12" />
      <path d="M11 13V4h2v9" />
    </>
  ),
  server: (
    <>
      <rect x="2" y="3" width="11" height="3.5" rx="1" />
      <rect x="2" y="8" width="11" height="3.5" rx="1" />
      <circle cx="4" cy="4.75" r="0.4" />
      <circle cx="4" cy="9.75" r="0.4" />
    </>
  ),
  file: (
    <>
      <path d="M3.5 1.5h5L12 5v8.5H3.5z" />
      <path d="M8.5 1.5V5H12" />
    </>
  ),
  gear: (
    <>
      <circle cx="8" cy="8" r="2" />
      <path d="M8 1.5v2M8 12.5v2M14.5 8h-2M3.5 8h-2M12.6 3.4l-1.4 1.4M4.8 11.2l-1.4 1.4M12.6 12.6l-1.4-1.4M4.8 4.8 3.4 3.4" />
    </>
  ),
  search: (
    <>
      <circle cx="7" cy="7" r="4" />
      <path d="m13 13-3-3" />
    </>
  ),
  plus: <path d="M8 3v10M3 8h10" />,
  refresh: (
    <>
      <path d="M13 4.5A5 5 0 1 0 13.5 9" />
      <path d="M13 2.5v2.5h-2.5" />
    </>
  ),
  chevdown: <path d="m4 6 4 4 4-4" />,
  chevright: <path d="m6 4 4 4-4 4" />,
  chevleft: <path d="m10 4-4 4 4 4" />,
  chevup: <path d="m4 10 4-4 4 4" />,
  chevdoubleleft: <path d="m9 4-4 4 4 4M13 4 9 8l4 4" />,
  chevdoubleright: <path d="m7 4 4 4-4 4M3 4l4 4-4 4" />,
  filter: <path d="M2 3h12l-4.5 5.5V13l-3-1.5V8.5z" />,
  bell: (
    <>
      <path d="M8 2c-2.2 0-3.5 1.5-3.5 4v2L3 10h10l-1.5-2V6c0-2.5-1.3-4-3.5-4z" />
      <path d="M6.5 12.5a1.5 1.5 0 0 0 3 0" />
    </>
  ),
  sun: (
    <>
      <circle cx="8" cy="8" r="3" />
      <path d="M8 1.5v1.5M8 13v1.5M14.5 8H13M3 8H1.5M12.6 3.4l-1 1M4.4 11.6l-1 1M12.6 12.6l-1-1M4.4 4.4l-1-1" />
    </>
  ),
  moon: <path d="M12.5 9.5A5 5 0 0 1 6.5 3.5a5 5 0 1 0 6 6z" />,
  cmd: (
    <>
      <rect x="3" y="3" width="10" height="10" rx="2" />
      <path d="M5.5 5.5h5v5h-5z" />
    </>
  ),
  arrowup: <path d="M8 3v10M4 7l4-4 4 4" />,
  arrowdown: <path d="M8 3v10M4 9l4 4 4-4" />,
  arrowleft: <path d="M13 8H3M7 4 3 8l4 4" />,
  download: <path d="M8 2v8M5 7l3 3 3-3M3 13h10" />,
  upload: <path d="M8 12V4M5 7l3-3 3 3M3 13h10" />,
  trash: <path d="M3 4h10M6 4V2.5h4V4M5 4l.5 9h5l.5-9" />,
  check: <path d="m3 8 3 3 7-7" />,
  x: <path d="M3.5 3.5l9 9M12.5 3.5l-9 9" />,
  dot: <circle cx="8" cy="8" r="1.5" fill="currentColor" />,
  save: (
    <>
      <path d="M3 3h7l3 3v7H3z" />
      <path d="M5 3v4h5V3M5 13v-4h6v4" />
    </>
  ),
  book: (
    <>
      <path d="M3 2.5h4l1 1 1-1h4V12H9l-1 1-1-1H3V2.5z" />
      <path d="M8 3.5V13" />
    </>
  ),
  keyboard: (
    <>
      <rect x="2" y="4.5" width="12" height="7" rx="1" />
      <path d="M4.5 7h.01M6.5 7h.01M8.5 7h.01M10.5 7h.01M4.5 9h.01M6.5 9h.01M8.5 9h6.5" />
    </>
  ),
  user: (
    <>
      <circle cx="8" cy="5.5" r="2.5" />
      <path d="M3 13.5c1-2.5 3-3.5 5-3.5s4 1 5 3.5" />
    </>
  ),
  activity: <path d="M2 8h2l2-5 4 10 2-5h2" />,
  wallet: (
    <>
      <path d="M2 5h10v8H2z" />
      <path d="M2 5V3.5h9V5" />
      <circle cx="10" cy="9" r="0.7" fill="currentColor" />
    </>
  ),
  bank: (
    <>
      <path d="M2 6 8 2.5 14 6" />
      <path d="M3 6v6M13 6v6M6 6v6M10 6v6" />
      <path d="M2 13h12" />
    </>
  ),
  receipt: (
    <>
      <path d="M3 1.5v13L5 13l1.5 1.5L8 13l1.5 1.5L11 13l2 1.5v-13z" />
      <path d="M5.5 4.5h5M5.5 7h5M5.5 9.5h3" />
    </>
  ),
  swap: (
    <>
      <path d="M3 5h9M9 2.5 12 5l-3 2.5" />
      <path d="M12 11H3M6 8.5 3 11l3 2.5" />
    </>
  ),
  layers: (
    <>
      <path d="m2 5 6-3 6 3-6 3z" />
      <path d="m2 8 6 3 6-3M2 11l6 3 6-3" />
    </>
  ),
  boxes: (
    <>
      <rect x="2" y="2.5" width="5" height="5" />
      <rect x="9" y="2.5" width="5" height="5" />
      <rect x="2" y="8.5" width="5" height="5" />
      <rect x="9" y="8.5" width="5" height="5" />
    </>
  ),
  truck: (
    <>
      <rect x="1.5" y="5" width="7" height="5.5" />
      <path d="M8.5 7H12l2 2.5v1H8.5" />
      <circle cx="4" cy="11.5" r="1" />
      <circle cx="11.5" cy="11.5" r="1" />
    </>
  ),
  pie: (
    <>
      <path d="M8 2v6h6A6 6 0 1 1 8 2z" />
      <path d="M10 2a6 6 0 0 1 4 4h-4z" />
    </>
  ),
  play: <path d="M4 3v10l8-5z" />,
  eye: (
    <>
      <path d="M1.5 8s2.5-4.5 6.5-4.5S14.5 8 14.5 8 12 12.5 8 12.5 1.5 8 1.5 8z" />
      <circle cx="8" cy="8" r="2" />
    </>
  ),
  history: (
    <>
      <path d="M2 8a6 6 0 1 0 1.5-4" />
      <path d="M2 2v3.5h3.5" />
      <path d="M8 5v3.5l2.5 1.5" />
    </>
  ),
  undo: (
    <>
      <path d="M3 7h6.5a3.5 3.5 0 0 1 0 7H6" />
      <path d="M5.5 4 3 7l2.5 3" />
    </>
  ),
  redo: (
    <>
      <path d="M13 7H6.5a3.5 3.5 0 0 0 0 7H10" />
      <path d="M10.5 4 13 7l-2.5 3" />
    </>
  ),
  info: (
    <>
      <circle cx="8" cy="8" r="6" />
      <path d="M8 7v4M8 5v.01" />
    </>
  ),
  calendar: (
    <>
      <rect x="2" y="3.5" width="12" height="10" rx="1" />
      <path d="M2 6.5h12M5 2v3M11 2v3" />
    </>
  ),
  'arrow-tr': <path d="M5 11 11 5M11 5H6M11 5v5" />,
  'arrow-br': <path d="M5 5 11 11M11 11V6M11 11H6" />,
  users: (
    <>
      <circle cx="6" cy="5.5" r="2" />
      <path d="M2 13.5c.8-2 2.3-3 4-3s3.2 1 4 3" />
      <circle cx="11" cy="5" r="1.8" />
      <path d="M10 10.5c.6-.3 1.3-.5 2-.5 1.5 0 2.8.9 3.5 2.5" />
    </>
  ),
  shield: (
    <>
      <path d="M8 1.5 3 4v4c0 3 2.5 5.5 5 6 2.5-.5 5-3 5-6V4z" />
      <path d="M5.5 8l2 2 3-3" />
    </>
  ),
  building: (
    <>
      <rect x="2" y="5" width="12" height="9" rx="1" />
      <path d="M5 14v-4h6v4" />
      <path d="M2 8h12" />
      <path d="M5 5V2.5h6V5" />
    </>
  ),
  calculator: (
    <>
      <rect x="3" y="2" width="10" height="12" rx="1.2" />
      <rect x="4.5" y="3.5" width="7" height="2.5" rx="0.4" />
      <circle cx="5.5" cy="8" r="0.5" fill="currentColor" />
      <circle cx="8" cy="8" r="0.5" fill="currentColor" />
      <circle cx="10.5" cy="8" r="0.5" fill="currentColor" />
      <circle cx="5.5" cy="10.5" r="0.5" fill="currentColor" />
      <circle cx="8" cy="10.5" r="0.5" fill="currentColor" />
      <circle cx="10.5" cy="10.5" r="0.5" fill="currentColor" />
      <circle cx="5.5" cy="12.5" r="0.5" fill="currentColor" />
      <circle cx="8" cy="12.5" r="0.5" fill="currentColor" />
      <circle cx="10.5" cy="12.5" r="0.5" fill="currentColor" />
    </>
  ),
  'shopping-cart': (
    <>
      <path d="M2 3h2l1.5 8h7l1.2-5H4.5" />
      <circle cx="6" cy="13" r="0.8" />
      <circle cx="11.5" cy="13" r="0.8" />
    </>
  ),
  'shopping-bag': (
    <>
      <path d="M3.5 5h9l-.7 8.5H4.2L3.5 5z" />
      <path d="M5.5 5V3.5a2.5 2.5 0 0 1 5 0V5" />
    </>
  ),
  'trending-up': (
    <>
      <path d="M2.5 11.5 7 7l2.5 2.5L13.5 5" />
      <path d="M10 5h3.5v3.5" />
    </>
  ),
  'layout-dashboard': (
    <>
      <rect x="2.5" y="2.5" width="5" height="6" rx="0.6" />
      <rect x="8.5" y="2.5" width="5" height="3.5" rx="0.6" />
      <rect x="2.5" y="9.5" width="5" height="4" rx="0.6" />
      <rect x="8.5" y="7.5" width="5" height="6" rx="0.6" />
    </>
  ),
  'users-round': (
    <>
      <circle cx="6.5" cy="6" r="2.5" />
      <path d="M2 13.5c.8-2.3 2.5-3.5 4.5-3.5s3.7 1.2 4.5 3.5" />
      <path d="M11 3.5a2.3 2.3 0 0 1 0 4.5" />
      <path d="M12.5 13.5c-.3-1.4-1-2.5-2-3.2" />
    </>
  ),
  hospital: (
    <>
      <rect x="2.5" y="3.5" width="11" height="10.5" rx="0.8" />
      <path d="M8 5.5v5M5.5 8h5" />
      <path d="M4.5 14v-2.5h2V14M9.5 14v-2.5h2V14" />
    </>
  ),
  'graduation-cap': (
    <>
      <path d="M1.5 6 8 3l6.5 3L8 9z" />
      <path d="M4.5 7.5v3c0 1 1.5 2 3.5 2s3.5-1 3.5-2v-3" />
      <path d="M13.5 6.5v3.5" />
    </>
  ),
  handshake: (
    <>
      <path d="M2 7.5 5 5l2 1.5 2-1.5 2 1.5 2 0" />
      <path d="M2 7.5v3l2.5 2 1.5-1 2 1.5 1.5-1 2 1 2-2v-3" />
      <path d="M7 6.5 9 8.5" />
    </>
  ),
  'more-vertical': (
    <>
      <circle cx="8" cy="3.5" r="1" />
      <circle cx="8" cy="8" r="1" />
      <circle cx="8" cy="12.5" r="1" />
    </>
  ),
  'alert-triangle': (
    <>
      <path d="M8 2 1.5 13.5h13z" />
      <path d="M8 6.5v3.5M8 11.5v.01" />
    </>
  ),
  'wifi-off': (
    <>
      <path d="M1.5 5.5C3 4 5.4 3 8 3c.7 0 1.4.07 2 .2" />
      <path d="M14.5 5.5a10 10 0 0 0-2.3-1.6" />
      <path d="M4 8.2A6.5 6.5 0 0 1 8 6.5" />
      <path d="M12 8.2c-.5-.4-1.1-.7-1.7-.9" />
      <path d="M6.5 10.7A3 3 0 0 1 8 10.2" />
      <path d="M8 13.5v.01" />
      <path d="m2 2 12 12" />
    </>
  ),
  'grip-vertical': (
    <>
      <path d="M6 3.5v.01M6 8v.01M6 12.5v.01M10 3.5v.01M10 8v.01M10 12.5v.01" />
    </>
  ),
} as const;

export type IconName = keyof typeof ICON_PATHS;

export interface IconProps
  extends Omit<React.SVGProps<SVGSVGElement>, 'name' | 'stroke'> {
  name: IconName;
  size?: number;
  stroke?: number;
}

export function Icon({
  name,
  size = 14,
  stroke = 1.5,
  className,
  ...props
}: IconProps) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 16 16"
      fill="none"
      stroke="currentColor"
      strokeWidth={stroke}
      strokeLinecap="round"
      strokeLinejoin="round"
      className={cn('inline-block shrink-0 align-middle', className)}
      aria-hidden
      {...props}
    >
      {ICON_PATHS[name] ?? null}
    </svg>
  );
}

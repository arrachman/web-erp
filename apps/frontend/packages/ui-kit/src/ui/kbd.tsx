import * as React from 'react';
import { cn } from '../utils';

/**
 * Keyboard hint chip. Mirrors prototype `.kbd` — 18px mono pill on a
 * subtle key surface. Inverts automatically inside a primary button
 * (matches `.btn:not(.primary) .kbd` / `.btn .kbd` rules).
 */
export const Kbd = React.forwardRef<
  HTMLSpanElement,
  React.HTMLAttributes<HTMLSpanElement>
>(({ className, ...props }, ref) => (
  <span
    ref={ref}
    className={cn(
      'inline-flex h-[18px] min-w-[18px] items-center justify-center rounded-[3px] border border-[var(--kbd-border)] bg-[var(--kbd-bg)] px-1 font-mono text-[10.5px] font-medium leading-none text-muted-foreground',
      className,
    )}
    {...props}
  />
));
Kbd.displayName = 'Kbd';

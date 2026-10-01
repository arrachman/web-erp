'use client';

import * as React from 'react';
import * as CheckboxPrimitive from '@radix-ui/react-checkbox';
import { cn } from '../utils';
import { Icon } from './icons';

/**
 * Dense ERP checkbox. Mirrors prototype `.checkbox` (14px box, primary
 * fill on checked/indeterminate). Pass `checked="indeterminate"` for
 * the tri-state header-select look.
 */
export const Checkbox = React.forwardRef<
  React.ElementRef<typeof CheckboxPrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof CheckboxPrimitive.Root>
>(({ className, ...props }, ref) => (
  <CheckboxPrimitive.Root
    ref={ref}
    className={cn(
      'inline-flex h-3.5 w-3.5 shrink-0 items-center justify-center rounded-[3px] border border-[var(--border-strong)] bg-card text-primary-foreground outline-none transition-colors',
      'focus-visible:ring-2 focus-visible:ring-ring/40',
      'data-[state=checked]:border-primary data-[state=checked]:bg-primary',
      'data-[state=indeterminate]:border-primary data-[state=indeterminate]:bg-primary',
      'disabled:cursor-not-allowed disabled:opacity-45',
      className,
    )}
    {...props}
  >
    <CheckboxPrimitive.Indicator className="flex items-center justify-center">
      {props.checked === 'indeterminate' ? (
        <span className="h-px w-2 bg-white" />
      ) : (
        <Icon name="check" size={10} stroke={2} />
      )}
    </CheckboxPrimitive.Indicator>
  </CheckboxPrimitive.Root>
));
Checkbox.displayName = CheckboxPrimitive.Root.displayName;

'use client';

import * as React from 'react';
import { Slot } from '@radix-ui/react-slot';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '../utils';

/**
 * Dense ERP button. Mirrors prototype `.btn` (+ `.primary` `.ghost`
 * `.danger` `.sm`). Default height 26px; `sm` 22px. Token-driven only.
 */
const buttonVariants = cva(
  'inline-flex items-center justify-center gap-1.5 whitespace-nowrap rounded-md border text-xs font-medium transition-colors outline-none focus-visible:ring-2 focus-visible:ring-ring/40 disabled:pointer-events-none disabled:opacity-45 [&_svg]:pointer-events-none [&_svg]:shrink-0',
  {
    variants: {
      variant: {
        default:
          'border-border bg-card text-foreground hover:bg-[var(--panel-hover)] hover:border-[var(--border-strong)] active:translate-y-px',
        primary:
          'border-primary bg-primary text-primary-foreground hover:bg-[var(--primary-hover)] hover:border-[var(--primary-hover)] active:translate-y-px',
        ghost:
          'border-transparent bg-transparent text-muted-foreground hover:bg-[var(--panel-hover)] hover:text-foreground',
        danger:
          'border-border bg-card text-danger hover:bg-danger-soft hover:border-danger/30',
      },
      size: {
        default: 'h-[26px] px-2.5 text-xs',
        sm: 'h-[22px] px-2 text-[11.5px]',
        icon: 'h-7 w-7 p-0',
      },
    },
    defaultVariants: { variant: 'default', size: 'default' },
  },
);

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean;
}

export const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, asChild = false, type, ...props }, ref) => {
    const Comp = asChild ? Slot : 'button';
    return (
      <Comp
        ref={ref}
        type={asChild ? undefined : (type ?? 'button')}
        className={cn(buttonVariants({ variant, size }), className)}
        {...props}
      />
    );
  },
);
Button.displayName = 'Button';

/** Split button group — mirrors prototype `.btn-split`. */
export function ButtonGroup({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn(
        'inline-flex [&>button:first-child]:rounded-r-none [&>button:last-child]:-ml-px [&>button:last-child]:rounded-l-none [&>button:not(:first-child):not(:last-child)]:-ml-px [&>button:not(:first-child):not(:last-child)]:rounded-none',
        className,
      )}
      {...props}
    />
  );
}

export { buttonVariants };

'use client';

import * as React from 'react';
import { cn } from '../utils';

/**
 * Dense ERP text input. Mirrors prototype `.field .ctrl input` —
 * height 26px, 12.5px text, primary focus ring. Pass `aria-invalid`
 * for the error border (mirrors `.field.err`).
 */
export const Input = React.forwardRef<
  HTMLInputElement,
  React.InputHTMLAttributes<HTMLInputElement>
>(({ className, type = 'text', ...props }, ref) => (
  <input
    ref={ref}
    type={type}
    className={cn(
      'h-[26px] w-full min-w-0 rounded-md border border-border bg-card px-2 text-[12.5px] text-foreground outline-none transition-[border-color,box-shadow] duration-75 placeholder:text-[var(--fg-subtle)]',
      'focus:border-primary focus:shadow-[0_0_0_2px_color-mix(in_oklab,var(--primary)_22%,transparent)]',
      'disabled:cursor-not-allowed disabled:opacity-45',
      'aria-[invalid=true]:border-danger',
      className,
    )}
    {...props}
  />
));
Input.displayName = 'Input';

/** Multi-line variant sharing the same dense field styling. */
export const Textarea = React.forwardRef<
  HTMLTextAreaElement,
  React.TextareaHTMLAttributes<HTMLTextAreaElement>
>(({ className, ...props }, ref) => (
  <textarea
    ref={ref}
    className={cn(
      'min-h-[64px] w-full min-w-0 rounded-md border border-border bg-card px-2 py-1.5 text-[12.5px] text-foreground outline-none transition-[border-color,box-shadow] duration-75 placeholder:text-[var(--fg-subtle)]',
      'focus:border-primary focus:shadow-[0_0_0_2px_color-mix(in_oklab,var(--primary)_22%,transparent)]',
      'disabled:cursor-not-allowed disabled:opacity-45',
      'aria-[invalid=true]:border-danger',
      className,
    )}
    {...props}
  />
));
Textarea.displayName = 'Textarea';

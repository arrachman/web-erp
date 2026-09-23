'use client';

import * as React from 'react';
import * as DialogPrimitive from '@radix-ui/react-dialog';
import { cn } from '@/lib/utils';
import { Icon } from '@/components/ui/icons';
import { tGlobal } from '@/lib/mock';

/**
 * Accessible modal built on @radix-ui/react-dialog. Mirrors prototype
 * `.cp-backdrop` + flyout/modal surface (centered card, modal shadow,
 * blurred scrim).
 */
export const Modal = DialogPrimitive.Root;
export const ModalTrigger = DialogPrimitive.Trigger;
export const ModalClose = DialogPrimitive.Close;
export const ModalPortal = DialogPrimitive.Portal;

export const ModalOverlay = React.forwardRef<
  React.ElementRef<typeof DialogPrimitive.Overlay>,
  React.ComponentPropsWithoutRef<typeof DialogPrimitive.Overlay>
>(({ className, ...props }, ref) => (
  <DialogPrimitive.Overlay
    ref={ref}
    className={cn(
      'fixed inset-0 z-[700] flex items-start justify-center bg-[rgba(15,23,42,0.35)] backdrop-blur-[2px] data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=open]:fade-in data-[state=closed]:fade-out',
      className,
    )}
    {...props}
  />
));
ModalOverlay.displayName = DialogPrimitive.Overlay.displayName;

export const ModalContent = React.forwardRef<
  React.ElementRef<typeof DialogPrimitive.Content>,
  React.ComponentPropsWithoutRef<typeof DialogPrimitive.Content> & {
    hideClose?: boolean;
    size?: 'md' | 'lg' | 'xl';
  }
>(({ className, children, hideClose, size = 'md', ...props }, ref) => (
  <ModalPortal>
    <ModalOverlay>
      <DialogPrimitive.Content
        ref={ref}
        aria-describedby={undefined}
        onClick={(e) => e.stopPropagation()}
        className={cn(
          'relative mt-[8vh] flex max-w-[calc(100vw-32px)] flex-col overflow-hidden rounded-lg border border-border bg-card text-card-foreground shadow-[var(--shadow-modal)] data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=open]:fade-in data-[state=closed]:fade-out data-[state=open]:zoom-in-95',
          size === 'xl' ? 'w-[1100px]' : size === 'lg' ? 'w-[900px]' : 'w-[560px]',
          className,
        )}
        {...props}
      >
        {children}
        {!hideClose && (
          <DialogPrimitive.Close className="absolute right-3 top-3 inline-flex h-6 w-6 items-center justify-center rounded-md text-muted-foreground outline-none transition-colors hover:bg-[var(--panel-hover)] hover:text-foreground focus-visible:ring-2 focus-visible:ring-ring/40">
            <Icon name="x" />
            <span className="sr-only">{tGlobal('Tutup')}</span>
          </DialogPrimitive.Close>
        )}
      </DialogPrimitive.Content>
    </ModalOverlay>
  </ModalPortal>
));
ModalContent.displayName = DialogPrimitive.Content.displayName;

export function ModalHeader({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn(
        'flex items-center gap-2 border-b border-border px-3.5 py-3',
        className,
      )}
      {...props}
    />
  );
}

export function ModalFooter({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn(
        'flex justify-end gap-2 border-t border-border bg-secondary px-4 py-3',
        className,
      )}
      {...props}
    />
  );
}

export const ModalTitle = React.forwardRef<
  React.ElementRef<typeof DialogPrimitive.Title>,
  React.ComponentPropsWithoutRef<typeof DialogPrimitive.Title>
>(({ className, ...props }, ref) => (
  <DialogPrimitive.Title
    ref={ref}
    className={cn('text-sm font-semibold text-foreground', className)}
    {...props}
  />
));
ModalTitle.displayName = DialogPrimitive.Title.displayName;

export const ModalDescription = React.forwardRef<
  React.ElementRef<typeof DialogPrimitive.Description>,
  React.ComponentPropsWithoutRef<typeof DialogPrimitive.Description>
>(({ className, ...props }, ref) => (
  <DialogPrimitive.Description
    ref={ref}
    className={cn('text-xs text-muted-foreground', className)}
    {...props}
  />
));
ModalDescription.displayName = DialogPrimitive.Description.displayName;

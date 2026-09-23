import * as React from 'react';
import { cn } from '../utils';

/**
 * Surface card. Mirrors prototype `.card` / `.card-h` / `.card-b`
 * (panel bg, lg radius, divided header). Compose with CardHeader +
 * CardBody; CardTitle/CardSubtitle/CardActions match the header parts.
 */
export const Card = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div
    ref={ref}
    className={cn(
      'rounded-lg border border-border bg-card text-card-foreground',
      className,
    )}
    {...props}
  />
));
Card.displayName = 'Card';

export const CardHeader = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div
    ref={ref}
    className={cn(
      'flex items-center gap-2 border-b border-border px-3 py-2.5',
      className,
    )}
    {...props}
  />
));
CardHeader.displayName = 'CardHeader';

export const CardTitle = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div
    ref={ref}
    className={cn('text-[12.5px] font-semibold text-foreground', className)}
    {...props}
  />
));
CardTitle.displayName = 'CardTitle';

export const CardSubtitle = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div
    ref={ref}
    className={cn('text-[11.5px] text-muted-foreground', className)}
    {...props}
  />
));
CardSubtitle.displayName = 'CardSubtitle';

/** Right-aligned header actions slot — mirrors `.card-h .more`. */
export const CardActions = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div
    ref={ref}
    className={cn('ml-auto flex items-center gap-1.5', className)}
    {...props}
  />
));
CardActions.displayName = 'CardActions';

export const CardBody = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div ref={ref} className={cn('p-3', className)} {...props} />
));
CardBody.displayName = 'CardBody';

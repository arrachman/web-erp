// Pure Badge re-exported from @sentient-factory/ui-kit (Tier 2 shared primitive).
// StatusBadge stays here — it maps ERP workflow status (lib/status) to a variant.
import * as React from 'react';
import { Badge, badgeVariants, type BadgeProps } from '@sentient-factory/ui-kit/ui/badge';
import { statusBadgeVariant } from '@/lib/status';

export { Badge, badgeVariants };
export type { BadgeProps };

export function StatusBadge({
  status,
  label,
  className,
}: {
  status: string;
  label?: React.ReactNode;
  className?: string;
}) {
  return (
    <Badge variant={statusBadgeVariant(status)} dot className={className}>
      {label ?? status}
    </Badge>
  );
}

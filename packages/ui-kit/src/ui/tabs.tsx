'use client';

import * as React from 'react';
import * as TabsPrimitive from '@radix-ui/react-tabs';
import { cn } from '../utils';

/**
 * Underline tab strip built on @radix-ui/react-tabs. Mirrors prototype
 * `.tabs` / `.tabs .tab` (muted text, primary underline on active).
 */
export const Tabs = TabsPrimitive.Root;

export const TabsList = React.forwardRef<
  React.ElementRef<typeof TabsPrimitive.List>,
  React.ComponentPropsWithoutRef<typeof TabsPrimitive.List>
>(({ className, ...props }, ref) => (
  <TabsPrimitive.List
    ref={ref}
    className={cn(
      'flex gap-0.5 border-b border-border bg-card px-4',
      className,
    )}
    {...props}
  />
));
TabsList.displayName = TabsPrimitive.List.displayName;

export const TabsTrigger = React.forwardRef<
  React.ElementRef<typeof TabsPrimitive.Trigger>,
  React.ComponentPropsWithoutRef<typeof TabsPrimitive.Trigger>
>(({ className, ...props }, ref) => (
  <TabsPrimitive.Trigger
    ref={ref}
    className={cn(
      'relative cursor-pointer border-0 bg-transparent px-3 py-[9px] text-[12.5px] font-medium text-muted-foreground outline-none transition-colors hover:text-foreground focus-visible:text-foreground',
      'data-[state=active]:text-foreground',
      'after:absolute after:inset-x-2 after:-bottom-px after:h-0.5 after:rounded-sm after:bg-primary after:opacity-0 data-[state=active]:after:opacity-100',
      className,
    )}
    {...props}
  />
));
TabsTrigger.displayName = TabsPrimitive.Trigger.displayName;

export const TabsContent = React.forwardRef<
  React.ElementRef<typeof TabsPrimitive.Content>,
  React.ComponentPropsWithoutRef<typeof TabsPrimitive.Content>
>(({ className, ...props }, ref) => (
  <TabsPrimitive.Content
    ref={ref}
    className={cn(
      'outline-none focus-visible:ring-2 focus-visible:ring-ring/40',
      className,
    )}
    {...props}
  />
));
TabsContent.displayName = TabsPrimitive.Content.displayName;

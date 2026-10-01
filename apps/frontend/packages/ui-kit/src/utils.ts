import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

/**
 * Merge Tailwind classes — resolves utility conflicts (twMerge) and supports
 * conditional classes (clsx). The standard shadcn helper, shared across every
 * Senti product frontend.
 */
export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs));
}

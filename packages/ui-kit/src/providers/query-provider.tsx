'use client';

import { ReactNode, useState } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

export interface AppQueryProviderProps {
  children: ReactNode;
  /** Override the default QueryClient options (merged shallow-per-group). */
  staleTime?: number;
}

/**
 * Single shared TanStack Query provider for every Senti product frontend.
 * Defaults: staleTime 30s, no refetch-on-focus, one retry. Mount once at the
 * app root, inside ThemeProvider and around the shell.
 */
export function AppQueryProvider({
  children,
  staleTime = 30_000,
}: AppQueryProviderProps) {
  const [client] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime,
            refetchOnWindowFocus: false,
            retry: 1,
          },
        },
      }),
  );

  return <QueryClientProvider client={client}>{children}</QueryClientProvider>;
}

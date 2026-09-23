import type { Metadata } from 'next';
import { ReactNode, Suspense } from 'react';
import { ThemeProvider } from 'next-themes';
import { TooltipProvider } from '@/components/ui/tooltip';
import { Toaster } from '@/components/ui/sonner';
import { AppQueryProvider } from '@/shared/providers/query-provider';
import { cn } from '@/lib/utils';
import '@/styles/globals.css';

export const metadata: Metadata = {
  title: {
    template: '%s | Sentient ERP',
    default: 'Sentient ERP',
  },
  description:
    'Sentient ERP — ERP modern: administrasi sistem, akses, dan master data.',
  applicationName: 'Sentient ERP',
  authors: [{ name: 'Sentient Factory' }],
  formatDetection: { telephone: false },
  icons: {
    icon: [
      { url: '/icon.png', sizes: '512x512', type: 'image/png' },
      { url: '/favicon.ico', sizes: 'any' },
      { url: '/favicon-32x32.png', sizes: '32x32', type: 'image/png' },
      { url: '/favicon-16x16.png', sizes: '16x16', type: 'image/png' },
    ],
    apple: [{ url: '/apple-icon.png', sizes: '180x180', type: 'image/png' }],
  },
};

export const viewport = {
  themeColor: '#007a55',
  width: 'device-width',
  initialScale: 1,
};

// Inline script content — runs synchronously before first paint so
// data-primary / data-sidebar / data-sidebar-menu / data-density / data-fontscale are never
// missing on the initial frame (prevents flash / FOUC).
const APPEARANCE_INIT_SCRIPT = `(function(){try{var s=JSON.parse(localStorage.getItem('erp-appearance')||'{}');var e=document.documentElement;e.setAttribute('data-density',s.density||'compact');e.setAttribute('data-fontscale',s.fontScale||'base');e.setAttribute('data-sidebar',s.sidebar||'icon');e.setAttribute('data-sidebar-menu',s.sidebarMenu||'flyout');e.setAttribute('data-primary',s.primary||'blue');}catch(x){}})();`;

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="id" className="h-full" suppressHydrationWarning>
      {/* eslint-disable-next-line @next/next/no-head-element */}
      <head>
        {/* Blocking script — MUST run before body to avoid FOUC */}
        <script dangerouslySetInnerHTML={{ __html: APPEARANCE_INIT_SCRIPT }} />
      </head>
      <body
        className={cn(
          'h-full antialiased text-sm text-foreground bg-background',
        )}
      >
        <ThemeProvider
          attribute="class"
          defaultTheme="light"
          storageKey="erp-theme"
          enableSystem={false}
          disableTransitionOnChange
          enableColorScheme
        >
          <AppQueryProvider>
            <TooltipProvider delayDuration={0}>
              <Suspense>{children}</Suspense>
              <Toaster />
            </TooltipProvider>
          </AppQueryProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}

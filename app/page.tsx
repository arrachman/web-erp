'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { GLOBAL_BASE_PATH } from '@/lib/shell-constants';

/** Root entry — redirects to global (no-workspace) mode. */
export default function RootPage() {
  const router = useRouter();

  useEffect(() => {
    router.replace(GLOBAL_BASE_PATH);
  }, [router]);

  return null;
}

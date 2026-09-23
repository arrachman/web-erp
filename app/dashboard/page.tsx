import { redirect } from 'next/navigation';
import { GLOBAL_BASE_PATH } from '@/lib/shell-constants';

/** /dashboard is superseded by /app — redirect for backwards compat. */
export default function DashboardPage() {
  redirect(GLOBAL_BASE_PATH);
}

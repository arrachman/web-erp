import { redirect } from 'next/navigation';
import { GLOBAL_BASE_PATH } from '@/lib/shell-constants';

/** /org is superseded by /dashboard — redirect for backwards compat. */
export default function OrgPage() {
  redirect(GLOBAL_BASE_PATH);
}

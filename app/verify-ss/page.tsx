import { redirect } from 'next/navigation';

/** Legacy verification route kept so generated Next types stay valid. */
export default function VerifySsPage() {
  redirect('/app');
}

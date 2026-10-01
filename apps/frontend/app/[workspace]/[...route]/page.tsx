import { redirect } from 'next/navigation';
import { AppShell } from '@/components/templates/app-shell';
import { GLOBAL_BASE_PATH } from '@/lib/shell-constants';

interface Props {
  params: Promise<{ workspace: string; route: string[] }>;
}

export default async function WorkspaceRoutePage({ params }: Props) {
  const { workspace, route } = await params;
  if (!/^ws\d+$/.test(workspace)) redirect(GLOBAL_BASE_PATH);
  const initialRoute = '/' + route.join('/');
  return <AppShell workspaceId={workspace} initialRoute={initialRoute} />;
}

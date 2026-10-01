import { redirect } from 'next/navigation';
import { AppShell } from '@/components/templates/app-shell';
import { GLOBAL_BASE_PATH } from '@/lib/shell-constants';

interface Props {
  params: Promise<{ workspace: string }>;
}

export default async function WorkspacePage({ params }: Props) {
  const { workspace } = await params;
  if (!/^ws\d+$/.test(workspace)) redirect(GLOBAL_BASE_PATH);
  return <AppShell workspaceId={workspace} />;
}

import { AppShell } from '@/components/templates/app-shell';

interface Props {
  params: Promise<{ route: string[] }>;
}

/** Global (no-workspace) mode — deep route. /app/master/provinces → initialRoute=/master/provinces */
export default async function AppRoutePage({ params }: Props) {
  const { route } = await params;
  const initialRoute = '/' + route.join('/');
  return <AppShell initialRoute={initialRoute} />;
}

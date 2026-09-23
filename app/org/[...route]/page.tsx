import { redirect } from 'next/navigation';

interface Props {
  params: Promise<{ route: string[] }>;
}

/** /org/[route] is superseded by /app/org/[route] — redirect for backwards compat. */
export default async function GlobalRoutePage({ params }: Props) {
  const { route } = await params;
  redirect('/app/org/' + route.join('/'));
}

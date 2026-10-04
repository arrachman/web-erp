'use client';
import { useEffect, useState } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import Link from 'next/link';
import { api, clearToken } from '@/lib/api';
import { cartCount } from '@/lib/cart';

export interface MeData {
  account: { fullName: string; email: string; role: string; schoolName: string };
  school: { name: string; jenjang: string | null; partnerId: string } | null;
}

export default function Shell({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const [me, setMe] = useState<MeData | null>(null);
  const [count, setCount] = useState(0);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    api<MeData>('/portal/me')
      .then(setMe)
      .catch(() => {
        setFailed(true);
        clearToken();
        router.replace('/login');
      });
    const update = () => setCount(cartCount());
    update();
    window.addEventListener('cart-changed', update);
    return () => window.removeEventListener('cart-changed', update);
  }, [router]);

  if (failed) return null;
  if (!me)
    return (
      <div className="auth-wrap">
        <div className="auth-card" style={{ textAlign: 'center' }}>
          Memuat portal…
        </div>
      </div>
    );

  const nav = [
    { href: '/dashboard', label: 'Dashboard' },
    { href: '/katalog', label: 'Katalog Buku' },
    { href: '/kebutuhan', label: 'Daftar Kebutuhan', badge: count > 0 ? String(count) : undefined },
    { href: '/pesanan', label: 'Pesanan' },
    { href: '/tagihan', label: 'Tagihan' },
    { href: '/profil', label: 'Profil Sekolah' },
  ];

  return (
    <div className="shell">
      <aside className="sidebar">
        <div className="brand">
          <span className="mark">B</span> Bahtera Madani
        </div>
        <div style={{ marginTop: 14 }}>
          <div style={{ fontWeight: 800, color: '#fff', fontSize: 15.5 }}>
            {me.school?.name ?? me.account.schoolName}
          </div>
          <div className="small" style={{ color: '#9db1d8' }}>
            {me.school?.jenjang ?? ''} · Portal Sekolah
          </div>
        </div>
        <nav>
          {nav.map((n) => (
            <Link key={n.href} href={n.href} className={pathname.startsWith(n.href) ? 'active' : ''}>
              {n.label}
              {n.badge && <span className="badge">{n.badge}</span>}
            </Link>
          ))}
        </nav>
        <div className="side-foot">
          <div style={{ fontWeight: 700, color: '#fff', fontSize: 14 }}>{me.account.fullName}</div>
          <div className="small" style={{ color: '#9db1d8', marginBottom: 10 }}>
            {me.account.email}
          </div>
          <button
            className="btn btn-ghost btn-sm"
            onClick={() => {
              clearToken();
              router.replace('/login');
            }}
          >
            Keluar
          </button>
        </div>
      </aside>
      <main className="main">{children}</main>
    </div>
  );
}

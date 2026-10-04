'use client';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { api, setToken } from '@/lib/api';

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError('');
    try {
      const res = await api<{ accessToken: string }>('/portal/auth/login', {
        method: 'POST',
        body: { email, password },
        auth: false,
      });
      setToken(res.accessToken);
      router.replace('/dashboard');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Gagal masuk');
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="auth-wrap">
      <form className="auth-card" onSubmit={submit}>
        <div className="brand">
          <span className="mark">B</span> Bahtera Madani
        </div>
        <h1 style={{ fontSize: 24, margin: '22px 0 4px', color: 'var(--navy)' }}>
          Masuk ke Portal Sekolah
        </h1>
        <p className="muted" style={{ margin: '0 0 20px' }}>
          Katalog, pemesanan, dan tagihan sekolah Anda dalam satu tempat.
        </p>
        {error && (
          <div className="error" style={{ marginBottom: 14 }}>
            {error}
          </div>
        )}
        <div className="field">
          <label>Email sekolah</label>
          <input
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="sekolah@contoh.sch.id"
          />
        </div>
        <div className="field">
          <label>Kata sandi</label>
          <input
            type="password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="••••••••"
          />
        </div>
        <button className="btn" type="submit" disabled={busy} style={{ width: '100%' }}>
          {busy ? 'Memproses…' : 'Masuk'}
        </button>
        <p className="small muted" style={{ textAlign: 'center', marginTop: 16 }}>
          Sekolah Anda belum punya akun?{' '}
          <Link href="/daftar" style={{ fontWeight: 700 }}>
            Daftar di sini
          </Link>
        </p>
      </form>
    </div>
  );
}

'use client';
import { useEffect, useState } from 'react';
import Link from 'next/link';
import { api, PublicSchool } from '@/lib/api';

const JENJANG = ['PAUD', 'TK', 'SD', 'SMP', 'SMA', 'SMK', 'SLB', 'OTHER'];

/** Fase 3 W4 — pendaftaran orang tua: pilih sekolah anak yang sudah terdaftar. */
function ParentDaftar({ onSwitch }: { onSwitch: () => void }) {
  const [schools, setSchools] = useState<PublicSchool[]>([]);
  const [form, setForm] = useState({
    schoolPartnerId: '',
    studentName: '',
    studentClass: '',
    fullName: '',
    email: '',
    phone: '',
    password: '',
  });
  const [error, setError] = useState('');
  const [done, setDone] = useState(false);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    api<{ data: PublicSchool[] }>('/portal/public/schools', { auth: false })
      .then((r) => setSchools(r.data))
      .catch(() => setError('Daftar sekolah gagal dimuat. Coba muat ulang halaman.'));
  }, []);

  const set = (k: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
    setForm((f) => ({ ...f, [k]: e.target.value }));

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError('');
    try {
      await api('/portal/register-parent', {
        method: 'POST',
        auth: false,
        body: {
          schoolPartnerId: form.schoolPartnerId,
          studentName: form.studentName,
          studentClass: form.studentClass,
          fullName: form.fullName,
          email: form.email,
          phone: form.phone || undefined,
          password: form.password,
        },
      });
      setDone(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Pendaftaran gagal');
    } finally {
      setBusy(false);
    }
  }

  if (done) {
    return (
      <div className="auth-wrap">
        <div className="auth-card" style={{ textAlign: 'center' }}>
          <div className="brand" style={{ justifyContent: 'center' }}>
            <span className="mark">B</span> Bahtera Madani
          </div>
          <h1 style={{ fontSize: 24, color: 'var(--navy)', margin: '20px 0 8px' }}>
            Pendaftaran terkirim
          </h1>
          <p className="muted">
            Terima kasih. Tim Bahtera Madani akan memverifikasi pendaftaran
            orang tua Anda. Akun aktif setelah disetujui — silakan coba masuk
            berkala dengan email dan kata sandi yang Anda daftarkan.
          </p>
          <Link className="btn" href="/login" style={{ marginTop: 10 }}>
            Ke halaman masuk
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-wrap">
      <form className="auth-card wide" onSubmit={submit}>
        <div className="brand">
          <span className="mark">B</span> Bahtera Madani
        </div>
        <div style={{ display: 'flex', gap: 8, marginTop: 16 }}>
          <button type="button" className="btn btn-ghost btn-sm" onClick={onSwitch}>
            Akun Sekolah
          </button>
          <button type="button" className="btn btn-sm">
            Akun Orang Tua
          </button>
        </div>
        <h1 style={{ fontSize: 24, margin: '16px 0 4px', color: 'var(--navy)' }}>
          Daftar Akun Orang Tua
        </h1>
        <p className="muted" style={{ margin: '0 0 18px' }}>
          Pesan paket kebutuhan anak (seragam, buku, paket kelas) dan pantau
          statusnya. Sekolah anak Anda harus sudah terdaftar di Bahtera Madani.
        </p>
        {error && (
          <div className="error" style={{ marginBottom: 14 }}>
            {error}
          </div>
        )}
        <div className="field">
          <label>Sekolah anak</label>
          <select required value={form.schoolPartnerId} onChange={set('schoolPartnerId')}>
            <option value="">— pilih sekolah —</option>
            {schools.map((s) => (
              <option key={s.id} value={s.id}>
                {s.name}
                {s.jenjang ? ` (${s.jenjang})` : ''}
              </option>
            ))}
          </select>
        </div>
        <div className="grid2">
          <div className="field">
            <label>Nama siswa</label>
            <input required value={form.studentName} onChange={set('studentName')} placeholder="Nama lengkap anak" />
          </div>
          <div className="field">
            <label>Kelas</label>
            <input required value={form.studentClass} onChange={set('studentClass')} placeholder="mis. 1A / B2" />
          </div>
        </div>
        <div className="grid2">
          <div className="field">
            <label>Nama orang tua</label>
            <input required value={form.fullName} onChange={set('fullName')} placeholder="Nama lengkap Anda" />
          </div>
          <div className="field">
            <label>No. HP / WhatsApp</label>
            <input value={form.phone} onChange={set('phone')} placeholder="08…" />
          </div>
        </div>
        <div className="field">
          <label>Email</label>
          <input type="email" required value={form.email} onChange={set('email')} placeholder="orangtua@contoh.id" />
        </div>
        <div className="field">
          <label>Kata sandi (min. 8 karakter)</label>
          <input type="password" required minLength={8} value={form.password} onChange={set('password')} />
        </div>
        <button className="btn" type="submit" disabled={busy} style={{ width: '100%' }}>
          {busy ? 'Mengirim…' : 'Kirim pendaftaran'}
        </button>
        <p className="small muted" style={{ textAlign: 'center', marginTop: 14 }}>
          Sudah punya akun?{' '}
          <Link href="/login" style={{ fontWeight: 700 }}>
            Masuk
          </Link>
        </p>
      </form>
    </div>
  );
}

export default function DaftarPage() {
  const [mode, setMode] = useState<'SEKOLAH' | 'ORANG_TUA'>('SEKOLAH');
  const [form, setForm] = useState({
    schoolName: '',
    jenjang: 'TK',
    npsn: '',
    address: '',
    fullName: '',
    email: '',
    phone: '',
    role: 'KEPALA_SEKOLAH',
    password: '',
  });
  const [error, setError] = useState('');
  const [done, setDone] = useState(false);
  const [busy, setBusy] = useState(false);

  const set = (k: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
    setForm((f) => ({ ...f, [k]: e.target.value }));

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError('');
    try {
      await api('/portal/register', {
        method: 'POST',
        auth: false,
        body: {
          schoolName: form.schoolName,
          jenjang: form.jenjang,
          npsn: form.npsn || undefined,
          address: form.address || undefined,
          fullName: form.fullName,
          email: form.email,
          phone: form.phone || undefined,
          role: form.role,
          password: form.password,
        },
      });
      setDone(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Pendaftaran gagal');
    } finally {
      setBusy(false);
    }
  }

  if (mode === 'ORANG_TUA') {
    return <ParentDaftar onSwitch={() => setMode('SEKOLAH')} />;
  }

  if (done) {
    return (
      <div className="auth-wrap">
        <div className="auth-card" style={{ textAlign: 'center' }}>
          <div className="brand" style={{ justifyContent: 'center' }}>
            <span className="mark">B</span> Bahtera Madani
          </div>
          <h1 style={{ fontSize: 24, color: 'var(--navy)', margin: '20px 0 8px' }}>
            Pendaftaran terkirim
          </h1>
          <p className="muted">
            Terima kasih. Tim Bahtera Madani akan memverifikasi sekolah Anda.
            Akun aktif setelah disetujui — silakan coba masuk berkala dengan
            email dan kata sandi yang Anda daftarkan.
          </p>
          <Link className="btn" href="/login" style={{ marginTop: 10 }}>
            Ke halaman masuk
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-wrap">
      <form className="auth-card wide" onSubmit={submit}>
        <div className="brand">
          <span className="mark">B</span> Bahtera Madani
        </div>
        <div style={{ display: 'flex', gap: 8, marginTop: 16 }}>
          <button type="button" className="btn btn-sm">
            Akun Sekolah
          </button>
          <button type="button" className="btn btn-ghost btn-sm" onClick={() => setMode('ORANG_TUA')}>
            Akun Orang Tua
          </button>
        </div>
        <h1 style={{ fontSize: 24, margin: '16px 0 4px', color: 'var(--navy)' }}>
          Daftar Akun Sekolah
        </h1>
        <p className="muted" style={{ margin: '0 0 18px' }}>
          Satu akun untuk kepala sekolah, bendahara, atau operator sekolah Anda.
        </p>
        {error && (
          <div className="error" style={{ marginBottom: 14 }}>
            {error}
          </div>
        )}
        <div className="field">
          <label>Nama sekolah</label>
          <input required value={form.schoolName} onChange={set('schoolName')} placeholder="TK Contoh Ceria" />
        </div>
        <div className="grid2">
          <div className="field">
            <label>Jenjang</label>
            <select value={form.jenjang} onChange={set('jenjang')}>
              {JENJANG.map((j) => (
                <option key={j} value={j}>
                  {j === 'OTHER' ? 'Lainnya' : j}
                </option>
              ))}
            </select>
          </div>
          <div className="field">
            <label>NPSN (opsional)</label>
            <input value={form.npsn} onChange={set('npsn')} placeholder="—" />
          </div>
        </div>
        <div className="field">
          <label>Alamat sekolah</label>
          <input value={form.address} onChange={set('address')} placeholder="Jalan, kelurahan, kecamatan, kota" />
        </div>
        <div className="grid2">
          <div className="field">
            <label>Nama penanggung jawab</label>
            <input required value={form.fullName} onChange={set('fullName')} placeholder="Nama lengkap" />
          </div>
          <div className="field">
            <label>Peran</label>
            <select value={form.role} onChange={set('role')}>
              <option value="KEPALA_SEKOLAH">Kepala Sekolah</option>
              <option value="BENDAHARA">Bendahara</option>
              <option value="OPERATOR">Operator / TU</option>
            </select>
          </div>
        </div>
        <div className="grid2">
          <div className="field">
            <label>Email</label>
            <input type="email" required value={form.email} onChange={set('email')} placeholder="sekolah@contoh.sch.id" />
          </div>
          <div className="field">
            <label>No. HP / WhatsApp</label>
            <input value={form.phone} onChange={set('phone')} placeholder="08…" />
          </div>
        </div>
        <div className="field">
          <label>Kata sandi (min. 8 karakter)</label>
          <input type="password" required minLength={8} value={form.password} onChange={set('password')} />
        </div>
        <button className="btn" type="submit" disabled={busy} style={{ width: '100%' }}>
          {busy ? 'Mengirim…' : 'Kirim pendaftaran'}
        </button>
        <p className="small muted" style={{ textAlign: 'center', marginTop: 14 }}>
          Sudah punya akun?{' '}
          <Link href="/login" style={{ fontWeight: 700 }}>
            Masuk
          </Link>
        </p>
      </form>
    </div>
  );
}

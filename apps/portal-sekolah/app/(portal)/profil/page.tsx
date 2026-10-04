'use client';
import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import type { MeData } from '@/components/Shell';

export default function ProfilPage() {
  const [me, setMe] = useState<MeData | null>(null);
  const [studentCount, setStudentCount] = useState('');
  const [classCount, setClassCount] = useState('');
  const [fullName, setFullName] = useState('');
  const [phone, setPhone] = useState('');
  const [msg, setMsg] = useState('');
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    api<MeData & { school: { studentCount: number | null; classCount: number | null } | null }>('/portal/me')
      .then((m) => {
        setMe(m as MeData);
        setFullName(m.account.fullName);
        setPhone((m.account as { phone?: string }).phone ?? '');
        setStudentCount(m.school?.studentCount != null ? String(m.school.studentCount) : '');
        setClassCount(m.school?.classCount != null ? String(m.school.classCount) : '');
      })
      .catch((e) => setError(e.message));
  }, []);

  async function save(e: React.FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError('');
    setMsg('');
    try {
      await api('/portal/me/profile', {
        method: 'PATCH',
        body: {
          fullName,
          phone: phone || undefined,
          studentCount: studentCount === '' ? undefined : Number(studentCount),
          classCount: classCount === '' ? undefined : Number(classCount),
        },
      });
      setMsg('Profil tersimpan.');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Gagal menyimpan');
    } finally {
      setBusy(false);
    }
  }

  if (!me)
    return error ? <div className="error">{error}</div> : <p className="muted">Memuat profil…</p>;

  const s = me.school as unknown as {
    name: string;
    code: string;
    npsn: string | null;
    jenjang: string | null;
    negeriSwasta: string | null;
    accreditation: string | null;
    pipelineStage: string | null;
    bosPagu: string | null;
    bosPeriodYear: number | null;
  } | null;

  return (
    <div>
      <h1 className="page">Profil Sekolah</h1>
      <p className="muted" style={{ marginTop: 0 }}>
        Data sekolah terhubung dengan profil CRM Bahtera Madani.
      </p>
      <div className="card" style={{ marginTop: 14 }}>
        <h2 className="sec">{s?.name ?? me.account.schoolName}</h2>
        <table className="list">
          <tbody>
            <tr>
              <td className="muted" style={{ width: 200 }}>Kode mitra</td>
              <td>{s?.code ?? '—'}</td>
            </tr>
            <tr>
              <td className="muted">NPSN</td>
              <td>{s?.npsn ?? '—'}</td>
            </tr>
            <tr>
              <td className="muted">Jenjang</td>
              <td>{s?.jenjang ?? '—'}</td>
            </tr>
            <tr>
              <td className="muted">Status sekolah</td>
              <td>{s?.negeriSwasta ?? '—'}</td>
            </tr>
            <tr>
              <td className="muted">Akreditasi</td>
              <td>{s?.accreditation ?? '—'}</td>
            </tr>
            <tr>
              <td className="muted">Pagu BOS</td>
              <td>
                {s?.bosPagu
                  ? `Rp${new Intl.NumberFormat('id-ID').format(Number(s.bosPagu))}${s?.bosPeriodYear ? ` (${s.bosPeriodYear})` : ''}`
                  : '—'}
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <form className="card" style={{ marginTop: 14 }} onSubmit={save}>
        <h2 className="sec">Data yang bisa diperbarui</h2>
        {error && (
          <div className="error" style={{ marginBottom: 12 }}>
            {error}
          </div>
        )}
        {msg && (
          <div className="success" style={{ marginBottom: 12 }}>
            {msg}
          </div>
        )}
        <div className="grid2">
          <div className="field">
            <label>Nama penanggung jawab</label>
            <input value={fullName} onChange={(e) => setFullName(e.target.value)} />
          </div>
          <div className="field">
            <label>No. HP / WhatsApp</label>
            <input value={phone} onChange={(e) => setPhone(e.target.value)} />
          </div>
        </div>
        <div className="grid2">
          <div className="field">
            <label>Jumlah siswa</label>
            <input inputMode="numeric" value={studentCount} onChange={(e) => setStudentCount(e.target.value)} />
          </div>
          <div className="field">
            <label>Jumlah kelas / rombel</label>
            <input inputMode="numeric" value={classCount} onChange={(e) => setClassCount(e.target.value)} />
          </div>
        </div>
        <button className="btn" type="submit" disabled={busy}>
          {busy ? 'Menyimpan…' : 'Simpan perubahan'}
        </button>
        <p className="small muted">
          Perubahan identitas resmi sekolah (nama, NPSN) dilakukan melalui tim
          Bahtera Madani agar data tetap valid.
        </p>
      </form>
    </div>
  );
}

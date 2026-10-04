'use client';
import { useEffect, useState } from 'react';
import { api, CatalogItem, fmtIDR } from '@/lib/api';
import { addToCart } from '@/lib/cart';

const JENJANG = ['', 'PAUD', 'TK', 'SD', 'SMP', 'SMA', 'SMK', 'SLB'];

export default function KatalogPage() {
  const [items, setItems] = useState<CatalogItem[]>([]);
  const [total, setTotal] = useState(0);
  const [search, setSearch] = useState('');
  const [jenjang, setJenjang] = useState('');
  const [page, setPage] = useState(1);
  const [busy, setBusy] = useState(false);
  const [added, setAdded] = useState<string>('');
  const [error, setError] = useState('');
  const pageSize = 24;

  async function load(p = page) {
    setBusy(true);
    setError('');
    try {
      const q = new URLSearchParams({ page: String(p), pageSize: String(pageSize) });
      if (search) q.set('search', search);
      if (jenjang) q.set('jenjang', jenjang);
      const res = await api<{ data: CatalogItem[]; total: number }>(`/portal/catalog?${q}`);
      setItems(res.data);
      setTotal(res.total);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Gagal memuat katalog');
    } finally {
      setBusy(false);
    }
  }

  useEffect(() => {
    load(1);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [jenjang]);

  const pages = Math.max(1, Math.ceil(total / pageSize));

  return (
    <div>
      <h1 className="page">Katalog Buku</h1>
      <p className="muted" style={{ marginTop: 0 }}>
        Buku dan kebutuhan sekolah dari CV Bahtera Madani. Tambahkan ke Daftar
        Kebutuhan, lalu kirim sebagai pesanan.
      </p>
      <div className="toolbar">
        <input
          placeholder="Cari judul atau kode buku…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter') {
              setPage(1);
              load(1);
            }
          }}
        />
        <select value={jenjang} onChange={(e) => setJenjang(e.target.value)} style={{ maxWidth: 180 }}>
          <option value="">Semua jenjang</option>
          {JENJANG.filter(Boolean).map((j) => (
            <option key={j} value={j}>
              {j}
            </option>
          ))}
        </select>
        <button
          className="btn btn-ghost"
          onClick={() => {
            setPage(1);
            load(1);
          }}
        >
          Cari
        </button>
      </div>
      {error && <div className="error">{error}</div>}
      {added && (
        <div className="success" style={{ marginBottom: 12 }}>
          {added} ditambahkan ke Daftar Kebutuhan.
        </div>
      )}
      {busy ? (
        <p className="muted">Memuat katalog…</p>
      ) : items.length === 0 ? (
        <div className="card">
          <h2 className="sec">Buku tidak ditemukan</h2>
          <p className="muted">Coba kata kunci atau jenjang lain.</p>
        </div>
      ) : (
        <div className="cat-grid">
          {items.map((i) => (
            <div key={i.id} className="card cat-item">
              <div>
                <span className="chip">{i.jenjang ?? i.category ?? 'Umum'}</span>
                {i.isCustomPrint && (
                  <span className="chip chip-amber" style={{ marginLeft: 6 }}>
                    Cetak khusus
                  </span>
                )}
              </div>
              <div style={{ fontWeight: 700, lineHeight: 1.35 }}>{i.name}</div>
              <div className="small muted">
                {i.publisherName ?? ''}
                {i.subject ? ` · ${i.subject}` : ''}
                {i.gradeLevel ? ` · Kelas ${i.gradeLevel}` : ''}
              </div>
              <div style={{ marginTop: 'auto' }}>
                <div className="price">{fmtIDR(i.price ?? i.salePrice)}</div>
                {i.priceSource && i.priceSource !== 'STANDAR' && (
                  <div>
                    <span className="chip chip-ok">Harga kontrak sekolah</span>
                    {Number(i.price) !== Number(i.salePrice) && (
                      <span className="het" style={{ textDecoration: 'line-through', marginLeft: 6 }}>
                        {fmtIDR(i.salePrice)}
                      </span>
                    )}
                  </div>
                )}
                {i.hetPrice && <div className="het">HET {fmtIDR(i.hetPrice)}</div>}
                {i.bundle && i.bundle.components.length > 0 && (
                  <div className="small muted" style={{ marginTop: 6 }}>
                    <strong>Isi paket:</strong>
                    <ul style={{ margin: '4px 0 0', paddingLeft: 16 }}>
                      {i.bundle.components.map((c) => (
                        <li key={c.itemId}>
                          {c.name ?? 'Item'} × {c.quantity}
                          {c.unit ? ` ${c.unit}` : ''}
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </div>
              <button
                className="btn btn-sm"
                onClick={() => {
                  addToCart(i);
                  setAdded(i.name);
                  setTimeout(() => setAdded(''), 2500);
                }}
              >
                + Daftar Kebutuhan
              </button>
            </div>
          ))}
        </div>
      )}
      {pages > 1 && (
        <div style={{ display: 'flex', gap: 8, marginTop: 18, alignItems: 'center' }}>
          <button
            className="btn btn-ghost btn-sm"
            disabled={page <= 1}
            onClick={() => {
              setPage(page - 1);
              load(page - 1);
            }}
          >
            ← Sebelumnya
          </button>
          <span className="small muted">
            Halaman {page} dari {pages} · {total} buku
          </span>
          <button
            className="btn btn-ghost btn-sm"
            disabled={page >= pages}
            onClick={() => {
              setPage(page + 1);
              load(page + 1);
            }}
          >
            Berikutnya →
          </button>
        </div>
      )}
    </div>
  );
}

import PDFDocument from 'pdfkit';
import { DocData } from './docpkg.data';

/**
 * A3 — Built-in document renderer (pdfkit). One generic layout for all six
 * procurement documents; BOS variant adds the funding statement and the
 * school-side signature block wording used for BOS accountability files.
 * (User-editable designer templates remain the Report Designer domain; this
 * renderer guarantees the procurement package always exists.)
 */

function fmtMoney(v: string): string {
  const n = Number(v || 0);
  return n.toLocaleString('id-ID', { minimumFractionDigits: 0, maximumFractionDigits: 2 });
}

function fmtDate(d: Date): string {
  return new Date(d).toLocaleDateString('id-ID', { day: '2-digit', month: 'long', year: 'numeric' });
}

function drawDoc(doc: PDFKit.PDFDocument, data: DocData, isFirst: boolean) {
  if (!isFirst) doc.addPage();
  const left = 48;
  const right = 547;
  let y = 48;

  // Company header
  doc.font('Helvetica-Bold').fontSize(14).text(data.company.name, left, y);
  y += 18;
  doc.font('Helvetica').fontSize(9);
  if (data.company.address) {
    doc.text(data.company.address, left, y);
    y += 12;
  }
  const contact = [data.company.phone, data.company.email].filter(Boolean).join(' · ');
  if (contact) {
    doc.text(contact, left, y);
    y += 12;
  }
  if (data.company.npwp) {
    doc.text(`NPWP: ${data.company.npwp}`, left, y);
    y += 12;
  }
  y += 4;
  doc.moveTo(left, y).lineTo(right, y).lineWidth(1.2).stroke();
  y += 16;

  // Title + number
  doc.font('Helvetica-Bold').fontSize(13).text(data.title, left, y, { align: 'center', width: right - left });
  y += 20;
  doc.font('Helvetica').fontSize(10).text(`Nomor: ${data.sourceDocNumber}`, left, y, { align: 'center', width: right - left });
  y += 14;
  doc.text(`Tanggal: ${fmtDate(data.docDate)}`, left, y, { align: 'center', width: right - left });
  y += 22;

  // Party block
  doc.fontSize(10);
  doc.text('Kepada Yth:', left, y);
  y += 13;
  doc.font('Helvetica-Bold').text(data.party.name, left, y);
  y += 13;
  doc.font('Helvetica');
  if (data.party.npsn) {
    doc.text(`NPSN: ${data.party.npsn}`, left, y);
    y += 13;
  }
  if (data.party.address) {
    doc.text(data.party.address, left, y, { width: 320 });
    y += 13 * Math.ceil(data.party.address.length / 52);
  }
  if (data.party.npwp) {
    doc.text(`NPWP: ${data.party.npwp}`, left, y);
    y += 13;
  }
  y += 6;
  if (data.fundingNote) {
    doc.font('Helvetica-Oblique').text(data.fundingNote, left, y);
    doc.font('Helvetica');
    y += 16;
  }
  if (data.receiptNote) {
    doc.text(data.receiptNote, left, y, { width: right - left });
    y += 16;
  }
  doc.text(`Berdasarkan Surat Pesanan Nomor: ${data.orderNumber}`, left, y);
  y += 20;

  // Lines table
  if (data.lines.length) {
    const cols = { no: left, item: left + 28, qty: left + 308, price: left + 368, amount: left + 448 };
    doc.font('Helvetica-Bold').fontSize(9);
    doc.text('No', cols.no, y);
    doc.text('Barang / Jasa', cols.item, y);
    doc.text('Qty', cols.qty, y);
    doc.text('Harga Satuan', cols.price, y);
    doc.text('Jumlah', cols.amount, y);
    y += 6;
    doc.moveTo(left, y + 8).lineTo(right, y + 8).lineWidth(0.6).stroke();
    y += 14;
    doc.font('Helvetica').fontSize(9);
    data.lines.forEach((l, i) => {
      if (y > 690) {
        doc.addPage();
        y = 56;
      }
      doc.text(String(i + 1), cols.no, y);
      doc.text(`${l.itemName}${l.itemCode ? ` (${l.itemCode})` : ''}`, cols.item, y, { width: 272 });
      doc.text(`${l.qty} ${l.unitName}`.trim(), cols.qty, y);
      doc.text(fmtMoney(l.unitPrice), cols.price, y);
      doc.text(fmtMoney(l.amount), cols.amount, y);
      y += 14;
    });
    y += 4;
    doc.moveTo(left, y).lineTo(right, y).lineWidth(0.6).stroke();
    y += 12;
  }

  // Totals
  const totalsX = 368;
  doc.fontSize(10);
  const totalRow = (label: string, value: string, bold = false) => {
    doc.font(bold ? 'Helvetica-Bold' : 'Helvetica');
    doc.text(label, totalsX, y);
    doc.text(fmtMoney(value), totalsX + 92, y);
    y += 14;
  };
  if (data.kind !== 'KUITANSI') {
    totalRow('Subtotal', data.subtotal);
    if (Number(data.discount) !== 0) totalRow('Diskon', `-${fmtMoney(data.discount)}`);
    if (Number(data.tax) !== 0) totalRow('Pajak', data.tax);
  }
  totalRow('TOTAL', data.total, true);
  y += 4;
  doc.font('Helvetica-Oblique').fontSize(9).text(`Terbilang: ${data.terbilang}`, left, y, { width: right - left });
  y += 22;

  // BAST acceptance block
  if (data.kind === 'BAST') {
    doc.font('Helvetica').fontSize(10);
    doc.text(
      'Barang tersebut di atas telah diterima dalam keadaan baik dan cukup, serta telah diperiksa kesesuaiannya dengan surat pesanan.',
      left,
      y,
      { width: right - left },
    );
    y += 28;
    if (data.acceptance?.acceptedAt) {
      doc.text(`Diterima pada: ${fmtDate(data.acceptance.acceptedAt)}`, left, y);
      y += 13;
    }
    if (data.acceptance?.notes) {
      doc.text(`Catatan: ${data.acceptance.notes}`, left, y, { width: right - left });
      y += 13;
    }
    if (data.acceptance && data.acceptance.photoCount > 0) {
      doc.text(`Lampiran foto serah terima: ${data.acceptance.photoCount} berkas (tersimpan di arsip digital).`, left, y);
      y += 13;
    }
    y += 8;
  }

  // Signature blocks
  if (y > 660) {
    doc.addPage();
    y = 64;
  } else {
    y = Math.max(y + 12, 620);
  }
  const signCol = (x: number, role: string, name?: string | null) => {
    doc.font('Helvetica').fontSize(10).text(role, x, y, { width: 200 });
    doc.text(' ', x, y + 44);
    doc.font('Helvetica-Bold').text(name || '( ……………………… )', x, y + 58, { width: 200 });
  };
  if (data.kind === 'BAST' || data.kind === 'SURAT_JALAN') {
    signCol(left, `Yang menyerahkan,\n${data.company.name}`);
    signCol(330, 'Yang menerima,', data.acceptance?.byName ?? null);
    if (data.acceptance?.byTitle) {
      doc.font('Helvetica').fontSize(9).text(data.acceptance.byTitle, 330, y + 74, { width: 200 });
    }
  } else if (data.kind === 'KUITANSI') {
    signCol(330, `Penerima,\n${data.company.name}`);
  } else {
    signCol(330, `Hormat kami,\n${data.company.name}`);
  }

  // Footer
  doc.font('Helvetica').fontSize(8).fillColor('#666666');
  doc.text(
    `Dokumen dibuat otomatis oleh sistem ERP ${data.company.name} · ${data.kind} · varian ${data.variant === 'BOS' ? 'BOS' : 'Non-BOS'} · berdasarkan data transaksi tanpa input ulang.`,
    left,
    800,
    { width: right - left, align: 'center' },
  );
  doc.fillColor('#000000');
}

function bufferize(build: (doc: PDFKit.PDFDocument) => void): Promise<Buffer> {
  return new Promise((resolve, reject) => {
    const doc = new PDFDocument({ size: 'A4', margin: 48 });
    const chunks: Buffer[] = [];
    doc.on('data', (c: Buffer) => chunks.push(c));
    doc.on('end', () => resolve(Buffer.concat(chunks)));
    doc.on('error', reject);
    build(doc);
    doc.end();
  });
}

export function renderSingle(data: DocData): Promise<Buffer> {
  return bufferize((doc) => drawDoc(doc, data, true));
}

export function renderPackage(datas: DocData[]): Promise<Buffer> {
  return bufferize((doc) => datas.forEach((d, i) => drawDoc(doc, d, i === 0)));
}

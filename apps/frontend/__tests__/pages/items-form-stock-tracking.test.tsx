import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { SectionBody } from '@/components/pages/items-form-sections';
import { defaultItemForm } from '@/components/pages/items-form-model';
import { toItemPayload } from '@/components/pages/items-form';
import { renderPage } from '../helpers/render-page';

const renderTrackingSection = (overrides = {}) => {
  const data = { ...defaultItemForm(), ...overrides };
  const onChange = vi.fn();

  renderPage(
    <SectionBody
      id="pergerakanstok"
      data={data}
      onChange={onChange}
      errors={{}}
      generating={false}
      onAutoCode={vi.fn()}
    />,
  );

  return { data, onChange };
};

describe('Item identity section', () => {
  it('combines identity and classification fields', () => {
    renderPage(
      <SectionBody
        id="identitas"
        data={defaultItemForm()}
        onChange={vi.fn()}
        errors={{}}
        generating={false}
        onAutoCode={vi.fn()}
      />,
    );

    expect(screen.getByText('Identitas & Klasifikasi')).toBeInTheDocument();
    expect(screen.getByText('Kode')).toBeInTheDocument();
    expect(screen.getByText('Nama')).toBeInTheDocument();
    expect(screen.getByText('Tipe')).toBeInTheDocument();
    expect(screen.getByText('Kategori')).toBeInTheDocument();
    expect(screen.getByText('Satuan')).toBeInTheDocument();
    expect(screen.getByText('Jenis Barang')).toBeInTheDocument();
  });
});

describe('Item stock tracking section', () => {
  it('explains all three exclusive tracking choices', () => {
    renderTrackingSection();

    expect(screen.getByRole('radiogroup', { name: 'Cara pencatatan stok' })).toBeInTheDocument();
    expect(screen.getByRole('radio', { name: /Tidak pakai/i })).toBeChecked();
    expect(screen.getByText('Stok hanya dicatat berdasarkan jumlah.')).toBeInTheDocument();
    expect(screen.getByText('Cocok untuk bahan curah dan ATK.')).toBeInTheDocument();
    expect(screen.getByText('Stok dikelompokkan berdasarkan batch atau lot.')).toBeInTheDocument();
    expect(screen.getByText('Cocok untuk makanan, obat, dan bahan baku.')).toBeInTheDocument();
    expect(screen.getByText('Setiap unit stok memiliki nomor unik.')).toBeInTheDocument();
    expect(screen.getByText('Cocok untuk mesin, laptop, dan elektronik.')).toBeInTheDocument();
  });

  it.each([
    ['Batch / Lot', false, true],
    ['Serial No.', true, false],
  ])('maps %s to mutually exclusive flags without mutating input', async (label, tracksSerial, tracksBatch) => {
    const user = userEvent.setup();
    const { data, onChange } = renderTrackingSection();

    await user.click(screen.getByRole('radio', { name: new RegExp(label, 'i') }));

    expect(onChange).toHaveBeenCalledWith({ ...data, tracksSerial, tracksBatch });
    expect(data.tracksSerial).toBe(false);
    expect(data.tracksBatch).toBe(false);
  });

  it('shows Serial No. for inconsistent legacy flags and normalizes them on save', () => {
    renderTrackingSection({ tracksSerial: true, tracksBatch: true });

    expect(screen.getByRole('radio', { name: /Serial No\./i })).toBeChecked();

    const payload = toItemPayload({
      ...defaultItemForm(),
      tracksSerial: true,
      tracksBatch: true,
    });
    expect(payload.tracksSerial).toBe(true);
    expect(payload.tracksBatch).toBe(false);
  });
});

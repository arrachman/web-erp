'use client';

/**
 * Left + middle columns of the <TrxForm> header form section.
 * Renders the party/uraian/akun/lokasi block and the
 * tanggal/no-transaksi/uang/kurs block. Right column (status +
 * ringkasan) lives in `trx-form-summary.tsx`.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { FormField } from '@/components/ui/form-field';
import { type Translator } from '@/lib/mock';
import type { TrxConfig } from './trx-form-config';
import type { LookupKind } from '@/components/organisms/lookup-modal';

const MONO: React.CSSProperties = { fontFamily: 'Geist Mono, monospace' };

export interface TrxHeadState {
  party: string;
  akun: string;
  uraian: string;
  lokasi: string;
  tanggal: string;
  auto: boolean;
  uang: string;
  kurs: string;
  status: string;
  noGiro: string;
  jatuhTempo: string;
  noTransaksi?: string;
}

export type TrxLookupTarget = 'akun' | 'lokasi' | 'coa-line';

export interface TrxFormFieldsProps {
  cfg: TrxConfig;
  t: Translator;
  head: TrxHeadState;
  setHead: React.Dispatch<React.SetStateAction<TrxHeadState>>;
  headerErr: boolean;
  openPicker: () => void;
  openLookup: (kind: LookupKind, target: TrxLookupTarget) => void;
}

export function TrxFormFieldsLeft({
  cfg,
  t,
  head,
  setHead,
  headerErr,
  openPicker,
  openLookup,
}: TrxFormFieldsProps): React.ReactElement {
  return (
    <div>
      {cfg.party ? (
        <FormField
          label={t(cfg.party)}
          required
          error={
            headerErr ? 'Wajib diisi · klik untuk cari kontak' : undefined
          }
          controlAddon={
            <button
              type="button"
              className="lookup"
              tabIndex={-1}
              onClick={(e) => {
                e.stopPropagation();
                openPicker();
              }}
            >
              <Icon name="search" size={11} />
            </button>
          }
        >
          <input
            type="text"
            value={head.party}
            placeholder="Klik untuk cari kontak…"
            readOnly
            onClick={(e) => {
              e.stopPropagation();
              openPicker();
            }}
            onFocus={(e) => {
              e.stopPropagation();
              openPicker();
            }}
          />
        </FormField>
      ) : (
        <FormField label={t('Uraian')} required help="Keterangan jurnal">
          <input
            type="text"
            value={head.uraian}
            placeholder="cth: Penyesuaian akhir bulan"
            onChange={(e) =>
              setHead((h) => ({ ...h, uraian: e.target.value }))
            }
          />
        </FormField>
      )}
      {cfg.acct && (
        <FormField
          label={t(cfg.acct)}
          required
          controlAddon={
            cfg.opening ? null : (
              <button
                type="button"
                className="lookup"
                tabIndex={-1}
                onClick={(e) => {
                  e.stopPropagation();
                  openLookup('coa', 'akun');
                }}
              >
                <Icon name="search" size={11} />
              </button>
            )
          }
        >
          <input
            type="text"
            value={head.akun}
            readOnly={!cfg.opening}
            onClick={
              cfg.opening
                ? undefined
                : (e) => {
                    e.stopPropagation();
                    openLookup('coa', 'akun');
                  }
            }
            onChange={(e) =>
              setHead((h) => ({ ...h, akun: e.target.value }))
            }
          />
        </FormField>
      )}
      {cfg.party && (
        <FormField label={t('Uraian')}>
          <input
            type="text"
            value={head.uraian}
            placeholder="cth: Pelunasan INV-2026-1024"
            onChange={(e) =>
              setHead((h) => ({ ...h, uraian: e.target.value }))
            }
          />
        </FormField>
      )}
      <FormField
        label={t('Lokasi')}
        controlAddon={
          <button
            type="button"
            className="lookup"
            tabIndex={-1}
            onClick={(e) => {
              e.stopPropagation();
              openLookup('lokasi', 'lokasi');
            }}
          >
            <Icon name="search" size={11} />
          </button>
        }
      >
        <input
          type="text"
          value={head.lokasi}
          readOnly
          onClick={(e) => {
            e.stopPropagation();
            openLookup('lokasi', 'lokasi');
          }}
        />
      </FormField>
    </div>
  );
}

export function TrxFormFieldsMid({
  cfg,
  t,
  head,
  setHead,
}: TrxFormFieldsProps): React.ReactElement {
  return (
    <div>
      <FormField
        label={t('Tanggal')}
        required
        controlAddon={
          <button type="button" className="lookup" tabIndex={-1}>
            <Icon name="calendar" size={11} />
          </button>
        }
      >
        <input
          type="text"
          value={head.tanggal}
          onChange={(e) =>
            setHead((h) => ({ ...h, tanggal: e.target.value }))
          }
        />
      </FormField>
      <FormField
        label={t('No Transaksi')}
        required
        controlAddon={
          <label
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: 4,
              position: 'absolute',
              right: 8,
              fontSize: 'calc(11.5px * var(--font-scale, 1))',
              color: 'var(--fg-muted)',
            }}
          >
            <input
              type="checkbox"
              className="checkbox"
              checked={head.auto}
              onChange={(e) =>
                setHead((h) => ({ ...h, auto: e.target.checked }))
              }
            />
            Auto
          </label>
        }
      >
        <input
          type="text"
          value={
            head.auto
              ? `Auto · ${cfg.code}-2605-2399`
              : head.noTransaksi ?? ''
          }
          disabled={head.auto}
          onChange={(e) =>
            setHead((h) => ({ ...h, noTransaksi: e.target.value }))
          }
        />
      </FormField>
      {cfg.giro && (
        <FormField label="No Giro / Cek" required>
          <input
            type="text"
            value={head.noGiro}
            placeholder="cth: BCA-0048213"
            onChange={(e) =>
              setHead((h) => ({ ...h, noGiro: e.target.value }))
            }
          />
        </FormField>
      )}
      {cfg.giro ? (
        <FormField
          label="Jatuh Tempo"
          required
          controlAddon={
            <button type="button" className="lookup" tabIndex={-1}>
              <Icon name="calendar" size={11} />
            </button>
          }
        >
          <input
            type="text"
            value={head.jatuhTempo}
            onChange={(e) =>
              setHead((h) => ({ ...h, jatuhTempo: e.target.value }))
            }
          />
        </FormField>
      ) : (
        <FormField label={t('Uang')} required>
          <select
            value={head.uang}
            onChange={(e) =>
              setHead((h) => ({ ...h, uang: e.target.value }))
            }
          >
            <option>IDR</option>
            <option>USD</option>
            <option>SGD</option>
            <option>EUR</option>
          </select>
        </FormField>
      )}
      <FormField label={t('Kurs')} required>
        <input
          type="text"
          value={head.kurs}
          onChange={(e) => setHead((h) => ({ ...h, kurs: e.target.value }))}
          style={{ textAlign: 'right', ...MONO }}
        />
      </FormField>
    </div>
  );
}

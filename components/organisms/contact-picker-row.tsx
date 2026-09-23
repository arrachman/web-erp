'use client';

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { fmtCompact } from '@/lib/mock';
import { cn } from '@/lib/utils';
import {
  avatarHue,
  CAT_PILL,
  type Contact,
  HighlightMatch,
  initials,
  MONO_FONT,
  RECENT_IDS,
} from './contact-picker-data';

/**
 * Single row in the contact picker list. Sub-organism extracted from
 * `ContactPicker` to keep the parent file under 400 LOC.
 */

export interface ContactPickerRowProps {
  contact: Contact;
  index: number;
  focused: boolean;
  query: string;
  onFocus: (index: number) => void;
  onChoose: (c: Contact) => void;
}

export function ContactPickerRow({
  contact: c,
  index,
  focused,
  query,
  onFocus,
  onChoose,
}: ContactPickerRowProps): React.ReactElement {
  return (
    <div
      data-row={index}
      className={cn('cm-row', focused && 'active')}
      onMouseEnter={() => onFocus(index)}
      onClick={() => onChoose(c)}
      role="option"
      aria-selected={focused}
    >
      <div
        className="cm-av"
        style={{
          background: `hsl(${avatarHue(c)} 70% 92%)`,
          color: `hsl(${avatarHue(c)} 70% 32%)`,
        }}
      >
        {initials(c.name)}
      </div>
      <div className="cm-main">
        <div className="cm-name">
          <HighlightMatch text={c.name} q={query} />
          {RECENT_IDS.includes(c.id - 1) && (
            <span className="cm-tag-recent">recent</span>
          )}
        </div>
        <div className="cm-sub">
          <span className="cm-code">
            <HighlightMatch text={c.code} q={query} />
          </span>
          <span className="cm-sep">·</span>
          <span>
            <Icon name="dot" size={6} /> {c.city}
          </span>
          <span className="cm-sep">·</span>
          <span>{c.trxCount} transaksi</span>
        </div>
      </div>
      <span
        className={`pill ${CAT_PILL[c.cat]}`}
        style={{ alignSelf: 'center' }}
      >
        {c.cat}
      </span>
      <div className="cm-bal" style={{ alignSelf: 'center' }}>
        <div
          className="muted"
          style={{
            fontSize: 'calc(10.5px * var(--font-scale, 1))',
            textTransform: 'uppercase',
            letterSpacing: '0.04em',
          }}
        >
          Saldo
        </div>
        <div
          className="num"
          style={{
            ...MONO_FONT,
            fontVariantNumeric: 'tabular-nums',
            fontWeight: 500,
            color: c.balance < 0 ? 'var(--danger)' : 'var(--fg)',
          }}
        >
          {fmtCompact(c.balance)}
        </div>
      </div>
      <Icon name="chevright" size={11} className="cm-go" />
    </div>
  );
}

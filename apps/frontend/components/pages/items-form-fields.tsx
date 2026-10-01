'use client';

/**
 * Item form UI orchestrator. Two entry modes:
 *  - Cepat: only Identitas & Klasifikasi (quick-add).
 *  - Lengkap: grouped side-nav with one section visible at a time, a live
 *    identity header, and a progress / prev-next footer.
 * Default: Cepat for a new item (no code), Lengkap when editing.
 *
 * Section metadata + nav + mode toggle → items-form-nav.
 * Section bodies → items-form-sections. Header/footer → items-form-chrome.
 * Data shape + adapters → items-form. Atomic tier: Organism.
 */

import * as React from 'react';
import type { FormErrors } from '@/lib/form-validation';
import { generateNextItemCode } from '@/lib/items-code-generator';
import type { ItemFormData } from './items-form';
import {
  buildSections, ModeToggle, SectionNav, CEPAT_SECTIONS,
  type Mode, type SectionId,
} from './items-form-nav';
import { SectionBody } from './items-form-sections';
import { ItemFormContextHeader, ItemFormFooter } from './items-form-chrome';

const PANEL_MAX_HEIGHT = 'calc(86vh - 120px)';

export function ItemFormFields({
  data, onChange, errors = {},
}: { data: ItemFormData; onChange: (d: ItemFormData) => void; errors?: FormErrors<ItemFormData> }) {
  // Mode default: Cepat for a new item (empty code) = quick-add; Lengkap when editing.
  const [mode, setMode] = React.useState<Mode>(() => (data.code ? 'lengkap' : 'cepat'));
  const [activeSection, setActiveSection] = React.useState<SectionId>('identitas');
  const [generating, setGenerating] = React.useState(false);

  const handleAutoCode = async () => {
    setGenerating(true);
    try { onChange({ ...data, code: await generateNextItemCode(data.itemType) }); }
    finally { setGenerating(false); }
  };

  const sections = buildSections(data, errors);
  const availableSections = sections.filter((s) => s.available);
  const accountError = sections.find((s) => s.id === 'akuntansi')?.hasError ?? false;

  // GL accounts (INVENTORY-required) are hidden in Cepat — if validation fails
  // there, jump to Lengkap + open Akuntansi so the error can be fixed.
  React.useEffect(() => {
    if (accountError && mode === 'cepat') { setMode('lengkap'); setActiveSection('akuntansi'); }
  }, [accountError, mode]);

  const bodyProps = { data, onChange, errors, generating, onAutoCode: handleAutoCode };

  if (mode === 'cepat') {
    return (
      <div className="flex flex-col" style={{ maxHeight: PANEL_MAX_HEIGHT }}>
        <ModeToggle mode={mode} onMode={setMode} />
        <ItemFormContextHeader data={data} />
        <div className="min-h-0 flex-1 overflow-y-auto">
          {CEPAT_SECTIONS.map((id) =>
            sections.find((s) => s.id === id)?.available && <SectionBody key={id} id={id} {...bodyProps} />)}
        </div>
      </div>
    );
  }

  // Lengkap mode: grouped side-nav + active section + footer.
  const activeIndex = Math.max(0, availableSections.findIndex((s) => s.id === activeSection));
  const active = availableSections[activeIndex] ?? availableSections[0];
  const filledCount = availableSections.filter((s) => s.filled).length;
  const goTo = (i: number) => { if (i >= 0 && i < availableSections.length) setActiveSection(availableSections[i].id); };

  return (
    <div className="flex flex-col" style={{ maxHeight: PANEL_MAX_HEIGHT }}>
      <ModeToggle mode={mode} onMode={setMode} />
      <ItemFormContextHeader data={data} />
      <div className="flex min-h-0 flex-1">
        <SectionNav sections={sections} activeId={active.id} onSelect={setActiveSection} />
        <div className="min-h-0 flex-1 overflow-y-auto">
          <SectionBody id={active.id} {...bodyProps} />
        </div>
      </div>
      <ItemFormFooter
        filledCount={filledCount}
        totalCount={availableSections.length}
        position={`Bagian ${activeIndex + 1} dari ${availableSections.length} · ${active.label}`}
        canPrev={activeIndex > 0}
        canNext={activeIndex < availableSections.length - 1}
        onPrev={() => goTo(activeIndex - 1)}
        onNext={() => goTo(activeIndex + 1)}
      />
    </div>
  );
}

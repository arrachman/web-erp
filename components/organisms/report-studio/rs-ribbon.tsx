'use client';

import { s } from '@/lib/report-studio/css';
import type { RsVals } from './vals';
import { RsRibbonHome } from './rs-ribbon-home';
import { RsRibbonOther } from './rs-ribbon-other';

export function RsRibbon({ v }: { v: RsVals }) {
  return (
    <>
      <div style={s('display:flex;align-items:flex-end;gap:1px;height:30px;flex:0 0 30px;padding:0 8px;background:var(--ribbonbar,#eef1f5);border-bottom:1px solid var(--border,#e1e5ea)')}>
        {v.ribbonTabs.map((rt) => (
          <button key={rt.label} onClick={rt.onClick} style={s(rt.style)}>{rt.label}</button>
        ))}
      </div>
      <div style={s('height:92px;flex:0 0 92px;display:flex;align-items:stretch;padding:0 4px;background:var(--ribbon,#f7f8fa);border-bottom:1px solid var(--border,#d7dce3);overflow:hidden')}>
        {v.isHome && <RsRibbonHome v={v} />}
        {(v.isPage || v.isLayout || v.isView) && <RsRibbonOther v={v} />}
      </div>
    </>
  );
}

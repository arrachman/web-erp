'use client';

import { s } from '@/lib/report-studio/css';
import { useReportStudio } from './hooks/use-report-studio';
import { buildVals } from './vals';
import { RsQuickbar } from './rs-quickbar';
import { RsRibbon } from './rs-ribbon';
import { RsDocTabs } from './rs-doctabs';
import { RsLeftRail } from './rs-left-rail';
import { RsCanvas } from './rs-canvas';
import { RsPreview } from './rs-preview';
import { RsRightPanel } from './rs-right-panel';

const STYLE_TAG = `
@import url('https://fonts.googleapis.com/css2?family=IBM+Plex+Mono:wght@400;500&display=swap');
@keyframes rsIn{from{opacity:0;transform:translateY(6px)}to{opacity:1;transform:none}}
@keyframes rsPop{from{opacity:0;transform:translateY(-4px) scale(.98)}to{opacity:1;transform:none}}
.rs-root *{box-sizing:border-box}
.rs-root input,.rs-root select,.rs-root button{font-family:inherit}
.rs-root ::-webkit-scrollbar{width:11px;height:11px}
.rs-root ::-webkit-scrollbar-thumb{background:rgba(128,138,155,.4);border-radius:6px;border:2px solid transparent;background-clip:content-box}
.rs-root ::-webkit-scrollbar-thumb:hover{background:rgba(128,138,155,.7);background-clip:content-box}
.rs-root ::-webkit-scrollbar-track{background:transparent}
`;

/** ReportStudio — band-based report designer (full functional port of the mockup). */
export function ReportStudio() {
  const ctrl = useReportStudio();
  const v = buildVals(ctrl);
  return (
    <>
      <style>{STYLE_TAG}</style>
      <div className="rs-root" style={s(v.rootStyle)}>
        <RsQuickbar v={v} />
        <RsRibbon v={v} />
        <RsDocTabs v={v} />
        <div style={s('flex:1;display:flex;min-height:0')}>
          <RsLeftRail v={v} />
          <main style={s('flex:1;min-width:0;display:flex;flex-direction:column;background:var(--canvas,#c9ced6);min-height:0')}>
            {v.isDesign && <RsCanvas v={v} />}
            {v.isPreview && <RsPreview v={v} />}
          </main>
          <RsRightPanel v={v} />
        </div>
        <footer style={s("height:24px;flex:0 0 24px;display:flex;align-items:center;padding:0 14px;gap:14px;background:var(--titlebar,#11161f);color:#9aa3b2;font-size:11px;font-family:'IBM Plex Mono',monospace")}>
          <span>{v.statusLeft}</span>
          <span style={s('margin-left:auto')}>{v.statusRight}</span>
        </footer>
        {v.toastOn && (
          <div style={s('position:fixed;bottom:42px;left:50%;transform:translateX(-50%);background:#1d2330;color:#fff;padding:9px 18px;border-radius:9px;font-size:13px;font-weight:500;box-shadow:0 10px 30px rgba(0,0,0,.3);z-index:90;animation:rsIn .15s ease')}>{v.toast}</div>
        )}
      </div>
    </>
  );
}

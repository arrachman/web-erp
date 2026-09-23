'use client';

import * as React from 'react';
import { s } from '@/lib/report-studio/css';
import type { RsVals } from './vals';

export function RsCanvas({ v }: { v: RsVals }) {
  const scrollRef = React.useRef<HTMLDivElement>(null);
  // Latest handler in a ref so the native listener is attached only once.
  const wheelRef = React.useRef(v.onCanvasWheel);
  wheelRef.current = v.onCanvasWheel;
  React.useEffect(() => {
    const el = scrollRef.current;
    if (!el) return undefined;
    // Native non-passive listener: required so preventDefault() can block
    // the browser's ctrl+wheel page zoom while we zoom the canvas instead.
    const onWheel = (e: WheelEvent) => {
      if (!(e.ctrlKey || e.metaKey)) return;
      e.preventDefault();
      wheelRef.current(el, e.clientX, e.clientY, e.deltaY);
    };
    el.addEventListener('wheel', onWheel, { passive: false });
    return () => el.removeEventListener('wheel', onWheel);
  }, []);
  return (
    <div ref={scrollRef} style={s('flex:1;overflow:auto;min-height:0;padding:16px')}>
      <div style={s('display:grid;grid-template-columns:16px max-content;grid-template-rows:16px max-content;width:max-content')}>
        <div style={s('position:relative;z-index:6;background:var(--panel2,#eceff3);border-right:1px solid var(--border,#cfd5dd);border-bottom:1px solid var(--border,#cfd5dd)')} />
        <div style={s('position:relative;z-index:5')}>
          {v.rulerOn && (
            <div style={s(v.hRulerStyle)}>
              {v.rulerH.map((m) => <div key={m.n} style={s(m.style)}>{m.n}</div>)}
            </div>
          )}
        </div>
        <div style={s('position:relative;z-index:4')}>
          {v.rulerOn && (
            <div style={s(v.vRulerStyle)}>
              {v.rulerV.map((m) => <div key={m.n} style={s(m.style)}>{m.n}</div>)}
            </div>
          )}
        </div>
        <div style={s(v.paperWrapStyle)}>
          <div style={s(v.paperStyle)}>
            {v.guides.map((g, i) => <div key={i} style={s(g)} />)}
            {v.bands.map((b) => (
              <div key={b.id} data-band={b.id} onDrop={v.onCanvasDrop} onDragOver={v.allowDrop} onMouseDown={v.onBandMouseDown} style={s(b.style)}>
                <div data-band={b.id} onMouseDown={v.onBandLabelDown} style={s(b.tabStyle)}>{b.label}</div>
                {b.els.map((el) => (
                  <div key={el.id} data-id={el.id} onMouseDown={v.onElementMouseDown} style={s(el.boxStyle)}>
                    {el.display}
                    {el.selected && (
                      <div data-id={el.id} onMouseDown={v.onResizeMouseDown} style={s('position:absolute;right:-5px;bottom:-5px;width:10px;height:10px;background:#2563eb;border:1.5px solid #fff;border-radius:2px;cursor:nwse-resize;z-index:8')} />
                    )}
                  </div>
                ))}
                <div data-band={b.id} onMouseDown={v.onBandResizeDown} style={s('position:absolute;left:0;right:0;bottom:-2px;height:6px;cursor:ns-resize;z-index:7')} />
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

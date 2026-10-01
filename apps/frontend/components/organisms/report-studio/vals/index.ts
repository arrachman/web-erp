import type { RsCtrl } from '../hooks/use-report-studio';
import { palette, rootStyle } from '@/lib/report-studio/palette';
import { DATASOURCE_NAME, tr } from '@/lib/report-studio/i18n';
import { quickbarVals } from './quickbar';
import { ribbonVals } from './ribbon';
import { leftVals } from './left';
import { canvasVals } from './canvas';
import { rightVals } from './right';

/** Port of ReportStudio renderVals(): every computed style/handler the UI consumes. */
export function buildVals(c: RsCtrl) {
  const id = c.isId; const st = c.st;
  const pal = palette(st.theme, st.accent);
  const cv = canvasVals(c);
  const dsName = c.sqlActive ? 'Query · SQL' : DATASOURCE_NAME[st.tplKey];
  const statusLeft = dsName + '  ·  ' + c.rowCount + (id ? ' baris' : ' rows') + (st.snap ? '  ·  ' + (id ? 'rekat' : 'snap') + ' ' + st.grid + 'px' : '');
  const statusRight = Math.round(c.zoom * 100) + '%   ·   ' + st.pageSize.toUpperCase() + ' ' + (st.orient === 'landscape' ? '↔' : '↕') + '   ·   ' + c.pageW + '×' + cv.paperH;
  return {
    rootStyle: rootStyle(pal), t: tr(id),
    ...quickbarVals(c), ...ribbonVals(c), ...leftVals(c), ...cv, ...rightVals(c),
    statusLeft, statusRight, toast: st.toast, toastOn: !!st.toast,
  };
}

export type RsVals = ReturnType<typeof buildVals>;

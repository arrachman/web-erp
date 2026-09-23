import {
  DesignerAction,
  DesignerState,
  RptBand,
  RptComponent,
  RptTemplate,
} from './report-types';

export function buildDefaultTemplate(): RptTemplate {
  return {
    name: 'Laporan Baru',
    module: 'sys',
    pageSize: 'A4',
    orientation: 'portrait',
    margins: { top: 10, right: 10, bottom: 10, left: 10 },
    dataSources: [],
    params: [],
    bands: [
      { id: 'ph', type: 'pageHeader', height: 30, components: [] },
      { id: 'db', type: 'data', height: 8, canGrow: false, minRows: 0, components: [] },
      { id: 'pf', type: 'pageFooter', height: 8, components: [] },
    ],
  };
}

export const INITIAL_STATE: DesignerState = {
  template: buildDefaultTemplate(),
  selection: { type: null },
  isDirty: false,
  zoom: 1.3,
  leftTab: 'data',
  leftOpen: true,
  rightOpen: true,
  previewOpen: false,
  past: [],
  future: [],
};

const HISTORY_LIMIT = 50;

/** Commit perubahan template + dorong snapshot lama ke `past`, kosongkan `future`. */
function commit(state: DesignerState, template: RptTemplate): DesignerState {
  const past = [...state.past, state.template].slice(-HISTORY_LIMIT);
  return { ...state, template, isDirty: true, past, future: [] };
}

function patchBand(bands: RptBand[], bandId: string, patch: Partial<RptBand>): RptBand[] {
  return bands.map(b => b.id === bandId ? { ...b, ...patch } : b);
}

function patchComponent(
  bands: RptBand[],
  bandId: string,
  componentId: string,
  patch: Partial<RptComponent>,
): RptBand[] {
  return bands.map(b =>
    b.id !== bandId ? b : {
      ...b,
      components: b.components.map(c =>
        c.id === componentId ? ({ ...c, ...patch } as RptComponent) : c,
      ),
    },
  );
}

export function designerReducer(state: DesignerState, action: DesignerAction): DesignerState {
  const t = state.template;

  switch (action.type) {
    case 'SET_TEMPLATE':
      return { ...state, template: action.template, isDirty: false, selection: { type: null }, past: [], future: [] };

    case 'SELECT_BAND':
      return { ...state, selection: { type: 'band', bandId: action.bandId } };

    case 'SELECT_COMPONENT':
      return { ...state, selection: { type: 'component', bandId: action.bandId, componentId: action.componentId, componentIds: [action.componentId] } };

    case 'SELECT_COMPONENTS':
      if (!action.componentIds.length) return { ...state, selection: { type: null } };
      return {
        ...state,
        selection: {
          type: 'component', bandId: action.bandId,
          componentId: action.componentIds[action.componentIds.length - 1],
          componentIds: action.componentIds,
        },
      };

    case 'TOGGLE_COMPONENT': {
      const s = state.selection;
      const same = s.type === 'component' && s.bandId === action.bandId;
      const ids = new Set(same ? (s.componentIds ?? (s.componentId ? [s.componentId] : [])) : []);
      if (ids.has(action.componentId)) ids.delete(action.componentId); else ids.add(action.componentId);
      const arr = [...ids];
      if (!arr.length) return { ...state, selection: { type: null } };
      return { ...state, selection: { type: 'component', bandId: action.bandId, componentId: arr[arr.length - 1], componentIds: arr } };
    }

    case 'DESELECT':
      return { ...state, selection: { type: null } };

    case 'ADD_BAND':
      return commit(state, { ...t, bands: [...t.bands, action.band] });

    case 'UPDATE_BAND': {
      const next = { ...t, bands: patchBand(t.bands, action.bandId, action.patch) };
      // transient (mis. drag-resize) → ubah template tanpa snapshot history
      return action.transient
        ? { ...state, template: next, isDirty: true }
        : commit(state, next);
    }

    case 'REMOVE_BAND': {
      const next = t.bands.filter(b => b.id !== action.bandId);
      const sel = state.selection.bandId === action.bandId ? { type: null } : state.selection;
      return { ...commit(state, { ...t, bands: next }), selection: sel };
    }

    case 'MOVE_BAND': {
      const idx = t.bands.findIndex(b => b.id === action.bandId);
      if (idx < 0) return state;
      const newIdx = action.direction === 'up' ? idx - 1 : idx + 1;
      if (newIdx < 0 || newIdx >= t.bands.length) return state;
      const bands = [...t.bands];
      [bands[idx], bands[newIdx]] = [bands[newIdx], bands[idx]];
      return commit(state, { ...t, bands });
    }

    case 'ADD_COMPONENT':
      return commit(state, {
        ...t, bands: t.bands.map(b => b.id !== action.bandId ? b : { ...b, components: [...b.components, action.component] }),
      });

    case 'ADD_COMPONENTS':
      return commit(state, {
        ...t, bands: t.bands.map(b => b.id !== action.bandId ? b : { ...b, components: [...b.components, ...action.components] }),
      });

    case 'PATCH_COMPONENTS': {
      const map = new Map(action.patches.map(p => [p.id, p.patch]));
      const next = {
        ...t,
        bands: t.bands.map(b => b.id !== action.bandId ? b : {
          ...b,
          components: b.components.map(c => map.has(c.id) ? ({ ...c, ...map.get(c.id) } as RptComponent) : c),
        }),
      };
      return action.transient ? { ...state, template: next, isDirty: true } : commit(state, next);
    }

    case 'REMOVE_SELECTED': {
      const s = state.selection;
      if (s.type !== 'component' || !s.bandId) return state;
      const ids = new Set(s.componentIds ?? (s.componentId ? [s.componentId] : []));
      if (!ids.size) return state;
      const next = {
        ...t,
        bands: t.bands.map(b => b.id !== s.bandId ? b : { ...b, components: b.components.filter(c => !ids.has(c.id)) }),
      };
      return { ...commit(state, next), selection: { type: null } };
    }

    case 'UPDATE_COMPONENT': {
      const next = { ...t, bands: patchComponent(t.bands, action.bandId, action.componentId, action.patch) };
      return action.transient
        ? { ...state, template: next, isDirty: true }
        : commit(state, next);
    }

    case 'REMOVE_COMPONENT': {
      const sel = state.selection.componentId === action.componentId ? { type: null } : state.selection;
      const next = {
        ...t, bands: t.bands.map(b =>
          b.id !== action.bandId ? b : { ...b, components: b.components.filter(c => c.id !== action.componentId) },
        ),
      };
      return { ...commit(state, next), selection: sel };
    }

    case 'ADD_DATASOURCE':
      return commit(state, { ...t, dataSources: [...t.dataSources, action.ds] });

    case 'UPDATE_DATASOURCE':
      return commit(state, { ...t, dataSources: t.dataSources.map(d => d.id !== action.dsId ? d : { ...d, ...action.patch }) });

    case 'REMOVE_DATASOURCE':
      return commit(state, { ...t, dataSources: t.dataSources.filter(d => d.id !== action.dsId) });

    case 'SET_ZOOM':
      return { ...state, zoom: action.zoom };

    case 'SET_LEFT_TAB':
      return { ...state, leftTab: action.tab, leftOpen: true };

    case 'TOGGLE_LEFT':
      return { ...state, leftOpen: action.open ?? !state.leftOpen };

    case 'TOGGLE_RIGHT':
      return { ...state, rightOpen: action.open ?? !state.rightOpen };

    case 'TOGGLE_PREVIEW':
      return { ...state, previewOpen: action.open ?? !state.previewOpen };

    case 'PUSH_HISTORY':
      return { ...state, past: [...state.past, state.template].slice(-HISTORY_LIMIT), future: [] };

    case 'UNDO': {
      if (!state.past.length) return state;
      const prev = state.past[state.past.length - 1];
      return {
        ...state,
        template: prev,
        past: state.past.slice(0, -1),
        future: [state.template, ...state.future].slice(0, HISTORY_LIMIT),
        isDirty: true,
        selection: { type: null },
      };
    }

    case 'REDO': {
      if (!state.future.length) return state;
      const nextT = state.future[0];
      return {
        ...state,
        template: nextT,
        past: [...state.past, state.template].slice(-HISTORY_LIMIT),
        future: state.future.slice(1),
        isDirty: true,
        selection: { type: null },
      };
    }

    case 'MARK_CLEAN':
      return { ...state, isDirty: false };

    default:
      return state;
  }
}

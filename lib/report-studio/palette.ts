import type { RsTheme } from './types';

export interface RsPalette {
  bg: string; panel: string; panel2: string; border: string; text: string; muted: string;
  accent: string; accentWeak: string; canvas: string; hover: string;
  titlebar: string; ribbon: string; ribbonbar: string;
}

export function palette(theme: RsTheme, accent: string): RsPalette {
  if (theme === 'dark') return {
    bg: '#0d0f13', panel: '#15181f', panel2: '#1b1f27', border: '#272c36', text: '#e4e8f0', muted: '#8b93a3',
    accent, accentWeak: '#16263f', canvas: '#06080b', hover: '#1e2430', titlebar: '#05070a', ribbon: '#12151c', ribbonbar: '#0e1117',
  };
  return {
    bg: '#eef0f3', panel: '#ffffff', panel2: '#f5f6f9', border: '#e1e5ea', text: '#1d2330', muted: '#6b7280',
    accent, accentWeak: '#e7efff', canvas: '#c9ced6', hover: '#eef2f8', titlebar: '#11161f', ribbon: '#fafbfc', ribbonbar: '#eef1f5',
  };
}

/** CSS-variable + base layout string applied to the root container. */
export function rootStyle(pal: RsPalette): string {
  return '--bg:' + pal.bg + ';--panel:' + pal.panel + ';--panel2:' + pal.panel2 + ';--border:' + pal.border
    + ';--text:' + pal.text + ';--muted:' + pal.muted + ';--accent:' + pal.accent + ';--accent-weak:' + pal.accentWeak
    + ';--canvas:' + pal.canvas + ';--hover:' + pal.hover + ';--titlebar:' + pal.titlebar + ';--ribbon:' + pal.ribbon
    + ';--ribbonbar:' + pal.ribbonbar + ";height:100%;min-height:0;display:flex;flex-direction:column;background:var(--bg);color:var(--text);font-family:var(--font-sans,'IBM Plex Sans',system-ui,sans-serif);overflow:hidden";
}

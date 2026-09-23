'use client';

import * as React from 'react';
import { cn } from '@/lib/utils';
import { Icon } from '@/components/ui/icons';
import {
  isNavGroupArray,
  type NavItem,
  type NavLeaf,
} from '@/lib/nav';

interface SidebarProps {
  nav: NavItem[];
  current: string;
  onNavigate: (id: string) => void;
  t: (key: string) => string;
  workspaceId?: string;
  sidebarMenuMode?: 'flyout' | 'accordion';
  sidebarMode?: 'icon' | 'label' | 'horizontal';
}

type SidebarMode = SidebarProps['sidebarMode'];

/** Minimum visible width (px) of the horizontal flyout when clamped near the right viewport edge. */
const H_FLYOUT_MIN_VISIBLE = 180;

/** Gap (px) between the horizontal nav bar and its fixed dropdown submenu. */
const H_FLYOUT_GAP = 4;

/** Builds a navigable href for a route id so browsers can offer right-click / Ctrl+click. */
function leafHref(id: string, workspaceId?: string): string {
  const base = workspaceId ? `/${workspaceId}` : '';
  return id.startsWith('/') ? `${base}${id}` : `${base}/${id}`;
}

/** Icon-only nav rail with a hover flyout submenu — ported from `sidebar.jsx`. */
export function Sidebar({ nav, current, onNavigate, t, workspaceId, sidebarMenuMode = 'flyout', sidebarMode = 'icon' }: SidebarProps) {
  const isHorizontal = sidebarMode === 'horizontal';
  const [open, setOpen] = React.useState<string | null>(null);
  const [openTop, setOpenTop] = React.useState(0);
  // Horizontal hover-dropdown pos (viewport coords) — `position: fixed`
  // escapes the nav bar's overflow-x scroll clip that would clip `absolute`.
  const [hFly, setHFly] = React.useState({ top: 0, left: 0 });
  const timer = React.useRef<ReturnType<typeof setTimeout> | null>(null);

  // Accordion state — tracks which module is expanded
  const [collapsedGroups, setCollapsedGroups] = React.useState<Set<string>>(() => {
    const initial = new Set<string>();
    for (const item of nav) {
      if (!item.children || !isNavGroupArray(item.children)) continue;
      const hasActiveGroup = item.children.some((grp) => grp.items.some((s) => s.id === current));
      item.children.forEach((grp, gi) => {
        const isActive = grp.items.some((s) => s.id === current);
        // Collapse non-active groups; fallback to keeping only the first open when no match
        if (hasActiveGroup ? !isActive : gi > 0) {
          initial.add(`${item.id}__${grp.group}`);
        }
      });
    }
    return initial;
  });

  // When nav swaps from static fallback to API nav: recompute from scratch so
  // the active route's group is expanded and siblings are collapsed.
  const navRef = React.useRef<NavItem[]>(nav);
  React.useEffect(() => {
    const navChanged = navRef.current !== nav;
    navRef.current = nav;

    if (navChanged) {
      const next = new Set<string>();
      for (const item of nav) {
        if (!item.children || !isNavGroupArray(item.children)) continue;
        const hasActiveGroup = item.children.some((grp) => grp.items.some((s) => s.id === current));
        item.children.forEach((grp, gi) => {
          const isActive = grp.items.some((s) => s.id === current);
          if (hasActiveGroup ? !isActive : gi > 0) next.add(`${item.id}__${grp.group}`);
        });
      }
      setCollapsedGroups(next);
    } else {
      // current changed: only open the active group, leave others as-is
      setCollapsedGroups((prev) => {
        let changed = false;
        const next = new Set(prev);
        for (const item of nav) {
          if (!item.children || !isNavGroupArray(item.children)) continue;
          for (const grp of item.children) {
            const key = `${item.id}__${grp.group}`;
            if (grp.items.some((s) => s.id === current) && next.has(key)) {
              next.delete(key);
              changed = true;
            }
          }
        }
        return changed ? next : prev;
      });
    }
  }, [nav, current]);

  const toggleGroup = (key: string) => {
    setCollapsedGroups((prev) => {
      const next = new Set(prev);
      if (next.has(key)) next.delete(key);
      else next.add(key);
      return next;
    });
  };

  const currentTop = nav.find(
    (i) =>
      !i.divider &&
      (i.id === current ||
        (i.children &&
          (isNavGroupArray(i.children)
            ? i.children.some((g) => g.items.some((s) => s.id === current))
            : i.children.some((c) => c.id === current)))),
  );
  const [expandedId, setExpandedId] = React.useState<string | null>(() =>
    // Horizontal bar is hover-driven — never auto-open on load (accordion keeps auto-expand).
    sidebarMode === 'horizontal' ? null : (currentTop?.id ?? null),
  );

  // Close expanded horizontal submenu when clicking outside the sidebar.
  const navElRef = React.useRef<HTMLElement | null>(null);
  React.useEffect(() => {
    if (!isHorizontal) return;
    const onOutside = (e: MouseEvent) => {
      if (navElRef.current && !navElRef.current.contains(e.target as Node)) setExpandedId(null);
    };
    document.addEventListener('click', onOutside);
    return () => document.removeEventListener('click', onOutside);
  }, [isHorizontal]);

  // When navigating to a different module, auto-expand that module in accordion mode
  React.useEffect(() => {
    if (!isHorizontal && sidebarMenuMode === 'accordion' && currentTop?.id) {
      setExpandedId(currentTop.id);
    }
  }, [currentTop?.id, sidebarMenuMode, isHorizontal]);

  // Switching to the horizontal menu bar at runtime: close any expanded dropdown.
  React.useEffect(() => {
    if (isHorizontal) setExpandedId(null);
  }, [isHorizontal]);

  // Flyout handlers
  const handleEnter = (e: React.MouseEvent<HTMLElement>, item: NavItem) => {
    if (sidebarMode === 'horizontal' || sidebarMenuMode === 'accordion' || !item.children) {
      setOpen(null);
      return;
    }
    if (timer.current) clearTimeout(timer.current);
    const rect = e.currentTarget.getBoundingClientRect();
    setOpenTop(rect.top);
    setOpen(item.id ?? null);
  };
  const handleLeaveAll = () => {
    timer.current = setTimeout(() => setOpen(null), 120);
  };
  const keepOpen = () => {
    if (timer.current) clearTimeout(timer.current);
  };

  const openItem = nav.find((i) => i.id === open);

  const renderLeaf = (sub: NavLeaf) => (
    <a
      key={sub.label}
      href={leafHref(sub.id, workspaceId)}
      className={cn('flyout-item', sub.id === current && 'active')}
      onClick={(e) => {
        if (!e.ctrlKey && !e.metaKey && !e.shiftKey && e.button === 0) {
          e.preventDefault();
          onNavigate(sub.id);
          setOpen(null);
        }
      }}
    >
      <Icon name="dot" size={8} />
      <span>{t(sub.label)}</span>
    </a>
  );

  const renderAccordionLeaf = (sub: NavLeaf) => (
    <a
      key={sub.label}
      href={leafHref(sub.id, workspaceId)}
      className={cn('accordion-item', sub.id === current && 'active')}
      onClick={(e) => {
        if (!e.ctrlKey && !e.metaKey && !e.shiftKey && e.button === 0) {
          e.preventDefault();
          onNavigate(sub.id);
        }
      }}
    >
      <Icon name="dot" size={8} />
      <span>{t(sub.label)}</span>
    </a>
  );

  const renderAccordionChildren = (item: NavItem) => {
    if (!item.children) return null;
    if (isNavGroupArray(item.children)) {
      return item.children.map((grp) => {
        const key = `${item.id}__${grp.group}`;
        const isCollapsed = collapsedGroups.has(key);
        return (
          <div key={grp.group} className="accordion-group">
            <div className="accordion-group-label" onClick={() => toggleGroup(key)}>
              <span>{t(grp.group)}</span>
              <Icon name={isCollapsed ? 'chevdown' : 'chevup'} size={10} stroke={1.6} style={{ opacity: 0.5 }} />
            </div>
            {!isCollapsed && grp.items.map(renderAccordionLeaf)}
          </div>
        );
      });
    }
    return item.children.map(renderAccordionLeaf);
  };

  return (
    <>
      <nav
        className="sidebar"
        ref={navElRef}
        onMouseLeave={sidebarMode === 'horizontal' || sidebarMenuMode === 'flyout' ? handleLeaveAll : undefined}
        style={sidebarMode === 'horizontal' ? { overflowX: 'auto', overflowY: 'hidden', scrollbarGutter: 'stable' } : sidebarMenuMode === 'accordion' ? { overflowY: 'auto', scrollbarGutter: 'stable' } : undefined}
      >
        {nav.map((item, i) => {
          if (item.divider)
            return <div key={`div-${i}`} className="nav-divider" />;

          const isActive = !!currentTop && currentTop.id === item.id;

          // Leaf top-level items (no children) become real links.
          if (!item.children && item.id) {
            return (
              <a
                key={item.id}
                href={leafHref(item.id, workspaceId)}
                className={cn('nav-item', isActive && 'active')}
                data-tip={t(item.label ?? '')}
                onMouseEnter={(e) => handleEnter(e, item)}
                onClick={(e) => {
                  if (sidebarMode === 'horizontal') {
                    e.preventDefault();
                    onNavigate(item.id!);
                    return;
                  }
                  if (!e.ctrlKey && !e.metaKey && !e.shiftKey && e.button === 0) {
                    e.preventDefault();
                    onNavigate(item.id!);
                  }
                }}
              >
                {item.icon && <Icon name={item.icon} size={16} stroke={1.6} />}
                <span className="nav-label">{t(item.label ?? '')}</span>
              </a>
            );
          }

// Horizontal mode: top bar with hover dropdown. Rendered `position: fixed`
          // at viewport coords to escape the nav bar's overflow-x scroll clip.
          if (isHorizontal) {
            const isExpanded = expandedId === item.id;
            return (
              <div
                key={item.id}
                className={cn('nav-item', isActive && 'active')}
                style={{ cursor: 'pointer' }}
                onMouseEnter={(e) => {
                  if (item.children) {
                    if (timer.current) clearTimeout(timer.current);
                    const rect = e.currentTarget.getBoundingClientRect();
                    const left = Math.max(8, Math.min(rect.left, window.innerWidth - H_FLYOUT_MIN_VISIBLE - 8));
                    setHFly({ top: rect.bottom, left });
                    setExpandedId(item.id ?? null);
                  }
                }}
                onMouseLeave={() => {
                  if (item.children) {
                    timer.current = setTimeout(() => setExpandedId(null), 120);
                  }
                }}
                onClick={() => {
                  if (!item.children) onNavigate(item.id!);
                }}
              >
                {item.icon && <Icon name={item.icon} size={16} stroke={1.6} />}
                <span className="nav-label">{t(item.label ?? '')}</span>
                {item.children && (
                  <Icon name={isExpanded ? 'chevup' : 'chevdown'} size={12} stroke={1.6} style={{ opacity: 0.5, flexShrink: 0 }} />
                )}
                {isExpanded && item.children && (
                  <div
                    className="accordion-submenu"
                    style={{
                      position: 'fixed',
                      top: hFly.top + H_FLYOUT_GAP,
                      left: hFly.left,
                      maxHeight: `calc(100vh - ${hFly.top + H_FLYOUT_GAP + 8}px)`,
                      maxWidth: `calc(100vw - ${hFly.left}px - 8px)`,
                    }}
                    onMouseEnter={keepOpen}
                    onMouseLeave={handleLeaveAll}
                  >
                    {renderAccordionChildren(item)}
                  </div>
                )}
              </div>
            );
          }

          // Accordion mode: click to expand/collapse inline
          if (sidebarMenuMode === 'accordion' && item.children) {
            const isExpanded = expandedId === item.id;
            return (
              <React.Fragment key={item.id}>
                <div
                  className={cn('nav-item', isActive && 'active')}
                  data-tip={t(item.label ?? '')}
                  style={{ cursor: 'pointer' }}
                  onClick={() => setExpandedId(isExpanded ? null : (item.id ?? null))}
                >
                  {item.icon && <Icon name={item.icon} size={16} stroke={1.6} />}
                  <span className="nav-label" style={{ flex: 1 }}>{t(item.label ?? '')}</span>
                  <Icon name={isExpanded ? 'chevup' : 'chevdown'} size={12} stroke={1.6} style={{ opacity: 0.5, flexShrink: 0 }} />
                </div>
                {isExpanded && (
                  <div className="accordion-submenu">
                    {renderAccordionChildren(item)}
                  </div>
                )}
              </React.Fragment>
            );
          }

          // Flyout mode: items with children open a flyout on hover.
          if ((sidebarMode as SidebarMode) === 'horizontal') {
            return null;
          }
          return (
            <div
              key={item.id}
              className={cn('nav-item', isActive && 'active')}
              data-tip={t(item.label ?? '')}
              onMouseEnter={(e) => handleEnter(e, item)}
            >
              {item.icon && <Icon name={item.icon} size={16} stroke={1.6} />}
              <span className="nav-label">{t(item.label ?? '')}</span>
            </div>
          );
        })}
        <div style={{ flex: 1 }} />
        <div
          className="nav-item"
          data-tip={t('Pintasan')}
          onClick={() =>
            window.dispatchEvent(new CustomEvent('open-shortcuts'))
          }
        >
          <Icon name="keyboard" size={16} />
          <span className="nav-label">{t('Pintasan')}</span>
        </div>
      </nav>
      {sidebarMode === 'horizontal' ? null : sidebarMenuMode === 'flyout' && openItem && openItem.children && (
        <div
          className="flyout fade-in"
          style={{
            top: Math.max(8, openTop),
            maxHeight: `calc(100vh - ${Math.max(8, openTop)}px - 8px)`,
          }}
          onMouseEnter={keepOpen}
          onMouseLeave={handleLeaveAll}
        >
          <div className="group-label">
            <span>{t(openItem.label ?? '')}</span>
            {openItem.icon && <Icon name={openItem.icon} size={12} />}
          </div>
          {isNavGroupArray(openItem.children)
            ? openItem.children.map((grp) => (
                <div key={grp.group}>
                  <div className="group-label">
                    <span>{t(grp.group)}</span>
                  </div>
                  {grp.items.map(renderLeaf)}
                </div>
              ))
            : openItem.children.map(renderLeaf)}
        </div>
      )}
    </>
  );
}

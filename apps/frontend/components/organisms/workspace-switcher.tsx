'use client';

import * as React from 'react';
import { useRouter } from 'next/navigation';
import { Icon } from '@/components/ui/icons';
import {
  listWorkspaces,
  createWorkspace,
  deleteWorkspace,
  nextWorkspaceId,
  type WorkspaceMeta,
} from '@/lib/workspace';
import { GLOBAL_BASE_PATH } from '@/lib/shell-constants';

interface WorkspaceSwitcherProps {
  /** Undefined = global (no-workspace) mode. */
  workspaceId?: string;
  workspaceName?: string;
  /** Called after creating a new workspace so the parent can re-read state. */
  onWorkspaceChange?: () => void;
  t: (key: string) => string;
}

export function WorkspaceSwitcher({
  workspaceId,
  workspaceName,
  onWorkspaceChange,
  t,
}: WorkspaceSwitcherProps) {
  const router = useRouter();
  const [open, setOpen] = React.useState(false);
  const [workspaces, setWorkspaces] = React.useState<WorkspaceMeta[]>([]);
  const ref = React.useRef<HTMLDivElement>(null);
  const isGlobal = !workspaceId;

  // Load workspace list when dropdown opens.
  React.useEffect(() => {
    if (open) setWorkspaces(listWorkspaces());
  }, [open]);

  // Close on outside click.
  React.useEffect(() => {
    const fn = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener('mousedown', fn);
    return () => document.removeEventListener('mousedown', fn);
  }, []);

  const navigateToWorkspace = (id: string) => {
    setOpen(false);
    if (id !== workspaceId) router.push(`/${id}`);
  };

  const navigateToGlobal = () => {
    setOpen(false);
    if (!isGlobal) router.push(GLOBAL_BASE_PATH);
  };

  const handleNew = () => {
    const id = nextWorkspaceId();
    createWorkspace(id);
    setOpen(false);
    onWorkspaceChange?.();
    router.push(`/${id}`);
  };

  const handleDelete = (e: React.MouseEvent, id: string) => {
    e.stopPropagation();
    const remaining = workspaces.filter((w) => w.id !== id);
    if (remaining.length === 0) return; // jangan hapus satu-satunya workspace
    deleteWorkspace(id);
    setWorkspaces(remaining);
    if (id === workspaceId) {
      router.push(GLOBAL_BASE_PATH);
    }
  };

  return (
    <div className="ws-switcher" ref={ref}>
      <button
        className="ws-pill"
        onClick={() => setOpen((o) => !o)}
        title={t('Ganti workspace')}
      >
        <Icon name="layers" size={12} />
        <span className="ws-name">{isGlobal ? t('Global') : workspaceName}</span>
        <Icon name="chevdown" size={11} />
      </button>

      {open && (
        <div className="ws-dropdown fade-in">
          <div className="ws-dropdown-hd">{t('Workspace')}</div>

          <button
            className={`ws-item${isGlobal ? ' active' : ''}`}
            onClick={navigateToGlobal}
          >
            <Icon name="database" size={12} />
            <span className="ws-item-name">{t('Global')}</span>
            {isGlobal && <span className="ws-item-badge">{t('aktif')}</span>}
          </button>

          {workspaces.length > 0 && <div className="ws-dropdown-sep" />}

          {workspaces.map((ws) => (
            <button
              key={ws.id}
              className={`ws-item${ws.id === workspaceId ? ' active' : ''}`}
              onClick={() => navigateToWorkspace(ws.id)}
            >
              <Icon name="layers" size={12} />
              <span className="ws-item-name">{ws.name}</span>
              {ws.id === workspaceId && (
                <span className="ws-item-badge">{t('aktif')}</span>
              )}
              {workspaces.length > 1 && (
                <button
                  className="ws-item-del"
                  title={t('Hapus workspace')}
                  onClick={(e) => handleDelete(e, ws.id)}
                >
                  <Icon name="x" size={11} />
                </button>
              )}
            </button>
          ))}

          <div className="ws-dropdown-sep" />

          <button className="ws-item ws-item-new" onClick={handleNew}>
            <Icon name="plus" size={12} />
            <span>{t('Buat Workspace')}</span>
          </button>
        </div>
      )}
    </div>
  );
}

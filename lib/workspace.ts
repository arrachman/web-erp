/** Workspace state types and localStorage persistence helpers. */

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export type WorkspaceMeta = {
  id: string;
  name: string;
  createdAt: string;
};

/** Serialisable subset of ShellTab — tanpa nonce (runtime only). */
export type PersistedTab = {
  id: string;
  route: string;
};

export type WorkspaceState = {
  meta: WorkspaceMeta;
  tabs: PersistedTab[];
  activeTabId: string;
};

// ---------------------------------------------------------------------------
// Storage keys
// ---------------------------------------------------------------------------

const WS_LIST_KEY = 'erp-ws-list';
const WS_LAST_KEY = 'erp-ws-last';
const wsKey = (id: string) => `erp-ws-${id}`;

// ---------------------------------------------------------------------------
// Internal helpers
// ---------------------------------------------------------------------------

function read<T>(key: string): T | null {
  try {
    const raw = localStorage.getItem(key);
    return raw ? (JSON.parse(raw) as T) : null;
  } catch {
    return null;
  }
}

function write(key: string, value: unknown): void {
  try {
    localStorage.setItem(key, JSON.stringify(value));
  } catch {
    // storage full or unavailable — fail silently
  }
}

function workspaceLabel(id: string): string {
  const num = id.replace(/^ws/, '');
  return num ? `Workspace ${num}` : id;
}

// ---------------------------------------------------------------------------
// Public API
// ---------------------------------------------------------------------------

export function listWorkspaceIds(): string[] {
  return read<string[]>(WS_LIST_KEY) ?? [];
}

export function listWorkspaces(): WorkspaceMeta[] {
  return listWorkspaceIds()
    .map((id) => read<WorkspaceState>(wsKey(id))?.meta)
    .filter((m): m is WorkspaceMeta => m !== undefined);
}

export function getWorkspace(id: string): WorkspaceState | null {
  return read<WorkspaceState>(wsKey(id));
}

export function saveWorkspace(state: WorkspaceState): void {
  const ids = listWorkspaceIds();
  if (!ids.includes(state.meta.id)) {
    write(WS_LIST_KEY, [...ids, state.meta.id]);
  }
  write(wsKey(state.meta.id), state);
}

export function createWorkspace(
  id: string,
  name?: string,
  initialRoute = 'home',
): WorkspaceState {
  const state: WorkspaceState = {
    meta: { id, name: name ?? workspaceLabel(id), createdAt: new Date().toISOString() },
    tabs: [{ id: 't1', route: initialRoute }],
    activeTabId: 't1',
  };
  saveWorkspace(state);
  return state;
}

/** Returns existing workspace or creates a fresh one. */
export function getOrCreateWorkspace(id: string): WorkspaceState {
  return getWorkspace(id) ?? createWorkspace(id);
}

export function deleteWorkspace(id: string): void {
  write(WS_LIST_KEY, listWorkspaceIds().filter((i) => i !== id));
  try {
    localStorage.removeItem(wsKey(id));
  } catch {
    // noop
  }
}

/** Generates the next available wsN id (ws1, ws2, ...). */
export function nextWorkspaceId(): string {
  const ids = listWorkspaceIds();
  let n = 1;
  while (ids.includes(`ws${n}`)) n++;
  return `ws${n}`;
}

export function getLastActiveId(): string | null {
  return read<string>(WS_LAST_KEY);
}

export function setLastActive(id: string): void {
  write(WS_LAST_KEY, id);
}

/** Clears all erp-ws-* keys from localStorage. Called on logout. */
export function clearAllWorkspaces(): void {
  try {
    Object.keys(localStorage)
      .filter((k) => k.startsWith('erp-ws'))
      .forEach((k) => localStorage.removeItem(k));
  } catch {
    // noop
  }
}

import type { Workspace } from '@life/types';
import type { SecureKeyValueStore } from '../sessionTokenStorage/sessionTokenStorage';
import type { WorkspaceService } from '../workspaceService/workspaceService';

export const ACTIVE_WORKSPACE_KEY = 'life.activeWorkspace';

export type WorkspaceState = {
  status: 'idle' | 'loading' | 'ready' | 'error';
  workspaces: Workspace[];
  activeId: string | null;
};

export type WorkspaceStore = {
  getState: () => WorkspaceState;
  load: (service: WorkspaceService) => Promise<void>;
  select: (id: string) => Promise<void>;
  reset: () => void;
  subscribe: (listener: () => void) => () => void;
};

export function resolveActiveWorkspace(
  workspaces: Workspace[],
  savedId: string | null,
): Workspace | null {
  return (
    workspaces.find((workspace) => workspace.id === savedId) ??
    workspaces.find((workspace) => workspace.type === 'Personal') ??
    null
  );
}

export function createWorkspaceStore(storage: SecureKeyValueStore): WorkspaceStore {
  let state: WorkspaceState = { status: 'idle', workspaces: [], activeId: null };
  const listeners = new Set<() => void>();

  function update(next: Partial<WorkspaceState>) {
    state = { ...state, ...next };
    listeners.forEach((listener) => listener());
  }

  return {
    getState: () => state,
    async load(service) {
      update({ status: 'loading' });
      try {
        const workspaces = await service.list();
        const saved = await storage.get(ACTIVE_WORKSPACE_KEY);
        const active = resolveActiveWorkspace(workspaces, saved);
        update({ status: 'ready', workspaces, activeId: active?.id ?? null });
        if (active) await storage.set(ACTIVE_WORKSPACE_KEY, active.id);
      } catch {
        update({ status: 'error' });
      }
    },
    async select(id) {
      if (!state.workspaces.some((workspace) => workspace.id === id)) return;
      update({ activeId: id });
      await storage.set(ACTIVE_WORKSPACE_KEY, id);
    },
    reset: () => update({ status: 'idle', workspaces: [], activeId: null }),
    subscribe(listener) {
      listeners.add(listener);
      return () => {
        listeners.delete(listener);
      };
    },
  };
}

import { useEffect, useSyncExternalStore } from 'react';
import type { WorkspaceService } from '../workspaceService/workspaceService';
import type { WorkspaceStore } from '../workspaceStore/workspaceStore';

export function useWorkspaces(store: WorkspaceStore, service: WorkspaceService) {
  const state = useSyncExternalStore(store.subscribe, store.getState, store.getState);

  useEffect(() => {
    if (store.getState().status === 'idle') void store.load(service);
  }, [store, service]);

  return { ...state, select: store.select };
}

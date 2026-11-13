export type ConnectivityState = {
  // Hay red y el servidor responde.
  online: boolean;
};

export type ConnectivityStatus = 'offline' | 'syncing' | 'synced';

export type ConnectivityStore = {
  getState: () => ConnectivityState;
  setOnline: (online: boolean) => void;
  subscribe: (listener: () => void) => () => void;
};

export function createConnectivityStore(
  initial: Partial<ConnectivityState> = {},
): ConnectivityStore {
  let state: ConnectivityState = { online: true, ...initial };
  const listeners = new Set<() => void>();

  return {
    getState: () => state,
    setOnline(online) {
      if (state.online === online) return;
      state = { ...state, online };
      listeners.forEach((listener) => listener());
    },
    subscribe(listener) {
      listeners.add(listener);
      return () => {
        listeners.delete(listener);
      };
    },
  };
}

// Sin conexión manda sobre todo; con conexión, las operaciones pendientes indican que se sincroniza.
export function resolveConnectivityStatus(
  online: boolean,
  pendingOperations: number,
): ConnectivityStatus {
  if (!online) return 'offline';
  return pendingOperations > 0 ? 'syncing' : 'synced';
}

export const connectivityStore = createConnectivityStore();

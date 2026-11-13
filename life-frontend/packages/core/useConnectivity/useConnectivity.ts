import { useSyncExternalStore } from 'react';
import {
  connectivityStore,
  resolveConnectivityStatus,
} from '../connectivityStore/connectivityStore';
import type { ConnectivityStatus, ConnectivityStore } from '../connectivityStore/connectivityStore';
import { session } from '../sessionStore/sessionStore';
import type { SessionStore } from '../sessionStore/sessionStore';

export type Connectivity = { online: boolean; status: ConnectivityStatus };

export function useConnectivity(
  store: ConnectivityStore = connectivityStore,
  sessionStore: SessionStore = session,
): Connectivity {
  const { online } = useSyncExternalStore(store.subscribe, store.getState, store.getState);
  const { pendingOperations } = useSyncExternalStore(
    sessionStore.subscribe,
    sessionStore.getState,
    sessionStore.getState,
  );

  return { online, status: resolveConnectivityStatus(online, pendingOperations) };
}

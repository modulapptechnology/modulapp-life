import { useSyncExternalStore } from 'react';
import type { SessionStore } from '../sessionStore/sessionStore';

export function useSession(store: SessionStore) {
  return useSyncExternalStore(store.subscribe, store.getState, store.getState);
}

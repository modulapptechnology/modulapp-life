import { useEffect, useSyncExternalStore } from 'react';
import type { NotificationPreferencesStore } from '../notificationPreferences/notificationPreferencesStore';

// Carga las preferencias la primera vez que se usa y se actualiza con cada cambio del store.
export function useNotificationPreferences(store: NotificationPreferencesStore) {
  const state = useSyncExternalStore(store.subscribe, store.getState, store.getState);

  useEffect(() => {
    if (store.getState().status === 'idle') void store.load();
  }, [store]);

  return state;
}

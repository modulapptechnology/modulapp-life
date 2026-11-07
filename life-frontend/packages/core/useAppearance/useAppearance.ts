import { useSyncExternalStore } from 'react';
import type { AppearanceStore } from '../appearance/appearanceStore';

export function useAppearance(store: AppearanceStore) {
  return useSyncExternalStore(store.subscribe, store.getState, store.getState);
}

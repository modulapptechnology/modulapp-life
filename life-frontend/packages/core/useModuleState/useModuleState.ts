import { useSyncExternalStore } from 'react';
import type { ModuleStateStore } from '../moduleStateStore/moduleStateStore';

export function useModuleState<T>(
  store: ModuleStateStore,
  key: string,
  initial: T,
): [T, (value: T) => void] {
  const stored = useSyncExternalStore(
    store.subscribe,
    () => store.get(key),
    () => store.get(key),
  ) as T | undefined;

  return [stored === undefined ? initial : stored, (value) => store.set(key, value)];
}

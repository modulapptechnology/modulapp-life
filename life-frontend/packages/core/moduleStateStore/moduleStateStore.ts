export type ModuleStateStore = {
  get: (key: string) => unknown;
  set: (key: string, value: unknown) => void;
  clear: () => void;
  subscribe: (listener: () => void) => () => void;
};

export function createModuleStateStore(): ModuleStateStore {
  const values = new Map<string, unknown>();
  const listeners = new Set<() => void>();

  function notify() {
    listeners.forEach((listener) => listener());
  }

  return {
    get: (key) => values.get(key),
    set(key, value) {
      values.set(key, value);
      notify();
    },
    clear() {
      values.clear();
      notify();
    },
    subscribe(listener) {
      listeners.add(listener);
      return () => {
        listeners.delete(listener);
      };
    },
  };
}

export const moduleStateStore = createModuleStateStore();

export function moduleStateKey(workspaceId: string | null, moduleId: string): string {
  return `${workspaceId ?? 'none'}:${moduleId}`;
}

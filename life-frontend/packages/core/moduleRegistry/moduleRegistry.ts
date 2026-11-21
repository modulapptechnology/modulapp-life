import type { ModuleDefinition } from '@life/types';

export type ModuleRegistry = {
  register: (definition: ModuleDefinition, enabled?: boolean) => void;
  setEnabled: (ids: string[]) => void;
  setModuleEnabled: (id: string, enabled: boolean) => void;
  getEnabled: () => ModuleDefinition[];
  getAll: () => ModuleDefinition[];
  subscribe: (listener: () => void) => () => void;
  getDisabled: () => ModuleDefinition[];
};

export function createModuleRegistry(initial: ModuleDefinition[] = []): ModuleRegistry {
  const definitions = new Map<string, ModuleDefinition>();
  const enabledIds = new Set<string>();
  const listeners = new Set<() => void>();
  let enabledSnapshot: ModuleDefinition[] = [];

  function commit() {
    enabledSnapshot = [...definitions.values()].filter((definition) =>
      enabledIds.has(definition.id),
    );
    listeners.forEach((listener) => listener());
  }

  initial.forEach((definition) => {
    definitions.set(definition.id, definition);
    enabledIds.add(definition.id);
  });
  enabledSnapshot = [...definitions.values()];

  return {
    register(definition, enabled = true) {
      definitions.set(definition.id, definition);
      if (enabled) enabledIds.add(definition.id);
      else enabledIds.delete(definition.id);
      commit();
    },
    setEnabled(ids) {
      enabledIds.clear();
      ids.forEach((id) => definitions.has(id) && enabledIds.add(id));
      commit();
    },
    setModuleEnabled(id, enabled) {
      if (!definitions.has(id)) return;
      if (enabled) enabledIds.add(id);
      else enabledIds.delete(id);
      commit();
    },
    getEnabled: () => enabledSnapshot,
    getDisabled: () =>
      [...definitions.values()].filter((definition) => !enabledIds.has(definition.id)),
    getAll: () => [...definitions.values()],
    subscribe(listener) {
      listeners.add(listener);
      return () => {
        listeners.delete(listener);
      };
    },
  };
}

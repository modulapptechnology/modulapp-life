import type { ModuleDefinition } from '@life/types';

export type ModuleRegistry = {
  register: (definition: ModuleDefinition, enabled?: boolean) => void;
  setEnabled: (ids: string[]) => void;
  setModuleEnabled: (id: string, enabled: boolean) => void;
  getEnabled: () => ModuleDefinition[];
  getAll: () => ModuleDefinition[];
  subscribe: (listener: () => void) => () => void;
};

// Registro de módulos: la navegación lee de aquí, así que registrar o habilitar un módulo no toca el layout.
export function createModuleRegistry(initial: ModuleDefinition[] = []): ModuleRegistry {
  const definitions = new Map<string, ModuleDefinition>();
  const enabledIds = new Set<string>();
  const listeners = new Set<() => void>();
  let enabledSnapshot: ModuleDefinition[] = [];

  // Una instancia nueva solo cuando algo cambia, para que React detecte el cambio.
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
    getAll: () => [...definitions.values()],
    subscribe(listener) {
      listeners.add(listener);
      return () => {
        listeners.delete(listener);
      };
    },
  };
}

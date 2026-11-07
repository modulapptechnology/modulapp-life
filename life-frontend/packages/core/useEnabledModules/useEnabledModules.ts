import { useSyncExternalStore } from 'react';
import type { ModuleRegistry } from '../moduleRegistry/moduleRegistry';

// Vuelve a renderizar cuando se habilita, deshabilita o registra un módulo, sin reiniciar la app.
export function useEnabledModules(registry: ModuleRegistry) {
  return useSyncExternalStore(registry.subscribe, registry.getEnabled, registry.getEnabled);
}

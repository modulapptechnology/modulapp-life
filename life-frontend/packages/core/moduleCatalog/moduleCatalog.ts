import type { ModuleDefinition } from '@life/types';
import { createModuleRegistry } from '../moduleRegistry/moduleRegistry';

// Para agregar un módulo: una entrada aquí y su clave de nombre en translate.ts. El layout no cambia.
// El id es también el segmento de la ruta (`/tareas`), porque la app móvil resuelve cada módulo por su id.
export const DEFAULT_MODULES: ModuleDefinition[] = [
  {
    id: 'tareas',
    path: '/tareas',
    nameKey: 'module.tasks',
    icon: 'ChecklistMinimalisticIcon',
    accent: 'primary',
  },
  {
    id: 'agenda',
    path: '/agenda',
    nameKey: 'module.agenda',
    icon: 'CalendarIcon',
    accent: 'secondary',
  },
];

// Captura es parte del núcleo: no es un módulo habilitable ni aparece en el menú lateral. Solo expone su ruta.
export const CAPTURE_SECTION = { path: '/captura', nameKey: 'module.capture' } as const;

export const moduleRegistry = createModuleRegistry(DEFAULT_MODULES);

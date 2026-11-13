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
    groups: [
      {
        id: 'tareas-mias',
        titleKey: 'module.tasks.mine',
        links: [
          { path: '/tareas/asignadas', nameKey: 'module.tasks.assigned', icon: 'CheckCircleIcon' },
          { path: '/tareas/hoy', nameKey: 'module.tasks.today', icon: 'ClockCircleIcon' },
          { path: '/tareas/mas', nameKey: 'nav.more', icon: 'MenuDotsIcon' },
        ],
      },
      {
        id: 'tareas-listas',
        titleKey: 'module.tasks.lists',
        links: [
          { path: '/tareas/personal', nameKey: 'module.tasks.personal', icon: 'ListIcon' },
          { path: '/tareas/agregar', nameKey: 'nav.add', icon: 'AddCircleIcon' },
        ],
      },
    ],
  },
  {
    id: 'agenda',
    path: '/agenda',
    nameKey: 'module.agenda',
    icon: 'CalendarIcon',
    accent: 'secondary',
    groups: [
      {
        id: 'agenda-calendario',
        titleKey: 'module.agenda.calendar',
        links: [
          { path: '/agenda/mi-calendario', nameKey: 'module.agenda.mine', icon: 'CalendarIcon' },
          { path: '/agenda/nuevo', nameKey: 'module.agenda.new', icon: 'CalendarAddIcon' },
          { path: '/agenda/mas', nameKey: 'nav.more', icon: 'MenuDotsIcon' },
        ],
      },
      {
        id: 'agenda-recordatorios',
        titleKey: 'module.agenda.reminders',
        links: [
          { path: '/agenda/proximos', nameKey: 'module.agenda.upcoming', icon: 'BellIcon' },
          { path: '/agenda/agregar', nameKey: 'nav.add', icon: 'AddCircleIcon' },
        ],
      },
    ],
  },
];

export const HOME_SECTION: Pick<ModuleDefinition, 'path' | 'nameKey' | 'icon' | 'groups'> = {
  path: '/',
  nameKey: 'nav.home',
  icon: 'Home2Icon',
  groups: [
    {
      id: 'inicio-recientes',
      titleKey: 'home.recent',
      links: [
        { path: '/inicio/bandeja', nameKey: 'home.inbox', icon: 'InboxIcon' },
        { path: '/inicio/nuevo', nameKey: 'home.new', icon: 'PenNewSquareIcon' },
        { path: '/inicio/mas', nameKey: 'nav.more', icon: 'MenuDotsIcon' },
      ],
    },
    {
      id: 'inicio-favoritos',
      titleKey: 'home.favorites',
      links: [
        { path: '/inicio/destacados', nameKey: 'home.starred', icon: 'StarIcon' },
        { path: '/inicio/agregar', nameKey: 'nav.add', icon: 'AddCircleIcon' },
      ],
    },
  ],
};

export const PROFILE_SECTION: Pick<ModuleDefinition, 'path' | 'nameKey' | 'icon' | 'groups'> = {
  path: '/profile',
  nameKey: 'user.profile',
  icon: 'UserIcon',
  groups: [
    {
      id: 'perfil-cuenta',
      titleKey: 'profile.account',
      links: [
        { path: '/profile/informacion', nameKey: 'profile.info', icon: 'UserIcon' },
        { path: '/profile/seguridad', nameKey: 'profile.security', icon: 'ShieldIcon' },
        { path: '/profile/mas', nameKey: 'nav.more', icon: 'MenuDotsIcon' },
      ],
    },
    {
      id: 'perfil-actividad',
      titleKey: 'profile.activity',
      links: [
        { path: '/profile/historial', nameKey: 'profile.history', icon: 'ClockCircleIcon' },
        { path: '/profile/agregar', nameKey: 'nav.add', icon: 'AddCircleIcon' },
      ],
    },
  ],
};

export const SETTINGS_SECTION: Pick<ModuleDefinition, 'path' | 'nameKey' | 'icon' | 'groups'> = {
  path: '/settings',
  nameKey: 'user.settings',
  icon: 'SettingsIcon',
  groups: [
    {
      id: 'ajustes-preferencias',
      titleKey: 'settings.preferences.title',
      links: [
        {
          path: '/settings/apariencia',
          nameKey: 'settings.preferences.appearance',
          icon: 'PaletteIcon',
        },
        { path: '/settings/idioma', nameKey: 'settings.preferences.language', icon: 'GlobalIcon' },
        { path: '/settings/mas', nameKey: 'nav.more', icon: 'MenuDotsIcon' },
      ],
    },
    {
      id: 'ajustes-notificaciones',
      titleKey: 'settings.notifications.title',
      links: [
        { path: '/settings/recordatorios', nameKey: 'settings.reminders', icon: 'BellIcon' },
        { path: '/settings/agregar', nameKey: 'nav.add', icon: 'AddCircleIcon' },
      ],
    },
  ],
};

// Captura es parte del núcleo: no es un módulo habilitable ni aparece en el menú lateral. Solo expone su ruta.
export const CAPTURE_SECTION = { path: '/captura', nameKey: 'module.capture' } as const;

export const MOBILE_TABS: Pick<ModuleDefinition, 'path' | 'nameKey' | 'icon'>[] = [
  { path: '/', nameKey: 'nav.home', icon: 'Home2Icon' },
  { path: '/inbox', nameKey: 'home.inbox', icon: 'InboxIcon' },
  { path: '/planner', nameKey: 'module.agenda', icon: 'CalendarIcon' },
  { path: '/modules', nameKey: 'nav.modules', icon: 'LayersIcon' },
];

export const moduleRegistry = createModuleRegistry(DEFAULT_MODULES);

import type { ModuleDefinition } from '@life/types';
import type { NotificationPreferencesState } from '../notificationPreferences/notificationPreferencesStore';
import { createSettingsViewModel } from './settingsViewModel';

const tasks: ModuleDefinition = {
  id: 'tareas',
  path: '/tareas',
  nameKey: 'module.tasks',
  icon: 'ChecklistMinimalisticIcon',
  accent: 'primary',
};
const agenda: ModuleDefinition = {
  ...tasks,
  id: 'agenda',
  path: '/agenda',
  nameKey: 'module.agenda',
};
const translate = (key: string) => key;

function build(
  notifications: Partial<NotificationPreferencesState> = {},
  platform: 'mobile' | 'web' = 'mobile',
) {
  return createSettingsViewModel({
    allModules: [tasks, agenda],
    enabledModules: [tasks],
    notifications: { status: 'ready', preferences: [], saveFailed: false, ...notifications },
    platform,
    translate,
  });
}

describe('createSettingsViewModel', () => {
  it('marca como activados solo los módulos habilitados', () => {
    const model = build();

    expect(model.modules).toEqual([
      { id: 'tareas', label: 'module.tasks', checked: true },
      { id: 'agenda', label: 'module.agenda', checked: false },
    ]);
  });

  it('muestra solo los canales de la plataforma', () => {
    const model = build({}, 'web');

    expect(model.notificationGroups[0].toggles.map((toggle) => toggle.id)).toEqual([
      'email',
      'inApp',
    ]);
  });

  it('refleja si cada tipo y canal está activado', () => {
    const model = build({
      preferences: [
        { type: 'taskReminder', channel: 'push', enabled: true },
        { type: 'taskReminder', channel: 'inApp', enabled: false },
      ],
    });

    expect(model.notificationGroups[0].toggles.map((toggle) => toggle.checked)).toEqual([
      true,
      false,
    ]);
  });

  it('pasa de idle y loading al estado de carga, y deja error y listo como están', () => {
    const statuses = (['idle', 'loading', 'error', 'ready'] as const).map(
      (status) => build({ status }).notificationsStatus,
    );

    expect(statuses).toEqual(['loading', 'loading', 'error', 'ready']);
  });

  it('incluye el aviso de error solo cuando falló guardar', () => {
    const failed = build({ saveFailed: true }).saveError;
    const fine = build({ saveFailed: false }).saveError;

    expect([failed, fine]).toEqual(['settings.notifications.saveError', undefined]);
  });

  it('pone el nombre del módulo en el título de la confirmación', () => {
    const translateWithTitle = (key: string) =>
      key === 'settings.modules.disableTitle' ? 'Disable {module}?' : key;
    const model = createSettingsViewModel({
      allModules: [tasks],
      enabledModules: [tasks],
      notifications: { status: 'ready', preferences: [], saveFailed: false },
      platform: 'mobile',
      translate: translateWithTitle,
    });

    expect(model.disableDialog.title('Tasks')).toBe('Disable Tasks?');
  });
});

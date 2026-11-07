import type { NotificationPreference } from '@life/types';
import { createNotificationPreferencesStore } from './notificationPreferencesStore';
import type { NotificationPreferencesService } from './notificationPreferencesService';

const initial: NotificationPreference[] = [
  { type: 'taskReminder', channel: 'push', enabled: true },
  { type: 'taskReminder', channel: 'inApp', enabled: true },
];

function createService(overrides: Partial<NotificationPreferencesService> = {}) {
  return {
    list: jest.fn().mockResolvedValue(initial),
    save: jest.fn(),
    ...overrides,
  } as jest.Mocked<NotificationPreferencesService>;
}

describe('notificationPreferencesStore', () => {
  it('carga las preferencias y queda listo', async () => {
    const store = createNotificationPreferencesStore(createService());

    await store.load();

    expect(store.getState()).toEqual({ status: 'ready', preferences: initial, saveFailed: false });
  });

  it('pasa por el estado de carga mientras lee', async () => {
    const store = createNotificationPreferencesStore(createService());
    const states: string[] = [];
    store.subscribe(() => states.push(store.getState().status));

    await store.load();

    expect(states).toEqual(['loading', 'ready']);
  });

  it('queda en error si la lectura falla y permite reintentar', async () => {
    const list = jest.fn().mockRejectedValueOnce(new Error('red')).mockResolvedValue(initial);
    const store = createNotificationPreferencesStore(createService({ list }));
    await store.load();

    const failedStatus = store.getState().status;
    await store.load();

    expect([failedStatus, store.getState().status]).toEqual(['error', 'ready']);
  });

  it('guarda el cambio por tipo y canal con la lista que devuelve el servicio', async () => {
    const saved = [
      { type: 'taskReminder', channel: 'push', enabled: false },
      { type: 'taskReminder', channel: 'inApp', enabled: true },
    ] as NotificationPreference[];
    const service = createService({ save: jest.fn().mockResolvedValue(saved) });
    const store = createNotificationPreferencesStore(service);
    await store.load();

    await store.toggle('taskReminder', 'push', false);

    expect(service.save).toHaveBeenCalledWith({
      type: 'taskReminder',
      channel: 'push',
      enabled: false,
    });
    expect(store.getState().preferences).toEqual(saved);
  });

  it('muestra el cambio de inmediato, antes de que el servicio responda', async () => {
    let resolveSave: (value: NotificationPreference[]) => void = () => undefined;
    const save = jest.fn().mockReturnValue(new Promise((resolve) => (resolveSave = resolve)));
    const store = createNotificationPreferencesStore(createService({ save }));
    await store.load();

    const pending = store.toggle('taskReminder', 'push', false);
    const optimistic = store.getState().preferences[0].enabled;
    resolveSave(initial);
    await pending;

    expect(optimistic).toBe(false);
  });

  it('revierte el cambio y avisa si guardar falla', async () => {
    const service = createService({ save: jest.fn().mockRejectedValue(new Error('red')) });
    const store = createNotificationPreferencesStore(service);
    await store.load();

    await store.toggle('taskReminder', 'push', false);

    expect(store.getState().preferences[0].enabled).toBe(true);
    expect(store.getState().saveFailed).toBe(true);
  });

  it('quita el aviso de error de guardado al descartarlo', async () => {
    const service = createService({ save: jest.fn().mockRejectedValue(new Error('red')) });
    const store = createNotificationPreferencesStore(service);
    await store.load();
    await store.toggle('taskReminder', 'push', false);

    store.dismissSaveError();

    expect(store.getState().saveFailed).toBe(false);
  });
});

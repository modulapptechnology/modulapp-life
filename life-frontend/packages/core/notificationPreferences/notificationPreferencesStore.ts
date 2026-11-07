import type { NotificationChannel, NotificationPreference, NotificationType } from '@life/types';
import {
  createMockNotificationPreferencesService,
  type NotificationPreferencesService,
} from './notificationPreferencesService';

export type NotificationPreferencesState = {
  status: 'idle' | 'loading' | 'ready' | 'error';
  preferences: NotificationPreference[];
  saveFailed: boolean;
};

export type NotificationPreferencesStore = {
  getState: () => NotificationPreferencesState;
  load: () => Promise<void>;
  toggle: (type: NotificationType, channel: NotificationChannel, enabled: boolean) => Promise<void>;
  dismissSaveError: () => void;
  subscribe: (listener: () => void) => () => void;
};

function withEnabled(
  preferences: NotificationPreference[],
  type: NotificationType,
  channel: NotificationChannel,
  enabled: boolean,
) {
  return preferences.map((preference) =>
    preference.type === type && preference.channel === channel
      ? { ...preference, enabled }
      : preference,
  );
}

export function createNotificationPreferencesStore(
  service: NotificationPreferencesService,
): NotificationPreferencesStore {
  let state: NotificationPreferencesState = {
    status: 'idle',
    preferences: [],
    saveFailed: false,
  };
  const listeners = new Set<() => void>();

  function update(next: Partial<NotificationPreferencesState>) {
    state = { ...state, ...next };
    listeners.forEach((listener) => listener());
  }

  return {
    getState: () => state,
    async load() {
      update({ status: 'loading' });
      try {
        update({ status: 'ready', preferences: await service.list() });
      } catch {
        update({ status: 'error' });
      }
    },
    // El cambio se muestra de inmediato; si guardar falla, se revierte y se avisa.
    async toggle(type, channel, enabled) {
      update({
        preferences: withEnabled(state.preferences, type, channel, enabled),
        saveFailed: false,
      });
      try {
        update({ preferences: await service.save({ type, channel, enabled }) });
      } catch {
        update({
          preferences: withEnabled(state.preferences, type, channel, !enabled),
          saveFailed: true,
        });
      }
    },
    dismissSaveError: () => update({ saveFailed: false }),
    subscribe(listener) {
      listeners.add(listener);
      return () => {
        listeners.delete(listener);
      };
    },
  };
}

export const notificationPreferencesStore = createNotificationPreferencesStore(
  createMockNotificationPreferencesService(),
);

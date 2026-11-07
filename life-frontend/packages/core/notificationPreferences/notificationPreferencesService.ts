import type { NotificationChannel, NotificationPreference, NotificationType } from '@life/types';

export type NotificationPreferencesService = {
  list: () => Promise<NotificationPreference[]>;
  save: (preference: NotificationPreference) => Promise<NotificationPreference[]>;
};

export type SettingsPlatform = 'mobile' | 'web';

export const NOTIFICATION_TYPES: NotificationType[] = [
  'taskReminder',
  'agendaEvent',
  'dailySummary',
];
export const NOTIFICATION_CHANNELS: NotificationChannel[] = ['push', 'email', 'inApp'];

const PLATFORM_CHANNELS: Record<SettingsPlatform, NotificationChannel[]> = {
  mobile: ['push', 'inApp'],
  web: ['email', 'inApp'],
};

// Los canales disponibles dependen de la plataforma.
export function channelsForPlatform(platform: SettingsPlatform): NotificationChannel[] {
  return PLATFORM_CHANNELS[platform];
}

function wait(ms: number) {
  return new Promise<void>((resolve) => setTimeout(resolve, ms));
}

// TEMPORAL: simula en memoria la API de preferencias de notificación, que aún no existe en el backend.
// Cuando exista, solo se reemplaza esta implementación por llamadas a @life/api-client; las pantallas no cambian.
export function createMockNotificationPreferencesService(
  delayMs = 300,
): NotificationPreferencesService {
  let preferences: NotificationPreference[] = NOTIFICATION_TYPES.flatMap((type) =>
    NOTIFICATION_CHANNELS.map((channel) => ({ type, channel, enabled: true })),
  );

  return {
    async list() {
      await wait(delayMs);
      return preferences.map((preference) => ({ ...preference }));
    },
    async save(next) {
      await wait(delayMs);
      preferences = preferences.map((preference) =>
        preference.type === next.type && preference.channel === next.channel
          ? { ...next }
          : preference,
      );
      return preferences.map((preference) => ({ ...preference }));
    },
  };
}

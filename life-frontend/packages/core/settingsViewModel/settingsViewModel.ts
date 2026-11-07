import type { ModuleDefinition, NotificationType } from '@life/types';
import {
  NOTIFICATION_TYPES,
  channelsForPlatform,
  type SettingsPlatform,
} from '../notificationPreferences/notificationPreferencesService';
import type { NotificationPreferencesState } from '../notificationPreferences/notificationPreferencesStore';

type Translate = (key: string) => string;

type SettingsViewModelInput = {
  allModules: ModuleDefinition[];
  enabledModules: ModuleDefinition[];
  notifications: NotificationPreferencesState;
  platform: SettingsPlatform;
  translate: Translate;
};

// Arma lo que muestra la pantalla de ajustes (textos incluidos) a partir del estado de la app.
export function createSettingsViewModel({
  allModules,
  enabledModules,
  notifications,
  platform,
  translate,
}: SettingsViewModelInput) {
  const enabledIds = new Set(enabledModules.map((module) => module.id));
  const channels = channelsForPlatform(platform);
  const isEnabled = (type: NotificationType, channel: string) =>
    notifications.preferences.some(
      (preference) =>
        preference.type === type && preference.channel === channel && preference.enabled,
    );

  return {
    modulesTitle: translate('settings.modules.title'),
    modules: allModules.map((module) => ({
      id: module.id,
      label: translate(module.nameKey),
      checked: enabledIds.has(module.id),
    })),
    disableDialog: {
      title: (moduleLabel: string) =>
        translate('settings.modules.disableTitle').replace('{module}', moduleLabel),
      message: translate('settings.modules.disableMessage'),
      confirmLabel: translate('settings.modules.disable'),
      cancelLabel: translate('common.cancel'),
    },
    notificationsTitle: translate('settings.notifications.title'),
    notificationsStatus:
      notifications.status === 'ready'
        ? ('ready' as const)
        : notifications.status === 'error'
          ? ('error' as const)
          : ('loading' as const),
    loadingMessage: translate('settings.notifications.loading'),
    errorTitle: translate('settings.notifications.errorTitle'),
    retryLabel: translate('settings.notifications.retry'),
    notificationGroups: NOTIFICATION_TYPES.map((type) => ({
      id: type,
      label: translate(`settings.notifications.type.${type}`),
      toggles: channels.map((channel) => ({
        id: channel,
        label: translate(`settings.notifications.channel.${channel}`),
        checked: isEnabled(type, channel),
      })),
    })),
    saveError: notifications.saveFailed ? translate('settings.notifications.saveError') : undefined,
    closeLabel: translate('common.close'),
    preferencesTitle: translate('settings.preferences.title'),
    appearanceTexts: {
      title: translate('settings.appearance.title'),
      themeTitle: translate('settings.appearance.theme'),
      accentTitle: translate('settings.appearance.accent'),
      themeOptions: (['light', 'dark', 'system'] as const).map((id) => ({
        id,
        label: translate(`settings.appearance.${id}`),
      })),
      accentOptions: (['modul', 'violet', 'ocean'] as const).map((id) => ({
        id,
        label: translate(`settings.appearance.accent.${id}`),
      })),
    },
    preferenceLinks: [
      {
        id: 'appearance',
        label: translate('settings.preferences.appearance'),
        icon: 'PaletteIcon',
      },
      { id: 'language', label: translate('settings.preferences.language'), icon: 'GlobalIcon' },
      { id: 'format', label: translate('settings.preferences.format'), icon: 'CalendarIcon' },
    ] as const,
  };
}

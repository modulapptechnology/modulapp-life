import { moduleRegistry } from '../moduleCatalog/moduleCatalog';
import {
  notificationPreferencesStore,
  type NotificationPreferencesStore,
} from '../notificationPreferences/notificationPreferencesStore';
import type { SettingsPlatform } from '../notificationPreferences/notificationPreferencesService';
import { createSettingsViewModel } from '../settingsViewModel/settingsViewModel';
import { translate } from '../translate/translate';
import { useEnabledModules } from '../useEnabledModules/useEnabledModules';
import { useNotificationPreferences } from '../useNotificationPreferences/useNotificationPreferences';

type ToggleArgs = Parameters<NotificationPreferencesStore['toggle']>;

// Todo lo que necesita la pantalla de ajustes, igual para mobile y web: <ModulSettings {...useSettings(plataforma)} />.
export function useSettings(platform: SettingsPlatform) {
  const enabledModules = useEnabledModules(moduleRegistry);
  const notifications = useNotificationPreferences(notificationPreferencesStore);

  return {
    ...createSettingsViewModel({
      allModules: moduleRegistry.getAll(),
      enabledModules,
      notifications,
      platform,
      translate,
    }),
    onModuleChange: (id: string, checked: boolean) => moduleRegistry.setModuleEnabled(id, checked),
    onNotificationChange: (type: string, channel: string, checked: boolean) =>
      notificationPreferencesStore.toggle(type as ToggleArgs[0], channel as ToggleArgs[1], checked),
    onRetry: () => notificationPreferencesStore.load(),
    onDismissSaveError: () => notificationPreferencesStore.dismissSaveError(),
  };
}

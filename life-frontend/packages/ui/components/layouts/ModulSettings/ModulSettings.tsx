import { useState } from 'react';
import { ScrollView } from 'react-native';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';
import type { IconName } from '../../atoms/ModulIcon/ModulIcon';
import { ModulConfirmDialog } from '../../molecules/ModulConfirmDialog/ModulConfirmDialog';
import { ModulErrorState } from '../../molecules/ModulErrorState/ModulErrorState';
import { ModulListItem } from '../../molecules/ModulListItem/ModulListItem';
import { ModulLoadingState } from '../../molecules/ModulLoadingState/ModulLoadingState';
import { ModulSwitch } from '../../molecules/ModulSwitch/ModulSwitch';
import { ModulToast } from '../../molecules/ModulToast/ModulToast';

export type ModulSettingsToggle = { id: string; label: string; checked: boolean };
export type ModulSettingsGroup = { id: string; label: string; toggles: ModulSettingsToggle[] };
export type ModulSettingsLink = { id: string; label: string; icon: IconName };

export type ModulSettingsProps = {
  modulesTitle: string;
  modules: ModulSettingsToggle[];
  disableDialog: {
    title: (moduleLabel: string) => string;
    message: string;
    confirmLabel: string;
    cancelLabel: string;
  };
  onModuleChange: (id: string, checked: boolean) => void;
  notificationsTitle: string;
  notificationsStatus: 'loading' | 'ready' | 'error';
  loadingMessage: string;
  errorTitle: string;
  retryLabel: string;
  onRetry: () => void;
  notificationGroups: ModulSettingsGroup[];
  onNotificationChange: (groupId: string, toggleId: string, checked: boolean) => void;
  saveError?: string;
  closeLabel: string;
  onDismissSaveError: () => void;
  preferencesTitle: string;
  preferenceLinks: readonly ModulSettingsLink[];
};

// Pantalla de ajustes por secciones. Habilitar un módulo es inmediato; deshabilitarlo pide confirmación.
export function ModulSettings({
  modulesTitle,
  modules,
  disableDialog,
  onModuleChange,
  notificationsTitle,
  notificationsStatus,
  loadingMessage,
  errorTitle,
  retryLabel,
  onRetry,
  notificationGroups,
  onNotificationChange,
  saveError,
  closeLabel,
  onDismissSaveError,
  preferencesTitle,
  preferenceLinks,
}: ModulSettingsProps) {
  const [pendingId, setPendingId] = useState<string | null>(null);
  const pending = modules.find((module) => module.id === pendingId);

  return (
    <ScrollView>
      <ModulView p16 gap16>
        <ModulText variant="sectionTitle">{modulesTitle}</ModulText>
        <ModulView>
          {modules.map((module) => (
            <ModulSwitch
              key={module.id}
              label={module.label}
              checked={module.checked}
              onChange={(checked) =>
                checked ? onModuleChange(module.id, true) : setPendingId(module.id)
              }
            />
          ))}
        </ModulView>

        <ModulText variant="sectionTitle">{notificationsTitle}</ModulText>
        {saveError ? (
          <ModulToast
            type="error"
            message={saveError}
            closeLabel={closeLabel}
            onClose={onDismissSaveError}
          />
        ) : null}
        {notificationsStatus === 'loading' ? <ModulLoadingState message={loadingMessage} /> : null}
        {notificationsStatus === 'error' ? (
          <ModulErrorState title={errorTitle} retryLabel={retryLabel} onRetry={onRetry} />
        ) : null}
        {notificationsStatus === 'ready'
          ? notificationGroups.map((group) => (
              <ModulView key={group.id} gap8>
                <ModulText variant="label">{group.label}</ModulText>
                {group.toggles.map((toggle) => (
                  <ModulSwitch
                    key={toggle.id}
                    label={`${group.label}: ${toggle.label}`}
                    checked={toggle.checked}
                    onChange={(checked) => onNotificationChange(group.id, toggle.id, checked)}
                  />
                ))}
              </ModulView>
            ))
          : null}

        <ModulText variant="sectionTitle">{preferencesTitle}</ModulText>
        <ModulView>
          {preferenceLinks.map((link) => (
            <ModulListItem key={link.id} title={link.label} icon={link.icon} />
          ))}
        </ModulView>
      </ModulView>

      <ModulConfirmDialog
        visible={pending !== undefined}
        title={pending ? disableDialog.title(pending.label) : ''}
        message={disableDialog.message}
        confirmLabel={disableDialog.confirmLabel}
        cancelLabel={disableDialog.cancelLabel}
        destructive
        onConfirm={() => {
          if (pendingId) onModuleChange(pendingId, false);
          setPendingId(null);
        }}
        onCancel={() => setPendingId(null)}
      />
    </ScrollView>
  );
}

import type { ThemeColors } from '@life/theme';
import { ModulIcon, type IconName } from '../../atoms/ModulIcon/ModulIcon';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';

export type ModulConnectionStatus = 'offline' | 'syncing' | 'synced';

export type ModulConnectionIndicatorProps = {
  status: ModulConnectionStatus;
  labels: Record<ModulConnectionStatus, string>;
};

const STATUS_ICONS: Record<ModulConnectionStatus, { icon: IconName; color: keyof ThemeColors }> = {
  offline: { icon: 'CloudCrossIcon', color: 'warning' },
  syncing: { icon: 'RefreshCircleIcon', color: 'info' },
  synced: { icon: 'CloudCheckIcon', color: 'success' },
};

export function ModulConnectionIndicator({ status, labels }: ModulConnectionIndicatorProps) {
  const { icon, color } = STATUS_ICONS[status];

  return (
    <ModulView
      row
      alignCenter
      gap4
      accessible
      accessibilityLabel={labels[status]}
      accessibilityLiveRegion="polite"
    >
      <ModulIcon name={icon} size="sm" color={color} />
      <ModulText variant="bodySecondary" textSecondary>
        {labels[status]}
      </ModulText>
    </ModulView>
  );
}

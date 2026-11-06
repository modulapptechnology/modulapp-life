import { Pressable } from 'react-native';
import { useStyles } from '../../utils/useStyles';
import { ModulIcon, type IconName } from '../atoms/ModulIcon';
import { ModulText } from '../atoms/ModulText';
import { ModulView } from '../atoms/ModulView';
import type { ThemeColors } from '@life/theme';

export type ModulToastType = 'success' | 'error' | 'info' | 'warning';

export type ModulToastProps = {
  message: string;
  type?: ModulToastType;
  closeLabel: string;
  onClose: () => void;
};

const TOAST_ICONS: Record<ModulToastType, { icon: IconName; color: keyof ThemeColors }> = {
  success: { icon: 'CheckCircleIcon', color: 'success' },
  error: { icon: 'DangerCircleIcon', color: 'danger' },
  info: { icon: 'InfoCircleIcon', color: 'info' },
  warning: { icon: 'DangerTriangleIcon', color: 'warning' },
};

export function ModulToast({ message, type = 'info', closeLabel, onClose }: ModulToastProps) {
  const { view } = useStyles();
  const { icon, color } = TOAST_ICONS[type];

  return (
    <ModulView
      row
      alignCenter
      gap8
      pl16
      bgSurface
      borderSubtle
      roundedLg
      accessibilityRole="alert"
      accessibilityLiveRegion="polite"
    >
      <ModulIcon name={icon} color={color} />
      <ModulText variant="body" style={view.flex1}>
        {message}
      </ModulText>
      <Pressable
        accessibilityRole="button"
        accessibilityLabel={closeLabel}
        onPress={onClose}
        style={[view.minW44, view.minH44, view.alignCenter, view.justifyCenter]}
      >
        <ModulIcon name="CloseCircleIcon" color="textSecondary" />
      </Pressable>
    </ModulView>
  );
}

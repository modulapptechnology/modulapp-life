import { Pressable } from 'react-native';
import { useStyles } from '../../utils/useStyles';
import { ModulText } from './ModulText';

export type ModulChipProps = {
  label: string;
  selected?: boolean;
  disabled?: boolean;
  onPress?: () => void;
};

export function ModulChip({ label, selected = false, disabled = false, onPress }: ModulChipProps) {
  const { view } = useStyles();

  return (
    <Pressable
      accessibilityRole="tab"
      accessibilityLabel={label}
      accessibilityState={{ selected, disabled }}
      disabled={disabled}
      onPress={onPress}
      style={[
        view.minH44,
        view.px16,
        view.alignCenter,
        view.justifyCenter,
        view.roundedFull,
        selected ? view.bgPrimary : view.bgSurface,
        selected ? view.border : view.borderSubtle,
        disabled && view.opacityDisabled,
      ]}
    >
      <ModulText variant="label" textOnPrimary={selected}>
        {label}
      </ModulText>
    </Pressable>
  );
}

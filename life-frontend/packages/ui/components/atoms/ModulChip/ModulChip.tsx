import { Pressable } from 'react-native';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulIcon } from '../ModulIcon/ModulIcon';
import { ModulText } from '../ModulText/ModulText';
import { ModulView } from '../ModulView/ModulView';

export type ModulChipProps = {
  label: string;
  selected?: boolean;
  disabled?: boolean;
  // Ocupa el ancho disponible con esquinas rectas; lo usa el control segmentado de ModulChipGroup.
  segmented?: boolean;
  // Chip de filtro activo: muestra un botón para quitarlo, con `removeLabel` como nombre accesible.
  onRemove?: () => void;
  removeLabel?: string;
  onPress?: () => void;
};

export function ModulChip({
  label,
  selected = false,
  disabled = false,
  segmented = false,
  onRemove,
  removeLabel,
  onPress,
}: ModulChipProps) {
  const { view } = useStyles();

  const chip = (
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
        segmented ? view.rounded : view.roundedFull,
        segmented && view.flex1,
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

  if (!onRemove) return chip;

  return (
    <ModulView row alignCenter>
      {chip}
      <Pressable
        accessibilityRole="button"
        accessibilityLabel={removeLabel}
        disabled={disabled}
        onPress={onRemove}
        style={[view.minW44, view.minH44, view.alignCenter, view.justifyCenter]}
      >
        <ModulIcon name="CloseCircleIcon" color="textSecondary" />
      </Pressable>
    </ModulView>
  );
}

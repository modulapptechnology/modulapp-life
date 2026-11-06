import { useState } from 'react';
import { Pressable, type PressableProps } from 'react-native';
import { resolveButtonKind } from '../../../utils/resolveButtonKind/resolveButtonKind';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulIcon, type IconName } from '../ModulIcon/ModulIcon';
import { ModulText } from '../ModulText/ModulText';

export type ModulButtonVariant = 'primary' | 'secondary' | 'danger';

export type ModulButtonProps = Omit<PressableProps, 'style' | 'children'> & {
  label?: string;
  icon?: IconName;
  variant?: ModulButtonVariant;
  danger?: boolean;
};

export function ModulButton({
  label,
  icon,
  variant = 'primary',
  danger = false,
  disabled,
  accessibilityLabel,
  onFocus,
  onBlur,
  ...props
}: ModulButtonProps) {
  const { view } = useStyles();
  const [focused, setFocused] = useState(false);

  const kind = resolveButtonKind(variant, danger);
  const filled = kind !== 'secondary';
  const iconOnly = !label && !!icon;

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={accessibilityLabel ?? label}
      accessibilityState={{ disabled: !!disabled }}
      disabled={disabled}
      onFocus={(event) => {
        setFocused(true);
        onFocus?.(event);
      }}
      onBlur={(event) => {
        setFocused(false);
        onBlur?.(event);
      }}
      {...props}
      style={[
        view.minH44,
        view.minW44,
        view.py8,
        iconOnly ? view.p8 : view.px16,
        view.row,
        view.gap8,
        view.alignCenter,
        view.justifyCenter,
        view.rounded,
        kind === 'danger' && view.bgDanger,
        kind === 'primary' && view.bgPrimary,
        kind === 'secondary' && view.borderPrimary,
        focused && view.border,
        disabled && view.opacityDisabled,
      ]}
    >
      {icon ? <ModulIcon name={icon} color={filled ? 'onAccent' : 'primary'} /> : null}
      {label ? (
        <ModulText variant="button" textOnPrimary={filled} textBrand={!filled}>
          {label}
        </ModulText>
      ) : null}
    </Pressable>
  );
}

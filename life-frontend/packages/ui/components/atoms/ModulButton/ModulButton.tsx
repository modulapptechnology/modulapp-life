import { useState } from 'react';
import { ActivityIndicator, Pressable, type PressableProps } from 'react-native';
import { resolveButtonKind } from '../../../utils/resolveButtonKind/resolveButtonKind';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulIcon, type IconName } from '../ModulIcon/ModulIcon';
import { ModulText } from '../ModulText/ModulText';
import { ModulView } from '../ModulView/ModulView';

export type ModulButtonVariant = 'primary' | 'secondary' | 'danger';

export type ModulButtonProps = Omit<PressableProps, 'style' | 'children'> & {
  label?: string;
  icon?: IconName;
  variant?: ModulButtonVariant;
  danger?: boolean;
  // Alto de 32 en lugar del área táctil de 44: para la web, donde se usa el puntero.
  compact?: boolean;
  // Muestra un indicador en lugar del icono y bloquea el botón mientras dura la operación.
  loading?: boolean;
  // Resultado correcto: fondo de éxito e icono de verificación.
  success?: boolean;
  // Resultado fallido: borde de error e icono de peligro, para que no dependa solo del color.
  error?: boolean;
};

export function ModulButton({
  label,
  icon,
  variant = 'primary',
  danger = false,
  compact = false,
  loading = false,
  success = false,
  error = false,
  disabled,
  accessibilityLabel,
  onFocus,
  onBlur,
  onHoverIn,
  onHoverOut,
  onPressIn,
  onPressOut,
  ...props
}: ModulButtonProps) {
  const { view, colors } = useStyles();
  const [focused, setFocused] = useState(false);
  const [hovered, setHovered] = useState(false);
  const [pressed, setPressed] = useState(false);

  const kind = resolveButtonKind(variant, danger);
  const filled = success || kind !== 'secondary';
  const blocked = !!disabled || loading;
  const iconOnly = !label && !!icon;
  const stateIcon: IconName | undefined = success
    ? 'CheckCircleIcon'
    : error
      ? 'DangerCircleIcon'
      : icon;

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={accessibilityLabel ?? label}
      accessibilityState={{ disabled: blocked, busy: loading }}
      disabled={blocked}
      onFocus={(event) => {
        setFocused(true);
        onFocus?.(event);
      }}
      onBlur={(event) => {
        setFocused(false);
        onBlur?.(event);
      }}
      onHoverIn={(event) => {
        setHovered(true);
        onHoverIn?.(event);
      }}
      onHoverOut={(event) => {
        setHovered(false);
        onHoverOut?.(event);
      }}
      onPressIn={(event) => {
        setPressed(true);
        onPressIn?.(event);
      }}
      onPressOut={(event) => {
        setPressed(false);
        onPressOut?.(event);
      }}
      {...props}
      style={[
        compact ? view.minH32 : view.minH44,
        compact ? view.minW32 : view.minW44,
        compact ? view.py4 : view.py8,
        iconOnly ? view.p8 : view.px16,
        view.row,
        view.gap8,
        view.alignCenter,
        view.justifyCenter,
        view.rounded,
        view.overflowHidden,
        success && view.bgSuccess,
        !success && kind === 'danger' && view.bgDanger,
        !success && kind === 'primary' && view.bgPrimary,
        !success && kind === 'secondary' && view.borderPrimary,
        focused && view.border,
        error && view.borderError,
        disabled && view.opacityDisabled,
      ]}
    >
      {!blocked && pressed ? <ModulView absoluteFill bgPressed pointerEvents="none" /> : null}
      {!blocked && hovered && !pressed ? (
        <ModulView absoluteFill bgHover pointerEvents="none" />
      ) : null}
      {loading ? (
        <ActivityIndicator color={filled ? colors.onAccent : colors.primary} size="small" />
      ) : stateIcon ? (
        <ModulIcon name={stateIcon} color={filled ? 'onAccent' : 'primary'} />
      ) : null}
      {label ? (
        <ModulText variant="button" textOnPrimary={filled} textBrand={!filled}>
          {label}
        </ModulText>
      ) : null}
    </Pressable>
  );
}

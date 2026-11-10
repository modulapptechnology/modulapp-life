import { useEffect, useState } from 'react';
import {
  ActivityIndicator,
  Animated,
  Pressable,
  TextInput,
  type TextInputProps,
  type TextStyle,
} from 'react-native';
import { typography } from '@life/theme';
import { animateTo } from '../../../utils/animations/animations';
import { formatDateInput, formatTimeInput } from '../../../utils/maskInput/maskInput';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulDatePicker } from '../../atoms/ModulDatePicker/ModulDatePicker';
import { ModulFieldMessage } from '../../atoms/ModulFieldMessage/ModulFieldMessage';
import { ModulIcon, type IconName } from '../../atoms/ModulIcon/ModulIcon';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { ModulMenu } from '../ModulMenu/ModulMenu';

export type ModulInputType =
  'text' | 'password' | 'number' | 'textarea' | 'search' | 'date' | 'time' | 'select';

export type ModulInputOption = {
  value: string;
  label: string;
};

export type ModulInputProps = Omit<
  TextInputProps,
  'style' | 'editable' | 'secureTextEntry' | 'keyboardType' | 'multiline' | 'accessibilityLabel'
> & {
  type?: ModulInputType;
  // 'floating': borde redondeado con la etiqueta dentro del campo, que sube al borde al enfocar o escribir.
  variant?: 'default' | 'floating';
  label: string;
  help?: string;
  error?: string;
  disabled?: boolean;
  // Muestra un indicador al final del campo, por ejemplo mientras se valida contra el servidor.
  loading?: boolean;
  // Valor validado: borde de éxito e icono de verificación al final del campo.
  success?: boolean;
  // Alto de 32 en lugar del área táctil de 44: para la web, donde se usa el puntero.
  compact?: boolean;
  // Solo type="select": opciones del desplegable; `value` y `onChangeText` trabajan con el `value` de la opción.
  options?: ModulInputOption[];
  // Nombre accesible del botón al final del campo (mostrar u ocultar contraseña, borrar búsqueda) y del cierre del selector.
  actionLabel?: string;
};

const LEADING_ICONS: Partial<Record<ModulInputType, IconName>> = {
  search: 'MagnifierIcon',
};

export function ModulInput({
  type = 'text',
  variant = 'default',
  label,
  help,
  error,
  disabled = false,
  loading = false,
  success = false,
  compact = false,
  options = [],
  actionLabel,
  value,
  onChangeText,
  onFocus,
  onBlur,
  ...props
}: ModulInputProps) {
  const { view, text, colors } = useStyles();
  const [focused, setFocused] = useState(false);
  const [hidden, setHidden] = useState(true);
  const [menuOpen, setMenuOpen] = useState(false);
  const [hovered, setHovered] = useState(false);

  // El error pesa más que el foco, y el foco más que el éxito.
  const showSuccess = success && !error;
  const floating = variant === 'floating';
  const raised = floating && (focused || !!value);
  // Etiqueta flotante: 0 = dentro del campo como texto guía, 1 = sobre el borde. Sube y baja con animación.
  const [labelProgress] = useState(() => new Animated.Value(raised ? 1 : 0));
  const [fieldHeight, setFieldHeight] = useState(0);

  useEffect(() => {
    if (!floating) return;
    animateTo(labelProgress, raised ? 1 : 0);
  }, [floating, raised, labelProgress]);
  const leadingIcon = LEADING_ICONS[type];
  const selectedOption = options.find((option) => option.value === value);
  // outlineStyle none: en web el navegador dibuja su propio contorno; el foco ya lo marca el borde del campo.
  // El tipo de React Native solo acepta solid, dotted y dashed, por eso el cast.
  const noOutline = { outlineStyle: 'none' } as unknown as TextStyle;
  const inputStyle = [
    typography.body,
    text.textPrimary,
    view.flex1,
    compact ? view.minH32 : view.minH44,
    compact ? view.py4 : view.py8,
    noOutline,
  ];

  const changeText = (newText: string) => {
    if (type === 'date') onChangeText?.(formatDateInput(newText));
    else if (type === 'time') onChangeText?.(formatTimeInput(newText));
    else onChangeText?.(newText);
  };

  let trailing: { icon: IconName; onPress: () => void } | null = null;
  if (type === 'password')
    trailing = { icon: hidden ? 'EyeIcon' : 'EyeClosedIcon', onPress: () => setHidden(!hidden) };
  if (type === 'search' && value)
    trailing = { icon: 'CloseCircleIcon', onPress: () => onChangeText?.('') };

  return (
    <ModulView gap4>
      {floating ? null : <ModulText variant="label">{label}</ModulText>}
      <ModulView
        onLayout={floating ? (event) => setFieldHeight(event.nativeEvent.layout.height) : undefined}
        row
        alignCenter
        gap8
        px16
        rounded={!floating}
        roundedLg={floating}
        minH52={floating}
        bgSurface={!floating}
        bg={floating}
        bgHover={hovered && !disabled}
        border={!floating && !error && !focused && !showSuccess}
        borderSubtle={floating && !error && !raised && !showSuccess}
        borderBrand={floating && !error && raised}
        borderFocus={!floating && !error && focused}
        borderSuccess={showSuccess && !focused}
        borderError={!!error}
        onPointerEnter={() => setHovered(true)}
        onPointerLeave={() => setHovered(false)}
        style={disabled ? view.opacityDisabled : undefined}
      >
        {floating ? (
          <Animated.View
            pointerEvents="none"
            style={{
              position: 'absolute',
              top: 0,
              bottom: 0,
              left: view.px16.paddingHorizontal - view.px4.paddingHorizontal,
              justifyContent: 'center',
              zIndex: 1,
              transformOrigin: 'left center',
              transform: [
                {
                  translateY: labelProgress.interpolate({
                    inputRange: [0, 1],
                    outputRange: [0, -fieldHeight / 2],
                  }),
                },
                {
                  scale: labelProgress.interpolate({ inputRange: [0, 1], outputRange: [1, 0.875] }),
                },
              ],
            }}
          >
            <ModulView bg px4>
              <ModulText
                variant="body"
                textSecondary={!raised && !error}
                textAccent={raised && !error}
                textDanger={!!error}
              >
                {label}
              </ModulText>
            </ModulView>
          </Animated.View>
        ) : null}
        {leadingIcon ? <ModulIcon name={leadingIcon} color="textSecondary" /> : null}
        {type === 'select' ? (
          <Pressable
            accessibilityRole="combobox"
            accessibilityLabel={label}
            accessibilityState={{ expanded: menuOpen, disabled }}
            disabled={disabled}
            onPress={() => setMenuOpen(true)}
            style={[view.flex1, compact ? view.minH32 : view.minH44, view.justifyCenter]}
          >
            <ModulText variant="body" textSecondary={!selectedOption}>
              {selectedOption?.label ?? props.placeholder}
            </ModulText>
          </Pressable>
        ) : (
          <TextInput
            {...props}
            value={value}
            onChangeText={changeText}
            accessibilityLabel={label}
            accessibilityState={{ disabled, busy: loading }}
            placeholder={floating ? undefined : props.placeholder}
            editable={!disabled}
            secureTextEntry={type === 'password' && hidden}
            keyboardType={
              type === 'number' || type === 'date' || type === 'time' ? 'numeric' : undefined
            }
            maxLength={type === 'date' ? 10 : type === 'time' ? 5 : props.maxLength}
            returnKeyType={type === 'search' ? 'search' : props.returnKeyType}
            multiline={type === 'textarea'}
            placeholderTextColor={colors.textSecondary}
            onFocus={(event) => {
              setFocused(true);
              onFocus?.(event);
            }}
            onBlur={(event) => {
              setFocused(false);
              onBlur?.(event);
            }}
            style={[inputStyle, type === 'textarea' && { minHeight: 96, textAlignVertical: 'top' }]}
          />
        )}
        {trailing ? (
          <Pressable
            accessibilityRole="button"
            accessibilityLabel={actionLabel}
            disabled={disabled}
            onPress={trailing.onPress}
            style={[
              compact ? view.minW32 : view.minW44,
              compact ? view.minH32 : view.minH44,
              view.alignCenter,
              view.justifyCenter,
            ]}
          >
            <ModulIcon name={trailing.icon} color="textSecondary" />
          </Pressable>
        ) : null}
        {type === 'date' || type === 'time' ? (
          <ModulDatePicker
            mode={type}
            value={value ?? ''}
            onChange={(newValue) => onChangeText?.(newValue)}
            accessibilityLabel={actionLabel}
            disabled={disabled}
          />
        ) : null}
        {type === 'select' ? <ModulIcon name="AltArrowDownIcon" color="textSecondary" /> : null}
        {loading ? <ActivityIndicator color={colors.primary} size="small" /> : null}
        {showSuccess && !loading ? <ModulIcon name="CheckCircleIcon" color="success" /> : null}
      </ModulView>
      <ModulFieldMessage help={help} error={error} />
      {type === 'select' ? (
        <ModulMenu
          visible={menuOpen}
          onClose={() => setMenuOpen(false)}
          closeLabel={actionLabel ?? label}
          title={label}
          items={options.map((option) => ({
            key: option.value,
            label: option.label,
            selected: option.value === value,
            onPress: () => onChangeText?.(option.value),
          }))}
        />
      ) : null}
    </ModulView>
  );
}

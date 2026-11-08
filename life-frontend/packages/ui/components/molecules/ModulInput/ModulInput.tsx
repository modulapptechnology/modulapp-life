import { useState } from 'react';
import { Pressable, TextInput, type TextInputProps, type TextStyle } from 'react-native';
import { typography } from '@life/theme';
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
  label: string;
  help?: string;
  error?: string;
  disabled?: boolean;
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
  label,
  help,
  error,
  disabled = false,
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
      <ModulText variant="label">{label}</ModulText>
      <ModulView
        row
        alignCenter
        gap8
        px16
        rounded
        bgSurface
        border={!error && !focused}
        borderFocus={!error && focused}
        borderError={!!error}
        style={disabled ? view.opacityDisabled : undefined}
      >
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

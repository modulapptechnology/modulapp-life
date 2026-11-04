import { useState } from 'react';
import { TextInput, type TextInputProps } from 'react-native';
import { typography } from '@life/theme';
import { useStyles } from '../../utils/useStyles';
import { ModulFieldMessage } from '../atoms/ModulFieldMessage';
import { ModulText } from '../atoms/ModulText';
import { ModulView } from '../atoms/ModulView';

export type ModulInputType = 'text' | 'password' | 'number' | 'textarea';

export type ModulInputProps = Omit<
  TextInputProps,
  'style' | 'editable' | 'secureTextEntry' | 'keyboardType' | 'multiline' | 'accessibilityLabel'
> & {
  type?: ModulInputType;
  label: string;
  help?: string;
  error?: string;
  disabled?: boolean;
};

export function ModulInput({
                             type = 'text',
                             label,
                             help,
                             error,
                             disabled = false,
                             onFocus,
                             onBlur,
                             ...props
                           }: ModulInputProps) {
  const { view, text, colors } = useStyles();
  const [focused, setFocused] = useState(false);

  return (
    <ModulView gap4>
      <ModulText variant="label">{label}</ModulText>
      <TextInput
        {...props}
        accessibilityLabel={label}
        editable={!disabled}
        secureTextEntry={type === 'password'}
        keyboardType={type === 'number' ? 'numeric' : undefined}
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
        style={[
          typography.body,
          text.textPrimary,
          view.minH44,
          view.px16,
          view.py8,
          view.rounded,
          view.bgSurface,
          error ? view.borderError : focused ? view.borderFocus : view.border,
          type === 'textarea' && { minHeight: 96, textAlignVertical: 'top' },
          disabled && view.opacityDisabled,
        ]}
      />
      <ModulFieldMessage help={help} error={error} />
    </ModulView>
  );
}

import { useState } from 'react';
import { Pressable, type PressableProps } from 'react-native';
import { useStyles } from '../../utils/useStyles';
import { ModulText } from './ModulText';

export type ModulButtonProps = Omit<PressableProps, 'style' | 'children'> & {
  label: string;
  danger?: boolean;
};

export function ModulButton({ label, danger = false, disabled, onFocus, onBlur, ...props }: ModulButtonProps) {
  const { view } = useStyles();
  const [focused, setFocused] = useState(false);

  return (
    <Pressable
      accessibilityRole="button"
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
        view.px16,
        view.py8,
        view.alignCenter,
        view.justifyCenter,
        view.rounded,
        danger ? view.bgDanger : view.bgPrimary,
        focused && view.border,
        disabled && view.opacityDisabled,
      ]}
    >
      <ModulText variant="button" textOnPrimary>
        {label}
      </ModulText>
    </Pressable>
  );
}

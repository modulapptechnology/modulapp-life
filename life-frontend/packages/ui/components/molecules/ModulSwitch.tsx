import { Switch } from 'react-native';
import { useStyles } from '../../utils/useStyles';
import { ModulFieldMessage } from '../atoms/ModulFieldMessage';
import { ModulText } from '../atoms/ModulText';
import { ModulView } from '../atoms/ModulView';

export type ModulSwitchProps = {
  label: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
  help?: string;
  error?: string;
  disabled?: boolean;
};

export function ModulSwitch({ label, checked, onChange, help, error, disabled = false }: ModulSwitchProps) {
  const { view, colors } = useStyles();

  return (
    <ModulView gap4>
      <ModulView row alignCenter justifyBetween gap8 minH44 style={disabled ? view.opacityDisabled : undefined}>
        <ModulText variant="label">{label}</ModulText>
        <Switch
          value={checked}
          onValueChange={onChange}
          disabled={disabled}
          accessibilityLabel={label}
          trackColor={{ false: colors.border, true: colors.primary }}
          thumbColor={colors.white}
        />
      </ModulView>
      <ModulFieldMessage help={help} error={error} />
    </ModulView>
  );
}

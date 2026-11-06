import { Pressable } from 'react-native';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulFieldMessage } from '../../atoms/ModulFieldMessage/ModulFieldMessage';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';

export type ModulCheckboxProps = {
  type?: 'checkbox' | 'radio';
  label: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
  round?: boolean;
  help?: string;
  error?: string;
  disabled?: boolean;
};

export function ModulCheckbox({
  type = 'checkbox',
  label,
  checked,
  onChange,
  round = false,
  help,
  error,
  disabled = false,
}: ModulCheckboxProps) {
  const { view } = useStyles();
  const circular = round || type === 'radio';

  return (
    <ModulView gap4>
      <Pressable
        accessibilityRole={type}
        accessibilityLabel={label}
        accessibilityState={{ checked, disabled }}
        disabled={disabled}
        onPress={() => onChange(type === 'radio' ? true : !checked)}
        style={[
          view.row,
          view.alignCenter,
          view.gap8,
          view.minH44,
          disabled && view.opacityDisabled,
        ]}
      >
        <ModulView
          alignCenter
          justifyCenter
          border
          rounded={!circular}
          roundedFull={circular}
          bgPrimary={checked && type === 'checkbox'}
          style={{ width: 24, height: 24 }}
        >
          {checked && type === 'checkbox' ? (
            <ModulText variant="label" textOnPrimary>
              ✓
            </ModulText>
          ) : null}
          {checked && type === 'radio' ? (
            <ModulView bgPrimary roundedFull style={{ width: 12, height: 12 }} />
          ) : null}
        </ModulView>
        <ModulText variant="body">{label}</ModulText>
      </Pressable>
      <ModulFieldMessage help={help} error={error} />
    </ModulView>
  );
}

import { ModulFieldMessage } from '../../atoms/ModulFieldMessage/ModulFieldMessage';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { ModulCheckbox } from '../ModulCheckbox/ModulCheckbox';

export type ModulOptionGroupOption = {
  value: string;
  label: string;
};

export type ModulOptionGroupProps = {
  label: string;
  options: ModulOptionGroupOption[];
  value: string;
  onChange: (value: string) => void;
  help?: string;
  error?: string;
  disabled?: boolean;
};

// Grupo de opciones excluyentes: una sola seleccionada, con etiqueta, ayuda y error del grupo.
export function ModulOptionGroup({
  label,
  options,
  value,
  onChange,
  help,
  error,
  disabled = false,
}: ModulOptionGroupProps) {
  return (
    <ModulView gap4 accessibilityRole="radiogroup" accessibilityLabel={label}>
      <ModulText variant="label">{label}</ModulText>
      {options.map((option) => (
        <ModulCheckbox
          key={option.value}
          type="radio"
          label={option.label}
          checked={option.value === value}
          disabled={disabled}
          onChange={() => onChange(option.value)}
        />
      ))}
      <ModulFieldMessage help={help} error={error} />
    </ModulView>
  );
}

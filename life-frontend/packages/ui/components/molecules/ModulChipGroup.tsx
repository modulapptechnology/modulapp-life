import { ScrollView } from 'react-native';
import { useStyles } from '../../utils/useStyles';
import { ModulChip } from '../atoms/ModulChip';

export type ModulChipOption = {
  value: string;
  label: string;
};

export type ModulChipGroupProps = {
  options: ModulChipOption[];
  value: string;
  onChange: (value: string) => void;
};

export function ModulChipGroup({ options, value, onChange }: ModulChipGroupProps) {
  const { view } = useStyles();

  return (
    <ScrollView
      horizontal
      accessibilityRole="tablist"
      showsHorizontalScrollIndicator={false}
      contentContainerStyle={[view.row, view.gap8, view.px16]}
    >
      {options.map((option) => (
        <ModulChip
          key={option.value}
          label={option.label}
          selected={option.value === value}
          onPress={() => onChange(option.value)}
        />
      ))}
    </ScrollView>
  );
}

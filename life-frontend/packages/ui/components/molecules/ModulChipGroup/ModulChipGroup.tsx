import { ScrollView } from 'react-native';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulChip } from '../../atoms/ModulChip/ModulChip';
import { ModulView } from '../../atoms/ModulView/ModulView';

export type ModulChipOption = {
  value: string;
  label: string;
};

export type ModulChipGroupProps = {
  options: ModulChipOption[];
  value: string;
  onChange: (value: string) => void;
  // Control segmentado: las opciones se reparten el ancho en una sola fila, sin deslizador.
  segmented?: boolean;
};

export function ModulChipGroup({
  options,
  value,
  onChange,
  segmented = false,
}: ModulChipGroupProps) {
  const { view } = useStyles();

  const chips = options.map((option) => (
    <ModulChip
      key={option.value}
      label={option.label}
      segmented={segmented}
      selected={option.value === value}
      onPress={() => onChange(option.value)}
    />
  ));

  if (segmented) {
    return (
      <ModulView row gap4 p4 rounded bgSurface borderSubtle accessibilityRole="tablist">
        {chips}
      </ModulView>
    );
  }

  return (
    <ScrollView
      horizontal
      accessibilityRole="tablist"
      showsHorizontalScrollIndicator={false}
      contentContainerStyle={[view.row, view.gap8, view.px16]}
    >
      {chips}
    </ScrollView>
  );
}

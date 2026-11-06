import type { ThemeColors } from '@life/theme';
import { scaleBars } from '../../utils/chartScale';
import { useStyles } from '../../utils/useStyles';
import { ModulText } from '../atoms/ModulText';
import { ModulView } from '../atoms/ModulView';

export type ModulChartDatum = {
  label: string;
  value: number;
  color?: keyof ThemeColors;
};

export type ModulBarChartProps = {
  data: ModulChartDatum[];
  accessibilityLabel: string;
  height?: number;
  formatValue?: (value: number) => string;
};

// Colores del tema que usan las barras cuando el dato no trae uno propio.
export const CHART_COLORS: (keyof ThemeColors)[] = ['primary', 'secondary', 'info', 'success', 'warning'];

export function ModulBarChart({ data, accessibilityLabel, height = 160, formatValue = String }: ModulBarChartProps) {
  const { colors } = useStyles();
  const heights = scaleBars(
    data.map((datum) => datum.value),
    height,
  );
  const colorOf = (datum: ModulChartDatum, index: number) =>
    colors[datum.color ?? CHART_COLORS[index % CHART_COLORS.length]];

  return (
    <ModulView gap16 accessibilityRole="image" accessibilityLabel={accessibilityLabel}>
      <ModulView row alignCenter gap8 style={{ height: height + 24, alignItems: 'flex-end' }}>
        {data.map((datum, index) => (
          <ModulView key={datum.label} flex1 alignCenter gap4>
            <ModulText variant="bodySecondary" textSecondary>
              {formatValue(datum.value)}
            </ModulText>
            <ModulView rounded w100 style={{ height: heights[index], backgroundColor: colorOf(datum, index) }} />
          </ModulView>
        ))}
      </ModulView>
      <ModulView row wrap gap16>
        {data.map((datum, index) => (
          <ModulView key={datum.label} row alignCenter gap8>
            <ModulView roundedFull style={{ width: 12, height: 12, backgroundColor: colorOf(datum, index) }} />
            <ModulText variant="bodySecondary">{datum.label}</ModulText>
          </ModulView>
        ))}
      </ModulView>
    </ModulView>
  );
}

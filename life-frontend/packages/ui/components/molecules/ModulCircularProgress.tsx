import type { ThemeColors } from '@life/theme';
import { activeTicks } from '../../utils/chartScale';
import { clampPercent } from '../../utils/clampPercent';
import { useStyles } from '../../utils/useStyles';
import { ModulText } from '../atoms/ModulText';
import { ModulView } from '../atoms/ModulView';

export type ModulCircularProgressProps = {
  value: number;
  label: string;
  size?: number;
  color?: keyof ThemeColors;
};

const TICKS = 60;

// Anillo hecho con marcas de View (sin SVG), así funciona igual en móvil, web y escritorio.
export function ModulCircularProgress({ value, label, size = 96, color = 'primary' }: ModulCircularProgressProps) {
  const { colors } = useStyles();
  const percent = clampPercent(value);
  const lit = activeTicks(percent, TICKS);
  const tickLength = Math.round(size * 0.14);

  return (
    <ModulView
      alignCenter
      gap8
      accessibilityRole="progressbar"
      accessibilityLabel={label}
      accessibilityValue={{ min: 0, max: 100, now: percent }}
    >
      <ModulView alignCenter justifyCenter style={{ width: size, height: size }}>
        {Array.from({ length: TICKS }, (_, index) => (
          <ModulView
            key={index}
            roundedFull
            absolute
            style={{
              width: 3,
              height: tickLength,
              top: 0,
              left: size / 2 - 1.5,
              backgroundColor: index < lit ? colors[color] : colors.border,
              transform: [{ translateY: size / 2 - tickLength / 2 }, { rotate: `${index * (360 / TICKS)}deg` }, { translateY: -(size / 2 - tickLength / 2) }],
            }}
          />
        ))}
        <ModulText variant="label">{`${Math.round(percent)}%`}</ModulText>
      </ModulView>
      <ModulText variant="bodySecondary" textSecondary>
        {label}
      </ModulText>
    </ModulView>
  );
}

import { clampPercent } from '../../../utils/clampPercent/clampPercent';
import { ModulView } from '../ModulView/ModulView';

export type ModulProgressProps = {
  value: number;
  accessibilityLabel: string;
};

export function ModulProgress({ value, accessibilityLabel }: ModulProgressProps) {
  const percent = clampPercent(value);

  return (
    <ModulView
      bgSurface
      borderSubtle
      roundedFull
      overflowHidden
      accessibilityRole="progressbar"
      accessibilityLabel={accessibilityLabel}
      accessibilityValue={{ min: 0, max: 100, now: percent }}
      style={{ height: 8 }}
    >
      <ModulView bgPrimary style={{ width: `${percent}%`, height: '100%' }} />
    </ModulView>
  );
}

import { ModulButton, type ModulButtonProps } from '../ModulButton/ModulButton';
import type { IconName } from '../ModulIcon/ModulIcon';
import { ModulView } from '../ModulView/ModulView';

export type ModulFloatingButtonProps = Omit<
  ModulButtonProps,
  'label' | 'variant' | 'danger' | 'icon'
> & {
  icon: IconName;
  accessibilityLabel: string;
};

export function ModulFloatingButton({
  icon,
  accessibilityLabel,
  ...props
}: ModulFloatingButtonProps) {
  return (
    <ModulView absolute bottom16 right16 roundedFull overflowHidden>
      <ModulButton {...props} icon={icon} accessibilityLabel={accessibilityLabel} />
    </ModulView>
  );
}

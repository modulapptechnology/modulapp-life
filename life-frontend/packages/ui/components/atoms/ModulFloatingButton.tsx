import { ModulButton, type ModulButtonProps } from './ModulButton';
import type { IconName } from './ModulIcon';
import { ModulView } from './ModulView';

export type ModulFloatingButtonProps = Omit<ModulButtonProps, 'label' | 'variant' | 'danger' | 'icon'> & {
  icon: IconName;
  accessibilityLabel: string;
};

export function ModulFloatingButton({ icon, accessibilityLabel, ...props }: ModulFloatingButtonProps) {
  return (
    <ModulView absolute bottom16 right16 roundedFull overflowHidden>
      <ModulButton {...props} icon={icon} accessibilityLabel={accessibilityLabel} />
    </ModulView>
  );
}

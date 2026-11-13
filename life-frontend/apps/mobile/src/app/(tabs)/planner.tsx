import { translate } from '@life/core';
import { ModulText, ModulView } from '@life/ui';

export default function PlannerScreen() {
  return (
    <ModulView flex1 bgSurface alignCenter justifyCenter>
      <ModulText>{translate('module.agenda')}</ModulText>
    </ModulView>
  );
}

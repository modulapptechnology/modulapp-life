import { translate } from '@life/core';
import { ModulText, ModulView } from '@life/ui';

export default function ModulesScreen() {
  return (
    <ModulView flex1 bgSurface alignCenter justifyCenter>
      <ModulText>{translate('nav.modules')}</ModulText>
    </ModulView>
  );
}

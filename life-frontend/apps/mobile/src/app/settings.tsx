import { translate } from '@life/core';
import { ModulText, ModulView } from '@life/ui';

export default function SettingsScreen() {
  return (
    <ModulView flex1 bgSurface alignCenter justifyCenter>
      <ModulText>{translate('user.settings')}</ModulText>
    </ModulView>
  );
}

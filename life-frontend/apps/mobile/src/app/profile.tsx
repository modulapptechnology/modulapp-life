import { translate } from '@life/core';
import { ModulText, ModulView } from '@life/ui';

export default function ProfileScreen() {
  return (
    <ModulView flex1 bgSurface alignCenter justifyCenter>
      <ModulText>{translate('user.profile')}</ModulText>
    </ModulView>
  );
}

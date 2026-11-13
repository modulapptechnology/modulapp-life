import { translate } from '@life/core';
import { ModulText, ModulView } from '@life/ui';

export default function InboxScreen() {
  return (
    <ModulView flex1 bgSurface alignCenter justifyCenter>
      <ModulText>{translate('home.inbox')}</ModulText>
    </ModulView>
  );
}

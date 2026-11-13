import { translate, useSettings } from '@life/core';
import { ModulSettings, ModulView } from '@life/ui';
import { MobileHeader } from '../components/MobileHeader';

export default function SettingsScreen() {
  const settings = useSettings('mobile');

  return (
    <ModulView flex1 bgSurface>
      <MobileHeader title={translate('user.settings')} />
      <ModulSettings {...settings} />
    </ModulView>
  );
}

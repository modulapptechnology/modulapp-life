import { useSettings } from '@life/core';
import { ModulSettings, ModulView } from '@life/ui';

export default function SettingsScreen() {
  const settings = useSettings('mobile');

  return (
    <ModulView flex1 bgSurface>
      <ModulSettings {...settings} />
    </ModulView>
  );
}

import { useSettings } from '@life/core';
import { ModulSettings } from '@life/ui';

export default function SettingsPage() {
  const settings = useSettings('web');

  return <ModulSettings {...settings} />;
}

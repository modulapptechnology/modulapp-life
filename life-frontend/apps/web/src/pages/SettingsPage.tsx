import { useOutletContext } from 'react-router-dom';
import { translate, useSettings } from '@life/core';
import { ModulSettings } from '@life/ui';

// Lo que el layout de la web entrega a sus rutas hijas.
type LayoutContext = { openShortcuts: () => void };

export default function SettingsPage() {
  const settings = useSettings('web');
  const { openShortcuts } = useOutletContext<LayoutContext>();

  return (
    <ModulSettings
      {...settings}
      shortcuts={{ label: translate('settings.preferences.shortcuts'), onPress: openShortcuts }}
    />
  );
}

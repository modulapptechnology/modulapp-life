import { useState } from 'react';
import { MOBILE_TABS, translate } from '@life/core';
import { ModulBottomSheet, ModulTabBar, ModulText, ModulView } from '@life/ui';
import { router, usePathname, type Href } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

export function MobileTabBar() {
  const pathname = usePathname();
  const { bottom } = useSafeAreaInsets();
  const [captureOpen, setCaptureOpen] = useState(false);
  const items = MOBILE_TABS.map((tab) => ({
    path: tab.path,
    label: translate(tab.nameKey),
    icon: tab.icon,
  }));

  return (
    <ModulView bg style={{ paddingBottom: bottom }}>
      <ModulTabBar
        items={items}
        activePath={pathname}
        onNavigate={(path) => router.navigate(path as Href)}
        action={{
          icon: 'AddIcon',
          label: translate('menu.newCapture'),
          onPress: () => setCaptureOpen(true),
        }}
      />
      <ModulBottomSheet
        visible={captureOpen}
        title={translate('menu.newCapture')}
        closeLabel={translate('common.close')}
        onClose={() => setCaptureOpen(false)}
      >
        <ModulText variant="body">{translate('auth.comingSoon')}</ModulText>
      </ModulBottomSheet>
    </ModulView>
  );
}

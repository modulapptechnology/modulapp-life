import { translate } from '@life/core';
import { ModulListItem, ModulText, ModulView } from '@life/ui';
import { router } from 'expo-router';
import { MobileHeader } from '../components/MobileHeader';

export default function ProfileScreen() {
  return (
    <ModulView flex1 bgSurface>
      <MobileHeader title={translate('user.profile')} />
      <ModulView flex1 gap16 py8>
        <ModulListItem
          icon="SettingsIcon"
          title={translate('user.settings')}
          onPress={() => router.navigate('/settings')}
        />
        <ModulView flex1 alignCenter justifyCenter>
          <ModulText>{translate('user.profile')}</ModulText>
        </ModulView>
      </ModulView>
    </ModulView>
  );
}

import { session, translate, useSession } from '@life/core';
import { ModulButton, ModulListItem, ModulText, ModulView, useMessages } from '@life/ui';
import { router } from 'expo-router';
import { MobileHeader } from '../components/MobileHeader';
import { sessionManager } from '../services/session/session';

export default function ProfileScreen() {
  const { confirm } = useMessages();
  const { pendingOperations } = useSession(session);

  const signOut = async () => {
    if (pendingOperations > 0) {
      const accepted = await confirm({
        title: translate('user.signOutTitle'),
        message: translate('user.signOutPending'),
        confirmLabel: translate('user.signOut'),
        cancelLabel: translate('common.cancel'),
        destructive: true,
      });
      if (!accepted) return;
    }
    await sessionManager.signOut();
  };

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
        <ModulView p16>
          <ModulButton
            variant="secondary"
            danger
            label={translate('user.signOut')}
            onPress={() => void signOut()}
          />
        </ModulView>
      </ModulView>
    </ModulView>
  );
}

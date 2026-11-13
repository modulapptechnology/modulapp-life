import { Pressable } from 'react-native';
import { session, translate, useSession } from '@life/core';
import { ModulAvatar, ModulHeader, ModulIcon, ModulText, ModulView } from '@life/ui';
import { router } from 'expo-router';

type MobileHeaderProps = {
  // Con título, el encabezado es el de una pantalla de detalle: flecha de volver y título.
  title?: string;
};

function goBack() {
  if (router.canGoBack()) router.back();
  else router.replace('/');
}

export function MobileHeader({ title }: MobileHeaderProps) {
  const { user } = useSession(session);

  return (
    <ModulHeader
      end={
        <ModulView
          accessible
          accessibilityLabel={translate('settings.notifications.title')}
          alignCenter
          justifyCenter
          minH44
          minW44
        >
          <ModulIcon name="BellIcon" color="textSecondary" />
        </ModulView>
      }
    >
      {title ? (
        <ModulView row alignCenter gap8>
          <Pressable
            accessibilityRole="button"
            accessibilityLabel={translate('common.back')}
            onPress={goBack}
            hitSlop={8}
          >
            <ModulIcon name="ArrowLeftIcon" color="text" />
          </Pressable>
          <ModulText variant="body" fwBold>
            {title}
          </ModulText>
        </ModulView>
      ) : user ? (
        <Pressable
          accessibilityRole="button"
          accessibilityLabel={translate('user.profile')}
          onPress={() => router.navigate('/profile')}
        >
          <ModulView row alignCenter gap8>
            <ModulAvatar name={user.name} uri={user.avatarUri} size="sm" />
            <ModulText variant="body" fwBold>
              {user.name}
            </ModulText>
          </ModulView>
        </Pressable>
      ) : null}
    </ModulHeader>
  );
}

import { APP_NAME } from '@life/constants';
import { session, useSession } from '@life/core';
import { ModulText, ModulView } from '@life/ui';
import { Redirect } from 'expo-router';
export default function HomeScreen() {
  const { user } = useSession(session);

  if (!user) return <Redirect href="/welcome" />;
  return (
    <ModulView flex1 bgSurface alignCenter justifyCenter>
      <ModulText>{APP_NAME}</ModulText>
    </ModulView>
  );
}

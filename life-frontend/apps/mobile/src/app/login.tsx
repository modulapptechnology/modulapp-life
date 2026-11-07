import { session, translate } from '@life/core';
import { ModulButton, ModulText, ModulView } from '@life/ui';

export default function LoginScreen() {
  return (
    <ModulView flex1 bgSurface alignCenter justifyCenter gap16>
      <ModulText variant="sectionTitle">{translate('user.login')}</ModulText>
      <ModulButton
        label={translate('user.loginDemo')}
        onPress={() => session.signIn({ id: 'demo', name: 'Usuario Demo' })}
      />
    </ModulView>
  );
}

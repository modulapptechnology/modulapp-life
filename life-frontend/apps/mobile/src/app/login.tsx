import { StyleSheet, View } from 'react-native';
import { session, translate } from '@life/core';
import { ModulButton, ModulText } from '@life/ui';

export default function LoginScreen() {
  return (
    <View style={styles.container}>
      <ModulText variant="sectionTitle">{translate('user.login')}</ModulText>
      <ModulButton
        label={translate('user.loginDemo')}
        onPress={() => session.signIn({ id: 'demo', name: 'Usuario Demo' })}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    gap: 16,
  },
});

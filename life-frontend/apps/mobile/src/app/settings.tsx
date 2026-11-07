import { StyleSheet, Text, View } from 'react-native';
import { translate } from '@life/core';

export default function SettingsScreen() {
  return (
    <View style={styles.container}>
      <Text>{translate('user.settings')}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
});

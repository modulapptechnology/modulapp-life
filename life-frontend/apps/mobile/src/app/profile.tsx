import { StyleSheet, Text, View } from 'react-native';
import { translate } from '@life/core';

export default function ProfileScreen() {
  return (
    <View style={styles.container}>
      <Text>{translate('user.profile')}</Text>
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

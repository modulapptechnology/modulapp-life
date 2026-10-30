import { StyleSheet, Text, View } from 'react-native';
import { APP_NAME } from '@life/constants';

export default function HomeScreen() {
  return (
    <View style={styles.container}>
      <Text>{APP_NAME}</Text>
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

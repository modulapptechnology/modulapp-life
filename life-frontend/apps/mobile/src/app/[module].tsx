import { StyleSheet, Text, View } from 'react-native';
import { Redirect, useLocalSearchParams } from 'expo-router';
import { moduleRegistry, translate, useEnabledModules } from '@life/core';

// Una sola ruta para todos los módulos: uno nuevo del registro funciona sin crear un archivo, y uno deshabilitado vuelve al inicio.
export default function ModuleScreen() {
  const { module } = useLocalSearchParams<{ module: string }>();
  const modules = useEnabledModules(moduleRegistry);
  const current = modules.find((item) => item.id === module);

  if (!current) return <Redirect href="/" />;

  return (
    <View style={styles.container}>
      <Text>{translate(current.nameKey)}</Text>
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

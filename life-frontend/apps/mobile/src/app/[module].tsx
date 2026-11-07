import { Redirect, useLocalSearchParams } from 'expo-router';
import { moduleRegistry, translate, useEnabledModules } from '@life/core';
import { ModulText, ModulView } from '@life/ui';

// Una sola ruta para todos los módulos: uno nuevo del registro funciona sin crear un archivo, y uno deshabilitado vuelve al inicio.
export default function ModuleScreen() {
  const { module } = useLocalSearchParams<{ module: string }>();
  const modules = useEnabledModules(moduleRegistry);
  const current = modules.find((item) => item.id === module);

  if (!current) return <Redirect href="/" />;

  return (
    <ModulView flex1 bgSurface alignCenter justifyCenter>
      <ModulText>{translate(current.nameKey)}</ModulText>
    </ModulView>
  );
}

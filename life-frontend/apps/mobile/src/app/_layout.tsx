import { useState } from 'react';
import { Audiowide_400Regular } from '@expo-google-fonts/audiowide';
import { Comfortaa_400Regular, Comfortaa_700Bold, useFonts } from '@expo-google-fonts/comfortaa';
import { ModulHeader, ModulMessagesProvider, ThemeProvider } from '@life/ui';
import { Stack } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';

export default function RootLayout() {
  const [fontsLoaded] = useFonts({ Audiowide_400Regular, Comfortaa_400Regular, Comfortaa_700Bold });
  const [sidebarOpen, setSidebarOpen] = useState(true);

  if (!fontsLoaded) return null;

  return (
    <ThemeProvider>
      <ModulMessagesProvider confirmLabel="Confirmar" cancelLabel="Cancelar" closeLabel="Cerrar">
        <SafeAreaView style={{ flex: 1 }} edges={['top', 'left', 'right']}>
          <ModulHeader
            sidebarOpen={sidebarOpen}
            onToggleSidebar={() => setSidebarOpen((open) => !open)}
          />
          <Stack screenOptions={{ headerShown: false }} />
        </SafeAreaView>
      </ModulMessagesProvider>
    </ThemeProvider>
  );
}

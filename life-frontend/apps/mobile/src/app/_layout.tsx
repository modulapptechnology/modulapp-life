import { useState } from 'react';
import { Audiowide_400Regular } from '@expo-google-fonts/audiowide';
import { Comfortaa_400Regular, Comfortaa_700Bold, useFonts } from '@expo-google-fonts/comfortaa';
import { session, useSession } from '@life/core';
import { ModulHeader, ModulMessagesProvider, ModulView, ThemeProvider } from '@life/ui';
import { Stack } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MobileSidebar } from '../components/MobileSidebar/MobileSidebar';

export default function RootLayout() {
  const [fontsLoaded] = useFonts({ Audiowide_400Regular, Comfortaa_400Regular, Comfortaa_700Bold });
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { user } = useSession(session);

  if (!fontsLoaded) return null;

  return (
    <ThemeProvider>
      <ModulMessagesProvider confirmLabel="Confirmar" cancelLabel="Cancelar" closeLabel="Cerrar">
        <SafeAreaView style={{ flex: 1 }} edges={['top', 'left', 'right']}>
          {user ? (
            <ModulHeader
              sidebarOpen={sidebarOpen}
              onToggleSidebar={() => setSidebarOpen((open) => !open)}
            />
          ) : null}
          <ModulView flex1>
            <Stack screenOptions={{ headerShown: false }}>
              <Stack.Protected guard={!user}>
                <Stack.Screen name="login" />
              </Stack.Protected>
              <Stack.Protected guard={!!user}>
                <Stack.Screen name="index" />
                <Stack.Screen name="profile" />
                <Stack.Screen name="settings" />
                <Stack.Screen name="[module]" />
              </Stack.Protected>
            </Stack>
            {user ? (
              <MobileSidebar visible={sidebarOpen} onClose={() => setSidebarOpen(false)} />
            ) : null}
          </ModulView>
        </SafeAreaView>
      </ModulMessagesProvider>
    </ThemeProvider>
  );
}

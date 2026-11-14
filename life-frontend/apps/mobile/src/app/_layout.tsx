import { useEffect, useRef } from 'react';
import { Audiowide_400Regular } from '@expo-google-fonts/audiowide';
import { Comfortaa_400Regular, Comfortaa_700Bold, useFonts } from '@expo-google-fonts/comfortaa';
import { appearanceStore, resolveReturnPath, session, useAppearance, useSession } from '@life/core';
import { ModulMessagesProvider, ModulView, ThemeProvider } from '@life/ui';
import { router, Stack, usePathname } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { connectivityMonitor } from '../services/connectivity/connectivity';
import { sessionManager } from '../services/session/session';
export const unstable_settings = {
  anchor: 'welcome',
};

const PUBLIC_PATHS = [
  '/welcome',
  '/login',
  '/register',
  '/forgot-password',
  '/reset-password',
  '/verify-email',
];

export default function RootLayout() {
  const [fontsLoaded] = useFonts({ Audiowide_400Regular, Comfortaa_400Regular, Comfortaa_700Bold });
  const appearance = useAppearance(appearanceStore);
  const { user, restoring } = useSession(session);
  const pathname = usePathname();
  const returnTo = useRef<string | null>(null);

  useEffect(() => {
    void sessionManager.restore();
  }, []);

  useEffect(() => {
    if (restoring) return;
    if (!user) {
      if (pathname !== '/' && !PUBLIC_PATHS.includes(pathname)) returnTo.current = pathname;
      return;
    }
    if (returnTo.current) {
      router.replace(resolveReturnPath(returnTo.current) as never);
      returnTo.current = null;
    }
  }, [user, restoring, pathname]);

  useEffect(() => connectivityMonitor.start(), []);

  if (!fontsLoaded || restoring) return null;

  return (
    <ThemeProvider preference={appearance.preference} accent={appearance.accent}>
      <ModulMessagesProvider confirmLabel="Confirmar" cancelLabel="Cancelar" closeLabel="Cerrar">
        <ModulView flex1 bgSurface>
          <SafeAreaView style={{ flex: 1 }} edges={['top', 'left', 'right']}>
            <ModulView flex1>
              <Stack screenOptions={{ headerShown: false }}>
                <Stack.Protected guard={!user}>
                  <Stack.Screen name="welcome" />
                  <Stack.Screen name="login" />
                  <Stack.Screen name="register" />
                  <Stack.Screen name="forgot-password" />
                </Stack.Protected>
                <Stack.Protected guard={!!user}>
                  <Stack.Screen name="(tabs)" />
                  <Stack.Screen name="profile" />
                  <Stack.Screen name="settings" />
                </Stack.Protected>
                {/* Se abren desde el enlace del correo, con o sin sesión iniciada. */}
                <Stack.Screen name="reset-password" />
                <Stack.Screen name="verify-email" />
              </Stack>
            </ModulView>
          </SafeAreaView>
        </ModulView>
      </ModulMessagesProvider>
    </ThemeProvider>
  );
}

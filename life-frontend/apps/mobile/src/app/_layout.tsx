import { Audiowide_400Regular } from '@expo-google-fonts/audiowide';
import { Comfortaa_400Regular, Comfortaa_700Bold, useFonts } from '@expo-google-fonts/comfortaa';
import { ThemeProvider } from '@life/ui';
import { Stack } from 'expo-router';

export default function RootLayout() {
  const [fontsLoaded] = useFonts({ Audiowide_400Regular, Comfortaa_400Regular, Comfortaa_700Bold });

  if (!fontsLoaded) return null;

  return (
    <ThemeProvider>
      <Stack />
    </ThemeProvider>
  );
}

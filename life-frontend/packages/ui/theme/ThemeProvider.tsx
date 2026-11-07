import { createContext, useContext, useMemo, type ReactNode } from 'react';
import { useColorScheme } from 'react-native';
import {
  createThemeColors,
  type ThemeAccent,
  type ThemeColors,
  type ThemePreference,
} from '@life/theme';

type ThemeContextValue = {
  preference: ThemePreference;
  accent: ThemeAccent;
  scheme: 'light' | 'dark';
  colors: ThemeColors;
};

const ThemeContext = createContext<ThemeContextValue | null>(null);

type ThemeProviderProps = {
  preference?: ThemePreference;
  accent?: ThemeAccent;
  children: ReactNode;
};

export function ThemeProvider({
  preference = 'system',
  accent = 'modul',
  children,
}: ThemeProviderProps) {
  const systemScheme = useColorScheme();
  const scheme = preference === 'system' ? (systemScheme ?? 'light') : preference;

  const value = useMemo<ThemeContextValue>(
    () => ({ preference, accent, scheme, colors: createThemeColors(scheme, accent) }),
    [preference, accent, scheme],
  );

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme() {
  const context = useContext(ThemeContext);
  if (!context) throw new Error('useTheme debe usarse dentro de <ThemeProvider>');
  return context;
}

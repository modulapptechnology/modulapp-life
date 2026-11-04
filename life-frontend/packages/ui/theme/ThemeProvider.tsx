import { createContext, useContext, useMemo, type ReactNode } from 'react';
import { useColorScheme } from 'react-native';
import { darkColors, lightColors, type ThemeColors, type ThemePreference } from '@life/theme';

type ThemeContextValue = {
  preference: ThemePreference;
  scheme: 'light' | 'dark';
  colors: ThemeColors;
};

const ThemeContext = createContext<ThemeContextValue | null>(null);

type ThemeProviderProps = {
  preference?: ThemePreference;
  children: ReactNode;
};

export function ThemeProvider({ preference = 'system', children }: ThemeProviderProps) {
  const systemScheme = useColorScheme();
  const scheme = preference === 'system' ? (systemScheme ?? 'light') : preference;

  const value = useMemo<ThemeContextValue>(
    () => ({ preference, scheme, colors: scheme === 'dark' ? darkColors : lightColors }),
    [preference, scheme],
  );

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme() {
  const context = useContext(ThemeContext);
  if (!context) throw new Error('useTheme debe usarse dentro de <ThemeProvider>');
  return context;
}

import type { ReactElement } from 'react';
import { render } from '@testing-library/react-native';
import type { ThemePreference } from '@life/theme';
import { ThemeProvider } from '../../theme/ThemeProvider';

// Dibuja un componente dentro del ThemeProvider, igual que lo usa la app.
export function renderWithTheme(ui: ReactElement, preference: ThemePreference = 'light') {
  return render(<ThemeProvider preference={preference}>{ui}</ThemeProvider>);
}

import { appearanceStore, useAppearance } from '@life/core';
import { createScrollbarCss } from '@life/theme';
import { ModulMessagesProvider, ThemeProvider, useTheme } from '@life/ui';
import App from './App';

function ScrollbarStyles() {
  const { colors } = useTheme();

  return <style>{createScrollbarCss(colors)}</style>;
}

export default function AppRoot() {
  const appearance = useAppearance(appearanceStore);

  return (
    <ThemeProvider preference={appearance.preference} accent={appearance.accent}>
      <ScrollbarStyles />
      <ModulMessagesProvider confirmLabel="Confirmar" cancelLabel="Cancelar" closeLabel="Cerrar">
        <App />
      </ModulMessagesProvider>
    </ThemeProvider>
  );
}

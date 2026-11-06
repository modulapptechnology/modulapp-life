import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { ModulMessagesProvider, ThemeProvider } from '@life/ui';
import '@fontsource/audiowide/400.css';
import '@fontsource/comfortaa/400.css';
import '@fontsource/comfortaa/700.css';
import './index.css';
import App from './App';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <ThemeProvider>
        <ModulMessagesProvider confirmLabel="Confirmar" cancelLabel="Cancelar" closeLabel="Cerrar">
          <App />
        </ModulMessagesProvider>
      </ThemeProvider>
    </BrowserRouter>
  </StrictMode>,
);

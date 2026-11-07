import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import '@fontsource/audiowide/400.css';
import '@fontsource/comfortaa/400.css';
import '@fontsource/comfortaa/700.css';
import './index.css';
import AppRoot from './AppRoot';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <AppRoot />
    </BrowserRouter>
  </StrictMode>,
);

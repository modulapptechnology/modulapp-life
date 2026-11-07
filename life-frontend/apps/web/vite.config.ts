import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  // Las animaciones de React Native (Animated) usan `global`, que el navegador no define.
  define: { global: 'globalThis' },
  server: { port: 5173, strictPort: true },
  // Web y escritorio leen la misma URL de la API que la móvil: los .env* de life-frontend.
  envDir: '../..',
  envPrefix: 'EXPO_PUBLIC_',
  resolve: {
    alias: { 'react-native': 'react-native-web' },
    extensions: ['.web.tsx', '.web.ts', '.tsx', '.ts', '.js', '.json'],
  },
});

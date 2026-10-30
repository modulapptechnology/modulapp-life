import js from '@eslint/js';
import { defineConfig } from 'eslint/config';
import tseslint from 'typescript-eslint';

export default defineConfig(js.configs.recommended, tseslint.configs.recommended, {
  rules: {
    'no-restricted-imports': [
      'error',
      {
        patterns: [
          {
            group: ['react-native', 'react-native/*', 'expo', 'expo-*', 'react-dom', 'react-dom/*'],
            message: 'Los paquetes son TypeScript puro, sin imports específicos de plataforma.',
          },
        ],
      },
    ],
  },
});

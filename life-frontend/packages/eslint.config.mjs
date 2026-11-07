import js from '@eslint/js';
import { defineConfig } from 'eslint/config';
import tseslint from 'typescript-eslint';
import english from '../eslint.english.cjs';

export default defineConfig(
  js.configs.recommended,
  tseslint.configs.recommended,
  english,
  {
    rules: {
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              group: [
                'react-native',
                'react-native/*',
                'expo',
                'expo-*',
                'react-dom',
                'react-dom/*',
              ],
              message: 'Los paquetes son TypeScript puro, sin imports específicos de plataforma.',
            },
          ],
        },
      ],
    },
  },
  {
    // @life/icons es el único paquete que reexporta librerías por plataforma.
    files: ['icons/**/*.ts', 'ui/**/*.{ts,tsx}'],
    rules: { 'no-restricted-imports': 'off' },
  },
);

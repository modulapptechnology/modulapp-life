import js from '@eslint/js';
import { defineConfig, globalIgnores } from 'eslint/config';
import tseslint from 'typescript-eslint';
import english from '../../eslint.english.cjs';

export default defineConfig(
  globalIgnores(['src-tauri/target', 'src-tauri/gen']),
  js.configs.recommended,
  tseslint.configs.recommended,
  english,
  {
    files: ['**/*.js'],
    languageOptions: { sourceType: 'commonjs', globals: { module: 'writable' } },
  },
);

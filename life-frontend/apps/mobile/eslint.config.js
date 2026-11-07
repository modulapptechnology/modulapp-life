const { defineConfig } = require('eslint/config');
const expoConfig = require('eslint-config-expo/flat');
const prettierConfig = require('eslint-config-prettier/flat');
const english = require('../../eslint.english.cjs');

module.exports = defineConfig([
  expoConfig,
  english,
  prettierConfig,
  {
    ignores: ['dist/*'],
  },
  {
    // El resolver de TypeScript falla al leer los reexports (export *) de @life/icons.
    rules: { 'import/namespace': 'off' },
  },
]);

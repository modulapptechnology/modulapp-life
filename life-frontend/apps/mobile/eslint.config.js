const { defineConfig } = require('eslint/config');
const expoConfig = require('eslint-config-expo/flat');
const prettierConfig = require('eslint-config-prettier/flat');

module.exports = defineConfig([
  expoConfig,
  prettierConfig,
  {
    ignores: ['dist/*'],
  },
  {
    // El resolver de TypeScript falla al leer los reexports (export *) de @life/icons.
    rules: { 'import/namespace': 'off' },
  },
]);

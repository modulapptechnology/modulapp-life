import { readAppEnv } from '@life/core';

// Vite expone solo las variables con el prefijo EXPO_PUBLIC_ (envPrefix en vite.config.ts).
export const env = readAppEnv({
  EXPO_PUBLIC_APP_ENV: import.meta.env.EXPO_PUBLIC_APP_ENV,
  EXPO_PUBLIC_API_URL: import.meta.env.EXPO_PUBLIC_API_URL,
});

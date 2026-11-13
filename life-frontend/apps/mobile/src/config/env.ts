import { readAppEnv } from '@life/core';

// Expo solo incrusta las variables que se leen de forma literal (process.env.EXPO_PUBLIC_*).
export const env = readAppEnv({
  EXPO_PUBLIC_APP_ENV: process.env.EXPO_PUBLIC_APP_ENV,
  EXPO_PUBLIC_API_URL: process.env.EXPO_PUBLIC_API_URL,
});

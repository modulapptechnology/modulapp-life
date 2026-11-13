export const APP_ENVIRONMENTS = ['development', 'test', 'production'] as const;

export type AppEnvironment = (typeof APP_ENVIRONMENTS)[number];

export interface AppEnvSource {
  EXPO_PUBLIC_APP_ENV?: string;
  EXPO_PUBLIC_API_URL?: string;
}

export interface AppEnv {
  appEnv: AppEnvironment;
  apiUrl: string;
}

const HTTP_URL = /^https?:\/\/[^\s/]+\S*$/i;

function isAppEnvironment(value: string): value is AppEnvironment {
  return (APP_ENVIRONMENTS as readonly string[]).includes(value);
}

/**
 * Lee y valida las variables públicas del ambiente. Lanza un error que nombra cada variable
 * faltante o inválida, para que la app falle al iniciar y no al llamar a la API.
 */
export function readAppEnv(source: AppEnvSource): AppEnv {
  const appEnv = source.EXPO_PUBLIC_APP_ENV?.trim() ?? '';
  const apiUrl = source.EXPO_PUBLIC_API_URL?.trim() ?? '';
  const problems: string[] = [];

  if (!appEnv) {
    problems.push(`EXPO_PUBLIC_APP_ENV es obligatoria (valores: ${APP_ENVIRONMENTS.join(', ')}).`);
  } else if (!isAppEnvironment(appEnv)) {
    problems.push(
      `EXPO_PUBLIC_APP_ENV tiene el valor "${appEnv}"; los valores válidos son ${APP_ENVIRONMENTS.join(', ')}.`,
    );
  }

  if (!apiUrl) {
    problems.push('EXPO_PUBLIC_API_URL es obligatoria (URL http o https de la API).');
  } else if (!HTTP_URL.test(apiUrl)) {
    problems.push(`EXPO_PUBLIC_API_URL tiene el valor "${apiUrl}"; debe ser una URL http o https.`);
  }

  if (problems.length > 0 || !isAppEnvironment(appEnv)) {
    throw new Error(
      `Configuración de entorno inválida:\n- ${problems.join('\n- ')}\n` +
        'Copia .env.example a .env.local o usa el archivo .env del ambiente (ver README de life-frontend).',
    );
  }

  return { appEnv, apiUrl };
}

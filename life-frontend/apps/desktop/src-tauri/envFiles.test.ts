import { readFileSync } from 'fs';
import { join } from 'path';

const MOBILE_ROOT = join(__dirname, '..', '..', 'mobile');
const FRONTEND_ROOT = join(__dirname, '..', '..', '..');

function readEnvFile(path: string): Record<string, string> {
  const variables: Record<string, string> = {};

  for (const line of readFileSync(path, 'utf8')
    .replace(/^\uFEFF/, '')
    .split(/\r?\n/)) {
    const match = /^([A-Z_]+)=(.*)$/.exec(line.trim());
    if (match) variables[match[1]] = match[2];
  }

  return variables;
}

describe('archivos de entorno', () => {
  it.each(['development', 'test', 'production'])(
    'el .env.%s de la app móvil coincide con el de la raíz que usan web y escritorio',
    (ambiente) => {
      const mobile = readEnvFile(join(MOBILE_ROOT, `.env.${ambiente}`));

      const raiz = readEnvFile(join(FRONTEND_ROOT, `.env.${ambiente}`));

      expect(mobile).toEqual(raiz);
    },
  );

  it.each([
    ['development', 'development'],
    ['preview', 'test'],
    ['production', 'production'],
  ])('el perfil de EAS %s usa las variables del ambiente %s', (perfil, ambiente) => {
    const eas = JSON.parse(readFileSync(join(MOBILE_ROOT, 'eas.json'), 'utf8'));
    const archivo = readEnvFile(join(MOBILE_ROOT, `.env.${ambiente}`));

    const variables = eas.build[perfil].env;

    expect(variables).toEqual(archivo);
  });
});

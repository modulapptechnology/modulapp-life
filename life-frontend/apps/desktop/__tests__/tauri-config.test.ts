import { readFileSync } from 'fs';
import { join } from 'path';
import { APP_NAME } from '@life/constants';

describe('tauri.conf.json del escritorio', () => {
  it('muestra la web de desarrollo en el puerto que fija Vite', () => {
    const tauri = JSON.parse(
      readFileSync(join(__dirname, '..', 'src-tauri', 'tauri.conf.json'), 'utf8'),
    );
    const vite = readFileSync(join(__dirname, '..', '..', 'web', 'vite.config.ts'), 'utf8');

    const puertoVite = /port:\s*(\d+)/.exec(vite)?.[1];

    expect(tauri.build.devUrl).toBe(`http://localhost:${puertoVite}`);
  });

  it('el nombre del producto es el nombre de la aplicación', () => {
    const tauri = JSON.parse(
      readFileSync(join(__dirname, '..', 'src-tauri', 'tauri.conf.json'), 'utf8'),
    );

    const nombre = tauri.productName;

    expect(nombre).toBe(APP_NAME);
  });
});

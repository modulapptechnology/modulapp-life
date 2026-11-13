import { readFileSync } from 'fs';
import { join } from 'path';
import { APP_NAME } from '@life/constants';

describe('tauri.conf.json del escritorio', () => {
  it('muestra la web de desarrollo en el puerto que fija Vite', () => {
    const tauri = JSON.parse(readFileSync(join(__dirname, 'tauri.conf.json'), 'utf8'));
    const vite = readFileSync(join(__dirname, '..', '..', 'web', 'vite.config.ts'), 'utf8');

    const puertoVite = /port:\s*(\d+)/.exec(vite)?.[1];

    expect(tauri.build.devUrl).toBe(`http://localhost:${puertoVite}`);
  });

  it('la ventana principal tiene un tamaño inicial y un mínimo coherentes', () => {
    const tauri = JSON.parse(readFileSync(join(__dirname, 'tauri.conf.json'), 'utf8'));

    const ventana = tauri.app.windows[0];

    expect(ventana.title).toBe(APP_NAME);
    expect(ventana.minWidth).toBeGreaterThan(0);
    expect(ventana.minHeight).toBeGreaterThan(0);
    expect(ventana.width).toBeGreaterThanOrEqual(ventana.minWidth);
    expect(ventana.height).toBeGreaterThanOrEqual(ventana.minHeight);
  });

  it('los permisos se limitan a los básicos más cambiar el título y cerrar la ventana', () => {
    const capacidad = JSON.parse(
      readFileSync(join(__dirname, 'capabilities', 'default.json'), 'utf8'),
    );

    const permisos = capacidad.permissions;

    expect(permisos).toEqual([
      'core:default',
      'core:window:allow-set-title',
      'core:window:allow-close',
    ]);
  });

  it('el nombre del producto es el nombre de la aplicación', () => {
    const tauri = JSON.parse(readFileSync(join(__dirname, 'tauri.conf.json'), 'utf8'));

    const nombre = tauri.productName;

    expect(nombre).toBe(APP_NAME);
  });

  it('la compilación de pruebas empaqueta la web en modo test', () => {
    const base = JSON.parse(readFileSync(join(__dirname, 'tauri.conf.json'), 'utf8'));
    const pruebas = JSON.parse(readFileSync(join(__dirname, 'tauri.conf.test-env.json'), 'utf8'));

    const comando = pruebas.build.beforeBuildCommand;

    expect(base.build.beforeBuildCommand).toBe('npm run build -w @life/web');
    expect(comando).toBe('npm run build:test -w @life/web');
  });
});

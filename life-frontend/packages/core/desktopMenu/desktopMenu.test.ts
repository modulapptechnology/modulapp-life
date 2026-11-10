import type { ModuleDefinition } from '@life/types';
import { DEFAULT_MODULES } from '../moduleCatalog/moduleCatalog';
import { translate } from '../translate/translate';
import { buildDesktopMenu, buildWindowTitle } from './desktopMenu';

const es = (key: string) => translate(key, 'es');

describe('buildDesktopMenu', () => {
  it('en Windows y Linux deja ajustes y salir dentro de Archivo, con nueva captura hacia /captura', () => {
    const modulos = DEFAULT_MODULES;

    const menu = buildDesktopMenu(modulos, es, 'other');

    expect(menu.map((grupo) => grupo.id)).toEqual(['file', 'goTo']);
    expect(menu[0].entries).toEqual([
      {
        kind: 'navigate',
        id: 'menu.newCapture',
        label: 'Nueva captura',
        accelerator: 'CmdOrCtrl+N',
        path: '/captura',
      },
      {
        kind: 'navigate',
        id: 'menu.settings',
        label: 'Ajustes',
        accelerator: 'CmdOrCtrl+,',
        path: '/settings',
      },
      { kind: 'reload', id: 'menu.reload', label: 'Recargar', accelerator: 'CmdOrCtrl+R' },
      { kind: 'separator' },
      { kind: 'quit', id: 'menu.quit', label: 'Salir', accelerator: 'CmdOrCtrl+Q' },
    ]);
  });

  it('en macOS pone primero el menú de la aplicación con ajustes y salir', () => {
    const modulos = DEFAULT_MODULES;

    const menu = buildDesktopMenu(modulos, es, 'mac');

    expect(menu.map((grupo) => grupo.id)).toEqual(['app', 'file', 'goTo']);
    expect(menu[0].label).toBe('ModulApp Life');
    expect(menu[0].entries.map((entrada) => entrada.kind)).toEqual([
      'navigate',
      'separator',
      'quit',
    ]);
    expect(menu[1].entries.map((entrada) => entrada.kind)).toEqual(['navigate', 'reload']);
  });

  it('Ir a lista inicio y los módulos habilitados con las mismas rutas del menú lateral', () => {
    const modulos = DEFAULT_MODULES;

    const goTo = buildDesktopMenu(modulos, es, 'other')[1];

    expect(goTo.entries).toEqual([
      {
        kind: 'navigate',
        id: 'menu.goTo.home',
        label: 'Inicio',
        accelerator: 'CmdOrCtrl+1',
        path: '/',
      },
      {
        kind: 'navigate',
        id: 'menu.goTo.tareas',
        label: 'Tareas',
        accelerator: 'CmdOrCtrl+2',
        path: '/tareas',
      },
      {
        kind: 'navigate',
        id: 'menu.goTo.agenda',
        label: 'Agenda',
        accelerator: 'CmdOrCtrl+3',
        path: '/agenda',
      },
    ]);
  });

  it('no ofrece un acceso para un módulo deshabilitado', () => {
    const soloTareas = DEFAULT_MODULES.filter((modulo) => modulo.id === 'tareas');

    const goTo = buildDesktopMenu(soloTareas, es, 'other')[1];

    expect(
      goTo.entries.map((entrada) => (entrada.kind === 'navigate' ? entrada.path : null)),
    ).toEqual(['/', '/tareas']);
  });
});

describe('buildWindowTitle', () => {
  const modulos: ModuleDefinition[] = DEFAULT_MODULES;

  it.each([
    ['/', 'ModulApp Life — Inicio'],
    ['/tareas', 'ModulApp Life — Tareas'],
    ['/tareas/123', 'ModulApp Life — Tareas'],
    ['/agenda', 'ModulApp Life — Agenda'],
    ['/captura', 'ModulApp Life — Captura'],
    ['/settings', 'ModulApp Life — Ajustes'],
    ['/profile', 'ModulApp Life — Perfil'],
  ])('refleja la sección de la ruta %s', (ruta, esperado) => {
    const pathname = ruta;

    const titulo = buildWindowTitle(pathname, modulos, es);

    expect(titulo).toBe(esperado);
  });

  it('muestra solo el nombre de la aplicación cuando la ruta no es una sección', () => {
    const pathname = '/login';

    const titulo = buildWindowTitle(pathname, modulos, es);

    expect(titulo).toBe('ModulApp Life');
  });
});

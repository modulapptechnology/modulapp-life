import type { ModuleDefinition } from '@life/types';
import { CAPTURE_SECTION } from '../moduleCatalog/moduleCatalog';

type Translate = (key: string) => string;

export type DesktopMenuEntry =
  | { kind: 'navigate'; id: string; label: string; accelerator: string; path: string }
  | { kind: 'reload'; id: string; label: string; accelerator: string }
  | { kind: 'quit'; id: string; label: string; accelerator: string }
  | { kind: 'separator' };

export type DesktopMenuGroup = { id: string; label: string; entries: DesktopMenuEntry[] };

export type DesktopPlatform = 'mac' | 'other';

const HOME_PATH = '/';
const SETTINGS_PATH = '/settings';
const PROFILE_PATH = '/profile';
const MAX_SHORTCUT_SECTIONS = 9;

// Estructura del menú nativo, sin depender de Tauri: las acciones de navegación usan las mismas rutas que la interfaz.
// En macOS el menú de la aplicación (ajustes y salir) va primero; en Windows y Linux ambos van en Archivo.
export function buildDesktopMenu(
  modules: ModuleDefinition[],
  translate: Translate,
  platform: DesktopPlatform,
): DesktopMenuGroup[] {
  const settings: DesktopMenuEntry = {
    kind: 'navigate',
    id: 'menu.settings',
    label: translate('user.settings'),
    accelerator: 'CmdOrCtrl+,',
    path: SETTINGS_PATH,
  };
  const quit: DesktopMenuEntry = {
    kind: 'quit',
    id: 'menu.quit',
    label: translate('menu.quit'),
    accelerator: 'CmdOrCtrl+Q',
  };
  const newCapture: DesktopMenuEntry = {
    kind: 'navigate',
    id: 'menu.newCapture',
    label: translate('menu.newCapture'),
    accelerator: 'CmdOrCtrl+N',
    path: CAPTURE_SECTION.path,
  };
  const reload: DesktopMenuEntry = {
    kind: 'reload',
    id: 'menu.reload',
    label: translate('menu.reload'),
    accelerator: 'CmdOrCtrl+R',
  };

  const sections = [
    { id: 'home', label: translate('nav.home'), path: HOME_PATH },
    ...modules.map((module) => ({
      id: module.id,
      label: translate(module.nameKey),
      path: module.path,
    })),
  ].slice(0, MAX_SHORTCUT_SECTIONS);
  const goTo: DesktopMenuGroup = {
    id: 'goTo',
    label: translate('menu.goTo'),
    entries: sections.map((section, index) => ({
      kind: 'navigate',
      id: `menu.goTo.${section.id}`,
      label: section.label,
      accelerator: `CmdOrCtrl+${index + 1}`,
      path: section.path,
    })),
  };

  if (platform === 'mac') {
    return [
      {
        id: 'app',
        label: translate('nav.title'),
        entries: [settings, { kind: 'separator' }, quit],
      },
      { id: 'file', label: translate('menu.file'), entries: [newCapture, reload] },
      goTo,
    ];
  }

  return [
    {
      id: 'file',
      label: translate('menu.file'),
      entries: [newCapture, settings, reload, { kind: 'separator' }, quit],
    },
    goTo,
  ];
}

function sectionName(pathname: string, modules: ModuleDefinition[], translate: Translate) {
  if (pathname === HOME_PATH) return translate('nav.home');
  if (pathname === SETTINGS_PATH) return translate('user.settings');
  if (pathname === PROFILE_PATH) return translate('user.profile');
  if (pathname === CAPTURE_SECTION.path) return translate(CAPTURE_SECTION.nameKey);

  const module = modules.find(
    (candidate) => pathname === candidate.path || pathname.startsWith(`${candidate.path}/`),
  );
  return module ? translate(module.nameKey) : null;
}

// Título de la ventana: el nombre de la aplicación y la sección actual (solo el nombre si la ruta no es una sección).
export function buildWindowTitle(
  pathname: string,
  modules: ModuleDefinition[],
  translate: Translate,
): string {
  const appName = translate('nav.title');
  const section = sectionName(pathname, modules, translate);

  return section ? `${appName} — ${section}` : appName;
}

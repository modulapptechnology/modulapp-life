import { useEffect, useRef } from 'react';
import { buildDesktopMenu, buildWindowTitle, translate, type DesktopMenuEntry } from '@life/core';
import { invoke, isTauri } from '@tauri-apps/api/core';
import { Menu, MenuItem, PredefinedMenuItem, Submenu } from '@tauri-apps/api/menu';
import { getCurrentWindow } from '@tauri-apps/api/window';

type NativeEntry = MenuItem | PredefinedMenuItem;
type EnabledModules = Parameters<typeof buildDesktopMenu>[0];

function reportError(error: unknown) {
  console.error('No se pudo configurar la ventana de escritorio', error);
}

// Solo en escritorio (Tauri): fija el título de la ventana según la sección actual y el menú nativo del sistema.
export function useDesktopWindow(
  pathname: string,
  modules: EnabledModules,
  navigate: (path: string) => void,
) {
  const navigateRef = useRef(navigate);

  useEffect(() => {
    navigateRef.current = navigate;
  }, [navigate]);

  useEffect(() => {
    if (!isTauri()) return;

    getCurrentWindow()
      .setTitle(buildWindowTitle(pathname, modules, translate))
      .catch(reportError);
  }, [pathname, modules]);

  useEffect(() => {
    if (!isTauri()) return;

    let cancelled = false;
    const platform = /Mac/i.test(navigator.userAgent) ? 'mac' : 'other';

    async function toNativeEntry(entry: DesktopMenuEntry): Promise<NativeEntry> {
      if (entry.kind === 'separator') return PredefinedMenuItem.new({ item: 'Separator' });

      const run = () => {
        if (entry.kind === 'navigate') navigateRef.current(entry.path);
        else if (entry.kind === 'reload') window.location.reload();
        else void getCurrentWindow().close();
      };
      return MenuItem.new({
        id: entry.id,
        text: entry.label,
        accelerator: entry.accelerator,
        action: run,
      });
    }

    async function install() {
      const groups = buildDesktopMenu(modules, translate, platform);
      const submenus = await Promise.all(
        groups.map(async (group) =>
          Submenu.new({
            text: group.label,
            items: await Promise.all(group.entries.map(toNativeEntry)),
          }),
        ),
      );
      const menu = await Menu.new({ items: submenus });
      if (cancelled) return;
      await menu.setAsAppMenu();
      // En Windows el menú existe (y sus atajos) pero la barra queda oculta.
      if (/Windows/i.test(navigator.userAgent)) await invoke('hide_menu');
    }

    install().catch(reportError);
    return () => {
      cancelled = true;
    };
  }, [modules]);
}

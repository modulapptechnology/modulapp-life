import type { ThemeColors } from '@life/theme';
import type { IconName } from '../../components/atoms/ModulIcon/ModulIcon';
import type { ModulSidebarSection } from '../../components/molecules/ModulSidebar/ModulSidebar';

type SidebarModule = {
  path: string;
  nameKey: string;
  icon: IconName;
  accent: keyof ThemeColors;
};

// Convierte los módulos habilitados en secciones del ModulSidebar, con el nombre ya traducido.
export function toSidebarSections(
  modules: SidebarModule[],
  translate: (key: string) => string,
): ModulSidebarSection[] {
  return modules.map((module) => ({
    path: module.path,
    label: translate(module.nameKey),
    icon: module.icon,
    accent: module.accent,
  }));
}

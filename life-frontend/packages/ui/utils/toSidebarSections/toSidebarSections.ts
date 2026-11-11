import type { ThemeColors } from '@life/theme';
import type { IconName } from '../../components/atoms/ModulIcon/ModulIcon';
import type { ModulSidebarRailSection } from '../../components/molecules/ModulSidebarRail/ModulSidebarRail';

type SidebarModule = {
  path: string;
  nameKey: string;
  icon: IconName;
  accent?: keyof ThemeColors;
  groups?: {
    id: string;
    titleKey: string;
    links: { path: string; nameKey: string; icon: IconName }[];
  }[];
};

export function toSidebarSections(
  modules: SidebarModule[],
  translate: (key: string) => string,
): ModulSidebarRailSection[] {
  return modules.map((module) => ({
    path: module.path,
    label: translate(module.nameKey),
    icon: module.icon,
    accent: module.accent,
    groups: module.groups?.map((group) => ({
      id: group.id,
      title: translate(group.titleKey),
      links: group.links.map((link) => ({
        path: link.path,
        label: translate(link.nameKey),
        icon: link.icon,
      })),
    })),
  }));
}

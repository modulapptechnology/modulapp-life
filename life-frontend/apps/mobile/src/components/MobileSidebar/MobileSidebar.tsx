import { moduleRegistry, translate, useEnabledModules } from '@life/core';
import { ModulDrawer, ModulSidebar, toSidebarSections, type ModulSidebarSection } from '@life/ui';
import { usePathname, useRouter, type Href } from 'expo-router';

type MobileSidebarProps = { visible: boolean; onClose: () => void };

// Sidebar del móvil: panel deslizable con los módulos habilitados. Lo abre el botón de ModulHeader.
export function MobileSidebar({ visible, onClose }: MobileSidebarProps) {
  const modules = useEnabledModules(moduleRegistry);
  const router = useRouter();
  const pathname = usePathname();

  const sections: ModulSidebarSection[] = [
    { path: '/', label: translate('nav.home'), icon: 'Home2Icon' },
    ...toSidebarSections(modules, translate),
  ];

  return (
    <ModulDrawer visible={visible} onClose={onClose} closeLabel={translate('nav.close')}>
      <ModulSidebar
        sections={sections}
        activePath={pathname}
        onNavigate={(path) => {
          onClose();
          router.push(path as Href);
        }}
      />
    </ModulDrawer>
  );
}

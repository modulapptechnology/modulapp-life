import { moduleRegistry, session, translate, useEnabledModules, useSession } from '@life/core';
import { ModulDrawer, ModulSidebar, toSidebarSections, type ModulSidebarSection } from '@life/ui';
import { usePathname, useRouter, type Href } from 'expo-router';
import { sessionManager } from '../../services/session/session';

type MobileSidebarProps = { visible: boolean; onClose: () => void };

// Sidebar del móvil: panel deslizable con los módulos habilitados. Lo abre el botón de ModulHeader.
export function MobileSidebar({ visible, onClose }: MobileSidebarProps) {
  const modules = useEnabledModules(moduleRegistry);
  const { user, pendingOperations } = useSession(session);
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
        user={user ? { name: user.name, avatarUri: user.avatarUri, path: '/profile' } : null}
        footerSections={[
          { path: '/settings', label: translate('user.settings'), icon: 'SettingsIcon' },
        ]}
        signOut={{
          label: translate('user.signOut'),
          onSignOut: () => {
            onClose();
            void sessionManager.signOut();
          },
          pendingOperations,
          confirmTitle: translate('user.signOutTitle'),
          confirmMessage: translate('user.signOutPending'),
          cancelLabel: translate('common.cancel'),
        }}
        onNavigate={(path) => {
          onClose();
          router.push(path as Href);
        }}
      />
    </ModulDrawer>
  );
}

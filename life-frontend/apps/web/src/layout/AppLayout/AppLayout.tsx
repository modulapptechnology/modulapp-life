import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { moduleRegistry, session, translate, useEnabledModules, useSession } from '@life/core';
import { ModulLayoutWeb, ModulText, toSidebarSections, type NavSection } from '@life/ui';

// Layout común de la web: el sidebar sale del registro de módulos habilitados, así que no cambia al agregar uno.
export default function AppLayout() {
  const modules = useEnabledModules(moduleRegistry);
  const { user, pendingOperations } = useSession(session);
  const navigate = useNavigate();
  const { pathname } = useLocation();

  const sections: NavSection[] = [
    { path: '/', label: translate('nav.home'), icon: 'Home2Icon' },
    ...toSidebarSections(modules, translate),
  ];

  return (
    <ModulLayoutWeb
      sections={sections}
      activePath={pathname}
      onNavigate={navigate}
      user={
        user
          ? {
              name: user.name,
              avatarUri: user.avatarUri,
              path: '/profile',
              caption: translate('user.profile'),
            }
          : null
      }
      footerSections={[
        { path: '/settings', label: translate('user.settings'), icon: 'SettingsIcon' },
      ]}
      signOut={{
        label: translate('user.signOut'),
        onSignOut: () => session.signOut(),
        pendingOperations,
        confirmTitle: translate('user.signOutTitle'),
        confirmMessage: translate('user.signOutPending'),
        cancelLabel: translate('common.cancel'),
      }}
      headerContent={
        <ModulText variant="body" fwBold>
          {translate('nav.title')}
        </ModulText>
      }
    >
      <Outlet />
    </ModulLayoutWeb>
  );
}

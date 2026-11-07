import { Navigate, Outlet, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import { moduleRegistry, session, translate, useEnabledModules, useSession } from '@life/core';
import { ModulLayoutWeb, ModulText, toSidebarSections, type NavSection } from '@life/ui';
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import ModulePage from './pages/ModulePage';

export default function App() {
  const modules = useEnabledModules(moduleRegistry);
  const { user, pendingOperations } = useSession(session);
  const navigate = useNavigate();
  const { pathname } = useLocation();

  // El sidebar sale del registro de módulos habilitados, así que no cambia al agregar uno.
  const sections: NavSection[] = [
    { path: '/', label: translate('nav.home'), icon: 'Home2Icon' },
    ...toSidebarSections(modules, translate),
  ];

  const layout = (
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

  return (
    <Routes>
      <Route path="/login" element={user ? <Navigate to="/" replace /> : <LoginPage />} />
      <Route element={user ? layout : <Navigate to="/login" replace />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/profile" element={<ModulePage nameKey="user.profile" />} />
        <Route path="/settings" element={<ModulePage nameKey="user.settings" />} />
        {modules.map((module) => (
          <Route
            key={module.id}
            path={module.path}
            element={<ModulePage nameKey={module.nameKey} />}
          />
        ))}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}

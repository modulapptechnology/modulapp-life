import { lazy, Suspense, useEffect } from 'react';
import { Navigate, Outlet, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import {
  CAPTURE_SECTION,
  HOME_SECTION,
  moduleRegistry,
  PROFILE_SECTION,
  SETTINGS_SECTION,
  session,
  translate,
  useConnectivity,
  useEnabledModules,
  useSession,
} from '@life/core';
import {
  ModulConnectionIndicator,
  ModulLayoutWeb,
  ModulText,
  ModulView,
  toSidebarSections,
  type NavSection,
} from '@life/ui';
import HomePage from './pages/HomePage';
import ForgotPasswordPage from './pages/ForgotPasswordPage';
import LoginPage from './pages/LoginPage';
import ModulePage from './pages/ModulePage';
import RegisterPage from './pages/RegisterPage';
import ResetPasswordPage from './pages/ResetPasswordPage';
import VerifyEmailPage from './pages/VerifyEmailPage';
import { useDesktopWindow } from './desktop/useDesktopWindow';
import { connectivityMonitor } from './services/connectivity/connectivity';
import { sessionManager } from './services/session/session';
import SettingsPage from './pages/SettingsPage';

const Catalog = lazy(() => import('@life/ui/catalog/Catalog'));

export default function App() {
  const modules = useEnabledModules(moduleRegistry);
  const { user, pendingOperations } = useSession(session);
  const { status } = useConnectivity();
  const navigate = useNavigate();
  const { pathname } = useLocation();

  useDesktopWindow(pathname, modules, navigate);

  useEffect(() => {
    void sessionManager.restore();
  }, []);

  useEffect(() => connectivityMonitor.start(), []);

  // El sidebar sale del registro de módulos habilitados, así que no cambia al agregar uno.
  const sections: NavSection[] = toSidebarSections([HOME_SECTION, ...modules], translate);
  const [profileSection, settingsSection] = toSidebarSections(
    [PROFILE_SECTION, SETTINGS_SECTION],
    translate,
  );
  const links = [HOME_SECTION, PROFILE_SECTION, SETTINGS_SECTION, ...modules].flatMap((section) =>
    (section.groups ?? []).flatMap((group) => group.links),
  );

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
              groups: profileSection.groups,
            }
          : null
      }
      footerSections={[settingsSection]}
      signOut={{
        label: translate('user.signOut'),
        onSignOut: () => void sessionManager.signOut(),
        pendingOperations,
        confirmTitle: translate('user.signOutTitle'),
        confirmMessage: translate('user.signOutPending'),
        cancelLabel: translate('common.cancel'),
      }}
      headerContent={
        <ModulView row alignCenter gap16>
          <ModulText variant="body" fwBold>
            {translate('nav.title')}
          </ModulText>
          <ModulConnectionIndicator
            status={status}
            labels={{
              offline: translate('connectivity.offline'),
              syncing: translate('connectivity.syncing'),
              synced: translate('connectivity.synced'),
            }}
          />
        </ModulView>
      }
    >
      <Outlet />
    </ModulLayoutWeb>
  );

  return (
    <Routes>
      {/* Catálogo del sistema de diseño: solo en desarrollo, sin sesión ni enlace en el menú. */}
      {import.meta.env.DEV && (
        <Route
          path="/Catalog"
          element={
            <Suspense fallback={null}>
              <Catalog />
            </Suspense>
          }
        />
      )}
      <Route path="/login" element={user ? <Navigate to="/" replace /> : <LoginPage />} />
      <Route path="/register" element={user ? <Navigate to="/" replace /> : <RegisterPage />} />
      <Route
        path="/forgot-password"
        element={user ? <Navigate to="/" replace /> : <ForgotPasswordPage />}
      />
      {/* Se abren desde el enlace del correo, con o sin sesión iniciada. */}
      <Route path="/reset-password" element={<ResetPasswordPage />} />
      <Route path="/verify-email" element={<VerifyEmailPage />} />
      <Route element={user ? layout : <Navigate to="/login" replace />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/profile" element={<ModulePage nameKey="user.profile" />} />
        <Route
          path={CAPTURE_SECTION.path}
          element={<ModulePage nameKey={CAPTURE_SECTION.nameKey} />}
        />
        <Route path="/settings" element={<SettingsPage />} />
        {modules.map((module) => (
          <Route
            key={module.id}
            path={module.path}
            element={<ModulePage nameKey={module.nameKey} />}
          />
        ))}
        {links.map((link) => (
          <Route key={link.path} path={link.path} element={<ModulePage nameKey={link.nameKey} />} />
        ))}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}

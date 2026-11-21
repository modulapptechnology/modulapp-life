import { lazy, Suspense } from 'react';
import { Navigate, Route, Routes, useLocation } from 'react-router-dom';
import {
  CAPTURE_SECTION,
  HOME_SECTION,
  moduleRegistry,
  PROFILE_SECTION,
  resolveReturnPath,
  SETTINGS_SECTION,
  session,
  useEnabledModules,
  useSession,
} from '@life/core';
import WebLayout from '../layout/WebLayout';
import ForgotPasswordPage from '../pages/ForgotPasswordPage';
import HomePage from '../pages/HomePage';
import LoginPage from '../pages/LoginPage';
import ModulePage from '../pages/ModulePage';
import RegisterPage from '../pages/RegisterPage';
import ResetPasswordPage from '../pages/ResetPasswordPage';
import SettingsPage from '../pages/SettingsPage';
import UnavailablePage from '../pages/UnavailablePage';
import VerifyEmailPage from '../pages/VerifyEmailPage';

const Catalog = lazy(() => import('@life/ui/catalog/Catalog'));

export default function WebRoutes() {
  const modules = useEnabledModules(moduleRegistry);
  const { user } = useSession(session);
  const location = useLocation();

  const links = [HOME_SECTION, PROFILE_SECTION, SETTINGS_SECTION, ...modules].flatMap((section) =>
    (section.groups ?? []).flatMap((group) => group.links),
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
      <Route
        path="/login"
        element={
          user ? (
            <Navigate
              to={resolveReturnPath((location.state as { from?: string } | null)?.from)}
              replace
            />
          ) : (
            <LoginPage />
          )
        }
      />
      <Route path="/register" element={user ? <Navigate to="/" replace /> : <RegisterPage />} />
      <Route
        path="/forgot-password"
        element={user ? <Navigate to="/" replace /> : <ForgotPasswordPage />}
      />
      {/* Se abren desde el enlace del correo, con o sin sesión iniciada. */}
      <Route path="/reset-password" element={<ResetPasswordPage />} />
      <Route path="/verify-email" element={<VerifyEmailPage />} />
      <Route
        element={
          user ? (
            <WebLayout />
          ) : (
            <Navigate to="/login" replace state={{ from: location.pathname + location.search }} />
          )
        }
      >
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
        {moduleRegistry
          .getDisabled()
          .flatMap((module) => [
            <Route key={module.id} path={module.path} element={<UnavailablePage />} />,
            <Route
              key={`${module.id}/*`}
              path={`${module.path}/*`}
              element={<UnavailablePage />}
            />,
          ])}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}

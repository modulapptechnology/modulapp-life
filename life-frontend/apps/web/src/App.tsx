import { Navigate, Route, Routes } from 'react-router-dom';
import { moduleRegistry, session, useEnabledModules, useSession } from '@life/core';
import AppLayout from './layout/AppLayout/AppLayout';
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import ModulePage from './pages/ModulePage';

export default function App() {
  const modules = useEnabledModules(moduleRegistry);
  const { user } = useSession(session);

  return (
    <Routes>
      <Route path="/login" element={user ? <Navigate to="/" replace /> : <LoginPage />} />
      <Route element={user ? <AppLayout /> : <Navigate to="/login" replace />}>
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

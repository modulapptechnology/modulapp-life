import { Navigate, Route, Routes } from 'react-router-dom';
import { moduleRegistry, useEnabledModules } from '@life/core';
import AppLayout from './layout/AppLayout/AppLayout';
import HomePage from './pages/HomePage';
import ModulePage from './pages/ModulePage';

export default function App() {
  const modules = useEnabledModules(moduleRegistry);

  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route path="/" element={<HomePage />} />
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

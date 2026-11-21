import { useLocation, useNavigate } from 'react-router-dom';
import {
  moduleRegistry,
  moduleStateStore,
  session,
  translate,
  useAppBootstrap,
  useEnabledModules,
  useSession,
} from '@life/core';
import { ModulSplash } from '@life/ui';
import { useDesktopWindow } from './desktop/useDesktopWindow';
import AppRoutes from './routes/WebRoutes';
import { connectivityMonitor } from './services/connectivity/connectivity';
import { sessionManager } from './services/session/session';
import { workspaceStore } from './services/workspace/workspaceStore';

const restoreSession = () => sessionManager.restore();
const startConnectivity = () => connectivityMonitor.start();
const onSignedOut = () => {
  workspaceStore.reset();
  moduleStateStore.clear();
};

export default function App() {
  const modules = useEnabledModules(moduleRegistry);
  const { user, restoring, entering } = useSession(session);
  const { pathname } = useLocation();
  const navigate = useNavigate();

  useDesktopWindow(pathname, modules, navigate);
  useAppBootstrap({ signedIn: !!user, restoreSession, startConnectivity, onSignedOut });

  if (restoring) return null;

  if (entering) {
    return (
      <ModulSplash brandName={translate('auth.brandName')} message={translate('common.loading')} />
    );
  }

  return <AppRoutes />;
}

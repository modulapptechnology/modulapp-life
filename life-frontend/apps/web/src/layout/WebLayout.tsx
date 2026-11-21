import { useState } from 'react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import {
  CAPTURE_SECTION,
  HOME_SECTION,
  moduleRegistry,
  PROFILE_SECTION,
  SETTINGS_SECTION,
  SHORTCUTS,
  session,
  translate,
  useConnectivity,
  useEnabledModules,
  useSession,
  useWorkspaces,
} from '@life/core';
import {
  ModulConnectionIndicator,
  ModulLayoutWeb,
  ModulShortcutsDialog,
  ModulText,
  ModulView,
  ModulWorkspaceSelector,
  toSidebarSections,
  type NavSection,
} from '@life/ui';
import { sessionManager } from '../services/session/session';
import { workspaceService } from '../services/workspace/workspaceService';
import { workspaceStore } from '../services/workspace/workspaceStore';
import { useShortcuts } from '../shortcuts/useShortcuts';

function WorkspaceSelector() {
  const { workspaces, activeId, select } = useWorkspaces(workspaceStore, workspaceService);

  return (
    <ModulWorkspaceSelector
      workspaces={workspaces}
      activeId={activeId}
      onSelect={(id) => void select(id)}
      title={translate('workspace.select')}
      closeLabel={translate('common.close')}
    />
  );
}

export default function WebLayout() {
  const modules = useEnabledModules(moduleRegistry);
  const { user, pendingOperations } = useSession(session);
  const { status } = useConnectivity();
  const navigate = useNavigate();
  const { pathname } = useLocation();
  const [shortcutsOpen, setShortcutsOpen] = useState(false);

  useShortcuts((id) => {
    if (id === 'newCapture') navigate(CAPTURE_SECTION.path);
    else if (id === 'openSettings') navigate(SETTINGS_SECTION.path);
    else setShortcutsOpen(true);
  }, !!user && !shortcutsOpen);

  // El sidebar sale del registro de módulos habilitados, así que no cambia al agregar uno.
  const sections: NavSection[] = toSidebarSections([HOME_SECTION, ...modules], translate);
  const [profileSection, settingsSection] = toSidebarSections(
    [PROFILE_SECTION, SETTINGS_SECTION],
    translate,
  );

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
          <WorkspaceSelector />
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
      <Outlet context={{ openShortcuts: () => setShortcutsOpen(true) }} />
      <ModulShortcutsDialog
        visible={shortcutsOpen}
        onClose={() => setShortcutsOpen(false)}
        title={translate('shortcuts.title')}
        closeLabel={translate('common.close')}
        items={SHORTCUTS.map((shortcut) => ({
          label: translate(shortcut.labelKey),
          keys: shortcut.key.toUpperCase(),
        }))}
      />
    </ModulLayoutWeb>
  );
}

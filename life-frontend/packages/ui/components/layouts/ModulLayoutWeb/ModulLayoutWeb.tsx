import { useState, type ReactNode } from 'react';
import { ScrollView } from 'react-native';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { ModulHeader } from '../../molecules/ModulHeader/ModulHeader';
import {
  ModulSidebarRail,
  type ModulSidebarRailProps,
  type ModulSidebarRailSection,
} from '../../molecules/ModulSidebarRail/ModulSidebarRail';

export type NavSection = ModulSidebarRailSection;

export type ModulLayoutWebProps = Pick<
  ModulSidebarRailProps,
  'activePath' | 'onNavigate' | 'user' | 'footerSections' | 'signOut'
> & {
  sections: NavSection[];
  // Contenido del encabezado exclusivo de la web, se pasa tal cual a ModulHeader.
  headerContent?: ReactNode;
  headerEnd?: ReactNode;
  children: ReactNode;
};

// Layout de la web (y del escritorio, que muestra la web): encabezado, barra lateral y contenido.
export function ModulLayoutWeb({
  sections,
  activePath,
  onNavigate,
  user,
  footerSections,
  signOut,
  headerContent,
  headerEnd,
  children,
}: ModulLayoutWebProps) {
  const [sidebarOpen, setSidebarOpen] = useState(true);

  return (
    <ModulView h100 flex1 bg overflowHidden>
      <ModulHeader
        sidebarOpen={sidebarOpen}
        onToggleSidebar={() => setSidebarOpen((open) => !open)}
        end={headerEnd}
      >
        {headerContent}
      </ModulHeader>
      <ModulView flex1 row>
        <ModulView testID="web-sidebar" bgSurface style={{ zIndex: 1 }}>
          <ModulSidebarRail
            open={sidebarOpen}
            sections={sections}
            activePath={activePath}
            onNavigate={onNavigate}
            user={user}
            footerSections={footerSections}
            signOut={signOut}
          />
        </ModulView>
        <ModulView testID="web-main" flex1>
          <ScrollView contentContainerStyle={{ flexGrow: 1 }}>{children}</ScrollView>
        </ModulView>
      </ModulView>
    </ModulView>
  );
}

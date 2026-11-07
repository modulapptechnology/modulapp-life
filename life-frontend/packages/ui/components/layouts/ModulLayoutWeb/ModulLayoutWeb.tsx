import { useState, type ReactNode } from 'react';
import { Pressable } from 'react-native';
import { ModulIcon, type IconName } from '../../atoms/ModulIcon/ModulIcon';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { ModulHeader } from '../../molecules/ModulHeader/ModulHeader';

export type NavSection = { path: string; label: string; icon: IconName };

export type ModulLayoutWebProps = {
  sections: NavSection[];
  activePath: string;
  onNavigate: (path: string) => void;
  // Contenido del encabezado exclusivo de la web, se pasa tal cual a ModulHeader.
  headerContent?: ReactNode;
  headerEnd?: ReactNode;
  children: ReactNode;
};

function isActive(sectionPath: string, activePath: string) {
  if (sectionPath === '/') return activePath === '/';
  return activePath === sectionPath || activePath.startsWith(`${sectionPath}/`);
}

// Layout de la web (y del escritorio, que muestra la web): encabezado, barra lateral y contenido.
export function ModulLayoutWeb({
  sections,
  activePath,
  onNavigate,
  headerContent,
  headerEnd,
  children,
}: ModulLayoutWebProps) {
  const [sidebarOpen, setSidebarOpen] = useState(true);

  return (
    <ModulView flex1 bg>
      <ModulHeader
        sidebarOpen={sidebarOpen}
        onToggleSidebar={() => setSidebarOpen((open) => !open)}
        end={headerEnd}
      >
        {headerContent}
      </ModulHeader>
      <ModulView flex1 row>
        {sidebarOpen && (
          <ModulView
            testID="web-sidebar"
            accessibilityRole="tablist"
            bgSurface
            p8
            gap4
            style={{ width: 220 }}
          >
            {sections.map((section) => {
              const active = isActive(section.path, activePath);

              return (
                <Pressable
                  key={section.path}
                  accessibilityRole="tab"
                  accessibilityLabel={section.label}
                  accessibilityState={{ selected: active }}
                  onPress={() => onNavigate(section.path)}
                >
                  <ModulView row alignCenter gap8 p8 rounded bg={active}>
                    <ModulIcon name={section.icon} color={active ? 'primary' : 'textSecondary'} />
                    <ModulText variant="label" textBrand={active} textSecondary={!active}>
                      {section.label}
                    </ModulText>
                  </ModulView>
                </Pressable>
              );
            })}
          </ModulView>
        )}
        <ModulView testID="web-main" flex1>
          {children}
        </ModulView>
      </ModulView>
    </ModulView>
  );
}

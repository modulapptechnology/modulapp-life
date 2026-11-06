import type { ReactNode } from 'react';
import { Pressable } from 'react-native';
import { ModulIcon, type IconName } from '../../atoms/ModulIcon/ModulIcon';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';

export type NavSection = { path: string; label: string; icon: IconName };

export type ModulLayoutWebProps = {
  title: string;
  sections: NavSection[];
  activePath: string;
  onNavigate: (path: string) => void;
  children: ReactNode;
};

function isActive(sectionPath: string, activePath: string) {
  if (sectionPath === '/') return activePath === '/';
  return activePath === sectionPath || activePath.startsWith(`${sectionPath}/`);
}

// Layout de la web (y del escritorio, que muestra la web): encabezado, barra lateral y contenido.
export function ModulLayoutWeb({
  title,
  sections,
  activePath,
  onNavigate,
  children,
}: ModulLayoutWebProps) {
  return (
    <ModulView flex1 bg>
      <ModulView testID="web-header" accessibilityRole="header" px16 py8 bgSurface>
        <ModulText variant="headerTitle">{title}</ModulText>
      </ModulView>
      <ModulView flex1 row>
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
        <ModulView testID="web-main" flex1>
          {children}
        </ModulView>
      </ModulView>
    </ModulView>
  );
}

import { Pressable } from 'react-native';
import type { ThemeColors } from '@life/theme';
import { ModulIcon, type IconName } from '../../atoms/ModulIcon/ModulIcon';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';

export type ModulSidebarSection = {
  path: string;
  label: string;
  icon: IconName;
  accent?: keyof ThemeColors;
};

export type ModulSidebarProps = {
  sections: ModulSidebarSection[];
  activePath: string;
  onNavigate: (path: string) => void;
};

function isActive(sectionPath: string, activePath: string) {
  if (sectionPath === '/') return activePath === '/';
  return activePath === sectionPath || activePath.startsWith(`${sectionPath}/`);
}

// Lista de secciones con la activa resaltada. La usan la web (dentro de ModulLayoutWeb) y el móvil (dentro de ModulDrawer);
// abrirla y cerrarla es trabajo del botón de ModulHeader.
export function ModulSidebar({ sections, activePath, onNavigate }: ModulSidebarProps) {
  return (
    <ModulView accessibilityRole="tablist" p8 gap4>
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
            <ModulView row alignCenter gap8 p8 minH44 rounded bg={active}>
              <ModulIcon
                name={section.icon}
                color={section.accent ?? (active ? 'primary' : 'textSecondary')}
              />
              <ModulText variant="label" textBrand={active} textSecondary={!active}>
                {section.label}
              </ModulText>
            </ModulView>
          </Pressable>
        );
      })}
    </ModulView>
  );
}

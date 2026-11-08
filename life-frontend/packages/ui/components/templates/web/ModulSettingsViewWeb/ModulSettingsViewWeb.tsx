import { useState } from 'react';
import { Pressable } from 'react-native';
import { ModulText } from '../../../atoms/ModulText/ModulText';
import { ModulView } from '../../../atoms/ModulView/ModulView';
import type { ModulSettingsSection } from '../../mobile/ModulSettingsView/ModulSettingsView';
import { ModulPageWeb } from '../ModulPageWeb/ModulPageWeb';

export type ModulSettingsViewWebProps = {
  title: string;
  sections: ModulSettingsSection[];
};

// Plantilla de ajustes para la web: lista de secciones a la izquierda y el contenido de la sección elegida a la
// derecha, en una tarjeta.
export function ModulSettingsViewWeb({ title, sections }: ModulSettingsViewWebProps) {
  const [selectedId, setSelectedId] = useState(sections[0]?.id);
  const selected = sections.find((section) => section.id === selectedId) ?? sections[0];

  return (
    <ModulPageWeb title={title}>
      <ModulView row gap16 style={{ alignItems: 'flex-start' }}>
        <ModulView gap4 accessibilityRole="tablist" style={{ width: 220 }}>
          {sections.map((section) => (
            <Pressable
              key={section.id}
              accessibilityRole="tab"
              accessibilityState={{ selected: section.id === selected?.id }}
              onPress={() => setSelectedId(section.id)}
            >
              <ModulView
                px8
                py8
                rounded
                bgSurface={section.id === selected?.id}
                borderSubtle={section.id === selected?.id}
              >
                <ModulText variant="body" fwBold={section.id === selected?.id}>
                  {section.title}
                </ModulText>
              </ModulView>
            </Pressable>
          ))}
        </ModulView>
        {selected ? (
          <ModulView flex1 bgSurface borderSubtle roundedLg p16 gap16>
            <ModulText variant="sectionTitle">{selected.title}</ModulText>
            {selected.content}
          </ModulView>
        ) : null}
      </ModulView>
    </ModulPageWeb>
  );
}

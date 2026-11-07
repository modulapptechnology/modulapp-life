import type { ReactNode } from 'react';
import { ScrollView } from 'react-native';
import { ModulText } from '../../../atoms/ModulText/ModulText';
import { ModulView } from '../../../atoms/ModulView/ModulView';

export type ModulSettingsSection = { id: string; title: string; content: ReactNode };

export type ModulSettingsViewProps = { sections: ModulSettingsSection[] };

// Plantilla de pantalla de ajustes: una lista de secciones con título y contenido libre.
export function ModulSettingsView({ sections }: ModulSettingsViewProps) {
  return (
    <ScrollView>
      <ModulView p16 gap16>
        {sections.map((section) => (
          <ModulView key={section.id} gap8>
            <ModulText variant="sectionTitle">{section.title}</ModulText>
            {section.content}
          </ModulView>
        ))}
      </ModulView>
    </ScrollView>
  );
}

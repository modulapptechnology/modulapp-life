import { ModulText } from '../../../atoms/ModulText/ModulText';
import { ModulView } from '../../../atoms/ModulView/ModulView';
import {
  ModulListPattern,
  type ModulListPatternProps,
} from '../../../organisms/ModulListPattern/ModulListPattern';

export type ModulListViewProps<T> = ModulListPatternProps<T> & { title: string };

// Plantilla de pantalla de lista: título y patrón de lista.
export function ModulListView<T>({ title, ...listProps }: ModulListViewProps<T>) {
  return (
    <ModulView flex1>
      <ModulView p16>
        <ModulText variant="sectionTitle" accessibilityRole="header">
          {title}
        </ModulText>
      </ModulView>
      <ModulListPattern {...listProps} />
    </ModulView>
  );
}

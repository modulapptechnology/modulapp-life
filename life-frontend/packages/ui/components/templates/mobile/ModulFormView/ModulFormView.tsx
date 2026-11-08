import { ScrollView } from 'react-native';
import { ModulText } from '../../../atoms/ModulText/ModulText';
import { ModulView } from '../../../atoms/ModulView/ModulView';
import {
  ModulFormContainer,
  type ModulFormContainerProps,
} from '../../../organisms/ModulFormContainer/ModulFormContainer';

export type ModulFormViewProps = ModulFormContainerProps & { title: string };

// Plantilla de pantalla de formulario: título y contenedor de formulario con desplazamiento.
export function ModulFormView({ title, ...formProps }: ModulFormViewProps) {
  return (
    <ScrollView keyboardShouldPersistTaps="handled">
      <ModulView p16 gap16>
        <ModulText variant="sectionTitle" accessibilityRole="header">
          {title}
        </ModulText>
        <ModulFormContainer {...formProps} />
      </ModulView>
    </ScrollView>
  );
}

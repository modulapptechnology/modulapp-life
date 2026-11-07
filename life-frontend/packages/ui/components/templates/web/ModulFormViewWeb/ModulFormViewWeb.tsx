import { Children } from 'react';
import { ModulView } from '../../../atoms/ModulView/ModulView';
import {
  ModulFormContainer,
  type ModulFormContainerProps,
} from '../../../organisms/ModulFormContainer/ModulFormContainer';
import { ModulPageWeb } from '../ModulPageWeb/ModulPageWeb';

export type ModulFormViewWebProps = Omit<ModulFormContainerProps, 'compact'> & {
  title: string;
  description?: string;
};

// Plantilla de formulario para la web: tarjeta de ancho acotado con los campos en dos columnas y las acciones
// alineadas a la derecha. Cada hijo directo es un campo; los campos largos pueden ocupar la fila completa
// envolviéndolos en un ModulView w100.
export function ModulFormViewWeb({
  title,
  description,
  children,
  ...formProps
}: ModulFormViewWebProps) {
  return (
    <ModulPageWeb title={title} description={description}>
      <ModulView bgSurface borderSubtle roundedLg p16 w100 style={{ maxWidth: 720 }}>
        <ModulFormContainer compact {...formProps}>
          <ModulView row gap16 style={{ flexWrap: 'wrap' }}>
            {Children.map(children, (child) => (
              <ModulView style={{ flexBasis: '45%', flexGrow: 1, minWidth: 240 }}>
                {child}
              </ModulView>
            ))}
          </ModulView>
        </ModulFormContainer>
      </ModulView>
    </ModulPageWeb>
  );
}

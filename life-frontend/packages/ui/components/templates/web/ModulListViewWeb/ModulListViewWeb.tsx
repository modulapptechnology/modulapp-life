import type { ReactNode } from 'react';
import { ModulView } from '../../../atoms/ModulView/ModulView';
import {
  ModulListPattern,
  type ModulListPatternProps,
} from '../../../organisms/ModulListPattern/ModulListPattern';
import { ModulPageWeb } from '../ModulPageWeb/ModulPageWeb';

export type ModulListViewWebProps<T> = Omit<ModulListPatternProps<T>, 'layout'> & {
  title: string;
  description?: string;
  actions?: ReactNode;
};

// Plantilla de lista para la web: encabezado con acciones y el patrón de lista (búsqueda y filtros en una fila)
// dentro de una tarjeta, con ancho máximo.
export function ModulListViewWeb<T>({
  title,
  description,
  actions,
  ...listProps
}: ModulListViewWebProps<T>) {
  return (
    <ModulPageWeb title={title} description={description} actions={actions} scroll={false}>
      <ModulView flex1 bgSurface borderSubtle roundedLg style={{ overflow: 'hidden' }}>
        <ModulListPattern layout="inline" {...listProps} />
      </ModulView>
    </ModulPageWeb>
  );
}

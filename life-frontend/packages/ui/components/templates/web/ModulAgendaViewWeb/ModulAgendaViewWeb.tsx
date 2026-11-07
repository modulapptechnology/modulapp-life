import type { ReactNode } from 'react';
import { ModulView } from '../../../atoms/ModulView/ModulView';
import { ModulPageWeb } from '../ModulPageWeb/ModulPageWeb';

export type ModulAgendaViewWebProps = {
  title: string;
  // Selector de vista (día, semana, mes) y navegación entre fechas: va a la derecha del título.
  toolbar?: ReactNode;
  children: ReactNode;
};

// Plantilla de agenda para la web: título y barra de herramientas en la misma fila, y la vista de calendario
// ocupando todo el alto disponible dentro de una tarjeta.
export function ModulAgendaViewWeb({ title, toolbar, children }: ModulAgendaViewWebProps) {
  return (
    <ModulPageWeb title={title} actions={toolbar} scroll={false}>
      <ModulView flex1 bgSurface borderSubtle roundedLg style={{ overflow: 'hidden' }}>
        {children}
      </ModulView>
    </ModulPageWeb>
  );
}

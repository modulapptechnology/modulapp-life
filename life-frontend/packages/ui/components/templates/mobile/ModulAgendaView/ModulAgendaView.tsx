import type { ReactNode } from 'react';
import { ModulText } from '../../../atoms/ModulText/ModulText';
import { ModulView } from '../../../atoms/ModulView/ModulView';

export type ModulAgendaViewProps = {
  title: string;
  // Selector de vista (día, semana, mes) y navegación entre fechas.
  toolbar?: ReactNode;
  children: ReactNode;
};

// Plantilla de pantalla de agenda: título, barra de herramientas y la vista de calendario como contenido.
export function ModulAgendaView({ title, toolbar, children }: ModulAgendaViewProps) {
  return (
    <ModulView flex1>
      <ModulView p16 gap8>
        <ModulText variant="sectionTitle" accessibilityRole="header">
          {title}
        </ModulText>
        {toolbar}
      </ModulView>
      <ModulView flex1>{children}</ModulView>
    </ModulView>
  );
}

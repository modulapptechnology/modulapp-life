import type { ReactNode } from 'react';
import { ScrollView } from 'react-native';
import { ModulText } from '../../../atoms/ModulText/ModulText';
import { ModulView } from '../../../atoms/ModulView/ModulView';

export type ModulPageWebProps = {
  title: string;
  description?: string;
  // Acciones principales de la página, a la derecha del título.
  actions?: ReactNode;
  // false: el contenido ocupa el alto disponible sin desplazamiento propio (listas y agenda).
  scroll?: boolean;
  children: ReactNode;
};

// Estructura común de las plantillas de la web: contenido centrado con ancho máximo y encabezado de página
// con el título a la izquierda y las acciones a la derecha.
export function ModulPageWeb({
  title,
  description,
  actions,
  scroll = true,
  children,
}: ModulPageWebProps) {
  const content = (
    <ModulView
      p16
      gap16
      w100
      flex1={!scroll}
      style={{ maxWidth: 1120, alignSelf: 'center', ...(scroll ? null : { minHeight: 0 }) }}
    >
      <ModulView row alignCenter justifyBetween gap16>
        <ModulView gap4 flex1>
          <ModulText variant="screenTitle" accessibilityRole="header">
            {title}
          </ModulText>
          {description ? (
            <ModulText variant="bodySecondary" textSecondary>
              {description}
            </ModulText>
          ) : null}
        </ModulView>
        {actions}
      </ModulView>
      {children}
    </ModulView>
  );

  return scroll ? <ScrollView>{content}</ScrollView> : <ModulView flex1>{content}</ModulView>;
}

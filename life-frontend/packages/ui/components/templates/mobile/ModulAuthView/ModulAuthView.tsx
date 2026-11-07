import type { ReactNode } from 'react';
import { ScrollView } from 'react-native';
import { ModulText } from '../../../atoms/ModulText/ModulText';
import { ModulView } from '../../../atoms/ModulView/ModulView';

export type ModulAuthViewProps = {
  title: string;
  subtitle?: string;
  children: ReactNode;
  footer?: ReactNode;
  // Enmarca el formulario en una tarjeta de ancho acotado (lo usa ModulAuthViewWeb).
  card?: boolean;
};

// Plantilla de pantalla de autenticación: título, formulario y pie con el enlace alterno.
export function ModulAuthView({
  title,
  subtitle,
  children,
  footer,
  card = false,
}: ModulAuthViewProps) {
  return (
    <ScrollView contentContainerStyle={{ flexGrow: 1, justifyContent: 'center' }}>
      <ModulView
        p16
        gap16
        w100
        maxW480={!card}
        bgSurface={card}
        borderSubtle={card}
        roundedLg={card}
        style={{ alignSelf: 'center', ...(card ? { maxWidth: 400 } : null) }}
      >
        <ModulText variant="sectionTitle" accessibilityRole="header">
          {title}
        </ModulText>
        {subtitle ? (
          <ModulText variant="body" textSecondary>
            {subtitle}
          </ModulText>
        ) : null}
        {children}
        {footer}
      </ModulView>
    </ScrollView>
  );
}

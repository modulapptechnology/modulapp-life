import type { ReactNode } from 'react';
import { KeyboardAvoidingView, Platform, ScrollView } from 'react-native';
import { ModulText } from '../../../atoms/ModulText/ModulText';
import { ModulView } from '../../../atoms/ModulView/ModulView';

export type ModulAuthViewProps = {
  title: string;
  subtitle?: string;
  children: ReactNode;
  footer?: ReactNode;
  // Enmarca el formulario en una tarjeta de ancho acotado (lo usa ModulAuthViewWeb).
  card?: boolean;
  // 'brand': título centrado, marca de agua LIFE y el pie fijo al fondo de la pantalla.
  variant?: 'default' | 'brand';
};

// Plantilla de pantalla de autenticación: título, formulario y pie con el enlace alterno.
export function ModulAuthView({
  title,
  subtitle,
  children,
  footer,
  card = false,
  variant = 'default',
}: ModulAuthViewProps) {
  if (variant === 'brand') {
    return (
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        style={{ flex: 1 }}
      >
        <ModulView flex1 bg>
          <ScrollView keyboardShouldPersistTaps="handled" contentContainerStyle={{ flexGrow: 1 }}>
            <ModulView px24 pt24 pb24 w100 maxW480 alignSelfCenter flexGrow justifyBetween gap24>
              <ModulView gap24>
                <ModulText variant="label" fontComfortaaBold textCenter accessibilityRole="header">
                  {title}
                </ModulText>
                {subtitle ? (
                  <ModulText variant="body" textSecondary textCenter>
                    {subtitle}
                  </ModulText>
                ) : null}
                {children}
              </ModulView>
              <ModulView gap16>
                <ModulText
                  variant="watermark"
                  fontBrand
                  textWatermark
                  textCenter
                  trackingWide
                  importantForAccessibility="no-hide-descendants"
                  accessibilityElementsHidden
                >
                  LIFE
                </ModulText>
                {footer}
              </ModulView>
            </ModulView>
          </ScrollView>
        </ModulView>
      </KeyboardAvoidingView>
    );
  }

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

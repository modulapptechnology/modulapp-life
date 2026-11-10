import { ScrollView, useWindowDimensions } from 'react-native';
import { ModulText } from '../../../atoms/ModulText/ModulText';
import { ModulView } from '../../../atoms/ModulView/ModulView';
import { ModulAuthView, type ModulAuthViewProps } from '../../mobile/ModulAuthView/ModulAuthView';

export type ModulAuthViewWebProps = Omit<ModulAuthViewProps, 'card' | 'variant'> & {
  // 'card': formulario en una tarjeta centrada. 'split': marca a la izquierda y formulario a la derecha.
  layout?: 'card' | 'split';
  // Solo layout 'split': nombre de la marca (pequeño arriba y grande al centro) y mensaje al pie.
  brandName?: string;
  tagline?: string;
};

// Plantilla de autenticación para la web: el mismo contenido que la móvil, en una tarjeta centrada
// o, en el diseño de web y escritorio, dividida en panel de marca y formulario.
export function ModulAuthViewWeb({
  layout = 'card',
  brandName,
  tagline,
  ...props
}: ModulAuthViewWebProps) {
  const { height } = useWindowDimensions();

  if (layout === 'card') return <ModulAuthView {...props} card />;

  const { title, subtitle, children, footer } = props;

  return (
    <ModulView flex1 row bg style={{ minHeight: height }}>
      <ModulView flex1 p40 justifyBetween>
        <ModulText variant="headerTitle" fontBrand textAccent trackingWide>
          {brandName}
        </ModulText>
        <ModulText variant="display" fontBrand textAccent>
          {brandName}
        </ModulText>
        <ModulText variant="heading" fontComfortaa style={{ maxWidth: 380 }}>
          {tagline}
        </ModulText>
      </ModulView>
      <ModulView flex1 borderLeftSubtle>
        <ScrollView
          keyboardShouldPersistTaps="handled"
          contentContainerStyle={{ flexGrow: 1, justifyContent: 'center', alignItems: 'center' }}
        >
          <ModulView w100 maxW400 gap24 p8>
            <ModulView gap4>
              <ModulText variant="heading" fontComfortaa accessibilityRole="header">
                {title}
              </ModulText>
              {subtitle ? (
                <ModulText variant="bodySecondary" fontComfortaa textSecondary>
                  {subtitle}
                </ModulText>
              ) : null}
            </ModulView>
            {children}
            {footer}
          </ModulView>
        </ScrollView>
      </ModulView>
    </ModulView>
  );
}

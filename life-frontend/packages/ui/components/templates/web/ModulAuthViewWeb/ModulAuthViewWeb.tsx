import { ModulAuthView, type ModulAuthViewProps } from '../../mobile/ModulAuthView/ModulAuthView';

export type ModulAuthViewWebProps = Omit<ModulAuthViewProps, 'card'>;

// Plantilla de autenticación para la web: el mismo contenido que la móvil, enmarcado en una tarjeta centrada.
export function ModulAuthViewWeb(props: ModulAuthViewWebProps) {
  return <ModulAuthView {...props} card />;
}

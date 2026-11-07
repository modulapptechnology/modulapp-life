import type { ReactNode } from 'react';
import { Pressable } from 'react-native';
import { useStyles } from '../../../utils/useStyles/useStyles';

export type ModulOptionCardProps = {
  label: string;
  selected: boolean;
  onPress: () => void;
  // Contenido en fila (acentos) o en columna (vista previa y nombre).
  row?: boolean;
  children: ReactNode;
};

// Tarjeta seleccionable de un grupo de opciones excluyentes: borde de color cuando está elegida.
export function ModulOptionCard({
  label,
  selected,
  onPress,
  row = false,
  children,
}: ModulOptionCardProps) {
  const { view } = useStyles();

  return (
    <Pressable
      accessibilityRole="radio"
      accessibilityLabel={label}
      accessibilityState={{ checked: selected, selected }}
      onPress={onPress}
      style={[
        view.flex1,
        view.alignCenter,
        view.radius10,
        row ? view.row : null,
        row ? view.gap8 : view.gap6,
        row ? view.p10 : view.p8,
        selected ? view.borderFocus : view.borderIdle,
      ]}
    >
      {children}
    </Pressable>
  );
}

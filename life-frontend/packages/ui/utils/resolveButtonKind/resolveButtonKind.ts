// `brand` (morado de marca, relleno), `brandOutline` (contorno morado) y `outline` (contorno neutro)
// son los botones de las pantallas de acceso.
export type ButtonKind = 'primary' | 'secondary' | 'danger' | 'brand' | 'brandOutline' | 'outline';

// `danger` (prop heredada) tiene prioridad sobre `variant`.
export function resolveButtonKind(variant: ButtonKind, danger: boolean): ButtonKind {
  return danger ? 'danger' : variant;
}

export type ButtonKind = 'primary' | 'secondary' | 'danger';

// `danger` (prop heredada) tiene prioridad sobre `variant`.
export function resolveButtonKind(variant: ButtonKind, danger: boolean): ButtonKind {
  return danger ? 'danger' : variant;
}

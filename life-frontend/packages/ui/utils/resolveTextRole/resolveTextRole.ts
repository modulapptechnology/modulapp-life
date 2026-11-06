import type { TextVariant } from '@life/theme';

const HEADING_VARIANTS: TextVariant[] = ['screenTitle', 'sectionTitle', 'headerTitle'];

// Rol de accesibilidad del texto: enlace, encabezado o ninguno.
export function resolveTextRole(variant: TextVariant, link: boolean) {
  if (link) return 'link';
  return HEADING_VARIANTS.includes(variant) ? 'header' : undefined;
}

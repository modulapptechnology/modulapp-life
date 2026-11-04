import type { createTextStyles, createViewStyles } from '@life/theme';

export type ViewStyleProps = { [K in keyof ReturnType<typeof createViewStyles>]?: boolean };
export type TextStyleProps = { [K in keyof ReturnType<typeof createTextStyles>]?: boolean };

// Separa las props booleanas de estilo (w100, bgSurface...) del resto de props.
export function splitStyleProps<S extends Record<string, object>>(
  styles: S,
  props: Record<string, unknown>,
) {
  const style: object[] = [];
  const rest: Record<string, unknown> = {};
  for (const [key, value] of Object.entries(props)) {
    if (key in styles) {
      if (value) style.push(styles[key]);
    } else {
      rest[key] = value;
    }
  }
  return { style, rest };
}

const SPACING_PROPS = new Set([
  'padding', 'paddingHorizontal', 'paddingVertical',
  'margin', 'marginTop', 'marginBottom', 'marginHorizontal', 'marginVertical',
  'gap',
]);

export function scaleSpacing<T extends Record<string, object>>(styles: T, scale: number): T {
  if (scale === 1) return styles;
  return Object.fromEntries(
    Object.entries(styles).map(([name, style]) => [
      name,
      Object.fromEntries(
        Object.entries(style).map(([prop, value]) => [
          prop,
          SPACING_PROPS.has(prop) && typeof value === 'number' ? value * scale : value,
        ]),
      ),
    ]),
  ) as T;
}

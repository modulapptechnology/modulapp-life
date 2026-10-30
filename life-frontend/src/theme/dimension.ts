export const layout = {
  maxContentWidth: 720,
  screenMargin: 16,
  screenPaddingY: 16,
} as const;

// Escalas responsivas: el espaciado crece según el ancho de pantalla
export const breakpoints = { tablet: 768, desktop: 1024 } as const;
export const spacingScale = { phone: 1, tablet: 1.25, desktop: 1.5 } as const;

export const getSpacingScale = (width: number) => {
  if (width >= breakpoints.desktop) return spacingScale.desktop;
  if (width >= breakpoints.tablet) return spacingScale.tablet;
  return spacingScale.phone;
};

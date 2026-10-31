import type { ThemeColors } from './colors';

export const createViewStyles = (colors: ThemeColors) =>
  ({
    // Dimensiones
    w100: { width: '100%' },
    w50: { width: '50%' },
    w25: { width: '25%' },
    h100: { height: '100%' },
    minH44: { minHeight: 44 },
    minW44: { minWidth: 44 },

    // Espaciado (escala: 4, 8, 12, 16, 24, 32)
    p4: { padding: 4 },
    p8: { padding: 8 },
    p16: { padding: 16 },
    px16: { paddingHorizontal: 16 },
    py8: { paddingVertical: 8 },
    mt8: { marginTop: 8 },
    mt16: { marginTop: 16 },
    mb8: { marginBottom: 8 },
    mb16: { marginBottom: 16 },
    gap2: { gap: 2 },
    gap4: { gap: 4 },
    gap8: { gap: 8 },
    gap16: { gap: 16 },

    // Layout
    flex1: { flex: 1 },
    row: { flexDirection: 'row' },
    wrap: { flexWrap: 'wrap' },
    alignCenter: { alignItems: 'center' },
    justifyCenter: { justifyContent: 'center' },
    justifyBetween: { justifyContent: 'space-between' },

    // Fondos
    bg: { backgroundColor: colors.background },
    bgSurface: { backgroundColor: colors.surface },
    bgPrimary: { backgroundColor: colors.primary },
    bgDanger: { backgroundColor: colors.danger },

    // Bordes y radios
    border: { borderWidth: 1, borderColor: colors.borderStrong },
    borderFocus: { borderWidth: 2, borderColor: colors.primary },
    borderError: { borderWidth: 2, borderColor: colors.danger },
    rounded: { borderRadius: 8 },
    roundedLg: { borderRadius: 12 },
    roundedFull: { borderRadius: 999 },

    opacityDisabled: { opacity: 0.4 },
  }) as const;

export const createTextStyles = (colors: ThemeColors) =>
  ({
    textPrimary: { color: colors.text },
    textSecondary: { color: colors.textSecondary },
    textOnPrimary: { color: colors.onAccent },
    textBrand: { color: colors.primary },
    textDanger: { color: colors.danger },

    fwMedium: { fontWeight: '500' },
    fwSemibold: { fontWeight: '600' },
    fwBold: { fontWeight: '700' },

    textCenter: { textAlign: 'center' },
    textRight: { textAlign: 'right' },
  }) as const;

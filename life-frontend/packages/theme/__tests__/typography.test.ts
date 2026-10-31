import { MIN_FONT_SIZE, typography } from '../typography';

describe('typography', () => {
  it('ninguna variante de texto baja del tamaño mínimo de fuente', () => {
    const tamanos = Object.values(typography).map((variante) => variante.fontSize);

    const menorTamano = Math.min(...tamanos);

    expect(menorTamano).toBeGreaterThanOrEqual(MIN_FONT_SIZE);
  });
});

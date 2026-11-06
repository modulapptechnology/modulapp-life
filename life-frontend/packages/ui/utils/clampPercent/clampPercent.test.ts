import { clampPercent } from './clampPercent';

describe('clampPercent', () => {
  it('devuelve 0 cuando el valor es negativo', () => {
    const valor = -20;

    const porcentaje = clampPercent(valor);

    expect(porcentaje).toBe(0);
  });

  it('devuelve 100 cuando el valor supera 100', () => {
    const valor = 250;

    const porcentaje = clampPercent(valor);

    expect(porcentaje).toBe(100);
  });

  it('conserva el valor cuando está entre 0 y 100', () => {
    const valor = 60;

    const porcentaje = clampPercent(valor);

    expect(porcentaje).toBe(60);
  });
});

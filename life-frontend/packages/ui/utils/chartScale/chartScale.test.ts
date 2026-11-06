import { activeTicks, scaleBars } from './chartScale';

describe('chartScale', () => {
  it('da la altura máxima a la barra más alta y el resto en proporción', () => {
    const valores = [10, 5, 0];

    const alturas = scaleBars(valores, 100);

    expect(alturas).toEqual([100, 50, 0]);
  });

  it('deja todas las barras en 0 cuando no hay valores positivos', () => {
    const valores = [0, -4];

    const alturas = scaleBars(valores, 100);

    expect(alturas).toEqual([0, 0]);
  });

  it('enciende la mitad de las marcas al 50% y limita los porcentajes fuera de rango', () => {
    const porcentajes = [50, 150, -10];

    const encendidas = porcentajes.map((porcentaje) => activeTicks(porcentaje, 60));

    expect(encendidas).toEqual([30, 60, 0]);
  });
});

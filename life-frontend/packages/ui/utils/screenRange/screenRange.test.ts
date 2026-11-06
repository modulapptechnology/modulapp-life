import { breakpoints } from '@life/theme';
import { getScreenRange } from './screenRange';

describe('getScreenRange', () => {
  it('devuelve phone por debajo del breakpoint de tablet', () => {
    const ancho = breakpoints.tablet - 1;

    const rango = getScreenRange(ancho);

    expect(rango).toBe('phone');
  });

  it('devuelve tablet desde el breakpoint de tablet', () => {
    const ancho = breakpoints.tablet;

    const rango = getScreenRange(ancho);

    expect(rango).toBe('tablet');
  });

  it('devuelve desktop desde el breakpoint de escritorio', () => {
    const ancho = breakpoints.desktop;

    const rango = getScreenRange(ancho);

    expect(rango).toBe('desktop');
  });
});

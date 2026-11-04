import { scaleSpacing } from '../utils/scaleSpacing';

describe('scaleSpacing', () => {
  it('multiplica padding, margin y gap por la escala', () => {
    const estilos = { p16: { padding: 16 }, gap8: { gap: 8 } };

    const escalados = scaleSpacing(estilos, 1.5);

    expect(escalados).toEqual({ p16: { padding: 24 }, gap8: { gap: 12 } });
  });

  it('no modifica las propiedades que no son de espaciado', () => {
    const estilos = { box: { padding: 10, minHeight: 44, backgroundColor: '#fff' } };

    const escalados = scaleSpacing(estilos, 2);

    expect(escalados.box).toEqual({ padding: 20, minHeight: 44, backgroundColor: '#fff' });
  });

  it('devuelve los mismos estilos cuando la escala es 1', () => {
    const estilos = { p16: { padding: 16 } };

    const escalados = scaleSpacing(estilos, 1);

    expect(escalados).toBe(estilos);
  });
});

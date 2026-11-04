import { splitStyleProps } from '../utils/styleProps';

describe('splitStyleProps', () => {
  it('separa las props booleanas de estilo del resto de props', () => {
    const estilos = { p16: { padding: 16 }, bg: { backgroundColor: '#fff' } };
    const props = { p16: true, bg: false, accessibilityLabel: 'caja' };

    const { style, rest } = splitStyleProps(estilos, props);

    expect(style).toEqual([{ padding: 16 }]);
    expect(rest).toEqual({ accessibilityLabel: 'caja' });
  });
});

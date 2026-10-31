import { isBlank, isValidLength } from '../index';

describe('isBlank', () => {
  it('considera en blanco un texto con solo espacios', () => {
    const texto = ' \t ';

    const resultado = isBlank(texto);

    expect(resultado).toBe(true);
  });
});

describe('isValidLength', () => {
  it('rechaza un texto más largo que el máximo', () => {
    const texto = 'abcdef';

    const resultado = isValidLength(texto, 5);

    expect(resultado).toBe(false);
  });

  it('acepta un texto con largo igual al máximo sin contar los espacios de los extremos', () => {
    const texto = ' abcde ';

    const resultado = isValidLength(texto, 5);

    expect(resultado).toBe(true);
  });
});

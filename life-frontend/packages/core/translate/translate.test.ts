import { translate } from './translate';

describe('translate', () => {
  it('devuelve el texto en el idioma pedido', () => {
    const clave = 'module.tasks';

    const textos = [translate(clave, 'es'), translate(clave, 'en')];

    expect(textos).toEqual(['Tareas', 'Tasks']);
  });

  it('devuelve la clave cuando no hay traducción', () => {
    const clave = 'module.inexistente';

    const texto = translate(clave, 'es');

    expect(texto).toBe(clave);
  });
});

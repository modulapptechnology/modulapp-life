import { documentTitle } from '../src/lib/documentTitle';

describe('documentTitle', () => {
  it('arma el título de la pestaña con la sección y el nombre de la aplicación', () => {
    const seccion = 'Agenda';

    const titulo = documentTitle(seccion);

    expect(titulo).toBe('Agenda · ModulApp Life');
  });
});

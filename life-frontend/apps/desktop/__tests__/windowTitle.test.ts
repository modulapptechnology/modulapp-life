import { windowTitle } from '../src/windowTitle';

describe('windowTitle', () => {
  it('usa solo el nombre de la aplicación cuando no hay sección', () => {
    const seccion = undefined;

    const titulo = windowTitle(seccion);

    expect(titulo).toBe('ModulApp Life');
  });
});

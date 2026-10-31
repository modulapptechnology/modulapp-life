import { buildTitle, normalizeText } from '../index';

describe('normalizeText', () => {
  it('quita los espacios de los extremos y reduce los repetidos', () => {
    const texto = '  Mis   tareas \n de hoy ';

    const resultado = normalizeText(texto);

    expect(resultado).toBe('Mis tareas de hoy');
  });
});

describe('buildTitle', () => {
  it('antepone la sección al nombre de la aplicación', () => {
    const nombreApp = 'ModulApp Life';

    const titulo = buildTitle(nombreApp, '  Tareas ');

    expect(titulo).toBe('Tareas · ModulApp Life');
  });

  it('devuelve solo el nombre de la aplicación si la sección está en blanco', () => {
    const nombreApp = 'ModulApp Life';

    const titulo = buildTitle(nombreApp, '   ');

    expect(titulo).toBe('ModulApp Life');
  });
});

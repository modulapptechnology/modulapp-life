import { parseProblem } from './parseProblem';

describe('parseProblem', () => {
  it('lee el título, el detalle y los errores por campo del formato de error de la API', () => {
    const text = JSON.stringify({
      title: 'Uno o más errores de validación.',
      detail: 'Revisa los campos.',
      errors: { name: ['El nombre es obligatorio.'] },
      traceId: 'interno',
    });

    const problem = parseProblem(text);

    expect(problem).toEqual({
      title: 'Uno o más errores de validación.',
      detail: 'Revisa los campos.',
      errors: { name: ['El nombre es obligatorio.'] },
    });
  });

  it('devuelve un problema vacío cuando el cuerpo no es JSON', () => {
    const text = '<html>Bad Gateway</html>';

    const problem = parseProblem(text);

    expect(problem).toEqual({});
  });

  it('ignora los campos con un tipo distinto al esperado', () => {
    const text = JSON.stringify({ title: 5, detail: null, errors: { name: ['ok', 3], age: 'x' } });

    const problem = parseProblem(text);

    expect(problem).toEqual({ errors: { name: ['ok'] } });
  });
});

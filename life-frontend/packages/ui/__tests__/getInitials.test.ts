import { getInitials } from '../utils/getInitials';

describe('getInitials', () => {
  it('devuelve las iniciales en mayúscula de nombre y apellido', () => {
    const nombre = 'carlos medina';

    const iniciales = getInitials(nombre);

    expect(iniciales).toBe('CM');
  });

  it('devuelve una sola letra cuando el nombre tiene una palabra', () => {
    const nombre = 'Carlos';

    const iniciales = getInitials(nombre);

    expect(iniciales).toBe('C');
  });

  it('usa solo las dos primeras palabras cuando hay más de dos', () => {
    const nombre = 'Carlos Juny Medina Álvarez';

    const iniciales = getInitials(nombre);

    expect(iniciales).toBe('CJ');
  });

  it('ignora los espacios sobrantes al inicio, al final y entre palabras', () => {
    const nombre = '   Ana    Gómez  ';

    const iniciales = getInitials(nombre);

    expect(iniciales).toBe('AG');
  });

  it('devuelve vacío cuando el nombre está vacío', () => {
    const nombre = '   ';

    const iniciales = getInitials(nombre);

    expect(iniciales).toBe('');
  });
});

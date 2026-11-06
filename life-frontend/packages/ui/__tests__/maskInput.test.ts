import { formatDateInput, formatTimeInput } from '../utils/maskInput';

describe('maskInput', () => {
  it('separa día, mes y año con barras', () => {
    const texto = '25122026';

    const fecha = formatDateInput(texto);

    expect(fecha).toBe('25/12/2026');
  });

  it('ignora las letras y deja los dígitos escritos hasta ahora', () => {
    const texto = '2a5/1';

    const fecha = formatDateInput(texto);

    expect(fecha).toBe('25/1');
  });

  it('separa hora y minutos con dos puntos y descarta el exceso', () => {
    const texto = '093055';

    const hora = formatTimeInput(texto);

    expect(hora).toBe('09:30');
  });
});

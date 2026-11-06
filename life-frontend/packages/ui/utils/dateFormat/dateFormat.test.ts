import {
  dateToIso,
  formatDate,
  formatTime,
  isoToDate,
  parseDateInput,
  parseTimeInput,
} from './dateFormat';

describe('dateFormat', () => {
  it('reconoce una fecha escrita y la devuelve con el mismo día, mes y año', () => {
    const texto = '25/12/2026';

    const fecha = parseDateInput(texto);

    expect(fecha && formatDate(fecha)).toBe('25/12/2026');
  });

  it('rechaza una fecha que no existe o está incompleta', () => {
    const textos = ['31/02/2026', '25/12'];

    const fechas = textos.map(parseDateInput);

    expect(fechas).toEqual([null, null]);
  });

  it('rechaza una hora fuera de rango y acepta una válida', () => {
    const textos = ['24:00', '09:60', '09:30'];

    const horas = textos.map((texto) => {
      const hora = parseTimeInput(texto);
      return hora && formatTime(hora);
    });

    expect(horas).toEqual([null, null, '09:30']);
  });

  it('convierte entre DD/MM/AAAA y el formato AAAA-MM-DD del calendario del navegador', () => {
    const texto = '05/01/2026';

    const iso = dateToIso(texto);

    expect([iso, isoToDate(iso)]).toEqual(['2026-01-05', texto]);
  });
});

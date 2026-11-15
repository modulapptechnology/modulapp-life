import { matchShortcut } from './shortcuts';

describe('matchShortcut', () => {
  it('reconoce la tecla n como nueva captura', () => {
    const evento = { key: 'n', target: { tagName: 'DIV' } };

    const resultado = matchShortcut(evento);

    expect(resultado).toBe('newCapture');
  });

  it('ignora la tecla cuando se escribe en un campo de texto', () => {
    const evento = { key: 'n', target: { tagName: 'INPUT' } };

    const resultado = matchShortcut(evento);

    expect(resultado).toBeNull();
  });

  it('ignora la tecla cuando lleva Ctrl para no chocar con el navegador', () => {
    const evento = { key: 'n', ctrlKey: true, target: { tagName: 'DIV' } };

    const resultado = matchShortcut(evento);

    expect(resultado).toBeNull();
  });
});

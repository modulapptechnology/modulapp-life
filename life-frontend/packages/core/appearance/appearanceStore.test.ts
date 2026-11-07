import { createAppearanceStore } from './appearanceStore';

describe('appearanceStore', () => {
  it('empieza en sistema y con el acento Modul', () => {
    const store = createAppearanceStore();

    expect(store.getState()).toEqual({ preference: 'system', accent: 'modul' });
  });

  it('cambia el tema y avisa a quien está suscrito', () => {
    const store = createAppearanceStore();
    const listener = jest.fn();
    store.subscribe(listener);

    store.setPreference('dark');

    expect(store.getState().preference).toBe('dark');
    expect(listener).toHaveBeenCalledTimes(1);
  });

  it('recupera lo guardado y ignora valores inválidos', () => {
    const storage = {
      getItem: jest.fn().mockReturnValue('{"preference":"neon","accent":"ocean"}'),
      setItem: jest.fn(),
    };

    const store = createAppearanceStore(storage);

    expect(store.getState()).toEqual({ preference: 'system', accent: 'ocean' });
  });
});

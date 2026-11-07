import { createSessionStore } from './sessionStore';

describe('sessionStore', () => {
  it('guarda el usuario al iniciar sesión', () => {
    const store = createSessionStore();

    store.signIn({ id: '1', name: 'Ana Pérez' });

    expect(store.getState().user).toEqual({ id: '1', name: 'Ana Pérez' });
  });

  it('invalida la sesión local al cerrarla', () => {
    const store = createSessionStore({
      user: { id: '1', name: 'Ana Pérez' },
      pendingOperations: 3,
    });

    store.signOut();

    expect(store.getState()).toEqual({ user: null, pendingOperations: 0 });
  });

  it('avisa a los suscriptores solo mientras están suscritos', () => {
    const store = createSessionStore();
    const listener = jest.fn();
    const unsubscribe = store.subscribe(listener);
    store.setPendingOperations(1);
    unsubscribe();

    store.setPendingOperations(2);

    expect(listener).toHaveBeenCalledTimes(1);
  });

  it('no permite un número negativo de operaciones pendientes', () => {
    const store = createSessionStore();

    store.setPendingOperations(-4);

    expect(store.getState().pendingOperations).toBe(0);
  });
});

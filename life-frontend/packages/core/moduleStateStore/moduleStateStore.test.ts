import { createModuleStateStore, moduleStateKey } from './moduleStateStore';

describe('moduleStateStore', () => {
  it('conserva el estado de un módulo al guardar el de otro', () => {
    const store = createModuleStateStore();

    store.set('p:tareas', 'filtro');
    store.set('p:agenda', 'semana');

    expect(store.get('p:tareas')).toBe('filtro');
  });

  it('no mezcla el estado de dos espacios para el mismo módulo', () => {
    const store = createModuleStateStore();

    store.set(moduleStateKey('p', 'tareas'), 'filtro');

    expect(store.get(moduleStateKey('c', 'tareas'))).toBeUndefined();
  });

  it('borra todo el estado al limpiar', () => {
    const store = createModuleStateStore();
    store.set('p:tareas', 'filtro');

    store.clear();

    expect(store.get('p:tareas')).toBeUndefined();
  });
});

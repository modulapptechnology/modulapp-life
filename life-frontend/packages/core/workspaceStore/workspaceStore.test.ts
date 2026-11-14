import type { Workspace } from '@life/types';
import type { SecureKeyValueStore } from '../sessionTokenStorage/sessionTokenStorage';
import type { WorkspaceService } from '../workspaceService/workspaceService';
import {
  ACTIVE_WORKSPACE_KEY,
  createWorkspaceStore,
  resolveActiveWorkspace,
} from './workspaceStore';

const base = { description: null, status: 'Active', role: 'Owner', rowVersion: '' } as const;
const personal: Workspace = { ...base, id: 'p', name: 'Personal', type: 'Personal' };
const casa: Workspace = { ...base, id: 'c', name: 'Casa', type: 'Household' };

function createStorage(initial: Record<string, string> = {}) {
  const data = { ...initial };
  return {
    get: jest.fn((key: string) => Promise.resolve(data[key] ?? null)),
    set: jest.fn((key: string, value: string) => {
      data[key] = value;
      return Promise.resolve();
    }),
    remove: jest.fn((key: string) => {
      delete data[key];
      return Promise.resolve();
    }),
  } as jest.Mocked<SecureKeyValueStore>;
}

function createService(overrides: Partial<WorkspaceService> = {}) {
  return {
    list: jest.fn().mockResolvedValue([personal, casa]),
    ...overrides,
  } as jest.Mocked<WorkspaceService>;
}

describe('resolveActiveWorkspace', () => {
  it('usa el espacio guardado cuando sigue siendo accesible', () => {
    const espacios = [personal, casa];

    const activo = resolveActiveWorkspace(espacios, 'c');

    expect(activo).toBe(casa);
  });

  it('usa el espacio Personal cuando el guardado ya no es accesible', () => {
    const espacios = [personal, casa];

    const activo = resolveActiveWorkspace(espacios, 'borrado');

    expect(activo).toBe(personal);
  });

  it('devuelve null cuando no hay espacio guardado ni Personal', () => {
    const espacios = [casa];

    const activo = resolveActiveWorkspace(espacios, null);

    expect(activo).toBeNull();
  });
});

describe('workspaceStore', () => {
  it('carga los espacios y activa el Personal cuando no hay uno guardado', async () => {
    const store = createWorkspaceStore(createStorage());

    await store.load(createService());

    expect(store.getState()).toEqual({
      status: 'ready',
      workspaces: [personal, casa],
      activeId: 'p',
    });
  });

  it('recupera el último espacio activo guardado', async () => {
    const storage = createStorage({ [ACTIVE_WORKSPACE_KEY]: 'c' });
    const store = createWorkspaceStore(storage);

    await store.load(createService());

    expect(store.getState().activeId).toBe('c');
  });

  it('pasa por el estado de carga mientras lee', async () => {
    const store = createWorkspaceStore(createStorage());
    const estados: string[] = [];
    store.subscribe(() => estados.push(store.getState().status));

    await store.load(createService());

    expect(estados).toEqual(['loading', 'ready']);
  });

  it('queda en error si la lectura falla y permite reintentar', async () => {
    const list = jest.fn().mockRejectedValueOnce(new Error('red')).mockResolvedValue([personal]);
    const store = createWorkspaceStore(createStorage());
    await store.load(createService({ list }));

    const estadoFallido = store.getState().status;
    await store.load(createService({ list }));

    expect([estadoFallido, store.getState().status]).toEqual(['error', 'ready']);
  });

  it('cambia el espacio activo y lo guarda', async () => {
    const storage = createStorage();
    const store = createWorkspaceStore(storage);
    await store.load(createService());

    await store.select('c');

    expect(store.getState().activeId).toBe('c');
    expect(storage.set).toHaveBeenLastCalledWith(ACTIVE_WORKSPACE_KEY, 'c');
  });

  it('ignora un espacio que no está en la lista', async () => {
    const storage = createStorage();
    const store = createWorkspaceStore(storage);
    await store.load(createService());

    await store.select('inexistente');

    expect(store.getState().activeId).toBe('p');
    expect(storage.set).not.toHaveBeenCalledWith(ACTIVE_WORKSPACE_KEY, 'inexistente');
  });

  it('limpia el estado al reiniciar sin borrar el espacio guardado', async () => {
    const storage = createStorage();
    const store = createWorkspaceStore(storage);
    await store.load(createService());

    store.reset();

    expect(store.getState()).toEqual({ status: 'idle', workspaces: [], activeId: null });
    expect(storage.remove).not.toHaveBeenCalled();
  });
});

import type { ApiClient } from '@life/api-client';
import type { Workspace } from '@life/types';
import { createWorkspaceService } from './workspaceService';

const espacio: Workspace = {
  id: 'p',
  name: 'Personal',
  description: null,
  type: 'Personal',
  status: 'Active',
  role: 'Owner',
  rowVersion: '',
};

function createClient(result: unknown) {
  return { get: jest.fn().mockResolvedValue(result) } as unknown as jest.Mocked<ApiClient>;
}

describe('workspaceService', () => {
  it('pide la lista de espacios a la ruta versionada de la API', async () => {
    const client = createClient({ kind: 'success', status: 200, data: [espacio] });
    const service = createWorkspaceService(client);

    await service.list();

    expect(client.get).toHaveBeenCalledWith('/api/v1/workspaces');
  });

  it('devuelve los espacios que responde la API', async () => {
    const client = createClient({ kind: 'success', status: 200, data: [espacio] });
    const service = createWorkspaceService(client);

    const espacios = await service.list();

    expect(espacios).toEqual([espacio]);
  });

  it('falla cuando la API no responde con éxito', async () => {
    const client = createClient({ kind: 'serverError', status: 500 });
    const service = createWorkspaceService(client);

    const lectura = service.list();

    await expect(lectura).rejects.toThrow('No se pudieron cargar los espacios de trabajo.');
  });
});

import type { ApiClient } from '@life/api-client';
import type { Workspace } from '@life/types';

export type WorkspaceService = {
  list: () => Promise<Workspace[]>;
};

export function createWorkspaceService(client: ApiClient): WorkspaceService {
  return {
    async list() {
      const result = await client.get<Workspace[]>('/api/v1/workspaces');
      if (result.kind !== 'success') {
        throw new Error('No se pudieron cargar los espacios de trabajo.');
      }
      return result.data;
    },
  };
}

import { createWorkspaceService } from '@life/core';
import { apiClient } from '../session/session';

export const workspaceService = createWorkspaceService(apiClient);

import { createAuthService } from '@life/core';
import { apiClient } from '../session/session';

export const authService = createAuthService(apiClient);

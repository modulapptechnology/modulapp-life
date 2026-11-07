import { createSessionApiClient } from '@life/core';

export const apiClient = createSessionApiClient(import.meta.env.EXPO_PUBLIC_API_URL ?? '');

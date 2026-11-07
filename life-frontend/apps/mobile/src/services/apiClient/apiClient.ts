import { createSessionApiClient } from '@life/core';

export const apiClient = createSessionApiClient(process.env.EXPO_PUBLIC_API_URL ?? '');

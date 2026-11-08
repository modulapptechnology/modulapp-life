import type { SessionUser } from '@life/types';

function decodeBase64Url(value: string): string {
  const base64 = value.replace(/-/g, '+').replace(/_/g, '/');
  const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=');
  const binary = atob(padded);
  const bytes = Uint8Array.from(binary, (char) => char.charCodeAt(0));
  return new TextDecoder().decode(bytes);
}

export function readUserFromAccessToken(accessToken: string): SessionUser | null {
  const payload = accessToken.split('.')[1];
  if (!payload) {
    return null;
  }
  try {
    const claims: unknown = JSON.parse(decodeBase64Url(payload));
    if (typeof claims !== 'object' || claims === null) {
      return null;
    }
    const { sub, email } = claims as Record<string, unknown>;
    if (typeof sub !== 'string' || typeof email !== 'string') {
      return null;
    }
    return { id: sub, name: email, token: accessToken };
  } catch {
    return null;
  }
}

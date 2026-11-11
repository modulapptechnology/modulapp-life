import type * as Icons from '@life/icons';
import type { ThemeColors } from '@life/theme';

export type ModuleIconName = Extract<keyof typeof Icons, `${string}Icon`>;
export type ModuleAccent = Extract<
  keyof ThemeColors,
  'primary' | 'secondary' | 'success' | 'info' | 'warning'
>;

export type ModuleNavLink = {
  path: string;
  nameKey: string;
  icon: ModuleIconName;
};

export type ModuleNavGroup = {
  id: string;
  titleKey: string;
  links: ModuleNavLink[];
};

// Un módulo de la app: lo que necesita la navegación para mostrarlo y llevar a él.
export type ModuleDefinition = {
  id: string;
  path: string;
  nameKey: string;
  icon: ModuleIconName;
  accent: ModuleAccent;
  groups?: ModuleNavGroup[];
};

export type SessionUser = {
  id: string;
  name: string;
  avatarUri?: string;
  token?: string;
};

export type ApiFieldErrors = Record<string, string[]>;

export type ApiProblem = {
  title?: string;
  detail?: string;
  errors?: ApiFieldErrors;
};

export type ApiResult<T> =
  | { kind: 'success'; status: number; data: T }
  | { kind: 'badRequest'; status: 400; problem: ApiProblem }
  | { kind: 'unauthorized'; status: 401 }
  | { kind: 'forbidden'; status: 403; problem: ApiProblem }
  | { kind: 'notFound'; status: 404; problem: ApiProblem }
  | { kind: 'conflict'; status: 409; problem: ApiProblem }
  | { kind: 'serverError'; status: number }
  | { kind: 'unexpected'; status: number }
  | { kind: 'networkError' }
  | { kind: 'timeout' }
  | { kind: 'cancelled' };

export type NotificationChannel = 'push' | 'email' | 'inApp';
export type NotificationType = 'taskReminder' | 'agendaEvent' | 'dailySummary';

// Preferencia de notificación: un tipo en un canal, activado o no.
export type NotificationPreference = {
  type: NotificationType;
  channel: NotificationChannel;
  enabled: boolean;
};

export type { ThemeAccent, ThemePreference } from '@life/theme';

export type SessionTokens = {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string | null;
  refreshTokenExpiresAt: string;
};

export type LoginCredentials = {
  email: string;
  password: string;
};

export type AuthResponse = {
  accessToken: string;
  tokenType: string;
  expiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
};

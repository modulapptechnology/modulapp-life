export type Language = 'es' | 'en';

const dictionaries: Record<Language, Record<string, string>> = {
  es: {
    'nav.home': 'Inicio',
    'nav.title': 'ModulApp Life',
    'nav.close': 'Cerrar menú',
    'user.login': 'Iniciar sesión',
    'user.loginDemo': 'Entrar como usuario de prueba',
    'user.profile': 'Perfil',
    'user.settings': 'Ajustes',
    'user.signOut': 'Cerrar sesión',
    'user.signOutTitle': '¿Cerrar sesión?',
    'user.signOutPending':
      'Hay operaciones pendientes de sincronizar. Si cierras sesión se perderán.',
    'common.cancel': 'Cancelar',
    'module.tasks': 'Tareas',
    'module.agenda': 'Agenda',
  },
  en: {
    'nav.home': 'Home',
    'nav.title': 'ModulApp Life',
    'nav.close': 'Close menu',
    'user.login': 'Sign in',
    'user.loginDemo': 'Enter as test user',
    'user.profile': 'Profile',
    'user.settings': 'Settings',
    'user.signOut': 'Sign out',
    'user.signOutTitle': 'Sign out?',
    'user.signOutPending': 'There are operations pending sync. They will be lost if you sign out.',
    'common.cancel': 'Cancel',
    'module.tasks': 'Tasks',
    'module.agenda': 'Agenda',
  },
};

export function detectLanguage(): Language {
  return Intl.DateTimeFormat().resolvedOptions().locale.startsWith('en') ? 'en' : 'es';
}

// TEMPORAL: se reemplaza por ModulApp Translation; el resto del código solo depende de esta firma.
export function translate(key: string, language: Language = detectLanguage()): string {
  return dictionaries[language][key] ?? key;
}

export type Language = 'es' | 'en';

const dictionaries: Record<Language, Record<string, string>> = {
  es: {
    'nav.home': 'Inicio',
    'nav.title': 'ModulApp Life',
    'nav.close': 'Cerrar menú',
    'module.tasks': 'Tareas',
    'module.agenda': 'Agenda',
  },
  en: {
    'nav.home': 'Home',
    'nav.title': 'ModulApp Life',
    'nav.close': 'Close menu',
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

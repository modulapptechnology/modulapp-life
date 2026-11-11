import { toSidebarSections } from './toSidebarSections';

describe('toSidebarSections', () => {
  it('convierte cada módulo en una sección con el nombre traducido', () => {
    const modulos = [
      {
        path: '/agenda',
        nameKey: 'module.agenda',
        icon: 'CalendarIcon' as const,
        accent: 'secondary' as const,
      },
    ];

    const secciones = toSidebarSections(modulos, (clave) => `traducido:${clave}`);

    expect(secciones).toEqual([
      {
        path: '/agenda',
        label: 'traducido:module.agenda',
        icon: 'CalendarIcon',
        accent: 'secondary',
      },
    ]);
  });

  it('traduce también los grupos y las sub-opciones del módulo', () => {
    const modulos = [
      {
        path: '/agenda',
        nameKey: 'module.agenda',
        icon: 'CalendarIcon' as const,
        groups: [
          {
            id: 'calendario',
            titleKey: 'module.agenda.calendar',
            links: [
              {
                path: '/agenda/nuevo',
                nameKey: 'module.agenda.new',
                icon: 'CalendarAddIcon' as const,
              },
            ],
          },
        ],
      },
    ];

    const secciones = toSidebarSections(modulos, (clave) => `traducido:${clave}`);

    expect(secciones[0].groups).toEqual([
      {
        id: 'calendario',
        title: 'traducido:module.agenda.calendar',
        links: [
          { path: '/agenda/nuevo', label: 'traducido:module.agenda.new', icon: 'CalendarAddIcon' },
        ],
      },
    ]);
  });

  it('devuelve una lista vacía cuando no hay módulos habilitados', () => {
    const modulos: Parameters<typeof toSidebarSections>[0] = [];

    const secciones = toSidebarSections(modulos, (clave) => clave);

    expect(secciones).toEqual([]);
  });
});

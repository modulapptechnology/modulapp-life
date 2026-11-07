import type { ModuleDefinition } from '@life/types';
import { createModuleRegistry } from './moduleRegistry';

const TAREAS: ModuleDefinition = {
  id: 'tareas',
  path: '/tareas',
  nameKey: 'module.tasks',
  icon: 'ChecklistMinimalisticIcon',
  accent: 'primary',
};

const AGENDA: ModuleDefinition = {
  id: 'agenda',
  path: '/agenda',
  nameKey: 'module.agenda',
  icon: 'CalendarIcon',
  accent: 'secondary',
};

describe('moduleRegistry', () => {
  it('devuelve solo los módulos habilitados', () => {
    const registry = createModuleRegistry([TAREAS, AGENDA]);

    registry.setModuleEnabled('agenda', false);

    expect(registry.getEnabled()).toEqual([TAREAS]);
  });

  it('incluye un módulo nuevo en cuanto se registra', () => {
    const registry = createModuleRegistry([TAREAS]);

    registry.register(AGENDA);

    expect(registry.getEnabled().map((modulo) => modulo.id)).toEqual(['tareas', 'agenda']);
  });

  it('no habilita un módulo registrado como deshabilitado', () => {
    const registry = createModuleRegistry([TAREAS]);

    registry.register(AGENDA, false);

    expect(registry.getEnabled()).toEqual([TAREAS]);
    expect(registry.getAll()).toEqual([TAREAS, AGENDA]);
  });

  it('deja habilitados solo los ids conocidos al reemplazar la lista', () => {
    const registry = createModuleRegistry([TAREAS, AGENDA]);

    registry.setEnabled(['agenda', 'inexistente']);

    expect(registry.getEnabled()).toEqual([AGENDA]);
  });

  it('avisa a los suscriptores al cambiar la habilitación y deja de avisar al darse de baja', () => {
    const registry = createModuleRegistry([TAREAS]);
    const listener = jest.fn();
    const darDeBaja = registry.subscribe(listener);

    registry.setModuleEnabled('tareas', false);
    darDeBaja();
    registry.setModuleEnabled('tareas', true);

    expect(listener).toHaveBeenCalledTimes(1);
  });

  it('mantiene la misma lista entre lecturas si nada cambió', () => {
    const registry = createModuleRegistry([TAREAS]);

    const primera = registry.getEnabled();

    expect(registry.getEnabled()).toBe(primera);
  });
});

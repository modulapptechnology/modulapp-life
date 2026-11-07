import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulSettings, type ModulSettingsProps } from './ModulSettings';

function buildProps(overrides: Partial<ModulSettingsProps> = {}): ModulSettingsProps {
  return {
    modulesTitle: 'Módulos',
    modules: [
      { id: 'tareas', label: 'Tareas', checked: true },
      { id: 'agenda', label: 'Agenda', checked: false },
    ],
    disableDialog: {
      title: (label) => `¿Deshabilitar ${label}?`,
      message: 'Sus datos no se eliminan.',
      confirmLabel: 'Deshabilitar',
      cancelLabel: 'Cancelar',
    },
    onModuleChange: jest.fn(),
    notificationsTitle: 'Notificaciones',
    notificationsStatus: 'ready',
    loadingMessage: 'Cargando',
    errorTitle: 'No se pudo cargar',
    retryLabel: 'Reintentar',
    onRetry: jest.fn(),
    notificationGroups: [
      {
        id: 'taskReminder',
        label: 'Recordatorios',
        toggles: [{ id: 'push', label: 'Push', checked: true }],
      },
    ],
    onNotificationChange: jest.fn(),
    closeLabel: 'Cerrar',
    onDismissSaveError: jest.fn(),
    preferencesTitle: 'Preferencias',
    preferenceLinks: [{ id: 'language', label: 'Idioma', icon: 'GlobalIcon' }],
    ...overrides,
  };
}

describe('ModulSettings', () => {
  it('habilita un módulo de inmediato, sin confirmación', async () => {
    const props = buildProps();
    await renderWithTheme(<ModulSettings {...props} />);

    await fireEvent(screen.getByLabelText('Agenda'), 'valueChange', true);

    expect(props.onModuleChange).toHaveBeenCalledWith('agenda', true);
  });

  it('pide confirmación al deshabilitar y no cambia hasta confirmar', async () => {
    const props = buildProps();
    await renderWithTheme(<ModulSettings {...props} />);

    await fireEvent(screen.getByLabelText('Tareas'), 'valueChange', false);

    expect(screen.getByText('¿Deshabilitar Tareas?')).toBeTruthy();
    expect(props.onModuleChange).not.toHaveBeenCalled();
  });

  it('deshabilita el módulo al confirmar', async () => {
    const props = buildProps();
    await renderWithTheme(<ModulSettings {...props} />);
    await fireEvent(screen.getByLabelText('Tareas'), 'valueChange', false);

    await fireEvent.press(screen.getByText('Deshabilitar'));

    expect(props.onModuleChange).toHaveBeenCalledWith('tareas', false);
  });

  it('no cambia el módulo si se cancela la confirmación', async () => {
    const props = buildProps();
    await renderWithTheme(<ModulSettings {...props} />);
    await fireEvent(screen.getByLabelText('Tareas'), 'valueChange', false);

    await fireEvent.press(screen.getByText('Cancelar'));

    expect(props.onModuleChange).not.toHaveBeenCalled();
  });

  it('guarda la preferencia por tipo y canal', async () => {
    const props = buildProps();
    await renderWithTheme(<ModulSettings {...props} />);

    await fireEvent(screen.getByLabelText('Recordatorios: Push'), 'valueChange', false);

    expect(props.onNotificationChange).toHaveBeenCalledWith('taskReminder', 'push', false);
  });

  it('muestra el estado de carga mientras lee las preferencias', async () => {
    const props = buildProps({ notificationsStatus: 'loading' });

    await renderWithTheme(<ModulSettings {...props} />);

    expect(screen.getByText('Cargando')).toBeTruthy();
    expect(screen.queryByLabelText('Recordatorios: Push')).toBeNull();
  });

  it('muestra el error de lectura y reintenta', async () => {
    const props = buildProps({ notificationsStatus: 'error' });
    await renderWithTheme(<ModulSettings {...props} />);

    await fireEvent.press(screen.getByText('Reintentar'));

    expect(screen.getByText('No se pudo cargar')).toBeTruthy();
    expect(props.onRetry).toHaveBeenCalled();
  });

  it('muestra el error de guardado y permite descartarlo', async () => {
    const props = buildProps({ saveError: 'No se pudo guardar' });
    await renderWithTheme(<ModulSettings {...props} />);

    await fireEvent.press(screen.getByLabelText('Cerrar'));

    expect(screen.getByText('No se pudo guardar')).toBeTruthy();
    expect(props.onDismissSaveError).toHaveBeenCalled();
  });

  it('lista los accesos a apariencia, idioma y formato', async () => {
    const props = buildProps();

    await renderWithTheme(<ModulSettings {...props} />);

    expect(screen.getByText('Idioma')).toBeTruthy();
  });
});

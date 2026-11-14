import { act, fireEvent, renderHook, screen } from '@testing-library/react-native';
import { animationDurations } from '../../../utils/animations/animations';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulButton } from '../../atoms/ModulButton/ModulButton';
import { ModulMessagesProvider, useMessages } from './ModulMessages';

function Pantalla({ onResultado }: { onResultado?: (aceptado: boolean) => void }) {
  const { toast, confirm } = useMessages();

  return (
    <>
      <ModulButton label="Avisar" onPress={() => toast('Guardado', { type: 'success' })} />
      <ModulButton
        label="Preguntar"
        onPress={() => {
          confirm({ title: '¿Eliminar?', destructive: true }).then((aceptado) =>
            onResultado?.(aceptado),
          );
        }}
      />
    </>
  );
}

const conMensajes = (pantalla: React.ReactElement) => (
  <ModulMessagesProvider confirmLabel="Aceptar" cancelLabel="Cancelar" closeLabel="Cerrar">
    {pantalla}
  </ModulMessagesProvider>
);

describe('ModulMessages', () => {
  afterEach(() => {
    jest.useRealTimers();
  });

  it('muestra el aviso al llamar a toast', async () => {
    await renderWithTheme(conMensajes(<Pantalla />));

    await fireEvent.press(screen.getByRole('button', { name: 'Avisar' }));

    expect(screen.getByText('Guardado')).toBeOnTheScreen();
  });

  it('quita el aviso cuando pasa su duración', async () => {
    jest.useFakeTimers();
    await renderWithTheme(conMensajes(<Pantalla />));
    await fireEvent.press(screen.getByRole('button', { name: 'Avisar' }));

    await act(async () => {
      jest.advanceTimersByTime(4000);
    });
    await act(async () => {
      jest.advanceTimersByTime(animationDurations.standard * 2);
    });

    expect(screen.queryByText('Guardado')).toBeNull();
  });

  it('resuelve confirm con true al aceptar el diálogo', async () => {
    const onResultado = jest.fn();
    await renderWithTheme(conMensajes(<Pantalla onResultado={onResultado} />));
    await fireEvent.press(screen.getByRole('button', { name: 'Preguntar' }));

    await fireEvent.press(screen.getByRole('button', { name: 'Aceptar' }));

    expect(onResultado).toHaveBeenCalledWith(true);
  });

  it('resuelve confirm con false al cancelar el diálogo', async () => {
    const onResultado = jest.fn();
    await renderWithTheme(conMensajes(<Pantalla onResultado={onResultado} />));
    await fireEvent.press(screen.getByRole('button', { name: 'Preguntar' }));

    await fireEvent.press(screen.getByRole('button', { name: 'Cancelar' }));

    expect(onResultado).toHaveBeenCalledWith(false);
  });

  it('falla con un mensaje claro si useMessages se usa fuera del proveedor', async () => {
    jest.spyOn(console, 'error').mockImplementation(() => undefined);

    const dibujar = renderHook(() => useMessages());

    await expect(dibujar).rejects.toThrow(
      'useMessages debe usarse dentro de <ModulMessagesProvider>',
    );
  });
});

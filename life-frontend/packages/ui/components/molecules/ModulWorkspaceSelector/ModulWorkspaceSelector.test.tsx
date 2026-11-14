import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulWorkspaceSelector } from './ModulWorkspaceSelector';

const espacios = [
  { id: 'p', name: 'Personal' },
  { id: 'c', name: 'Casa' },
];

describe('ModulWorkspaceSelector', () => {
  it('muestra el nombre del espacio activo', async () => {
    await renderWithTheme(
      <ModulWorkspaceSelector
        workspaces={espacios}
        activeId="c"
        onSelect={jest.fn()}
        title="Cambiar de espacio"
        closeLabel="Cerrar"
      />,
    );

    expect(screen.getByText('Casa')).toBeOnTheScreen();
  });

  it('no se muestra cuando no hay espacios', async () => {
    await renderWithTheme(
      <ModulWorkspaceSelector
        workspaces={[]}
        activeId={null}
        onSelect={jest.fn()}
        title="Cambiar de espacio"
        closeLabel="Cerrar"
      />,
    );

    expect(screen.queryByRole('button', { name: 'Cambiar de espacio' })).toBeNull();
  });

  it('lista los espacios y avisa cuál se eligió', async () => {
    const onSelect = jest.fn();
    await renderWithTheme(
      <ModulWorkspaceSelector
        workspaces={espacios}
        activeId="p"
        onSelect={onSelect}
        title="Cambiar de espacio"
        closeLabel="Cerrar"
      />,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Cambiar de espacio' }));
    await fireEvent.press(screen.getByRole('button', { name: 'Casa' }));

    expect(onSelect).toHaveBeenCalledWith('c');
  });
});

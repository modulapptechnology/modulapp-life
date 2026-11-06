import { screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulView } from './ModulView';

describe('ModulView', () => {
  it('dibuja la vista con el testID recibido', async () => {
    const identificador = 'contenedor';

    await renderWithTheme(<ModulView testID={identificador} />);

    expect(screen.getByTestId('contenedor')).toBeOnTheScreen();
  });

  it('convierte las props de estilo en estilos y no las pasa a la vista', async () => {
    const props = { row: true, testID: 'fila' };

    await renderWithTheme(<ModulView {...props} />);

    const vista = screen.getByTestId('fila');
    expect(vista.props.row).toBeUndefined();
    expect(vista).toHaveStyle({ flexDirection: 'row' });
  });
});

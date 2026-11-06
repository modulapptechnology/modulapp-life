import { screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulFieldMessage } from './ModulFieldMessage';

describe('ModulFieldMessage', () => {
  it('no muestra nada cuando no hay ayuda ni error', async () => {
    const ayuda = undefined;

    await renderWithTheme(<ModulFieldMessage help={ayuda} />);

    expect(screen.toJSON()).toBeNull();
  });

  it('muestra la ayuda sin rol de alerta', async () => {
    const ayuda = 'Usa 8 caracteres';

    await renderWithTheme(<ModulFieldMessage help={ayuda} />);

    expect(screen.getByText('Usa 8 caracteres').props.accessibilityRole).toBeUndefined();
  });

  it('muestra el error en lugar de la ayuda, con rol de alerta', async () => {
    const mensajes = { help: 'Usa 8 caracteres', error: 'Campo obligatorio' };

    await renderWithTheme(<ModulFieldMessage {...mensajes} />);

    expect(screen.getByRole('alert')).toHaveTextContent('Campo obligatorio');
  });
});

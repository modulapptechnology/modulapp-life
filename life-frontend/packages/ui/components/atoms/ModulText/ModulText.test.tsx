import { screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulText } from './ModulText';

describe('ModulText', () => {
  it('muestra el texto recibido', async () => {
    const texto = 'Hola';

    await renderWithTheme(<ModulText>{texto}</ModulText>);

    expect(screen.getByText('Hola')).toBeOnTheScreen();
  });

  it('usa el rol de encabezado en las variantes de título', async () => {
    const variante = 'screenTitle';

    await renderWithTheme(<ModulText variant={variante}>Mis tareas</ModulText>);

    expect(screen.getByRole('header', { name: 'Mis tareas' })).toBeOnTheScreen();
  });

  it('usa el rol de enlace cuando link es verdadero', async () => {
    const esEnlace = true;

    await renderWithTheme(<ModulText link={esEnlace}>Ver más</ModulText>);

    expect(screen.getByRole('link', { name: 'Ver más' })).toBeOnTheScreen();
  });
});

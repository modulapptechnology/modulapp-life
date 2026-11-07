import { screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulIcon } from './ModulIcon';

describe('ModulIcon', () => {
  it('dibuja el icono pedido por nombre', async () => {
    const nombre = 'CloseCircleIcon';

    await renderWithTheme(<ModulIcon name={nombre} />);

    expect(screen.getByTestId('CloseCircleIcon')).toBeOnTheScreen();
  });

  it('usa el tamaño md por defecto y el pedido cuando se indica', async () => {
    const tamanos = ['md', 'xl'] as const;

    await renderWithTheme(<ModulIcon name="CloseCircleIcon" size={tamanos[1]} />);

    expect(screen.getByTestId('CloseCircleIcon').props.size).toBe(46);
  });

  it('usa 20 px en el tamaño md por defecto', async () => {
    const noSize = undefined;

    await renderWithTheme(<ModulIcon name="CloseCircleIcon" size={noSize} />);

    expect(screen.getByTestId('CloseCircleIcon').props.size).toBe(20);
  });
});

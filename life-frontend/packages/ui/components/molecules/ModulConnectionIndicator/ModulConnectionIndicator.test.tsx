import { screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulConnectionIndicator } from './ModulConnectionIndicator';

const labels = { offline: 'Sin conexión', syncing: 'Sincronizando', synced: 'Sincronizado' };

describe('ModulConnectionIndicator', () => {
  it('shows the offline label and icon', async () => {
    const status = 'offline';

    await renderWithTheme(<ModulConnectionIndicator status={status} labels={labels} />);

    expect(screen.getByText('Sin conexión')).toBeOnTheScreen();
    expect(screen.getByTestId('CloudCrossIcon')).toBeOnTheScreen();
  });

  it('shows the syncing label and icon', async () => {
    const status = 'syncing';

    await renderWithTheme(<ModulConnectionIndicator status={status} labels={labels} />);

    expect(screen.getByText('Sincronizando')).toBeOnTheScreen();
    expect(screen.getByTestId('RefreshCircleIcon')).toBeOnTheScreen();
  });

  it('shows the synced label and icon', async () => {
    const status = 'synced';

    await renderWithTheme(<ModulConnectionIndicator status={status} labels={labels} />);

    expect(screen.getByText('Sincronizado')).toBeOnTheScreen();
    expect(screen.getByTestId('CloudCheckIcon')).toBeOnTheScreen();
  });
});

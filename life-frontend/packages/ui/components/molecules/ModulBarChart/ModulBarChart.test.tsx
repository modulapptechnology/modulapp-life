import { screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulBarChart } from './ModulBarChart';

const datos = [
  { label: 'Lun', value: 10 },
  { label: 'Mar', value: 20 },
];

describe('ModulBarChart', () => {
  it('muestra la etiqueta y el valor de cada dato', async () => {
    const accesibilidad = 'Tareas por día';

    await renderWithTheme(<ModulBarChart data={datos} accessibilityLabel={accesibilidad} />);

    expect(screen.getByLabelText('Tareas por día')).toBeOnTheScreen();
    expect(screen.getByText('Lun')).toBeOnTheScreen();
    expect(screen.getByText('20')).toBeOnTheScreen();
  });

  it('formatea los valores con formatValue', async () => {
    const formato = (valor: number) => `${valor} h`;

    await renderWithTheme(
      <ModulBarChart data={datos} accessibilityLabel="Horas" formatValue={formato} />,
    );

    expect(screen.getByText('10 h')).toBeOnTheScreen();
  });

  it('no falla cuando no hay datos', async () => {
    const sinDatos: typeof datos = [];

    await renderWithTheme(<ModulBarChart data={sinDatos} accessibilityLabel="Vacío" />);

    expect(screen.getByLabelText('Vacío')).toBeOnTheScreen();
  });
});

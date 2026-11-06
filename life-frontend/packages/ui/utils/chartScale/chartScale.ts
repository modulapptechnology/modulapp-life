import { clampPercent } from '../clampPercent/clampPercent';

// Altura de cada barra en proporción al valor más alto; los valores negativos cuentan como 0.
export function scaleBars(values: number[], maxHeight: number) {
  const highest = Math.max(0, ...values);
  if (highest === 0) return values.map(() => 0);

  return values.map((value) => Math.round((Math.max(0, value) / highest) * maxHeight));
}

// Cantidad de marcas encendidas del indicador circular para un porcentaje de 0 a 100.
export function activeTicks(percent: number, totalTicks: number) {
  return Math.round((clampPercent(percent) / 100) * totalTicks);
}

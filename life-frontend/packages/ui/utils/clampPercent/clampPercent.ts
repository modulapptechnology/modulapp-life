// Limita un valor al rango 0-100 de la barra de progreso.
export function clampPercent(value: number) {
  return Math.min(100, Math.max(0, value));
}

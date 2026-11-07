import { translate } from '@life/core';

// Marcador de posición: el contenido de cada módulo queda fuera de la navegación.
export default function ModulePage({ nameKey }: { nameKey: string }) {
  return (
    <main className="home">
      <h1>{translate(nameKey)}</h1>
    </main>
  );
}

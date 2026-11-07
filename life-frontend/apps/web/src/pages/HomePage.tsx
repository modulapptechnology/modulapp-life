import { APP_NAME } from '@life/constants';

// El encabezado y la barra lateral los pone AppLayout.
export default function HomePage() {
  return (
    <main className="home">
      <h1>{APP_NAME}</h1>
    </main>
  );
}

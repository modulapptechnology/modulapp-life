import { APP_NAME } from '@life/constants';

// El encabezado y la barra lateral los pone ModulLayoutWeb.
export default function HomePage() {
  return (
    <main className="home">
      <h1>{APP_NAME}</h1>
    </main>
  );
}

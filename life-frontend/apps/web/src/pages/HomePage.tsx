import { useState } from 'react';
import { APP_NAME } from '@life/constants';
import { ModulHeader } from '@life/ui';

export default function HomePage() {
  const [sidebarOpen, setSidebarOpen] = useState(true);

  return (
    <>
      <ModulHeader
        sidebarOpen={sidebarOpen}
        onToggleSidebar={() => setSidebarOpen((open) => !open)}
      />
      <main className="home">
        <h1>{APP_NAME}</h1>
      </main>
    </>
  );
}

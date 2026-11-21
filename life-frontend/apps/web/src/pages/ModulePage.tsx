import {
  moduleStateStore,
  moduleStateKey,
  translate,
  useModuleState,
  useWorkspaces,
} from '@life/core';
import { workspaceService } from '../services/workspace/workspaceService';
import { workspaceStore } from '../services/workspace/workspaceStore';

// Marcador de posición: el contenido real de cada módulo queda fuera de la navegación.
// El borrador de notas demuestra que el estado se conserva por módulo y por espacio de trabajo.
export default function ModulePage({ nameKey }: { nameKey: string }) {
  const { activeId } = useWorkspaces(workspaceStore, workspaceService);
  const [notes, setNotes] = useModuleState(moduleStateStore, moduleStateKey(activeId, nameKey), '');

  return (
    <main className="home">
      <h1>{translate(nameKey)}</h1>
      <label>
        {translate('module.notes.label')}
        <textarea value={notes} onChange={(event) => setNotes(event.target.value)} />
      </label>
    </main>
  );
}

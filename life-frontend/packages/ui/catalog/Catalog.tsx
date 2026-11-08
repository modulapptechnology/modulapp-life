import Components from './Components';
import { Fragment, useState, type ReactNode } from 'react';
import {
  ModulAgendaView,
  ModulAgendaViewWeb,
  ModulAuthView,
  ModulAuthViewWeb,
  ModulBottomSheet,
  ModulButton,
  ModulChipGroup,
  ModulDetailView,
  ModulDetailViewWeb,
  ModulDialog,
  ModulEmptyState,
  ModulFormView,
  ModulFormViewWeb,
  ModulInput,
  ModulLayoutWeb,
  ModulListView,
  ModulListViewWeb,
  ModulSettingsView,
  ModulSettingsViewWeb,
  ModulSwitch,
  ModulText,
  ModulView,
  toFieldErrors,
  type NavSection,
} from '../index';

// Catálogo del sistema de diseño (LIF-244): muestra los componentes y plantillas reales en una sola vista, solo en desarrollo.
const TASKS = ['Comprar pan', 'Pagar la luz', 'Llamar al médico', 'Preparar informe'];
const FILTERS = [
  { value: 'all', label: 'Todas' },
  { value: 'open', label: 'Pendientes' },
  { value: 'done', label: 'Hechas' },
];
const AGENDA_VIEWS = [
  { value: 'day', label: 'Día' },
  { value: 'week', label: 'Semana' },
  { value: 'month', label: 'Mes' },
];

// Opciones del menú lateral: la vista del catálogo y espacio para otras vistas del sistema de diseño.
const SECTIONS: NavSection[] = [
  { path: 'components', label: 'Components', icon: 'ChecklistMinimalisticIcon' },
  { path: 'catalog', label: 'Templates', icon: 'PaletteIcon' },
  { path: 'layouts', label: 'Layouts', icon: 'Home2Icon' },
];

function Block({ title, children }: { title: string; children: ReactNode }) {
  return (
    <ModulView gap8>
      <ModulText variant="sectionTitle">{title}</ModulText>
      {children}
    </ModulView>
  );
}

// Compara la plantilla móvil (ancho de teléfono) con la de la web (ancho completo).
function Compare({
  title,
  height,
  mobile,
  web,
}: {
  title: string;
  height: number;
  mobile: ReactNode;
  web: ReactNode;
}) {
  return (
    <Block title={title}>
      <ModulView row gap16 style={{ alignItems: 'flex-start', flexWrap: 'wrap' }}>
        <ModulView gap4>
          <ModulText variant="label">Móvil (ModulXxxView)</ModulText>
          <ModulView
            borderSubtle
            roundedLg
            bgSurface
            style={{ width: 390, height, overflow: 'hidden' }}
          >
            {mobile}
          </ModulView>
        </ModulView>
        <ModulView gap4 flex1 style={{ minWidth: 560 }}>
          <ModulText variant="label">Web (ModulXxxViewWeb)</ModulText>
          <ModulView borderSubtle roundedLg style={{ height, overflow: 'hidden' }}>
            {web}
          </ModulView>
        </ModulView>
      </ModulView>
    </Block>
  );
}

export default function Catalog() {
  const [path, setPath] = useState(SECTIONS[0].path);
  const [dialog, setDialog] = useState(false);
  const [sheet, setSheet] = useState(false);
  const [query, setQuery] = useState('');
  const [filter, setFilter] = useState('all');
  const [agenda, setAgenda] = useState('week');
  const [email, setEmail] = useState('');
  const fieldErrors = toFieldErrors({ Email: ['El correo ya está registrado'] });
  const items = TASKS.filter((task) => task.toLowerCase().includes(query.toLowerCase()));
  const renderTask = ({ item }: { item: string }) => (
    <ModulView p16>
      <ModulText variant="body">{item}</ModulText>
    </ModulView>
  );
  const listProps = {
    searchLabel: 'Buscar',
    searchValue: query,
    onSearchChange: setQuery,
    filters: FILTERS,
    filterValue: filter,
    onFilterChange: setFilter,
    data: items,
    keyExtractor: (item: string) => item,
    renderItem: renderTask,
    emptyTitle: 'Sin tareas',
  };
  const agendaToolbar = (
    <ModulChipGroup segmented value={agenda} onChange={setAgenda} options={AGENDA_VIEWS} />
  );
  const agendaBody = (
    <ModulView flex1 alignCenter justifyCenter>
      <ModulText variant="body" textSecondary>
        Contenido de la vista: {agenda}
      </ModulText>
    </ModulView>
  );
  const fields = (
    <>
      <ModulInput label="Título" value="" onChangeText={() => undefined} />
      <ModulInput label="Correo" value={email} onChangeText={setEmail} error={fieldErrors.Email} />
      <ModulInput label="Fecha" type="date" value="" onChangeText={() => undefined} />
      <ModulInput label="Notas" value="" onChangeText={() => undefined} />
    </>
  );

  const blocks: Record<string, ReactNode> = {
    overlays: (
      <>
        <Block title="ModulDialog y ModulBottomSheet">
          <ModulView row gap8>
            <ModulButton compact label="Abrir diálogo" onPress={() => setDialog(true)} />
            <ModulButton
              compact
              variant="secondary"
              label="Abrir hoja inferior"
              onPress={() => setSheet(true)}
            />
          </ModulView>
        </Block>
        <ModulDialog
          visible={dialog}
          title="Nueva tarea"
          closeLabel="Cerrar"
          onClose={() => setDialog(false)}
        >
          <ModulText variant="body">
            Contenido del diálogo. Cierra con el botón, tocando fuera o con Escape.
          </ModulText>
        </ModulDialog>
        <ModulBottomSheet
          visible={sheet}
          title="Filtros"
          closeLabel="Cerrar"
          onClose={() => setSheet(false)}
        >
          <ModulText variant="body">Contenido de la hoja inferior.</ModulText>
        </ModulBottomSheet>
      </>
    ),
    list: (
      <Compare
        title="Lista"
        height={460}
        mobile={<ModulListView title="Tareas" {...listProps} />}
        web={
          <ModulListViewWeb
            title="Tareas"
            description="4 tareas en total"
            actions={<ModulButton compact label="Nueva tarea" />}
            {...listProps}
          />
        }
      />
    ),
    form: (
      <Compare
        title="Formulario"
        height={520}
        mobile={
          <ModulFormView
            title="Nueva tarea"
            submitLabel="Guardar"
            cancelLabel="Cancelar"
            onSubmit={() => undefined}
          >
            {fields}
          </ModulFormView>
        }
        web={
          <ModulFormViewWeb
            title="Nueva tarea"
            description="Completa los datos de la tarea"
            submitLabel="Guardar"
            cancelLabel="Cancelar"
            onSubmit={() => undefined}
          >
            {fields}
          </ModulFormViewWeb>
        }
      />
    ),
    detail: (
      <Compare
        title="Detalle"
        height={340}
        mobile={
          <ModulDetailView
            title="Comprar pan"
            actions={<ModulButton variant="secondary" label="Editar" />}
          >
            <ModulText variant="body">Descripción de la tarea.</ModulText>
          </ModulDetailView>
        }
        web={
          <ModulDetailViewWeb
            title="Comprar pan"
            actions={<ModulButton compact variant="secondary" label="Editar" />}
            aside={
              <>
                <ModulText variant="label">Estado</ModulText>
                <ModulText variant="body">Pendiente</ModulText>
                <ModulText variant="label">Vence</ModulText>
                <ModulText variant="body">12/11/2026</ModulText>
              </>
            }
          >
            <ModulText variant="body">Descripción de la tarea.</ModulText>
          </ModulDetailViewWeb>
        }
      />
    ),
    auth: (
      <Compare
        title="Autenticación"
        height={400}
        mobile={
          <ModulAuthView
            title="Iniciar sesión"
            subtitle="Ingresa con tu cuenta"
            footer={<ModulText link>¿No tienes cuenta? Regístrate</ModulText>}
          >
            <ModulInput label="Correo" value="" onChangeText={() => undefined} />
            <ModulButton label="Entrar" />
          </ModulAuthView>
        }
        web={
          <ModulAuthViewWeb
            title="Iniciar sesión"
            subtitle="Ingresa con tu cuenta"
            footer={<ModulText link>¿No tienes cuenta? Regístrate</ModulText>}
          >
            <ModulInput compact label="Correo" value="" onChangeText={() => undefined} />
            <ModulButton compact label="Entrar" />
          </ModulAuthViewWeb>
        }
      />
    ),
    settings: (
      <Compare
        title="Ajustes"
        height={300}
        mobile={
          <ModulSettingsView
            sections={[
              {
                id: 'a',
                title: 'Notificaciones',
                content: <ModulSwitch label="Correo" checked onChange={() => undefined} />,
              },
              {
                id: 'b',
                title: 'Privacidad',
                content: (
                  <ModulSwitch label="Perfil público" checked={false} onChange={() => undefined} />
                ),
              },
            ]}
          />
        }
        web={
          <ModulSettingsViewWeb
            title="Ajustes"
            sections={[
              {
                id: 'a',
                title: 'Notificaciones',
                content: <ModulSwitch label="Correo" checked onChange={() => undefined} />,
              },
              {
                id: 'b',
                title: 'Privacidad',
                content: (
                  <ModulSwitch label="Perfil público" checked={false} onChange={() => undefined} />
                ),
              },
            ]}
          />
        }
      />
    ),
    agenda: (
      <Compare
        title="Agenda"
        height={320}
        mobile={
          <ModulAgendaView title="Agenda" toolbar={agendaToolbar}>
            {agendaBody}
          </ModulAgendaView>
        }
        web={
          <ModulAgendaViewWeb title="Agenda" toolbar={agendaToolbar}>
            {agendaBody}
          </ModulAgendaViewWeb>
        }
      />
    ),
  };
  const views: Record<string, ReactNode> = {
    catalog: Object.entries(blocks).map(([key, block]) => <Fragment key={key}>{block}</Fragment>),
    components: <Components />,
    templates: <ModulEmptyState title="Plantillas" description="Vista pendiente de definir." />,
    layouts: <ModulEmptyState title="Layouts" description="Vista pendiente de definir." />,
  };
  const current = SECTIONS.find((section) => section.path === path) ?? SECTIONS[0];

  return (
    <ModulLayoutWeb
      sections={SECTIONS}
      activePath={current.path}
      onNavigate={setPath}
      headerContent={
        <ModulText variant="body" fwBold>
          Sistema de diseño
        </ModulText>
      }
    >
      <ModulView p16 gap16>
        <ModulText variant="screenTitle">{current.label}</ModulText>
        {views[current.path]}
      </ModulView>
    </ModulLayoutWeb>
  );
}

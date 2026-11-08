import { useState, type ReactNode } from 'react';
import {
  ModulAvatar,
  ModulBadge,
  ModulBarChart,
  ModulButton,
  ModulCheckbox,
  ModulChip,
  ModulChipGroup,
  ModulCircularProgress,
  ModulConfirmDialog,
  ModulDatePicker,
  ModulDialog,
  ModulDragProvider,
  ModulDraggable,
  ModulDrawer,
  ModulDropZone,
  ModulEmptyState,
  ModulErrorState,
  ModulFieldMessage,
  ModulFloatingButton,
  ModulFormContainer,
  ModulHeader,
  ModulIcon,
  ModulInput,
  ModulList,
  ModulListItem,
  ModulListPattern,
  ModulLoadingState,
  ModulMenu,
  ModulModal,
  ModulOptionGroup,
  ModulProgress,
  ModulSidebar,
  ModulSwitch,
  ModulText,
  ModulToast,
  ModulView,
  useMessages,
  type NavSection,
} from '../index';
import { ModulCloseButton } from '../components/atoms/ModulCloseButton/ModulCloseButton';
import { ModulOptionCard } from '../components/atoms/ModulOptionCard/ModulOptionCard';

// Datos estáticos de ejemplo: el catálogo no llama a la API ni necesita sesión.
const BADGE_TONES = ['primary', 'secondary', 'danger', 'success', 'warning', 'info'] as const;
const AVATAR_SIZES = ['xs', 'sm', 'md', 'lg'] as const;
const TOAST_TYPES = ['success', 'error', 'info', 'warning'] as const;
const CHART = [
  { label: 'Lun', value: 4 },
  { label: 'Mar', value: 7 },
  { label: 'Mié', value: 3 },
  { label: 'Jue', value: 9 },
];
const CHIPS = [
  { value: 'all', label: 'Todas' },
  { value: 'open', label: 'Pendientes' },
  { value: 'done', label: 'Hechas' },
];
const SELECT = [
  { value: 'a', label: 'Opción A' },
  { value: 'b', label: 'Opción B' },
];
const SIDEBAR: NavSection[] = [
  { path: '/', label: 'Inicio', icon: 'Home2Icon' },
  { path: '/tasks', label: 'Tareas', icon: 'ChecklistMinimalisticIcon' },
  { path: '/agenda', label: 'Agenda', icon: 'CalendarIcon' },
];
const NOOP = () => undefined;

// Una de las tres secciones de la vista (atoms, molecules, organisms).
function Section({ title, children }: { title: string; children: ReactNode }) {
  return (
    <ModulView gap16>
      <ModulText variant="headerTitle">{title}</ModulText>
      {children}
    </ModulView>
  );
}

// Muestra de un componente con sus variantes y estados.
function Sample({ name, children }: { name: string; children: ReactNode }) {
  return (
    <ModulView gap8 p16 borderSubtle roundedLg bgSurface>
      <ModulText variant="sectionTitle">{name}</ModulText>
      {children}
    </ModulView>
  );
}

function Row({ children }: { children: ReactNode }) {
  return (
    <ModulView row gap8 style={{ alignItems: 'center', flexWrap: 'wrap' }}>
      {children}
    </ModulView>
  );
}

function Atoms() {
  const [modal, setModal] = useState(false);
  const [drawer, setDrawer] = useState(false);
  const [date, setDate] = useState('2026-11-12');
  const [time, setTime] = useState('09:30');
  const [option, setOption] = useState(true);

  return (
    <Section title="Atoms">
      <Sample name="ModulText">
        <ModulText variant="screenTitle">screenTitle</ModulText>
        <ModulText variant="sectionTitle">sectionTitle</ModulText>
        <ModulText variant="headerTitle">headerTitle</ModulText>
        <ModulText variant="body">body</ModulText>
        <ModulText variant="bodySecondary" textSecondary>
          bodySecondary
        </ModulText>
        <ModulText variant="label">label</ModulText>
        <ModulText link>link</ModulText>
      </Sample>
      <Sample name="ModulButton">
        <Row>
          <ModulButton label="Primario" />
          <ModulButton variant="secondary" label="Secundario" />
          <ModulButton variant="danger" label="Peligro" />
          <ModulButton label="Con ícono" icon="CalendarIcon" />
          <ModulButton compact label="Compacto" />
          <ModulButton label="Deshabilitado" disabled />
        </Row>
      </Sample>
      <Sample name="ModulFloatingButton y ModulCloseButton">
        <Row>
          <ModulFloatingButton icon="CalendarIcon" accessibilityLabel="Agregar" />
          <ModulCloseButton label="Cerrar" onPress={NOOP} />
        </Row>
      </Sample>
      <Sample name="ModulIcon">
        <Row>
          {(['sm', 'md', 'lg', 'xl'] as const).map((size) => (
            <ModulIcon key={size} name="CalendarIcon" size={size} />
          ))}
          <ModulIcon name="CalendarIcon" color="primary" />
          <ModulIcon name="CalendarIcon" color="textSecondary" />
        </Row>
      </Sample>
      <Sample name="ModulBadge">
        <Row>
          {BADGE_TONES.map((tone) => (
            <ModulBadge key={tone} tone={tone} label={tone} />
          ))}
          <ModulBadge label={12} />
        </Row>
      </Sample>
      <Sample name="ModulAvatar">
        <Row>
          {AVATAR_SIZES.map((size) => (
            <ModulAvatar key={size} name="Carlos Medina" size={size} />
          ))}
        </Row>
      </Sample>
      <Sample name="ModulChip">
        <Row>
          <ModulChip label="Normal" />
          <ModulChip label="Seleccionado" selected />
          <ModulChip label="Deshabilitado" disabled />
          <ModulChip label="Filtro activo" selected onRemove={NOOP} removeLabel="Quitar filtro" />
        </Row>
      </Sample>
      <Sample name="ModulProgress">
        <ModulProgress value={0} accessibilityLabel="Sin avance" />
        <ModulProgress value={60} accessibilityLabel="Avance 60%" />
        <ModulProgress value={100} accessibilityLabel="Completo" />
      </Sample>
      <Sample name="ModulFieldMessage">
        <ModulFieldMessage help="Texto de ayuda del campo" />
        <ModulFieldMessage error="Mensaje de error del campo" />
      </Sample>
      <Sample name="ModulOptionCard">
        <Row>
          <ModulOptionCard label="Seleccionada" selected={option} onPress={() => setOption(true)}>
            <ModulText variant="body">Opción 1</ModulText>
          </ModulOptionCard>
          <ModulOptionCard
            label="Sin seleccionar"
            selected={!option}
            onPress={() => setOption(false)}
          >
            <ModulText variant="body">Opción 2</ModulText>
          </ModulOptionCard>
        </Row>
      </Sample>
      <Sample name="ModulDatePicker">
        <Row>
          <ModulDatePicker mode="date" value={date} onChange={setDate} accessibilityLabel="Fecha" />
          <ModulText variant="body">{date}</ModulText>
          <ModulDatePicker mode="time" value={time} onChange={setTime} accessibilityLabel="Hora" />
          <ModulText variant="body">{time}</ModulText>
          <ModulDatePicker
            mode="date"
            value={date}
            onChange={setDate}
            accessibilityLabel="Fecha deshabilitada"
            disabled
          />
        </Row>
      </Sample>
      <Sample name="ModulModal y ModulDrawer">
        <Row>
          <ModulButton compact label="Abrir modal" onPress={() => setModal(true)} />
          <ModulButton
            compact
            variant="secondary"
            label="Abrir drawer"
            onPress={() => setDrawer(true)}
          />
        </Row>
        <ModulModal visible={modal} onClose={() => setModal(false)} closeLabel="Cerrar">
          <ModulText variant="body">Contenido del modal.</ModulText>
        </ModulModal>
        <ModulDrawer visible={drawer} onClose={() => setDrawer(false)} closeLabel="Cerrar">
          <ModulText variant="body">Contenido del drawer.</ModulText>
        </ModulDrawer>
      </Sample>
    </Section>
  );
}

function Molecules() {
  const [text, setText] = useState('');
  const [select, setSelect] = useState('a');
  const [checked, setChecked] = useState(true);
  const [radio, setRadio] = useState(false);
  const [on, setOn] = useState(true);
  const [chip, setChip] = useState('all');
  const [group, setGroup] = useState('a');
  const [menu, setMenu] = useState(false);
  const [confirm, setConfirm] = useState(false);
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [zone, setZone] = useState<string | null>(null);
  const [path, setPath] = useState('/');
  const { toast } = useMessages();

  return (
    <Section title="Molecules">
      <Sample name="ModulInput">
        <ModulInput label="Texto" value={text} onChangeText={setText} help="Texto de ayuda" />
        <ModulInput label="Con error" value="" onChangeText={NOOP} error="Campo obligatorio" />
        <ModulInput label="Deshabilitado" value="Valor" onChangeText={NOOP} disabled />
        <ModulInput
          label="Contraseña"
          type="password"
          value="secreto"
          onChangeText={NOOP}
          actionLabel="Mostrar"
        />
        <ModulInput
          label="Búsqueda"
          type="search"
          value=""
          onChangeText={NOOP}
          actionLabel="Borrar"
        />
        <ModulInput label="Notas" type="textarea" value="" onChangeText={NOOP} />
        <ModulInput label="Fecha" type="date" value="" onChangeText={NOOP} />
        <ModulInput
          label="Selector"
          type="select"
          value={select}
          onChangeText={setSelect}
          options={SELECT}
          actionLabel="Cerrar"
        />
        <ModulInput compact label="Compacto" value="" onChangeText={NOOP} />
      </Sample>
      <Sample name="ModulCheckbox, ModulSwitch y ModulOptionGroup">
        <ModulCheckbox label="Casilla" checked={checked} onChange={setChecked} />
        <ModulCheckbox label="Con error" checked={false} onChange={NOOP} error="Obligatorio" />
        <ModulCheckbox label="Deshabilitada" checked disabled onChange={NOOP} />
        <ModulCheckbox type="radio" label="Radio" checked={radio} onChange={setRadio} />
        <ModulSwitch label="Interruptor" checked={on} onChange={setOn} help="Texto de ayuda" />
        <ModulSwitch label="Deshabilitado" checked={false} disabled onChange={NOOP} />
        <ModulOptionGroup
          label="Grupo de opciones"
          options={SELECT}
          value={group}
          onChange={setGroup}
        />
      </Sample>
      <Sample name="ModulChipGroup">
        <ModulChipGroup value={chip} onChange={setChip} options={CHIPS} />
        <ModulChipGroup segmented value={chip} onChange={setChip} options={CHIPS} />
      </Sample>
      <Sample name="ModulListItem">
        <ModulListItem title="Elemento simple" />
        <ModulListItem
          title="Con descripción"
          description="Detalle del elemento"
          icon="CalendarIcon"
          onPress={NOOP}
        />
        <ModulListItem
          title="Con acciones"
          icon="CalendarIcon"
          actions={[{ icon: 'SettingsIcon', label: 'Ajustes', onPress: NOOP }]}
        />
        <ModulListItem title="Seleccionado" selected />
        <ModulListItem title="Destructivo" destructive />
        <ModulListItem title="Compacto" compact />
        <ModulListItem title="Deshabilitado" disabled />
      </Sample>
      <Sample name="ModulList">
        <ModulView borderSubtle roundedLg style={{ height: 130 }}>
          <ModulList
            data={['Uno', 'Dos']}
            keyExtractor={(item) => item}
            renderItem={({ item }) => <ModulListItem title={item} />}
            emptyTitle="Sin elementos"
          />
        </ModulView>
        <ModulView borderSubtle roundedLg style={{ height: 130 }}>
          <ModulList data={[]} loading loadingMessage="Cargando…" emptyTitle="Sin elementos" />
        </ModulView>
        <ModulView borderSubtle roundedLg style={{ height: 130 }}>
          <ModulList
            data={[]}
            error="No se pudo cargar"
            retryLabel="Reintentar"
            onRetry={NOOP}
            emptyTitle="Sin elementos"
          />
        </ModulView>
        <ModulView borderSubtle roundedLg style={{ height: 130 }}>
          <ModulList data={[]} emptyTitle="Sin elementos" emptyDescription="Agrega el primero" />
        </ModulView>
      </Sample>
      <Sample name="ModulEmptyState, ModulErrorState y ModulLoadingState">
        <ModulView borderSubtle roundedLg>
          <ModulEmptyState
            title="Sin tareas"
            description="Aún no hay tareas"
            actionLabel="Nueva tarea"
            onAction={NOOP}
          />
        </ModulView>
        <ModulView borderSubtle roundedLg>
          <ModulErrorState
            title="Algo salió mal"
            description="No se pudo cargar"
            retryLabel="Reintentar"
            onRetry={NOOP}
          />
        </ModulView>
        <ModulView borderSubtle roundedLg>
          <ModulLoadingState message="Cargando…" />
        </ModulView>
      </Sample>
      <Sample name="ModulToast">
        {TOAST_TYPES.map((type) => (
          <ModulToast
            key={type}
            type={type}
            message={`Aviso de tipo ${type}`}
            closeLabel="Cerrar"
            onClose={NOOP}
          />
        ))}
        <Row>
          <ModulButton
            compact
            label="Mostrar aviso (useMessages)"
            onPress={() => toast('Guardado', { type: 'success' })}
          />
        </Row>
      </Sample>
      <Sample name="ModulMenu y ModulConfirmDialog">
        <Row>
          <ModulButton compact label="Abrir menú" onPress={() => setMenu(true)} />
          <ModulButton
            compact
            variant="danger"
            label="Confirmar"
            onPress={() => setConfirm(true)}
          />
        </Row>
        <ModulMenu
          visible={menu}
          onClose={() => setMenu(false)}
          closeLabel="Cerrar"
          title="Acciones"
          items={[
            { key: 'edit', label: 'Editar', icon: 'SettingsIcon', onPress: () => setMenu(false) },
            { key: 'sel', label: 'Seleccionada', selected: true, onPress: () => setMenu(false) },
            { key: 'del', label: 'Eliminar', destructive: true, onPress: () => setMenu(false) },
            { key: 'off', label: 'Deshabilitada', disabled: true, onPress: NOOP },
          ]}
        />
        <ModulConfirmDialog
          visible={confirm}
          title="¿Eliminar tarea?"
          message="Esta acción no se puede deshacer."
          confirmLabel="Eliminar"
          cancelLabel="Cancelar"
          destructive
          onConfirm={() => setConfirm(false)}
          onCancel={() => setConfirm(false)}
        />
      </Sample>
      <Sample name="ModulBarChart y ModulCircularProgress">
        <ModulBarChart data={CHART} accessibilityLabel="Tareas por día" />
        <Row>
          <ModulCircularProgress value={25} label="25%" />
          <ModulCircularProgress value={70} label="70%" color="success" size={80} />
        </Row>
      </Sample>
      <Sample name="ModulDragDrop">
        <ModulDragProvider onDrop={(item, target) => setZone(`${item} → ${target}`)}>
          <Row>
            <ModulDraggable itemKey="tarea" label="Mover tarea">
              <ModulBadge label="Tarea" />
            </ModulDraggable>
            <ModulDropZone zoneKey="hechas" label="Zona hechas">
              <ModulText variant="body">Soltar aquí</ModulText>
            </ModulDropZone>
          </Row>
        </ModulDragProvider>
        <ModulText variant="bodySecondary" textSecondary>
          {zone ?? 'Arrastra la tarea a la zona'}
        </ModulText>
      </Sample>
      <Sample name="ModulHeader">
        <ModulView borderSubtle roundedLg overflowHidden>
          <ModulHeader
            sidebarOpen={sidebarOpen}
            onToggleSidebar={() => setSidebarOpen((open) => !open)}
          >
            <ModulText variant="body" fwBold>
              Encabezado
            </ModulText>
          </ModulHeader>
        </ModulView>
      </Sample>
      <Sample name="ModulSidebar">
        <ModulView borderSubtle roundedLg bgSurface style={{ width: 260, height: 300 }}>
          <ModulSidebar
            sections={SIDEBAR}
            activePath={path}
            onNavigate={setPath}
            user={{ name: 'Carlos Medina', path: '/profile', caption: 'Perfil' }}
            footerSections={[{ path: '/settings', label: 'Ajustes', icon: 'SettingsIcon' }]}
          />
        </ModulView>
      </Sample>
    </Section>
  );
}

function Organisms() {
  const [dialog, setDialog] = useState(false);
  const [sheet, setSheet] = useState(false);
  const [query, setQuery] = useState('');
  const [filter, setFilter] = useState('all');
  const tasks = ['Comprar pan', 'Pagar la luz', 'Llamar al médico'].filter((task) =>
    task.toLowerCase().includes(query.toLowerCase()),
  );

  return (
    <Section title="Organisms">
      <Sample name="ModulDialog y ModulBottomSheet">
        <Row>
          <ModulButton compact label="Abrir diálogo" onPress={() => setDialog(true)} />
          <ModulButton
            compact
            variant="secondary"
            label="Abrir hoja inferior"
            onPress={() => setSheet(true)}
          />
        </Row>
        <ModulDialog
          visible={dialog}
          title="Nueva tarea"
          closeLabel="Cerrar"
          onClose={() => setDialog(false)}
        >
          <ModulText variant="body">Contenido del diálogo.</ModulText>
        </ModulDialog>
        <ModulDialog
          visible={sheet}
          title="Filtros"
          closeLabel="Cerrar"
          placement="bottom"
          onClose={() => setSheet(false)}
        >
          <ModulText variant="body">Contenido de la hoja inferior.</ModulText>
        </ModulDialog>
      </Sample>
      <Sample name="ModulListPattern">
        <ModulView borderSubtle roundedLg style={{ height: 320 }}>
          <ModulListPattern
            searchLabel="Buscar"
            searchValue={query}
            onSearchChange={setQuery}
            filters={CHIPS}
            filterValue={filter}
            onFilterChange={setFilter}
            data={tasks}
            keyExtractor={(item) => item}
            renderItem={({ item }) => <ModulListItem title={item} />}
            emptyTitle="Sin tareas"
          />
        </ModulView>
      </Sample>
      <Sample name="ModulFormContainer">
        <ModulFormContainer
          submitLabel="Guardar"
          cancelLabel="Cancelar"
          onSubmit={NOOP}
          onCancel={NOOP}
          serverErrors={['El correo ya está registrado']}
        >
          <ModulInput label="Título" value="" onChangeText={NOOP} />
          <ModulInput label="Correo" value="" onChangeText={NOOP} error="Correo inválido" />
        </ModulFormContainer>
        <ModulFormContainer submitLabel="Guardando…" submitting onSubmit={NOOP}>
          <ModulInput label="Título" value="" onChangeText={NOOP} />
        </ModulFormContainer>
      </Sample>
    </Section>
  );
}

// Vista "Componentes" del catálogo: los componentes reales agrupados en atoms, molecules y organisms.
export default function Components() {
  return (
    <ModulView gap16>
      <Atoms />
      <Molecules />
      <Organisms />
    </ModulView>
  );
}

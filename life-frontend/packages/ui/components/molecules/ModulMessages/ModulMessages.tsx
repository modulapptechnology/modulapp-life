import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { ModulConfirmDialog } from '../ModulConfirmDialog/ModulConfirmDialog';
import { ModulToast, type ModulToastType } from '../ModulToast/ModulToast';

export type ToastOptions = {
  type?: ModulToastType;
  duration?: number;
};

export type ConfirmOptions = {
  title: string;
  message?: string;
  confirmLabel?: string;
  cancelLabel?: string;
  destructive?: boolean;
};

type MessagesApi = {
  toast: (message: string, options?: ToastOptions) => void;
  confirm: (options: ConfirmOptions) => Promise<boolean>;
};

type ToastEntry = { id: number; message: string; type: ModulToastType; visible: boolean };
type PendingConfirm = { options: ConfirmOptions; resolve: (accepted: boolean) => void };

const MessagesContext = createContext<MessagesApi | null>(null);

const DEFAULT_DURATION = 4000;
const MAX_TOASTS = 3;

export type ModulMessagesProviderProps = {
  // Textos por defecto de los botones; cada llamada puede reemplazarlos.
  confirmLabel: string;
  cancelLabel: string;
  closeLabel: string;
  children: ReactNode;
};

// Se monta una vez por app, dentro del ThemeProvider.
export function ModulMessagesProvider({
  confirmLabel,
  cancelLabel,
  closeLabel,
  children,
}: ModulMessagesProviderProps) {
  const [toasts, setToasts] = useState<ToastEntry[]>([]);
  const [pending, setPending] = useState<PendingConfirm | null>(null);
  const nextId = useRef(0);
  const timers = useRef(new Set<ReturnType<typeof setTimeout>>());

  useEffect(() => {
    const active = timers.current;
    return () => active.forEach(clearTimeout);
  }, []);

  const remove = useCallback((id: number) => {
    setToasts((current) => current.filter((entry) => entry.id !== id));
  }, []);

  const dismiss = useCallback((id: number) => {
    setToasts((current) =>
      current.map((entry) => (entry.id === id ? { ...entry, visible: false } : entry)),
    );
  }, []);

  const toast = useCallback(
    (message: string, { type = 'info', duration = DEFAULT_DURATION }: ToastOptions = {}) => {
      const id = nextId.current++;
      setToasts((current) => [...current, { id, message, type, visible: true }].slice(-MAX_TOASTS));
      const timer = setTimeout(() => {
        timers.current.delete(timer);
        dismiss(id);
      }, duration);
      timers.current.add(timer);
    },
    [dismiss],
  );

  const confirm = useCallback(
    (options: ConfirmOptions) =>
      new Promise<boolean>((resolve) => setPending({ options, resolve })),
    [],
  );

  const answer = (accepted: boolean) => {
    pending?.resolve(accepted);
    setPending(null);
  };

  const api = useMemo(() => ({ toast, confirm }), [toast, confirm]);

  return (
    <MessagesContext.Provider value={api}>
      {children}
      <ModulView absolute bottom16 w100 px16 gap8 style={{ pointerEvents: 'box-none' }}>
        {toasts.map((entry) => (
          <ModulToast
            key={entry.id}
            message={entry.message}
            type={entry.type}
            visible={entry.visible}
            closeLabel={closeLabel}
            onClose={() => dismiss(entry.id)}
            onExited={() => remove(entry.id)}
          />
        ))}
      </ModulView>
      <ModulConfirmDialog
        visible={pending !== null}
        title={pending?.options.title ?? ''}
        message={pending?.options.message}
        confirmLabel={pending?.options.confirmLabel ?? confirmLabel}
        cancelLabel={pending?.options.cancelLabel ?? cancelLabel}
        destructive={pending?.options.destructive}
        onConfirm={() => answer(true)}
        onCancel={() => answer(false)}
      />
    </MessagesContext.Provider>
  );
}

// toast('Guardado', { type: 'success' }) y await confirm({ title: '¿Eliminar?', destructive: true }) desde cualquier pantalla.
export function useMessages() {
  const context = useContext(MessagesContext);
  if (!context) throw new Error('useMessages debe usarse dentro de <ModulMessagesProvider>');
  return context;
}

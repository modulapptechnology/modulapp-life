import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ComponentRef,
  type ReactNode,
  type RefObject,
} from 'react';
import { Animated, PanResponder, Pressable, type View } from 'react-native';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulView } from '../../atoms/ModulView/ModulView';

type ViewHandle = ComponentRef<typeof View>;

type DragContextValue = {
  draggingKey: string | null;
  pickedKey: string | null;
  setPicked: (key: string | null) => void;
  setDragging: (key: string | null) => void;
  registerZone: (key: string, ref: RefObject<ViewHandle | null>) => () => void;
  zoneAt: (x: number, y: number) => Promise<string | null>;
  drop: (itemKey: string, zoneKey: string) => void;
};

const DragContext = createContext<DragContextValue | null>(null);

function useDrag() {
  const context = useContext(DragContext);
  if (!context)
    throw new Error('ModulDraggable y ModulDropZone deben estar dentro de <ModulDragProvider>');
  return context;
}

export type ModulDragProviderProps = {
  onDrop: (itemKey: string, zoneKey: string) => void;
  children: ReactNode;
};

// Conecta los elementos arrastrables con las zonas de destino y avisa con onDrop(elemento, zona).
export function ModulDragProvider({ onDrop, children }: ModulDragProviderProps) {
  const [draggingKey, setDragging] = useState<string | null>(null);
  const [pickedKey, setPicked] = useState<string | null>(null);
  const zones = useRef(new Map<string, RefObject<ViewHandle | null>>());

  const value = useMemo<DragContextValue>(
    () => ({
      draggingKey,
      pickedKey,
      setPicked,
      setDragging,
      registerZone: (key, ref) => {
        zones.current.set(key, ref);
        return () => {
          zones.current.delete(key);
        };
      },
      zoneAt: (x, y) =>
        new Promise((resolve) => {
          const entries = [...zones.current.entries()];
          let pending = entries.length;
          let found: string | null = null;
          if (pending === 0) resolve(null);

          const finish = () => {
            pending -= 1;
            if (pending === 0) resolve(found);
          };

          entries.forEach(([key, ref]) => {
            if (!ref.current) return finish();
            ref.current.measureInWindow((zx, zy, width, height) => {
              if (x >= zx && x <= zx + width && y >= zy && y <= zy + height) found = key;
              finish();
            });
          });
        }),
      drop: (itemKey, zoneKey) => {
        setPicked(null);
        onDrop(itemKey, zoneKey);
      },
    }),
    [draggingKey, pickedKey, onDrop],
  );

  return <DragContext.Provider value={value}>{children}</DragContext.Provider>;
}

export type ModulDraggableProps = {
  itemKey: string;
  label: string;
  disabled?: boolean;
  children: ReactNode;
};

// Se mueve con ratón o dedo; con teclado o lector de pantalla se activa (Enter/Espacio) y luego se activa la zona de destino.
export function ModulDraggable({
  itemKey,
  label,
  disabled = false,
  children,
}: ModulDraggableProps) {
  const drag = useDrag();
  const dragRef = useRef(drag);
  dragRef.current = drag;
  const pan = useRef(new Animated.ValueXY()).current;
  const picked = drag.pickedKey === itemKey;

  const panResponder = useMemo(() => {
    const reset = () => {
      dragRef.current.setDragging(null);
      Animated.spring(pan, { toValue: { x: 0, y: 0 }, useNativeDriver: false }).start();
    };

    return PanResponder.create({
      onMoveShouldSetPanResponder: (_, gesture) =>
        Math.abs(gesture.dx) > 4 || Math.abs(gesture.dy) > 4,
      onPanResponderGrant: () => dragRef.current.setDragging(itemKey),
      onPanResponderMove: (_, gesture) => pan.setValue({ x: gesture.dx, y: gesture.dy }),
      onPanResponderRelease: async (_, gesture) => {
        const zone = await dragRef.current.zoneAt(gesture.moveX, gesture.moveY);
        if (zone) dragRef.current.drop(itemKey, zone);
        reset();
      },
      onPanResponderTerminate: reset,
    });
  }, [itemKey, pan]);

  return (
    <Animated.View
      {...(disabled ? {} : panResponder.panHandlers)}
      style={{
        transform: pan.getTranslateTransform(),
        zIndex: drag.draggingKey === itemKey ? 10 : 0,
      }}
    >
      <Pressable
        accessibilityRole="button"
        accessibilityLabel={label}
        accessibilityState={{ selected: picked, disabled }}
        disabled={disabled}
        onPress={() => drag.setPicked(picked ? null : itemKey)}
      >
        <ModulView roundedLg borderFocus={picked}>
          {children}
        </ModulView>
      </Pressable>
    </Animated.View>
  );
}

export type ModulDropZoneProps = {
  zoneKey: string;
  label: string;
  children?: ReactNode;
};

// Zona de destino: se resalta mientras se arrastra y, con un elemento activado, se vuelve un botón para soltarlo.
export function ModulDropZone({ zoneKey, label, children }: ModulDropZoneProps) {
  const { view } = useStyles();
  const drag = useDrag();
  const ref = useRef<ViewHandle>(null);
  const { registerZone } = drag;

  useEffect(() => registerZone(zoneKey, ref), [registerZone, zoneKey]);

  const waiting = drag.draggingKey !== null || drag.pickedKey !== null;

  return (
    <Pressable
      ref={ref}
      accessibilityRole="button"
      accessibilityLabel={label}
      disabled={drag.pickedKey === null}
      onPress={() => drag.pickedKey && drag.drop(drag.pickedKey, zoneKey)}
      style={[
        view.minH44,
        view.p16,
        view.roundedLg,
        waiting ? view.borderPrimary : view.borderSubtle,
      ]}
    >
      {children}
    </Pressable>
  );
}

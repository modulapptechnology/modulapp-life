import { ModulView } from '../../atoms/ModulView/ModulView';
import {
  ModulChipGroup,
  type ModulChipOption,
} from '../../molecules/ModulChipGroup/ModulChipGroup';
import { ModulInput } from '../../molecules/ModulInput/ModulInput';
import { ModulList, type ModulListProps } from '../../molecules/ModulList/ModulList';

export type ModulListPatternProps<T> = ModulListProps<T> & {
  searchLabel: string;
  searchValue: string;
  onSearchChange: (value: string) => void;
  filters?: ModulChipOption[];
  filterValue?: string;
  onFilterChange?: (value: string) => void;
  // 'stacked': búsqueda y filtros uno debajo del otro (móvil). 'inline': en una sola fila y con controles
  // compactos (web).
  layout?: 'stacked' | 'inline';
};

// Patrón de lista: búsqueda, filtros opcionales y la lista con sus estados de carga, error y vacío.
export function ModulListPattern<T>({
  searchLabel,
  searchValue,
  onSearchChange,
  filters,
  filterValue = '',
  onFilterChange,
  layout = 'stacked',
  ...listProps
}: ModulListPatternProps<T>) {
  const inline = layout === 'inline';

  const search = (
    <ModulInput
      compact={inline}
      type="search"
      label={searchLabel}
      value={searchValue}
      onChangeText={onSearchChange}
    />
  );
  const chips =
    filters?.length && onFilterChange ? (
      <ModulChipGroup
        segmented={inline}
        options={filters}
        value={filterValue}
        onChange={onFilterChange}
      />
    ) : null;

  return (
    <ModulView flex1 gap8>
      {inline ? (
        <ModulView row alignCenter gap16 p16>
          <ModulView w100 style={{ maxWidth: 320 }}>
            {search}
          </ModulView>
          {chips}
        </ModulView>
      ) : (
        <>
          <ModulView px16 py8>
            {search}
          </ModulView>
          {/* flexShrink 0: el selector es un ScrollView horizontal y, sin esto, la columna lo encoge y recorta los chips. */}
          {chips ? <ModulView style={{ flexShrink: 0, flexGrow: 0 }}>{chips}</ModulView> : null}
        </>
      )}
      <ModulList {...listProps} />
    </ModulView>
  );
}

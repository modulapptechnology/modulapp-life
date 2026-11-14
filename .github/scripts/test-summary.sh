#!/usr/bin/env bash
# Escribe en el resumen del job una tabla con los resultados de los archivos .trx de una carpeta.
# Uso: test-summary.sh "<título>" <carpeta con .trx>
set -u

title="$1"
dir="$2"

counter() {
  echo "$1" | sed -n "s/.* $2=\"\([0-9]*\)\".*/\1/p"
}

{
  echo "### $title"
  echo
  echo "| Proyecto | Total | Pasaron | Fallaron | Omitidas |"
  echo "|---|---|---|---|---|"
  found=0
  for file in "$dir"/*.trx; do
    [ -e "$file" ] || continue
    found=1
    counters=$(grep -o '<Counters [^>]*>' "$file" | head -1)
    total=$(counter "$counters" total)
    passed=$(counter "$counters" passed)
    failed=$(counter "$counters" failed)
    skipped=$(counter "$counters" notExecuted)
    echo "| $(basename "$file" .trx) | ${total:-0} | ${passed:-0} | ${failed:-0} | ${skipped:-0} |"
  done
  if [ "$found" -eq 0 ]; then
    echo "| (sin resultados: la compilación o la ejecución falló antes de probar) | | | | |"
  fi
} >> "${GITHUB_STEP_SUMMARY:-/dev/stdout}"

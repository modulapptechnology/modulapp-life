// Deja solo los dígitos y los separa con el patrón dado (cada # es un dígito).
function applyMask(text: string, mask: string) {
  const digits = text.replace(/\D/g, '');
  let result = '';
  let index = 0;

  for (const char of mask) {
    if (index >= digits.length) break;
    if (char === '#') {
      result += digits[index];
      index += 1;
    } else {
      result += char;
    }
  }

  return result;
}

export const DATE_MASK = '##/##/####';
export const TIME_MASK = '##:##';

// Fecha escrita a mano: 25122026 -> 25/12/2026
export const formatDateInput = (text: string) => applyMask(text, DATE_MASK);

// Hora escrita a mano: 0930 -> 09:30
export const formatTimeInput = (text: string) => applyMask(text, TIME_MASK);

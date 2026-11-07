import type { ThemeColors } from '../colors/colors';

const arrow = (color: string, points: string) =>
  `url("data:image/svg+xml,${encodeURIComponent(
    `<svg xmlns='http://www.w3.org/2000/svg' width='10' height='10'><path d='${points}' fill='${color}'/></svg>`,
  )}")`;

export const createScrollbarCss = (colors: ThemeColors) => `
  /* Chrome, Edge, Safari */
  *::-webkit-scrollbar { width: 12px; height: 10px; }
  *::-webkit-scrollbar-track { background: ${colors.border}; }
  *::-webkit-scrollbar-corner { background: ${colors.border}; }
  *::-webkit-scrollbar-thumb {
    background-color: ${colors.textSecondary};
    border: 3px solid ${colors.border};
    border-radius: 999px;
  }
  *::-webkit-scrollbar-thumb:hover { background-color: ${colors.text}; }
  *::-webkit-scrollbar-thumb:active { background-color: ${colors.primary}; }

  *::-webkit-scrollbar-button:single-button {
    display: block;
    background-color: ${colors.border};
    background-repeat: no-repeat;
    background-position: center;
    width: 14px;
    height: 14px;
  }
  *::-webkit-scrollbar-button:single-button:vertical:decrement {
    background-image: ${arrow(colors.textSecondary, 'M5 2 L9 8 L1 8 Z')};
  }
  *::-webkit-scrollbar-button:single-button:vertical:increment {
    background-image: ${arrow(colors.textSecondary, 'M5 8 L9 2 L1 2 Z')};
  }
  *::-webkit-scrollbar-button:single-button:horizontal:decrement {
    background-image: ${arrow(colors.textSecondary, 'M2 5 L8 1 L8 9 Z')};
  }
  *::-webkit-scrollbar-button:single-button:horizontal:increment {
    background-image: ${arrow(colors.textSecondary, 'M8 5 L2 1 L2 9 Z')};
  }

  /* Firefox (no admite flechas) */
  @supports not selector(::-webkit-scrollbar) {
    * { scrollbar-width: auto; scrollbar-color: ${colors.textSecondary} ${colors.border}; }
  }
`;

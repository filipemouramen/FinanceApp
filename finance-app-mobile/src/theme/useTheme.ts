import { useThemeContext } from '../contexts/ThemeContext';
import { LightColors, DarkColors } from './colors';

export function useTheme() {
  const { theme, isDark, toggleTheme } = useThemeContext();
  const colors = isDark ? DarkColors : LightColors;
  return { colors, isDark, toggleTheme, theme };
}

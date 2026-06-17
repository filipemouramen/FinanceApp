import React, { createContext, useContext, useState, useEffect } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';

type Theme = 'light' | 'dark';

interface ThemeContextData {
  theme: Theme;
  isDark: boolean;
  toggleTheme: () => void;
}

const ThemeContext = createContext<ThemeContextData>({
  theme: 'light',
  isDark: false,
  toggleTheme: () => {},
});

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [theme, setTheme] = useState<Theme>('light');

  useEffect(() => {
    AsyncStorage.getItem('app_theme').then((saved) => {
      if (saved === 'dark' || saved === 'light') setTheme(saved);
    });
  }, []);

  function toggleTheme() {
    setTheme((prev) => {
      const next = prev === 'light' ? 'dark' : 'light';
      AsyncStorage.setItem('app_theme', next);
      return next;
    });
  }

  return (
    <ThemeContext.Provider value={{ theme, isDark: theme === 'dark', toggleTheme }}>
      {children}
    </ThemeContext.Provider>
  );
}

export function useThemeContext() {
  return useContext(ThemeContext);
}

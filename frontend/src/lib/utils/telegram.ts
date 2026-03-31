declare global {
  interface Window {
    Telegram?: {
      WebApp?: {
        initData: string;
        initDataUnsafe: {
          user?: {
            id: number;
            first_name: string;
            last_name?: string;
            username?: string;
            photo_url?: string;
            language_code?: string;
          };
          auth_date: number;
          hash: string;
        };
        ready(): void;
        expand(): void;
        close(): void;
        MainButton: {
          text: string;
          show(): void;
          hide(): void;
          onClick(cb: () => void): void;
        };
        BackButton: {
          show(): void;
          hide(): void;
          onClick(cb: () => void): void;
        };
        colorScheme: 'light' | 'dark';
        themeParams: Record<string, string>;
      };
    };
  }
}

export function getTelegramWebApp() {
  if (typeof window === 'undefined') return null;
  return window.Telegram?.WebApp ?? null;
}

export function getTelegramInitData(): string | null {
  const twa = getTelegramWebApp();
  if (twa?.initData) return twa.initData;
  // Fallback: read from URL hash (when opened in browser for testing)
  if (typeof window !== 'undefined') {
    const params = new URLSearchParams(window.location.hash.slice(1));
    const initData = params.get('tgWebAppData');
    if (initData) return decodeURIComponent(initData);
  }
  return null;
}

export function isTelegramWebApp(): boolean {
  return !!getTelegramWebApp()?.initData;
}

export function getTelegramUser() {
  return getTelegramWebApp()?.initDataUnsafe?.user ?? null;
}

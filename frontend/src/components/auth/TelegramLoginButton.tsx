'use client';
import { useEffect, useRef } from 'react';

interface TelegramLoginButtonProps {
  botName: string;
  onAuth: (initData: string) => void;
  buttonSize?: 'large' | 'medium' | 'small';
  cornerRadius?: number;
  requestAccess?: boolean;
}

declare global {
  interface Window {
    TelegramLoginWidget: { dataOnauth: (user: Record<string, unknown>) => void };
  }
}

/**
 * Renders the official Telegram Login Widget.
 * When the user authorises, we pass the raw query string (initData) to onAuth.
 */
export function TelegramLoginButton({
  botName,
  onAuth,
  buttonSize = 'large',
  cornerRadius = 8,
  requestAccess = true,
}: TelegramLoginButtonProps) {
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!containerRef.current) return;

    window.TelegramLoginWidget = {
      dataOnauth: (user) => {
        // Convert user object back to query-string format expected by backend
        const params = new URLSearchParams();
        Object.entries(user).forEach(([k, v]) => params.set(k, String(v)));
        onAuth(params.toString());
      },
    };

    const script = document.createElement('script');
    script.src = 'https://telegram.org/js/telegram-widget.js?22';
    script.setAttribute('data-telegram-login', botName);
    script.setAttribute('data-size', buttonSize);
    script.setAttribute('data-radius', String(cornerRadius));
    script.setAttribute('data-onauth', 'TelegramLoginWidget.dataOnauth(user)');
    if (requestAccess) script.setAttribute('data-request-access', 'write');
    script.async = true;

    containerRef.current.innerHTML = '';
    containerRef.current.appendChild(script);
  }, [botName, buttonSize, cornerRadius, requestAccess, onAuth]);

  return <div ref={containerRef} className="flex justify-center" />;
}

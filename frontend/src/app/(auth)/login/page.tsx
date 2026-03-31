'use client';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { GraduationCap, AlertCircle } from 'lucide-react';
import { TelegramLoginButton } from '@/components/auth/TelegramLoginButton';
import { useAuth } from '@/hooks/useAuth';

export default function LoginPage() {
  const { loginWithTelegram } = useAuth();
  const router  = useRouter();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const botName = process.env.NEXT_PUBLIC_TELEGRAM_BOT_NAME ?? 'GrammarAiBot';

  const handleTelegramAuth = async (initData: string) => {
    setLoading(true);
    setError(null);
    try {
      await loginWithTelegram(initData);
      router.push('/dashboard');
    } catch {
      setError('Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-gradient-to-br from-blue-50 via-white to-indigo-50 px-4">
      <div className="w-full max-w-sm">
        {/* Logo */}
        <div className="mb-8 flex flex-col items-center">
          <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-blue-600 shadow-lg shadow-blue-200">
            <GraduationCap className="h-9 w-9 text-white" />
          </div>
          <h1 className="mt-4 text-2xl font-bold text-gray-900">GrammarAI</h1>
          <p className="mt-1 text-sm text-gray-500">Practice English exercises from images</p>
        </div>

        {/* Card */}
        <div className="rounded-2xl border border-gray-200 bg-white p-8 shadow-sm">
          <h2 className="mb-2 text-center text-lg font-semibold text-gray-900">Sign in</h2>
          <p className="mb-6 text-center text-sm text-gray-500">
            Use your Telegram account to get started
          </p>

          {error && (
            <div className="mb-4 flex items-center gap-2 rounded-lg bg-red-50 p-3 text-sm text-red-700">
              <AlertCircle className="h-4 w-4 shrink-0" />
              {error}
            </div>
          )}

          {loading ? (
            <div className="flex justify-center py-2">
              <div className="h-6 w-6 animate-spin rounded-full border-2 border-blue-600 border-t-transparent" />
            </div>
          ) : (
            <TelegramLoginButton botName={botName} onAuth={handleTelegramAuth} buttonSize="large" />
          )}

          <p className="mt-6 text-center text-xs text-gray-400">
            By signing in you agree to our{' '}
            <a href="#" className="underline hover:text-gray-600">Terms of Service</a>
          </p>
        </div>
      </div>
    </div>
  );
}

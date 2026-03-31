'use client';
import { useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/lib/store/authStore';
import { authApi } from '@/lib/api/auth';
import toast from 'react-hot-toast';

export function useAuth() {
  const router = useRouter();
  const { user, isAuthenticated, setAuth, clearAuth } = useAuthStore();

  const loginWithTelegram = useCallback(
    async (initData: string) => {
      const result = await authApi.telegramLogin(initData);
      setAuth(result.user, result.accessToken, result.refreshToken);
      router.push('/dashboard');
    },
    [setAuth, router],
  );

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } catch {
      // proceed even if server call fails
    } finally {
      clearAuth();
      toast.success('Logged out');
      router.push('/login');
    }
  }, [clearAuth, router]);

  return { user, isAuthenticated, loginWithTelegram, logout };
}

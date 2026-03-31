import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { UserDto } from '../types/api';

interface AuthState {
  user: UserDto | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  setAuth: (user: UserDto, accessToken: string, refreshToken: string) => void;
  clearAuth: () => void;
  updateUser: (user: Partial<UserDto>) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,

      setAuth: (user, accessToken, refreshToken) => {
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', refreshToken);
        set({ user, accessToken, refreshToken, isAuthenticated: true });
      },

      clearAuth: () => {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false });
      },

      updateUser: (partial) => {
        const current = get().user;
        if (current) set({ user: { ...current, ...partial } });
      },
    }),
    {
      name: 'grammarai-auth',
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated,
      }),
    },
  ),
);

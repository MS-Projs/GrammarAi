import apiClient from './client';
import type { AuthResultDto } from '../types/api';

export const authApi = {
  telegramLogin: (initData: string) =>
    apiClient.post<AuthResultDto>('/auth/telegram', { initData }).then((r) => r.data),

  refresh: (refreshToken: string) =>
    apiClient.post<AuthResultDto>('/auth/refresh', { refreshToken }).then((r) => r.data),

  logout: () =>
    apiClient.post('/auth/logout').then((r) => r.data),
};

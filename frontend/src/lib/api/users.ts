import apiClient from './client';
import type { UserDto, UserStatsDto, StreakDto } from '../types/api';

export interface UpdateMePayload {
  displayName?: string;
  timezone?: string;
  languageCode?: string;
}

export const usersApi = {
  getMe: () => apiClient.get<UserDto>('/users/me').then((r) => r.data),
  updateMe: (payload: UpdateMePayload) =>
    apiClient.patch<UserDto>('/users/me', payload).then((r) => r.data),
  getStats: () => apiClient.get<UserStatsDto>('/users/me/stats').then((r) => r.data),
  getStreak: () => apiClient.get<StreakDto>('/users/me/streak').then((r) => r.data),
};

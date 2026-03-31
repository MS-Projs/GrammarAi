import apiClient from './client';
import type {
  ExerciseDto,
  ExerciseDetailDto,
  PaginatedResult,
  SolveAnswerItem,
  SolveResultDto,
  ImageUploadResultDto,
  ExerciseStatus,
} from '../types/api';

export interface ExercisesFilter {
  page?: number;
  limit?: number;
  status?: string;
  difficulty?: string;
  tags?: string;
  search?: string;
}

export interface CreateExercisePayload {
  title?: string;
  description?: string;
  exerciseType?: string;
  difficulty?: string;
  tags?: string[];
  isPublic?: boolean;
}

export interface UpdateExercisePayload {
  title?: string;
  description?: string;
  isPublic?: boolean;
  tags?: string[];
}

export const exercisesApi = {
  list: (params: ExercisesFilter = {}) =>
    apiClient.get<PaginatedResult<ExerciseDto>>('/exercises', { params }).then((r) => r.data),

  create: (payload: CreateExercisePayload) =>
    apiClient.post<{ id: string }>('/exercises', payload).then((r) => r.data),

  getById: (id: string) =>
    apiClient.get<ExerciseDetailDto>(`/exercises/${id}`).then((r) => r.data),

  update: (id: string, payload: UpdateExercisePayload) =>
    apiClient.patch(`/exercises/${id}`, payload).then((r) => r.data),

  delete: (id: string) =>
    apiClient.delete(`/exercises/${id}`).then((r) => r.data),

  getStatus: (id: string) =>
    apiClient
      .get<{ id: string; status: ExerciseStatus; errorMessage?: string }>(`/exercises/${id}/status`)
      .then((r) => r.data),

  retry: (id: string) =>
    apiClient.post<{ jobId: string }>(`/exercises/${id}/retry`).then((r) => r.data),

  uploadImages: (id: string, files: File[], onProgress?: (pct: number) => void) => {
    const form = new FormData();
    files.forEach((f) => form.append('files', f));
    return apiClient
      .post<{ images: ImageUploadResultDto[] }>(`/exercises/${id}/images`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
        onUploadProgress: (e) => {
          if (onProgress && e.total) onProgress(Math.round((e.loaded / e.total) * 100));
        },
      })
      .then((r) => r.data);
  },

  solve: (id: string, answers: SolveAnswerItem[]) =>
    apiClient.post<SolveResultDto>(`/exercises/${id}/solve`, { answers }).then((r) => r.data),

  getResults: (id: string) =>
    apiClient
      .get<{ score: number; maxScore: number; accuracyPercent: number; attemptedAt: string }>(
        `/exercises/${id}/results`,
      )
      .then((r) => r.data),

  listPublic: (params: ExercisesFilter = {}) =>
    apiClient.get<PaginatedResult<ExerciseDto>>('/public/exercises', { params }).then((r) => r.data),

  getPublicById: (id: string) =>
    apiClient.get<ExerciseDetailDto>(`/public/exercises/${id}`).then((r) => r.data),
};

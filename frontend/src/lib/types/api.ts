export interface PaginatedResult<T> {
  data: T[];
  page: number;
  limit: number;
  total: number;
  totalPages: number;
}

export interface ApiError {
  type: string;
  title: string;
  status: number;
  detail: string;
  traceId?: string;
}

export interface AuthResultDto {
  accessToken: string;
  refreshToken: string;
  user: UserDto;
}

export interface UserDto {
  id: string;
  displayName: string;
  avatarUrl?: string;
  telegramId?: number;
  webEmail?: string;
  isPremium: boolean;
  createdAt: string;
}

export interface UserStatsDto {
  totalExercises: number;
  totalQuestions: number;
  correctAnswers: number;
  accuracyPercent: number;
  currentStreak: number;
  longestStreak: number;
}

export interface StreakDto {
  currentStreak: number;
  longestStreak: number;
  lastActiveDate?: string;
}

export interface ExerciseDto {
  id: string;
  title?: string;
  description?: string;
  exerciseType: ExerciseType;
  difficulty?: DifficultyLevel;
  status: ExerciseStatus;
  isPublic: boolean;
  tags: string[];
  createdAt: string;
}

export interface ExerciseDetailDto extends ExerciseDto {
  questions: QuestionDto[];
}

export interface QuestionDto {
  id: string;
  orderIndex: number;
  body: string;
  explanation?: string;
  exerciseType: ExerciseType;
  maxScore: number;
  answers: AnswerDto[];
}

export interface AnswerDto {
  id: string;
  orderIndex: number;
  text: string;
  isCorrect: boolean;
  explanation?: string;
}

export interface SolveAnswerItem {
  questionId: string;
  answerId?: string;
  freeText?: string;
  timeSpentMs?: number;
}

export interface SolveResultDto {
  score: number;
  maxScore: number;
  accuracyPercent: number;
  breakdown: AnswerBreakdownDto[];
}

export interface AnswerBreakdownDto {
  questionId: string;
  isCorrect?: boolean;
  score: number;
  correctAnswerIds: string[];
  explanation?: string;
}

export interface ImageUploadResultDto {
  id: string;
  fileUrl: string;
  mimeType: string;
  sizeBytes: number;
}

export type ExerciseType = 'MultipleChoice' | 'FillBlank' | 'Reorder' | 'TrueFalse' | 'Essay';
export type DifficultyLevel = 'A1' | 'A2' | 'B1' | 'B2' | 'C1' | 'C2';
export type ExerciseStatus = 'Pending' | 'Processing' | 'Ready' | 'Failed';

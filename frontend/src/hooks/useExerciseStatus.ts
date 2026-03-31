'use client';
import { useState, useEffect, useRef, useCallback } from 'react';
import { exercisesApi } from '@/lib/api/exercises';
import type { ExerciseStatus } from '@/lib/types/api';

interface StatusState {
  status: ExerciseStatus | null;
  errorMessage?: string;
  isPolling: boolean;
}

export function useExerciseStatus(exerciseId: string, initialStatus?: ExerciseStatus) {
  const isActive = initialStatus === 'Pending' || initialStatus === 'Processing';
  const [state, setState] = useState<StatusState>({
    status: initialStatus ?? null,
    isPolling: isActive,
  });
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const stopPolling = useCallback(() => {
    if (intervalRef.current) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
    }
  }, []);

  useEffect(() => {
    if (!state.isPolling) return;

    const poll = async () => {
      try {
        const data = await exercisesApi.getStatus(exerciseId);
        const s = data.status;
        const stillActive = s === 'Pending' || s === 'Processing';
        setState({ status: s, errorMessage: data.errorMessage, isPolling: stillActive });
        if (!stillActive) stopPolling();
      } catch {
        // ignore transient network errors
      }
    };

    poll();
    intervalRef.current = setInterval(poll, 3000);
    return stopPolling;
  }, [exerciseId, state.isPolling, stopPolling]);

  return state;
}

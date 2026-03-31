'use client';
import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { GraduationCap, Eye, Loader2, AlertCircle } from 'lucide-react';
import { ExercisePlayer } from '@/components/exercises/player/ExercisePlayer';
import { Badge }          from '@/components/ui/Badge';
import { Button }         from '@/components/ui/Button';
import { exercisesApi }   from '@/lib/api/exercises';
import type { ExerciseDetailDto } from '@/lib/types/api';

export default function PublicExercisePage() {
  const { id } = useParams<{ id: string }>();
  const [exercise, setExercise] = useState<ExerciseDetailDto | null>(null);
  const [loading, setLoading]   = useState(true);
  const [error, setError]       = useState<string | null>(null);

  useEffect(() => {
    exercisesApi.getPublicById(id)
      .then((ex) => {
        if (ex.status !== 'Ready') {
          setError('This exercise is not available yet.');
          return;
        }
        setExercise(ex);
      })
      .catch(() => setError('Exercise not found or not public.'))
      .finally(() => setLoading(false));
  }, [id]);

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Top bar */}
      <header className="border-b border-gray-200 bg-white px-6 py-3">
        <div className="mx-auto flex max-w-3xl items-center justify-between">
          <div className="flex items-center gap-2">
            <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-blue-600">
              <GraduationCap className="h-4 w-4 text-white" />
            </div>
            <span className="text-sm font-bold text-gray-900">GrammarAI</span>
          </div>
          <div className="flex items-center gap-2">
            <Badge variant="purple" className="gap-1"><Eye className="h-3 w-3" /> Read-only</Badge>
            <Link href="/login">
              <Button size="sm" variant="outline">Sign in to track progress</Button>
            </Link>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-3xl px-6 py-8 space-y-6">
        {loading && (
          <div className="flex min-h-[400px] items-center justify-center">
            <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
          </div>
        )}

        {error && (
          <div className="flex min-h-[300px] flex-col items-center justify-center gap-3 text-center">
            <AlertCircle className="h-10 w-10 text-red-400" />
            <p className="text-sm font-medium text-gray-700">{error}</p>
          </div>
        )}

        {exercise && (
          <>
            <div>
              <h1 className="text-xl font-bold text-gray-900">{exercise.title ?? 'English Exercise'}</h1>
              {exercise.description && (
                <p className="mt-1 text-sm text-gray-500">{exercise.description}</p>
              )}
              <div className="mt-2 flex gap-2">
                <Badge variant="default">{exercise.exerciseType}</Badge>
                {exercise.difficulty && <Badge variant="info">{exercise.difficulty}</Badge>}
                {exercise.tags.map((t) => <Badge key={t} variant="default" size="sm">{t}</Badge>)}
              </div>
            </div>

            {/* Read-only player — shows questions without submit */}
            <ExercisePlayer exercise={exercise} readOnly />

            <div className="rounded-xl border border-blue-200 bg-blue-50 p-4 text-sm text-blue-800">
              <strong>Want to track your answers and progress?</strong>{' '}
              <Link href="/login" className="underline hover:text-blue-600">Sign in with Telegram</Link> — it&apos;s free.
            </div>
          </>
        )}
      </main>
    </div>
  );
}

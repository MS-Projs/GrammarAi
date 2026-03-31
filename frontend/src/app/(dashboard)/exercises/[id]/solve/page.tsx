'use client';
import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { ArrowLeft, Loader2, AlertCircle } from 'lucide-react';
import { ExercisePlayer } from '@/components/exercises/player/ExercisePlayer';
import { Button }         from '@/components/ui/Button';
import { exercisesApi }   from '@/lib/api/exercises';
import type { ExerciseDetailDto } from '@/lib/types/api';

export default function SolvePage() {
  const { id } = useParams<{ id: string }>();
  const [exercise, setExercise] = useState<ExerciseDetailDto | null>(null);
  const [loading, setLoading]   = useState(true);
  const [error, setError]       = useState<string | null>(null);

  useEffect(() => {
    exercisesApi.getById(id)
      .then((ex) => {
        if (ex.status !== 'Ready') {
          setError('This exercise is not ready yet. Please wait for processing to complete.');
          return;
        }
        if (!ex.questions.length) {
          setError('No questions found in this exercise.');
          return;
        }
        setExercise(ex);
      })
      .catch(() => setError('Failed to load exercise.'))
      .finally(() => setLoading(false));
  }, [id]);

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Link href={`/exercises/${id}`}>
          <Button variant="ghost" size="sm" icon={<ArrowLeft className="h-4 w-4" />}>Back</Button>
        </Link>
        {exercise && (
          <div>
            <h2 className="text-lg font-bold text-gray-900">{exercise.title ?? 'Exercise'}</h2>
            <p className="text-xs text-gray-500">{exercise.questions.length} questions</p>
          </div>
        )}
      </div>

      {loading && (
        <div className="flex min-h-[400px] items-center justify-center">
          <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
        </div>
      )}

      {error && (
        <div className="flex min-h-[300px] flex-col items-center justify-center gap-3 text-center">
          <AlertCircle className="h-10 w-10 text-red-400" />
          <p className="text-sm font-medium text-gray-700">{error}</p>
          <Link href={`/exercises/${id}`}>
            <Button variant="outline">Back to Exercise</Button>
          </Link>
        </div>
      )}

      {exercise && <ExercisePlayer exercise={exercise} />}
    </div>
  );
}

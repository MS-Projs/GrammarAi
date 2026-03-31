'use client';
import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { ArrowLeft, Loader2 } from 'lucide-react';
import { ResultsSummary }  from '@/components/results/ResultsSummary';
import { AnswerBreakdown } from '@/components/results/AnswerBreakdown';
import { Button }          from '@/components/ui/Button';
import { exercisesApi }    from '@/lib/api/exercises';
import type { ExerciseDetailDto, SolveResultDto } from '@/lib/types/api';

export default function ResultsPage() {
  const { id } = useParams<{ id: string }>();
  const [exercise, setExercise] = useState<ExerciseDetailDto | null>(null);
  const [result, setResult]     = useState<SolveResultDto | null>(null);
  const [loading, setLoading]   = useState(true);

  useEffect(() => {
    Promise.all([
      exercisesApi.getById(id),
      exercisesApi.getResults(id),
    ])
      .then(([ex, r]) => {
        setExercise(ex);
        setResult({
          score:           r.score,
          maxScore:        r.maxScore,
          accuracyPercent: r.accuracyPercent,
          breakdown:       [],
        });
      })
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="flex items-center gap-3">
        <Link href={`/exercises/${id}`}>
          <Button variant="ghost" size="sm" icon={<ArrowLeft className="h-4 w-4" />}>Back</Button>
        </Link>
        <h2 className="text-lg font-bold text-gray-900">Results</h2>
      </div>

      {result && (
        <ResultsSummary
          result={result}
          exerciseId={id}
          title={exercise?.title ?? undefined}
        />
      )}

      {exercise && result && result.breakdown.length > 0 && (
        <AnswerBreakdown
          questions={exercise.questions}
          breakdown={result.breakdown}
        />
      )}
    </div>
  );
}

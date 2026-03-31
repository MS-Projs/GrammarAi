'use client';
import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { Play, RefreshCw, Globe, Lock, AlertCircle, Loader2 } from 'lucide-react';
import { Button }               from '@/components/ui/Button';
import { Badge }                from '@/components/ui/Badge';
import { Card }                 from '@/components/ui/Card';
import { ExerciseStatusBadge }  from '@/components/exercises/ExerciseStatusBadge';
import { useExerciseStatus }    from '@/hooks/useExerciseStatus';
import { exercisesApi }         from '@/lib/api/exercises';
import { formatDate }           from '@/lib/utils/formatters';
import type { ExerciseDetailDto } from '@/lib/types/api';
import toast from 'react-hot-toast';

export default function ExerciseDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [exercise, setExercise] = useState<ExerciseDetailDto | null>(null);
  const [loading, setLoading]   = useState(true);
  const [retrying, setRetrying] = useState(false);

  const { status, isPolling } = useExerciseStatus(id, exercise?.status);

  useEffect(() => {
    exercisesApi.getById(id)
      .then(setExercise)
      .catch(() => toast.error('Exercise not found'))
      .finally(() => setLoading(false));
  }, [id]);

  // Refresh detail when polling detects Ready
  useEffect(() => {
    if (status === 'Ready' && exercise?.status !== 'Ready') {
      exercisesApi.getById(id).then(setExercise);
    }
  }, [status, exercise?.status, id]);

  const handleRetry = async () => {
    setRetrying(true);
    try {
      await exercisesApi.retry(id);
      toast.success('Requeued for processing');
    } catch {
      toast.error('Retry failed');
    } finally {
      setRetrying(false);
    }
  };

  if (loading) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
      </div>
    );
  }

  if (!exercise) {
    return (
      <div className="flex min-h-[400px] flex-col items-center justify-center gap-3 text-center">
        <AlertCircle className="h-10 w-10 text-red-400" />
        <p className="font-medium text-gray-700">Exercise not found.</p>
        <Link href="/exercises"><Button variant="outline">Back to exercises</Button></Link>
      </div>
    );
  }

  const currentStatus = status ?? exercise.status;

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      {/* Header */}
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h2 className="text-xl font-bold text-gray-900">{exercise.title ?? 'Untitled Exercise'}</h2>
          {exercise.description && (
            <p className="mt-1 text-sm text-gray-500">{exercise.description}</p>
          )}
        </div>
        <div className="flex items-center gap-2">
          <ExerciseStatusBadge status={currentStatus} />
          {exercise.isPublic
            ? <Badge variant="purple" className="gap-1"><Globe className="h-3 w-3" /> Public</Badge>
            : <Badge variant="default" className="gap-1"><Lock className="h-3 w-3" /> Private</Badge>}
        </div>
      </div>

      {/* Processing state */}
      {isPolling && (
        <Card padding="md" className="flex items-center gap-3 border-blue-200 bg-blue-50">
          <Loader2 className="h-5 w-5 animate-spin text-blue-600" />
          <div>
            <p className="text-sm font-medium text-blue-800">Processing your exercise…</p>
            <p className="text-xs text-blue-600">The AI is extracting and structuring your content.</p>
          </div>
        </Card>
      )}

      {/* Failed state */}
      {currentStatus === 'Failed' && (
        <Card padding="md" className="flex items-center justify-between border-red-200 bg-red-50">
          <div className="flex items-center gap-3">
            <AlertCircle className="h-5 w-5 text-red-500" />
            <div>
              <p className="text-sm font-medium text-red-800">Processing failed</p>
              <p className="text-xs text-red-600">An error occurred while processing your image.</p>
            </div>
          </div>
          <Button variant="outline" size="sm" loading={retrying} icon={<RefreshCw className="h-3.5 w-3.5" />} onClick={handleRetry}>
            Retry
          </Button>
        </Card>
      )}

      {/* Metadata */}
      <Card padding="sm">
        <div className="flex flex-wrap gap-4 text-sm">
          <div><span className="text-gray-500">Type:</span> <span className="font-medium text-gray-800">{exercise.exerciseType}</span></div>
          {exercise.difficulty && <div><span className="text-gray-500">Level:</span> <Badge variant="info" size="sm">{exercise.difficulty}</Badge></div>}
          <div><span className="text-gray-500">Created:</span> <span className="font-medium text-gray-800">{formatDate(exercise.createdAt)}</span></div>
          {exercise.tags.length > 0 && (
            <div className="flex items-center gap-1.5">
              <span className="text-gray-500">Tags:</span>
              {exercise.tags.map((t) => <Badge key={t} variant="default" size="sm">{t}</Badge>)}
            </div>
          )}
        </div>
      </Card>

      {/* Questions preview */}
      {currentStatus === 'Ready' && exercise.questions.length > 0 && (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <h3 className="text-base font-semibold text-gray-900">
              {exercise.questions.length} Question{exercise.questions.length !== 1 ? 's' : ''}
            </h3>
            <Link href={`/exercises/${id}/solve`}>
              <Button icon={<Play className="h-4 w-4" />}>Start Exercise</Button>
            </Link>
          </div>

          {exercise.questions.slice().sort((a,b) => a.orderIndex - b.orderIndex).map((q, i) => (
            <Card key={q.id} padding="sm" className="text-sm">
              <p className="text-xs font-semibold text-gray-400 mb-1">Q{i + 1}</p>
              <p className="font-medium text-gray-800">{q.body}</p>
              {q.answers.length > 0 && (
                <ul className="mt-2 space-y-1">
                  {q.answers.map((a) => (
                    <li
                      key={a.id}
                      className={`flex items-center gap-2 rounded-md px-2 py-1 text-xs ${a.isCorrect ? 'bg-green-50 text-green-700 font-medium' : 'text-gray-600'}`}
                    >
                      {a.isCorrect && <span>✓</span>} {a.text}
                    </li>
                  ))}
                </ul>
              )}
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}

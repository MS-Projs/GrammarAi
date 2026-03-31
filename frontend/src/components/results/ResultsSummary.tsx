import Link from 'next/link';
import { RotateCcw, Home } from 'lucide-react';
import { Button }      from '@/components/ui/Button';
import { Card }        from '@/components/ui/Card';
import { Progress }    from '@/components/ui/Progress';
import { formatScore, formatPercent } from '@/lib/utils/formatters';
import type { SolveResultDto } from '@/lib/types/api';

interface ResultsSummaryProps {
  result: SolveResultDto;
  exerciseId: string;
  title?: string;
}

function getGrade(pct: number): { label: string; color: string; emoji: string } {
  if (pct >= 90) return { label: 'Excellent!',  color: 'text-green-600',  emoji: '🏆' };
  if (pct >= 75) return { label: 'Great job!',  color: 'text-blue-600',   emoji: '🎉' };
  if (pct >= 60) return { label: 'Good work',   color: 'text-amber-600',  emoji: '👍' };
  if (pct >= 40) return { label: 'Keep going',  color: 'text-orange-600', emoji: '💪' };
  return             { label: 'Keep practicing', color: 'text-red-600',   emoji: '📚' };
}

export function ResultsSummary({ result, exerciseId, title }: ResultsSummaryProps) {
  const pct   = Number(result.accuracyPercent);
  const grade = getGrade(pct);
  const progressColor: 'green' | 'blue' | 'amber' | 'red' =
    pct >= 75 ? 'green' : pct >= 50 ? 'blue' : pct >= 30 ? 'amber' : 'red';

  return (
    <Card padding="lg" className="mx-auto max-w-md text-center">
      {/* Grade */}
      <div className="mb-6">
        <span className="text-5xl">{grade.emoji}</span>
        <h2 className={`mt-2 text-2xl font-bold ${grade.color}`}>{grade.label}</h2>
        {title && <p className="mt-1 text-sm text-gray-500">{title}</p>}
      </div>

      {/* Score circle */}
      <div className="mb-6 flex flex-col items-center">
        <div className="relative flex h-28 w-28 items-center justify-center rounded-full border-4 border-gray-100 bg-gray-50">
          <div className="text-center">
            <p className={`text-3xl font-bold ${grade.color}`}>{Math.round(pct)}%</p>
            <p className="text-xs text-gray-400">accuracy</p>
          </div>
        </div>
        <p className="mt-3 text-sm text-gray-600">
          Score: <strong>{formatScore(result.score, result.maxScore)}</strong>
        </p>
      </div>

      {/* Progress bar */}
      <Progress value={pct} max={100} color={progressColor} showPercent className="mb-6" />

      {/* Stats grid */}
      <div className="mb-8 grid grid-cols-3 divide-x divide-gray-200 rounded-xl border border-gray-200 bg-gray-50 py-4">
        <div className="text-center">
          <p className="text-lg font-bold text-gray-900">{result.breakdown.filter((b) => b.isCorrect).length}</p>
          <p className="text-xs text-gray-500">Correct</p>
        </div>
        <div className="text-center">
          <p className="text-lg font-bold text-gray-900">{result.breakdown.filter((b) => b.isCorrect === false).length}</p>
          <p className="text-xs text-gray-500">Wrong</p>
        </div>
        <div className="text-center">
          <p className="text-lg font-bold text-gray-900">{result.breakdown.length}</p>
          <p className="text-xs text-gray-500">Total</p>
        </div>
      </div>

      {/* Actions */}
      <div className="flex gap-3">
        <Link href={`/exercises/${exerciseId}/solve`} className="flex-1">
          <Button variant="outline" fullWidth icon={<RotateCcw className="h-4 w-4" />}>
            Try Again
          </Button>
        </Link>
        <Link href="/exercises" className="flex-1">
          <Button fullWidth icon={<Home className="h-4 w-4" />}>
            Exercises
          </Button>
        </Link>
      </div>
    </Card>
  );
}

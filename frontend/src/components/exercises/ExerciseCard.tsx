'use client';
import Link from 'next/link';
import { MoreVertical, Play, Trash2, Globe } from 'lucide-react';
import { useState } from 'react';
import { Card } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { ExerciseStatusBadge } from './ExerciseStatusBadge';
import { formatRelativeDate } from '@/lib/utils/formatters';
import type { ExerciseDto } from '@/lib/types/api';

interface ExerciseCardProps {
  exercise: ExerciseDto;
  onDelete?: (id: string) => void;
}

const difficultyVariant: Record<string, 'success' | 'info' | 'warning' | 'danger' | 'purple'> = {
  A1: 'success', A2: 'success',
  B1: 'info',    B2: 'info',
  C1: 'warning', C2: 'danger',
};

const typeLabel: Record<string, string> = {
  MultipleChoice: 'MC',
  FillBlank:      'Fill',
  Reorder:        'Reorder',
  TrueFalse:      'T/F',
  Essay:          'Essay',
};

export function ExerciseCard({ exercise, onDelete }: ExerciseCardProps) {
  const [menuOpen, setMenuOpen] = useState(false);
  const canSolve = exercise.status === 'Ready';

  return (
    <Card padding="md" className="relative flex flex-col gap-3">
      {/* Header row */}
      <div className="flex items-start justify-between gap-2">
        <Link href={`/exercises/${exercise.id}`} className="flex-1 min-w-0">
          <h3 className="truncate text-sm font-semibold text-gray-900 hover:text-blue-600 transition-colors">
            {exercise.title ?? 'Untitled Exercise'}
          </h3>
        </Link>
        <div className="flex shrink-0 items-center gap-1.5">
          <ExerciseStatusBadge status={exercise.status} />
          <div className="relative">
            <Button
              variant="ghost"
              size="sm"
              className="h-7 w-7 p-0"
              onClick={() => setMenuOpen((v) => !v)}
            >
              <MoreVertical className="h-4 w-4" />
            </Button>
            {menuOpen && (
              <div className="absolute right-0 top-8 z-10 w-36 rounded-lg border border-gray-200 bg-white py-1 shadow-lg">
                {onDelete && (
                  <button
                    className="flex w-full items-center gap-2 px-3 py-2 text-sm text-red-600 hover:bg-red-50"
                    onClick={() => { setMenuOpen(false); onDelete(exercise.id); }}
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                    Delete
                  </button>
                )}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Description */}
      {exercise.description && (
        <p className="text-xs text-gray-500 line-clamp-2">{exercise.description}</p>
      )}

      {/* Meta row */}
      <div className="flex flex-wrap items-center gap-1.5">
        <Badge variant="default" size="sm">{typeLabel[exercise.exerciseType] ?? exercise.exerciseType}</Badge>
        {exercise.difficulty && (
          <Badge variant={difficultyVariant[exercise.difficulty] ?? 'default'} size="sm">
            {exercise.difficulty}
          </Badge>
        )}
        {exercise.isPublic && (
          <Badge variant="purple" size="sm" className="gap-1">
            <Globe className="h-3 w-3" /> Public
          </Badge>
        )}
        {exercise.tags.slice(0, 3).map((tag) => (
          <Badge key={tag} variant="default" size="sm">{tag}</Badge>
        ))}
      </div>

      {/* Footer */}
      <div className="flex items-center justify-between border-t border-gray-100 pt-3">
        <span className="text-xs text-gray-400">{formatRelativeDate(exercise.createdAt)}</span>
        {canSolve && (
          <Link href={`/exercises/${exercise.id}/solve`}>
            <Button size="sm" icon={<Play className="h-3.5 w-3.5" />}>Solve</Button>
          </Link>
        )}
      </div>
    </Card>
  );
}

'use client';
import { useEffect, useState, useCallback } from 'react';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { Plus, BookOpen, Search } from 'lucide-react';
import { ExerciseCard }    from '@/components/exercises/ExerciseCard';
import { ExerciseCardSkeleton } from '@/components/ui/Skeleton';
import { EmptyState }      from '@/components/ui/EmptyState';
import { Button }          from '@/components/ui/Button';
import { Badge }           from '@/components/ui/Badge';
import { exercisesApi }    from '@/lib/api/exercises';
import type { ExerciseDto, ExerciseStatus, DifficultyLevel } from '@/lib/types/api';
import toast from 'react-hot-toast';

const DIFFICULTIES: DifficultyLevel[] = ['A1','A2','B1','B2','C1','C2'];
const STATUSES: ExerciseStatus[]      = ['Ready','Processing','Pending','Failed'];

export default function ExercisesPage() {
  const searchParams = useSearchParams();
  const [exercises, setExercises] = useState<ExerciseDto[]>([]);
  const [loading, setLoading]     = useState(true);
  const [page, setPage]           = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [search, setSearch]       = useState(searchParams.get('search') ?? '');
  const [difficulty, setDifficulty] = useState('');
  const [status, setStatus]       = useState('');

  const fetchExercises = useCallback(async (p = 1) => {
    setLoading(true);
    try {
      const result = await exercisesApi.list({
        page: p, limit: 12,
        search:     search || undefined,
        difficulty: difficulty || undefined,
        status:     status || undefined,
      });
      setExercises(result.data);
      setTotalPages(result.totalPages);
      setPage(p);
    } finally {
      setLoading(false);
    }
  }, [search, difficulty, status]);

  useEffect(() => { fetchExercises(1); }, [fetchExercises]);

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this exercise? This cannot be undone.')) return;
    try {
      await exercisesApi.delete(id);
      setExercises((prev) => prev.filter((e) => e.id !== id));
      toast.success('Exercise deleted');
    } catch {
      toast.error('Failed to delete exercise');
    }
  };

  return (
    <div className="space-y-6">
      {/* Toolbar */}
      <div className="flex flex-wrap items-center gap-3">
        {/* Search */}
        <div className="relative min-w-[200px] flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && fetchExercises(1)}
            placeholder="Search exercises…"
            className="w-full rounded-lg border border-gray-300 bg-white py-2 pl-9 pr-3 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
          />
        </div>

        {/* Difficulty filter */}
        <select
          value={difficulty}
          onChange={(e) => setDifficulty(e.target.value)}
          className="rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
        >
          <option value="">All levels</option>
          {DIFFICULTIES.map((d) => <option key={d} value={d}>{d}</option>)}
        </select>

        {/* Status filter */}
        <select
          value={status}
          onChange={(e) => setStatus(e.target.value)}
          className="rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
        >
          <option value="">All statuses</option>
          {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>

        <Link href="/exercises/new">
          <Button icon={<Plus className="h-4 w-4" />}>New Exercise</Button>
        </Link>
      </div>

      {/* Active filters */}
      {(difficulty || status || search) && (
        <div className="flex flex-wrap items-center gap-2">
          <span className="text-xs text-gray-500">Active filters:</span>
          {search     && <Badge variant="info">{search} <button onClick={() => setSearch('')}   className="ml-1">×</button></Badge>}
          {difficulty && <Badge variant="info">{difficulty} <button onClick={() => setDifficulty('')} className="ml-1">×</button></Badge>}
          {status     && <Badge variant="info">{status}     <button onClick={() => setStatus('')}     className="ml-1">×</button></Badge>}
        </div>
      )}

      {/* Grid */}
      {loading ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => <ExerciseCardSkeleton key={i} />)}
        </div>
      ) : exercises.length === 0 ? (
        <EmptyState
          icon={BookOpen}
          title="No exercises found"
          description="Try adjusting your filters or upload a new exercise."
          action={
            <Link href="/exercises/new">
              <Button icon={<Plus className="h-4 w-4" />}>Upload Exercise</Button>
            </Link>
          }
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {exercises.map((ex) => (
            <ExerciseCard key={ex.id} exercise={ex} onDelete={handleDelete} />
          ))}
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-2">
          <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => fetchExercises(page - 1)}>
            Previous
          </Button>
          <span className="text-sm text-gray-500">Page {page} of {totalPages}</span>
          <Button variant="outline" size="sm" disabled={page >= totalPages} onClick={() => fetchExercises(page + 1)}>
            Next
          </Button>
        </div>
      )}
    </div>
  );
}

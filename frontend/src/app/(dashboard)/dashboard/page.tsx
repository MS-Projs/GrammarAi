'use client';
import { useEffect, useState } from 'react';
import Link from 'next/link';
import { BookOpen, CheckCircle, Target, Flame, Plus } from 'lucide-react';
import { StatsCard } from '@/components/dashboard/StatsCard';
import { ExerciseCard } from '@/components/exercises/ExerciseCard';
import { Button } from '@/components/ui/Button';
import { StatCardSkeleton, ExerciseCardSkeleton } from '@/components/ui/Skeleton';
import { usersApi } from '@/lib/api/users';
import { exercisesApi } from '@/lib/api/exercises';
import { useAuthStore } from '@/lib/store/authStore';
import type { UserStatsDto, ExerciseDto } from '@/lib/types/api';

export default function DashboardPage() {
  const { user } = useAuthStore();
  const [stats, setStats]       = useState<UserStatsDto | null>(null);
  const [recent, setRecent]     = useState<ExerciseDto[]>([]);
  const [loadingStats, setLoadingStats] = useState(true);
  const [loadingRecent, setLoadingRecent] = useState(true);

  useEffect(() => {
    usersApi.getStats()
      .then(setStats)
      .finally(() => setLoadingStats(false));

    exercisesApi.list({ limit: 6, page: 1 })
      .then((r) => setRecent(r.data))
      .finally(() => setLoadingRecent(false));
  }, []);

  return (
    <div className="space-y-8">
      {/* Greeting */}
      <div>
        <h2 className="text-xl font-bold text-gray-900">
          Welcome back{user?.displayName ? `, ${user.displayName}` : ''}! 👋
        </h2>
        <p className="text-sm text-gray-500">Keep up the great work on your English practice.</p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        {loadingStats ? (
          Array.from({ length: 4 }).map((_, i) => <StatCardSkeleton key={i} />)
        ) : stats ? (
          <>
            <StatsCard label="Total Exercises"  value={stats.totalExercises}  icon={BookOpen}     color="blue"   />
            <StatsCard label="Correct Answers"  value={stats.correctAnswers}  icon={CheckCircle}  color="green"  />
            <StatsCard label="Accuracy"         value={`${Math.round(stats.accuracyPercent)}%`} icon={Target} color="amber" />
            <StatsCard label="Day Streak"       value={stats.currentStreak}   icon={Flame}        color="purple" sub={`Best: ${stats.longestStreak}`} />
          </>
        ) : null}
      </div>

      {/* Recent exercises */}
      <div>
        <div className="mb-4 flex items-center justify-between">
          <h3 className="text-base font-semibold text-gray-900">Recent Exercises</h3>
          <div className="flex gap-2">
            <Link href="/exercises/new">
              <Button size="sm" icon={<Plus className="h-3.5 w-3.5" />}>New</Button>
            </Link>
            <Link href="/exercises">
              <Button size="sm" variant="outline">View all</Button>
            </Link>
          </div>
        </div>

        {loadingRecent ? (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 6 }).map((_, i) => <ExerciseCardSkeleton key={i} />)}
          </div>
        ) : recent.length > 0 ? (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {recent.map((ex) => (
              <ExerciseCard key={ex.id} exercise={ex} />
            ))}
          </div>
        ) : (
          <div className="rounded-xl border-2 border-dashed border-gray-200 py-14 text-center">
            <BookOpen className="mx-auto mb-3 h-10 w-10 text-gray-300" />
            <p className="text-sm font-medium text-gray-500">No exercises yet</p>
            <Link href="/exercises/new" className="mt-3 inline-block">
              <Button size="sm" icon={<Plus className="h-3.5 w-3.5" />}>Upload first exercise</Button>
            </Link>
          </div>
        )}
      </div>
    </div>
  );
}

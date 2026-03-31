'use client';
import { useEffect, useState } from 'react';
import { BookOpen, Target, CheckCircle, Flame, Calendar } from 'lucide-react';
import { Card, CardHeader, CardTitle } from '@/components/ui/Card';
import { StatsCard }   from '@/components/dashboard/StatsCard';
import { StatCardSkeleton } from '@/components/ui/Skeleton';
import { Input }       from '@/components/ui/Input';
import { Button }      from '@/components/ui/Button';
import { usersApi }    from '@/lib/api/users';
import { useAuthStore } from '@/lib/store/authStore';
import type { UserStatsDto, StreakDto } from '@/lib/types/api';
import toast from 'react-hot-toast';

export default function ProfilePage() {
  const { user, updateUser } = useAuthStore();
  const [stats, setStats]     = useState<UserStatsDto | null>(null);
  const [streak, setStreak]   = useState<StreakDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving]   = useState(false);
  const [displayName, setDisplayName] = useState(user?.displayName ?? '');

  useEffect(() => {
    Promise.all([usersApi.getStats(), usersApi.getStreak()])
      .then(([s, sk]) => { setStats(s); setStreak(sk); })
      .finally(() => setLoading(false));
  }, []);

  const handleSave = async () => {
    if (!displayName.trim()) return;
    setSaving(true);
    try {
      const updated = await usersApi.updateMe({ displayName: displayName.trim() });
      updateUser({ displayName: updated.displayName });
      toast.success('Profile updated');
    } catch {
      toast.error('Failed to update profile');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="mx-auto max-w-2xl space-y-8">
      {/* Profile info */}
      <Card>
        <CardHeader>
          <CardTitle>Profile</CardTitle>
        </CardHeader>
        <div className="flex items-center gap-4 mb-6">
          <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-blue-100 text-2xl font-bold text-blue-700">
            {user?.displayName?.charAt(0)?.toUpperCase() ?? '?'}
          </div>
          <div>
            <p className="font-semibold text-gray-900">{user?.displayName}</p>
            {user?.telegramId && (
              <p className="text-xs text-gray-400">Telegram ID: {user.telegramId}</p>
            )}
            {user?.isPremium && (
              <span className="mt-1 inline-flex items-center rounded-full bg-amber-100 px-2 py-0.5 text-xs font-medium text-amber-700">
                ✨ Premium
              </span>
            )}
          </div>
        </div>

        <div className="space-y-4">
          <Input
            label="Display Name"
            value={displayName}
            onChange={(e) => setDisplayName(e.target.value)}
            placeholder="Your name"
          />
          <Button loading={saving} onClick={handleSave} disabled={displayName === user?.displayName}>
            Save Changes
          </Button>
        </div>
      </Card>

      {/* Stats */}
      <div>
        <h3 className="mb-4 text-base font-semibold text-gray-900">Learning Statistics</h3>
        <div className="grid grid-cols-2 gap-4">
          {loading ? (
            Array.from({ length: 4 }).map((_, i) => <StatCardSkeleton key={i} />)
          ) : stats ? (
            <>
              <StatsCard label="Exercises"     value={stats.totalExercises}  icon={BookOpen}    color="blue"   />
              <StatsCard label="Accuracy"      value={`${Math.round(stats.accuracyPercent)}%`} icon={Target} color="amber" />
              <StatsCard label="Correct"       value={stats.correctAnswers}  icon={CheckCircle} color="green"  />
              <StatsCard label="Day Streak"    value={stats.currentStreak}   icon={Flame}       color="purple" sub={`Best: ${stats.longestStreak}`} />
            </>
          ) : null}
        </div>
      </div>

      {/* Streak info */}
      {streak && (
        <Card>
          <CardHeader>
            <CardTitle>Streak</CardTitle>
          </CardHeader>
          <div className="flex items-center gap-6">
            <div className="text-center">
              <p className="text-3xl font-bold text-orange-500">{streak.currentStreak}</p>
              <p className="text-xs text-gray-500">Current</p>
            </div>
            <div className="text-center">
              <p className="text-3xl font-bold text-purple-500">{streak.longestStreak}</p>
              <p className="text-xs text-gray-500">Best</p>
            </div>
            {streak.lastActiveDate && (
              <div className="flex items-center gap-2 text-sm text-gray-500">
                <Calendar className="h-4 w-4" />
                Last active: {streak.lastActiveDate}
              </div>
            )}
          </div>
        </Card>
      )}
    </div>
  );
}

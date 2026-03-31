import { cn } from '@/lib/utils/cn';

interface ProgressProps {
  value: number;
  max?: number;
  label?: string;
  showPercent?: boolean;
  color?: 'blue' | 'green' | 'amber' | 'red';
  size?: 'sm' | 'md';
  className?: string;
}

const colors = {
  blue:  'bg-blue-600',
  green: 'bg-green-500',
  amber: 'bg-amber-500',
  red:   'bg-red-500',
};

export function Progress({ value, max = 100, label, showPercent, color = 'blue', size = 'md', className }: ProgressProps) {
  const pct = Math.min(100, Math.max(0, (value / max) * 100));
  return (
    <div className={cn('w-full', className)}>
      {(label || showPercent) && (
        <div className="mb-1 flex justify-between text-sm text-gray-600">
          {label && <span>{label}</span>}
          {showPercent && <span>{Math.round(pct)}%</span>}
        </div>
      )}
      <div className={cn('w-full overflow-hidden rounded-full bg-gray-200', size === 'sm' ? 'h-1.5' : 'h-2.5')}>
        <div
          className={cn('h-full rounded-full transition-all duration-500', colors[color])}
          style={{ width: `${pct}%` }}
          role="progressbar"
          aria-valuenow={value}
          aria-valuemax={max}
        />
      </div>
    </div>
  );
}

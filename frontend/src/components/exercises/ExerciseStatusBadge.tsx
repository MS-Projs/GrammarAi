import { Badge } from '@/components/ui/Badge';
import { Loader2, CheckCircle, Clock, XCircle } from 'lucide-react';
import type { ExerciseStatus } from '@/lib/types/api';

const config: Record<ExerciseStatus, { label: string; variant: 'default' | 'success' | 'warning' | 'danger' | 'info'; icon: React.ReactNode }> = {
  Pending:    { label: 'Pending',    variant: 'default', icon: <Clock className="h-3 w-3" /> },
  Processing: { label: 'Processing', variant: 'info',    icon: <Loader2 className="h-3 w-3 animate-spin" /> },
  Ready:      { label: 'Ready',      variant: 'success', icon: <CheckCircle className="h-3 w-3" /> },
  Failed:     { label: 'Failed',     variant: 'danger',  icon: <XCircle className="h-3 w-3" /> },
};

export function ExerciseStatusBadge({ status }: { status: ExerciseStatus }) {
  const { label, variant, icon } = config[status] ?? config.Pending;
  return (
    <Badge variant={variant} className="gap-1">
      {icon}
      {label}
    </Badge>
  );
}

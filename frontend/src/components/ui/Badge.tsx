import { HTMLAttributes } from 'react';
import { cn } from '@/lib/utils/cn';

type Variant = 'default' | 'success' | 'warning' | 'danger' | 'info' | 'purple';

interface BadgeProps extends HTMLAttributes<HTMLSpanElement> {
  variant?: Variant;
  size?: 'sm' | 'md';
}

const variants: Record<Variant, string> = {
  default: 'bg-gray-100 text-gray-700',
  success: 'bg-green-100 text-green-700',
  warning: 'bg-amber-100 text-amber-700',
  danger:  'bg-red-100  text-red-700',
  info:    'bg-blue-100 text-blue-700',
  purple:  'bg-purple-100 text-purple-700',
};

export function Badge({ className, variant = 'default', size = 'md', children, ...props }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full font-medium',
        size === 'sm' ? 'px-2 py-0.5 text-xs' : 'px-2.5 py-1 text-xs',
        variants[variant],
        className,
      )}
      {...props}
    >
      {children}
    </span>
  );
}

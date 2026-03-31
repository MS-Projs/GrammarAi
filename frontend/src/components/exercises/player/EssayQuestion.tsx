'use client';
import { cn } from '@/lib/utils/cn';

interface EssayQuestionProps {
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
  minLength?: number;
}

export function EssayQuestion({ value, onChange, disabled, minLength = 20 }: EssayQuestionProps) {
  return (
    <div className="space-y-2">
      <textarea
        value={value}
        onChange={(e) => onChange(e.target.value)}
        disabled={disabled}
        rows={5}
        placeholder="Write your answer here…"
        className={cn(
          'w-full resize-y rounded-xl border-2 border-gray-200 bg-white px-4 py-3 text-sm text-gray-900',
          'placeholder:text-gray-400',
          'focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200',
          disabled && 'cursor-not-allowed opacity-70',
        )}
      />
      <div className="flex justify-between text-xs text-gray-400">
        <span>{value.length} characters</span>
        {value.length > 0 && value.length < minLength && (
          <span className="text-amber-500">Minimum {minLength} characters recommended</span>
        )}
      </div>
    </div>
  );
}

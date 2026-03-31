'use client';
import { cn } from '@/lib/utils/cn';

interface FillBlankQuestionProps {
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
  isCorrect?: boolean | null;
  placeholder?: string;
}

export function FillBlankQuestion({
  value,
  onChange,
  disabled,
  isCorrect,
  placeholder = 'Type your answer here…',
}: FillBlankQuestionProps) {
  return (
    <div className="space-y-2">
      <input
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        disabled={disabled}
        placeholder={placeholder}
        className={cn(
          'w-full rounded-xl border-2 bg-white px-4 py-3 text-sm text-gray-900 shadow-sm',
          'placeholder:text-gray-400',
          'focus:outline-none focus:ring-2',
          disabled ? 'cursor-not-allowed opacity-70' : '',
          isCorrect === true  ? 'border-green-500 bg-green-50 focus:border-green-500 focus:ring-green-200' :
          isCorrect === false ? 'border-red-400  bg-red-50  focus:border-red-400  focus:ring-red-200' :
                                'border-gray-200 focus:border-blue-500 focus:ring-blue-200',
        )}
      />
      {isCorrect === false && (
        <p className="text-xs text-red-600">Incorrect — check the correct answer below.</p>
      )}
      {isCorrect === true && (
        <p className="text-xs text-green-600 font-medium">Correct!</p>
      )}
    </div>
  );
}

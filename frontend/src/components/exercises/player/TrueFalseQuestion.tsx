'use client';
import { cn } from '@/lib/utils/cn';
import type { AnswerDto } from '@/lib/types/api';

interface TrueFalseQuestionProps {
  answers: AnswerDto[];
  selected: string | null;
  onChange: (answerId: string) => void;
  disabled?: boolean;
  showCorrect?: boolean;
}

export function TrueFalseQuestion({ answers, selected, onChange, disabled, showCorrect }: TrueFalseQuestionProps) {
  return (
    <div className="flex gap-4">
      {answers.map((answer) => {
        const isSelected = selected === answer.id;
        const isCorrect  = showCorrect && answer.isCorrect;
        const isWrong    = showCorrect && isSelected && !answer.isCorrect;
        const label      = answer.text.trim().toLowerCase();
        const isTrue     = label === 'true';

        return (
          <button
            key={answer.id}
            onClick={() => !disabled && onChange(answer.id)}
            disabled={disabled}
            className={cn(
              'flex flex-1 items-center justify-center gap-2 rounded-2xl border-2 py-5 text-lg font-bold transition-all',
              'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500',
              !disabled && !showCorrect && 'hover:shadow-md',
              isSelected && !showCorrect
                ? isTrue ? 'border-blue-500 bg-blue-500 text-white' : 'border-gray-700 bg-gray-700 text-white'
                : 'border-gray-200 bg-white text-gray-700',
              isCorrect ? 'border-green-500 bg-green-500 text-white' : '',
              isWrong   ? 'border-red-400  bg-red-50  text-red-700' : '',
              disabled  ? 'cursor-not-allowed' : 'cursor-pointer',
            )}
          >
            {isTrue ? '✓ True' : '✗ False'}
          </button>
        );
      })}
    </div>
  );
}

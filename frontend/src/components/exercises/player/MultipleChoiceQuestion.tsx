'use client';
import { cn } from '@/lib/utils/cn';
import type { AnswerDto } from '@/lib/types/api';

interface MultipleChoiceQuestionProps {
  answers: AnswerDto[];
  selected: string | null;
  onChange: (answerId: string) => void;
  disabled?: boolean;
  showCorrect?: boolean;
}

export function MultipleChoiceQuestion({
  answers,
  selected,
  onChange,
  disabled,
  showCorrect,
}: MultipleChoiceQuestionProps) {
  return (
    <div className="space-y-2.5">
      {answers.map((answer) => {
        const isSelected = selected === answer.id;
        const isCorrect  = showCorrect && answer.isCorrect;
        const isWrong    = showCorrect && isSelected && !answer.isCorrect;

        return (
          <button
            key={answer.id}
            onClick={() => !disabled && onChange(answer.id)}
            disabled={disabled}
            className={cn(
              'flex w-full items-center gap-3 rounded-xl border-2 p-4 text-left text-sm font-medium transition-all',
              'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500',
              !disabled && !showCorrect && 'hover:border-blue-300 hover:bg-blue-50',
              isSelected && !showCorrect ? 'border-blue-500 bg-blue-50 text-blue-800' : 'border-gray-200 bg-white text-gray-800',
              isCorrect  ? 'border-green-500 bg-green-50 text-green-800' : '',
              isWrong    ? 'border-red-400 bg-red-50 text-red-700' : '',
              disabled   ? 'cursor-not-allowed opacity-70' : 'cursor-pointer',
            )}
          >
            {/* Radio indicator */}
            <span
              className={cn(
                'flex h-5 w-5 shrink-0 items-center justify-center rounded-full border-2',
                isSelected && !showCorrect ? 'border-blue-500 bg-blue-500' : 'border-gray-300',
                isCorrect  ? 'border-green-500 bg-green-500' : '',
                isWrong    ? 'border-red-400 bg-red-400' : '',
              )}
            >
              {(isSelected || isCorrect) && (
                <span className="h-2 w-2 rounded-full bg-white" />
              )}
            </span>
            <span className="flex-1">{answer.text}</span>
            {showCorrect && isCorrect && (
              <span className="text-xs font-semibold text-green-600">✓ Correct</span>
            )}
          </button>
        );
      })}
    </div>
  );
}

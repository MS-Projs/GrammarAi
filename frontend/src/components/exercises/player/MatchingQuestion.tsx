'use client';
import { useState } from 'react';
import { cn } from '@/lib/utils/cn';
import type { AnswerDto } from '@/lib/types/api';

interface MatchingQuestionProps {
  answers: AnswerDto[];
  /**
   * Map of left-item index → selected right-item answer id.
   * The parent stores this as { [leftIdx]: answerId }.
   */
  selections: Record<number, string>;
  onChange: (selections: Record<number, string>) => void;
  disabled?: boolean;
  showCorrect?: boolean;
}

/**
 * Matching question: left column = prompts (odd indices or first half),
 * right column = options to match.
 * We split answers into two halves: left = items to match, right = options.
 * The AI parser stores them interleaved (pair 0L, 0R, 1L, 1R…) or split.
 * We assume the first half are "left" items and second half are "right" options.
 */
export function MatchingQuestion({ answers, selections, onChange, disabled, showCorrect }: MatchingQuestionProps) {
  const [activeLeft, setActiveLeft] = useState<number | null>(null);
  const half     = Math.ceil(answers.length / 2);
  const leftItems  = answers.slice(0, half);
  const rightItems = answers.slice(half);

  const handleLeftClick = (idx: number) => {
    if (disabled) return;
    setActiveLeft(idx === activeLeft ? null : idx);
  };

  const handleRightClick = (rightAnswer: AnswerDto) => {
    if (disabled || activeLeft === null) return;
    onChange({ ...selections, [activeLeft]: rightAnswer.id });
    setActiveLeft(null);
  };

  const getStatusColor = (leftIdx: number) => {
    if (!showCorrect) return '';
    const selectedId = selections[leftIdx];
    const correctId  = rightItems.find((r, ri) => leftItems[leftIdx]?.isCorrect && ri === leftIdx)?.id;
    if (!selectedId) return 'border-gray-200';
    if (selectedId === correctId || (showCorrect && leftItems[leftIdx]?.isCorrect)) return 'border-green-500 bg-green-50';
    return 'border-red-400 bg-red-50';
  };

  return (
    <div className="grid grid-cols-2 gap-4">
      {/* Left column */}
      <div className="space-y-2">
        <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-gray-400">Items</p>
        {leftItems.map((item, idx) => {
          const isActive    = activeLeft === idx;
          const isConnected = selections[idx] !== undefined;
          return (
            <button
              key={item.id}
              onClick={() => handleLeftClick(idx)}
              disabled={disabled}
              className={cn(
                'w-full rounded-xl border-2 px-3 py-2.5 text-left text-sm font-medium transition-all',
                isActive     ? 'border-blue-500 bg-blue-50 text-blue-800 ring-2 ring-blue-200' :
                isConnected  ? 'border-blue-300 bg-blue-50/60 text-blue-700' :
                               'border-gray-200 bg-white text-gray-700 hover:border-blue-300',
                disabled     ? 'cursor-not-allowed' : 'cursor-pointer',
                getStatusColor(idx),
              )}
            >
              {item.text}
            </button>
          );
        })}
      </div>

      {/* Right column */}
      <div className="space-y-2">
        <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-gray-400">Match</p>
        {rightItems.map((item) => {
          const isMatched = Object.values(selections).includes(item.id);
          return (
            <button
              key={item.id}
              onClick={() => handleRightClick(item)}
              disabled={disabled || (isMatched && activeLeft === null)}
              className={cn(
                'w-full rounded-xl border-2 px-3 py-2.5 text-left text-sm font-medium transition-all',
                activeLeft !== null && !isMatched
                  ? 'border-blue-300 bg-blue-50/60 hover:border-blue-500 hover:bg-blue-100'
                  : 'border-gray-200 bg-white text-gray-700',
                isMatched ? 'border-indigo-300 bg-indigo-50 text-indigo-700' : '',
                disabled  ? 'cursor-not-allowed' : 'cursor-pointer',
              )}
            >
              {item.text}
            </button>
          );
        })}
      </div>

      {activeLeft !== null && (
        <p className="col-span-2 text-center text-xs text-blue-600">
          Select the matching item on the right →
        </p>
      )}
    </div>
  );
}

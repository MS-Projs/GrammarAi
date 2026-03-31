import { CheckCircle, XCircle, HelpCircle, ChevronDown } from 'lucide-react';
import { useState } from 'react';
import { cn } from '@/lib/utils/cn';
import type { QuestionDto, AnswerBreakdownDto } from '@/lib/types/api';

interface AnswerBreakdownProps {
  questions: QuestionDto[];
  breakdown: AnswerBreakdownDto[];
}

export function AnswerBreakdown({ questions, breakdown }: AnswerBreakdownProps) {
  const [expanded, setExpanded] = useState<Set<string>>(new Set());

  const toggle = (id: string) =>
    setExpanded((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });

  return (
    <div className="space-y-3">
      <h3 className="text-sm font-semibold text-gray-700">Question Review</h3>
      {questions.map((q, i) => {
        const b          = breakdown.find((x) => x.questionId === q.id);
        const isCorrect  = b?.isCorrect;
        const isOpen     = expanded.has(q.id);
        const correctAnswers = q.answers.filter((a) => a.isCorrect || b?.correctAnswerIds.includes(a.id));

        return (
          <div
            key={q.id}
            className={cn(
              'overflow-hidden rounded-xl border-2',
              isCorrect === true  ? 'border-green-200 bg-green-50/50' :
              isCorrect === false ? 'border-red-200   bg-red-50/50'   :
                                    'border-gray-200  bg-gray-50/50',
            )}
          >
            <button
              className="flex w-full items-start gap-3 p-4 text-left"
              onClick={() => toggle(q.id)}
            >
              <span className="mt-0.5 shrink-0">
                {isCorrect === true  ? <CheckCircle className="h-5 w-5 text-green-500" /> :
                 isCorrect === false ? <XCircle     className="h-5 w-5 text-red-400"   /> :
                                       <HelpCircle  className="h-5 w-5 text-gray-400"  />}
              </span>
              <div className="flex-1 min-w-0">
                <span className="text-xs font-medium text-gray-400">Q{i + 1}</span>
                <p className="text-sm font-medium text-gray-800 line-clamp-2">{q.body}</p>
              </div>
              <ChevronDown className={cn('h-4 w-4 shrink-0 text-gray-400 transition-transform mt-1', isOpen && 'rotate-180')} />
            </button>

            {isOpen && (
              <div className="border-t border-gray-200 px-4 pb-4 pt-3 space-y-2">
                {correctAnswers.length > 0 && (
                  <div>
                    <p className="mb-1 text-xs font-semibold text-green-700">Correct answer(s):</p>
                    <ul className="space-y-1">
                      {correctAnswers.map((a) => (
                        <li key={a.id} className="rounded-lg bg-green-100 px-3 py-1.5 text-xs text-green-800">
                          {a.text}
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
                {(b?.explanation ?? q.explanation) && (
                  <div className="rounded-lg bg-amber-50 border border-amber-100 px-3 py-2 text-xs text-amber-800">
                    <strong>Explanation:</strong> {b?.explanation ?? q.explanation}
                  </div>
                )}
                <div className="flex justify-between text-xs text-gray-500">
                  <span>Score: {b?.score ?? 0} / {q.maxScore}</span>
                </div>
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}

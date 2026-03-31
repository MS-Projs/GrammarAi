'use client';
import { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import toast from 'react-hot-toast';
import { ChevronLeft, ChevronRight, CheckCircle } from 'lucide-react';
import { Button }           from '@/components/ui/Button';
import { Progress }         from '@/components/ui/Progress';
import { Card }             from '@/components/ui/Card';
import { QuestionRenderer, type QuestionAnswer } from './QuestionRenderer';
import { exercisesApi }     from '@/lib/api/exercises';
import type { ExerciseDetailDto, SolveAnswerItem, SolveResultDto } from '@/lib/types/api';

interface ExercisePlayerProps {
  exercise: ExerciseDetailDto;
  readOnly?: boolean;
  onResult?: (result: SolveResultDto) => void;
}

export function ExercisePlayer({ exercise, readOnly, onResult }: ExercisePlayerProps) {
  const router  = useRouter();
  const startTs = useRef<number>(Date.now());

  const questions = exercise.questions.slice().sort((a, b) => a.orderIndex - b.orderIndex);
  const total     = questions.length;

  const [idx, setIdx]       = useState(0);
  const [answers, setAnswers] = useState<Record<string, QuestionAnswer>>({});
  const [submitting, setSubmitting] = useState(false);
  const [result, setResult] = useState<SolveResultDto | null>(null);

  const currentQ = questions[idx];

  // Track per-question time
  const qStartTs = useRef<number>(Date.now());
  useEffect(() => { qStartTs.current = Date.now(); }, [idx]);

  const setAnswer = (qId: string, ans: QuestionAnswer) =>
    setAnswers((prev) => ({ ...prev, [qId]: ans }));

  // Stamp per-question elapsed time before navigating away
  const stampTime = () => {
    const qId = currentQ?.id;
    if (!qId) return;
    setAnswers((prev) => ({
      ...prev,
      [qId]: { ...(prev[qId] ?? {}), timeSpentMs: Date.now() - qStartTs.current },
    }));
  };

  const handleSubmit = async () => {
    if (readOnly || submitting) return;
    setSubmitting(true);
    try {
      const payload: SolveAnswerItem[] = questions.map((q) => {
        const ans = answers[q.id] ?? {};
        return {
          questionId:  q.id,
          answerId:    ans.answerId,
          freeText:    ans.freeText,
          // Per-question time tracked via qStartTs on navigation; total time split evenly as fallback
          timeSpentMs: ans.timeSpentMs ?? Math.round((Date.now() - startTs.current) / total),
        };
      });
      const res = await exercisesApi.solve(exercise.id, payload);
      setResult(res);
      onResult?.(res);
      // Persist result so the results page can read it after redirect
      sessionStorage.setItem(`result:${exercise.id}`, JSON.stringify(res));
      router.push(`/exercises/${exercise.id}/results`);
    } catch {
      toast.error('Failed to submit. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  if (!currentQ) return null;

  const progress = ((idx + 1) / total) * 100;

  return (
    <div className="mx-auto max-w-2xl space-y-5">
      {/* Progress bar */}
      <div className="space-y-1">
        <div className="flex justify-between text-xs text-gray-500">
          <span>Question {idx + 1} of {total}</span>
          <span>{Math.round(progress)}% complete</span>
        </div>
        <Progress value={idx + 1} max={total} color="blue" size="sm" />
      </div>

      {/* Question card */}
      <Card padding="lg" className="animate-fade-in">
        <p className="mb-6 text-base font-semibold leading-relaxed text-gray-900">
          {currentQ.body}
        </p>

        <QuestionRenderer
          question={currentQ}
          answer={answers[currentQ.id] ?? {}}
          onChange={(ans) => setAnswer(currentQ.id, ans)}
          readOnly={readOnly}
        />

        {currentQ.explanation && readOnly && (
          <div className="mt-4 rounded-lg bg-amber-50 border border-amber-200 p-3 text-xs text-amber-800">
            <strong>Explanation:</strong> {currentQ.explanation}
          </div>
        )}
      </Card>

      {/* Navigation */}
      <div className="flex items-center justify-between">
        <Button
          variant="outline"
          icon={<ChevronLeft className="h-4 w-4" />}
          onClick={() => { stampTime(); setIdx((i) => Math.max(0, i - 1)); }}
          disabled={idx === 0}
        >
          Previous
        </Button>

        {idx < total - 1 ? (
          <Button
            icon={<ChevronRight className="h-4 w-4" />}
            onClick={() => { stampTime(); setIdx((i) => i + 1); }}
          >
            Next
          </Button>
        ) : (
          !readOnly && (
            <Button
              icon={<CheckCircle className="h-4 w-4" />}
              loading={submitting}
              onClick={handleSubmit}
            >
              Submit Answers
            </Button>
          )
        )}
      </div>
    </div>
  );
}

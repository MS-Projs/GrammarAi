'use client';
import { MultipleChoiceQuestion } from './MultipleChoiceQuestion';
import { FillBlankQuestion }       from './FillBlankQuestion';
import { TrueFalseQuestion }       from './TrueFalseQuestion';
import { MatchingQuestion }        from './MatchingQuestion';
import { EssayQuestion }           from './EssayQuestion';
import type { QuestionDto, AnswerBreakdownDto } from '@/lib/types/api';

export interface QuestionAnswer {
  answerId?:    string;
  freeText?:    string;
  matchMap?:    Record<number, string>;
}

interface QuestionRendererProps {
  question:   QuestionDto;
  answer:     QuestionAnswer;
  onChange:   (answer: QuestionAnswer) => void;
  breakdown?: AnswerBreakdownDto;
  readOnly?:  boolean;
}

export function QuestionRenderer({ question, answer, onChange, breakdown, readOnly }: QuestionRendererProps) {
  const showCorrect = !!breakdown;
  const disabled    = readOnly;

  switch (question.exerciseType) {
    case 'MultipleChoice':
      return (
        <MultipleChoiceQuestion
          answers={question.answers}
          selected={answer.answerId ?? null}
          onChange={(id) => onChange({ ...answer, answerId: id })}
          disabled={disabled}
          showCorrect={showCorrect}
        />
      );

    case 'TrueFalse':
      return (
        <TrueFalseQuestion
          answers={question.answers}
          selected={answer.answerId ?? null}
          onChange={(id) => onChange({ ...answer, answerId: id })}
          disabled={disabled}
          showCorrect={showCorrect}
        />
      );

    case 'FillBlank':
      return (
        <FillBlankQuestion
          value={answer.freeText ?? ''}
          onChange={(v) => onChange({ ...answer, freeText: v })}
          disabled={disabled}
          isCorrect={breakdown ? breakdown.isCorrect ?? null : undefined}
        />
      );

    case 'Essay':
      return (
        <EssayQuestion
          value={answer.freeText ?? ''}
          onChange={(v) => onChange({ ...answer, freeText: v })}
          disabled={disabled}
        />
      );

    case 'Reorder':
    default:
      return (
        <MatchingQuestion
          answers={question.answers}
          selections={answer.matchMap ?? {}}
          onChange={(m) => onChange({ ...answer, matchMap: m })}
          disabled={disabled}
          showCorrect={showCorrect}
        />
      );
  }
}

import { ExerciseUpload } from '@/components/exercises/ExerciseUpload';

export default function NewExercisePage() {
  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-xl font-bold text-gray-900">New Exercise</h2>
        <p className="text-sm text-gray-500">
          Upload a photo or screenshot of an exercise. Our AI will extract and structure it automatically.
        </p>
      </div>
      <ExerciseUpload />
    </div>
  );
}

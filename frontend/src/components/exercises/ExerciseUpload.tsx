'use client';
import { useState, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { useRouter } from 'next/navigation';
import { Upload, X, Image as ImageIcon, AlertCircle } from 'lucide-react';
import toast from 'react-hot-toast';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Progress } from '@/components/ui/Progress';
import { Badge } from '@/components/ui/Badge';
import { cn } from '@/lib/utils/cn';
import { exercisesApi } from '@/lib/api/exercises';
import { formatBytes } from '@/lib/utils/formatters';
import type { ExerciseType, DifficultyLevel } from '@/lib/types/api';

const ACCEPTED = { 'image/jpeg': [], 'image/png': [], 'image/webp': [] };
const MAX_FILES = 5;
const MAX_SIZE  = 10 * 1024 * 1024; // 10 MB

interface FileItem { file: File; preview: string; }

export function ExerciseUpload() {
  const router = useRouter();
  const [files, setFiles]         = useState<FileItem[]>([]);
  const [title, setTitle]         = useState('');
  const [type, setType]           = useState<ExerciseType>('MultipleChoice');
  const [difficulty, setDifficulty] = useState<DifficultyLevel | ''>('');
  const [uploadPct, setUploadPct] = useState(0);
  const [loading, setLoading]     = useState(false);
  const [error, setError]         = useState<string | null>(null);

  const onDrop = useCallback((accepted: File[], rejected: { file: File; errors: { message: string }[] }[]) => {
    if (rejected.length) {
      const msg = rejected[0].errors[0]?.message ?? 'Some files were rejected';
      setError(msg);
      return;
    }
    setError(null);
    const items = accepted.slice(0, MAX_FILES - files.length).map((f) => ({
      file: f,
      preview: URL.createObjectURL(f),
    }));
    setFiles((prev) => [...prev, ...items].slice(0, MAX_FILES));
  }, [files.length]);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: ACCEPTED,
    maxSize: MAX_SIZE,
    maxFiles: MAX_FILES,
  });

  const removeFile = (idx: number) => {
    setFiles((prev) => {
      URL.revokeObjectURL(prev[idx].preview);
      return prev.filter((_, i) => i !== idx);
    });
  };

  const handleSubmit = async () => {
    if (!files.length) { setError('Please add at least one image.'); return; }
    setLoading(true);
    setError(null);
    try {
      const { id } = await exercisesApi.create({
        title: title.trim() || undefined,
        exerciseType: type,
        difficulty: difficulty || undefined,
        isPublic: false,
      });
      await exercisesApi.uploadImages(id, files.map((f) => f.file), setUploadPct);
      toast.success('Exercise created! Processing…');
      router.push(`/exercises/${id}`);
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'Upload failed. Please try again.';
      setError(msg);
      setLoading(false);
    }
  };

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      {/* Dropzone */}
      <div
        {...getRootProps()}
        className={cn(
          'flex cursor-pointer flex-col items-center justify-center rounded-2xl border-2 border-dashed p-10 transition-colors',
          isDragActive ? 'border-blue-500 bg-blue-50' : 'border-gray-300 bg-gray-50 hover:border-blue-400 hover:bg-blue-50/50',
        )}
      >
        <input {...getInputProps()} />
        <Upload className="mb-3 h-10 w-10 text-gray-400" />
        <p className="text-sm font-medium text-gray-700">
          {isDragActive ? 'Drop here…' : 'Drag & drop images, or click to browse'}
        </p>
        <p className="mt-1 text-xs text-gray-400">
          JPEG, PNG, WebP · max {formatBytes(MAX_SIZE)} · up to {MAX_FILES} images
        </p>
      </div>

      {/* Previews */}
      {files.length > 0 && (
        <div className="grid grid-cols-3 gap-3 sm:grid-cols-5">
          {files.map((item, idx) => (
            <div key={idx} className="group relative aspect-square overflow-hidden rounded-lg border border-gray-200 bg-gray-100">
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={item.preview} alt={`page ${idx + 1}`} className="h-full w-full object-cover" />
              <button
                onClick={() => removeFile(idx)}
                className="absolute right-1 top-1 hidden rounded-full bg-black/60 p-0.5 text-white group-hover:flex"
              >
                <X className="h-3 w-3" />
              </button>
              <div className="absolute bottom-1 left-1">
                <Badge size="sm" variant="default" className="bg-white/90 text-gray-700 text-xs">
                  <ImageIcon className="mr-1 h-2.5 w-2.5" />
                  {formatBytes(item.file.size)}
                </Badge>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Metadata */}
      <div className="space-y-4 rounded-xl border border-gray-200 bg-white p-5">
        <Input
          label="Title (optional)"
          placeholder="e.g. Present Simple Exercises"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="mb-1.5 block text-sm font-medium text-gray-700">Exercise Type</label>
            <select
              value={type}
              onChange={(e) => setType(e.target.value as ExerciseType)}
              className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            >
              <option value="MultipleChoice">Multiple Choice</option>
              <option value="FillBlank">Fill in the Blank</option>
              <option value="TrueFalse">True / False</option>
              <option value="Reorder">Reorder</option>
              <option value="Essay">Essay</option>
            </select>
          </div>

          <div>
            <label className="mb-1.5 block text-sm font-medium text-gray-700">Difficulty</label>
            <select
              value={difficulty}
              onChange={(e) => setDifficulty(e.target.value as DifficultyLevel | '')}
              className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            >
              <option value="">Auto-detect</option>
              {['A1','A2','B1','B2','C1','C2'].map((d) => (
                <option key={d} value={d}>{d}</option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Upload progress */}
      {loading && uploadPct > 0 && uploadPct < 100 && (
        <Progress value={uploadPct} showPercent label="Uploading…" color="blue" />
      )}

      {/* Error */}
      {error && (
        <div className="flex items-center gap-2 rounded-lg bg-red-50 p-3 text-sm text-red-700">
          <AlertCircle className="h-4 w-4 shrink-0" />
          {error}
        </div>
      )}

      <Button fullWidth size="lg" loading={loading} onClick={handleSubmit}>
        {loading ? 'Processing…' : 'Create Exercise'}
      </Button>
    </div>
  );
}

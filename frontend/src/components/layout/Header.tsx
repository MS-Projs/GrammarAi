'use client';
import { Search } from 'lucide-react';
import { usePathname, useRouter } from 'next/navigation';

const titles: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/exercises': 'My Exercises',
  '/exercises/new': 'New Exercise',
  '/profile': 'Profile',
};

export function Header() {
  const pathname = usePathname();
  const router   = useRouter();
  const title = Object.entries(titles).find(([path]) => pathname === path || pathname.startsWith(path + '/'))?.[1] ?? 'GrammarAI';

  return (
    <header className="flex h-16 items-center justify-between border-b border-gray-200 bg-white px-6">
      <h1 className="text-lg font-semibold text-gray-900">{title}</h1>
      <div className="flex items-center gap-3">
        <div className="relative hidden sm:block">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search exercises…"
            className="w-56 rounded-lg border border-gray-300 bg-gray-50 py-2 pl-9 pr-3 text-sm text-gray-900 placeholder:text-gray-400 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                const q = (e.target as HTMLInputElement).value.trim();
                if (q) router.push(`/exercises?search=${encodeURIComponent(q)}`);
              }
            }}
          />
        </div>
      </div>
    </header>
  );
}

import { NextRequest, NextResponse } from 'next/server';

const PUBLIC_PATHS  = ['/login', '/public'];
const API_PATHS     = ['/api'];

export function middleware(req: NextRequest) {
  const { pathname } = req.nextUrl;

  // Allow public routes and API routes through
  if (
    PUBLIC_PATHS.some((p) => pathname.startsWith(p)) ||
    API_PATHS.some((p)    => pathname.startsWith(p)) ||
    pathname === '/_next' ||
    pathname.startsWith('/_next/')
  ) {
    return NextResponse.next();
  }

  // Check for auth token in cookie (set by client after login)
  // The real token is stored in localStorage (client-side only).
  // For SSR protection we rely on a lightweight `auth` cookie that the
  // client sets after successful login.
  const authCookie = req.cookies.get('grammarai_auth');
  if (!authCookie?.value && pathname !== '/login') {
    return NextResponse.redirect(new URL('/login', req.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/((?!_next/static|_next/image|favicon.ico).*)'],
};

import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

/**
 * Route guard for /app/** — intercepts every request under the app prefix.
 * Auth check: verify erp_token cookie; redirect to /login when absent.
 * Login page not yet implemented — pass-through until ready.
 */
export function middleware(request: NextRequest) {
  // TODO: uncomment when /login page is implemented
  // const token = request.cookies.get('erp_token');
  // if (!token) {
  //   const loginUrl = new URL('/login', request.url);
  //   loginUrl.searchParams.set('next', request.nextUrl.pathname);
  //   return NextResponse.redirect(loginUrl);
  // }
  return NextResponse.next();
}

export const config = {
  matcher: '/app/:path*',
};

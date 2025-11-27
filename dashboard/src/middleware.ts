import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export function middleware(request: NextRequest) {
    // We can't easily access local storage or zustand store in middleware
    // So we rely on cookies if available, or client-side checks.
    // For now, we'll just pass through and let the client-side auth guard handle it
    // or if we had an auth cookie, we would check it here.

    // Since we are using a client-side only auth (token in local storage via zustand persist),
    // we can't do true server-side protection in middleware without cookies.
    // However, we can protect routes by checking if the user is visiting a protected page
    // and redirecting if we know they are not logged in (which we don't know here).

    // A common pattern with JWT in localStorage is to use a client-side AuthGuard component.
    // But we can at least ensure we don't redirect endlessly.

    return NextResponse.next();
}

export const config = {
    matcher: [
        /*
         * Match all request paths except for the ones starting with:
         * - api (API routes)
         * - _next/static (static files)
         * - _next/image (image optimization files)
         * - favicon.ico (favicon file)
         * - login
         * - register
         */
        '/((?!api|_next/static|_next/image|favicon.ico|login|register).*)',
    ],
};

'use client';

import { useEffect, useState } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import { useAuthStore } from '@/stores/auth-store';

export function AuthGuard({ children }: { children: React.ReactNode }) {
    const router = useRouter();
    const pathname = usePathname();
    const { isAuthenticated, accessToken, _hasHydrated } = useAuthStore();
    const [authorized, setAuthorized] = useState(false);

    useEffect(() => {
        // Wait for hydration
        if (!_hasHydrated) return;

        // Check if user is authenticated
        if (!isAuthenticated || !accessToken) {
            // Redirect to login page
            router.push('/login');
            setAuthorized(false);
        } else {
            setAuthorized(true);
        }
    }, [isAuthenticated, accessToken, _hasHydrated, router, pathname]);

    // Show nothing while checking auth or if not authorized
    if (!authorized) {
        return null;
    }

    return <>{children}</>;
}

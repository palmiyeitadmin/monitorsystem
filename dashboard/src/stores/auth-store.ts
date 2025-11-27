'use client';

import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { User } from '@/types';
import { apiClient } from '@/lib/api/client';

interface AuthState {
    user: User | null;
    accessToken: string | null;
    refreshToken: string | null;
    isAuthenticated: boolean;
    setAuth: (user: User, accessToken: string, refreshToken: string) => void;
    logout: () => Promise<void>;
    updateUser: (user: User) => void;
    _hasHydrated: boolean;
    setHasHydrated: (state: boolean) => void;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            user: null,
            accessToken: null,
            refreshToken: null,
            isAuthenticated: false,
            _hasHydrated: false,
            setHasHydrated: (state) => set({ _hasHydrated: state }),
            setAuth: (user, accessToken, refreshToken) => {
                apiClient.setAccessToken(accessToken);
                set({ user, accessToken, refreshToken, isAuthenticated: true });
            },
            logout: async () => {
                // Clear local state
                set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false });
                apiClient.setAccessToken(null);
                // Optional: Call API to revoke token
            },
            updateUser: (user) => set({ user }),
        }),
        {
            name: 'auth-storage',
            onRehydrateStorage: () => (state) => {
                if (state?.accessToken) {
                    apiClient.setAccessToken(state.accessToken);
                }
                state?.setHasHydrated(true);
            },
        }
    )
);

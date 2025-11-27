'use client';

import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import { apiClient, api } from '@/lib/api';

interface User {
    id: string;
    email: string;
    fullName: string;
    role: string;
    organizationId: string;
}

interface AuthContextType {
    user: User | null;
    loading: boolean;
    login: (email: string, password: string) => Promise<void>;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<User | null>(null);
    const [loading, setLoading] = useState(true);
    const router = useRouter();
    const pathname = usePathname();

    useEffect(() => {
        checkAuth();
    }, []);

    const checkAuth = async () => {
        const token = localStorage.getItem('token');
        if (!token) {
            setLoading(false);
            if (!pathname?.startsWith('/login')) {
                router.push('/login');
            }
            return;
        }

        try {
            const userData = await api.auth.me();
            setUser(userData);
        } catch (error: any) {
            console.error('Auth check failed:', error);
            if (error?.status === 401 || error?.response?.status === 401) {
                localStorage.removeItem('token');
                if (!pathname?.startsWith('/login')) {
                    router.push('/login');
                }
            }
            // For other errors (e.g. 500), do not logout, just stop loading.
            // The user might still be authenticated but the server is having issues.
        } finally {
            setLoading(false);
        }
    };

    const login = async (email: string, password: string) => {
        const response = await api.auth.login(email, password);
        apiClient.setToken(response.token);
        setUser(response.user);
        router.push('/');
    };

    const logout = () => {
        apiClient.setToken(null);
        setUser(null);
        router.push('/login');
    };

    return (
        <AuthContext.Provider value={{ user, loading, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
}

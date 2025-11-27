import { useAuthStore } from '@/stores/auth-store';

export function useAuth() {
    const { user, isAuthenticated, accessToken, setAuth, logout, updateUser } = useAuthStore();

    return {
        user,
        isAuthenticated,
        accessToken,
        setAuth,
        logout,
        updateUser,
        isAdmin: user?.role === 'Admin' || user?.role === 'SuperAdmin',
    };
}

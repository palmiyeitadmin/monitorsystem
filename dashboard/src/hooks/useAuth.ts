import { useMutation } from '@tanstack/react-query';
import { api, apiClient } from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';
import { LoginResponse } from '@/types';
import { useRouter } from 'next/navigation';

export const useAuth = () => {
    const setAuth = useAuthStore((state) => state.setAuth);
    const logoutStore = useAuthStore((state) => state.logout);
    const router = useRouter();

    const loginMutation = useMutation({
        mutationFn: async (credentials: { email: string; password: string }) => {
            const result = await api.auth.login(credentials.email, credentials.password);
            const mapped: LoginResponse = {
                accessToken: result.token,
                refreshToken: result.refreshToken,
                user: result.user,
            };
            return mapped;
        },
        onSuccess: (data) => {
            apiClient.setToken(data.accessToken);
            setAuth(data.user, data.accessToken, data.refreshToken);
            router.push('/');
        },
    });

    const logout = () => {
        apiClient.setToken(null);
        logoutStore();
        router.push('/login');
    };

    return {
        login: loginMutation.mutate,
        isLoading: loginMutation.isPending,
        error: loginMutation.error,
        logout,
    };
};

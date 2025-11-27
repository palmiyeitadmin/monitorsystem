import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api, apiClient, Check } from "@/lib/api";

export type { Check };

export const useChecks = () => {
    const queryClient = useQueryClient();

    const checksQuery = useQuery({
        queryKey: ["checks"],
        queryFn: async () => {
            const response = await api.checks.list();
            return (Array.isArray(response) ? response : response?.items) || [];
        },
    });

    const createCheckMutation = useMutation({
        mutationFn: async (newCheck: Partial<Check>) => {
            return api.checks.create(newCheck);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["checks"] });
        },
    });

    return {
        checks: checksQuery.data || [],
        isLoading: checksQuery.isLoading,
        createCheck: createCheckMutation.mutate,
        isCreating: createCheckMutation.isPending,
    };
};

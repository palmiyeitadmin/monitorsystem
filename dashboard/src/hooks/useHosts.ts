import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api, Host } from "@/lib/api";

export type { Host };

export const useHosts = () => {
    const queryClient = useQueryClient();

    const hostsQuery = useQuery({
        queryKey: ["hosts"],
        queryFn: async () => {
            const response = await api.hosts.list();
            return (Array.isArray(response) ? response : response?.items) || [];
        },
    });

    const createHostMutation = useMutation({
        mutationFn: async (newHost: { name: string; ipAddress: string }) => {
            return api.hosts.create(newHost);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["hosts"] });
        },
    });

    const deleteHostMutation = useMutation({
        mutationFn: async (id: string) => {
            await api.hosts.delete(id);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["hosts"] });
        },
    });

    return {
        hosts: hostsQuery.data || [],
        isLoading: hostsQuery.isLoading,
        error: hostsQuery.error,
        createHost: createHostMutation.mutate,
        isCreating: createHostMutation.isPending,
        deleteHost: deleteHostMutation.mutate,
        isDeleting: deleteHostMutation.isPending,
    };
};

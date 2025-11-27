import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api, Incident } from "@/lib/api";

export type { Incident };

export const useIncidents = () => {
    const queryClient = useQueryClient();

    const incidentsQuery = useQuery({
        queryKey: ["incidents"],
        queryFn: async () => {
            const response = await api.incidents.list();
            return (Array.isArray(response) ? response : response?.items) || [];
        },
    });

    const updateIncidentStatusMutation = useMutation({
        mutationFn: async ({ id, status }: { id: string; status: string }) => {
            await api.incidents.update(id, { status });
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["incidents"] });
        },
    });

    return {
        incidents: incidentsQuery.data || [],
        isLoading: incidentsQuery.isLoading,
        updateStatus: updateIncidentStatusMutation.mutate,
    };
};

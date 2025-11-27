import { useQuery } from "@tanstack/react-query";
import { api, Service } from "@/lib/api";

export type { Service };

export const useServices = () => {
    const servicesQuery = useQuery({
        queryKey: ["services"],
        queryFn: async () => {
            const response = await api.services.list();
            return (Array.isArray(response) ? response : response?.items) || [];
        },
    });

    return {
        services: servicesQuery.data || [],
        isLoading: servicesQuery.isLoading,
        error: servicesQuery.error,
    };
};



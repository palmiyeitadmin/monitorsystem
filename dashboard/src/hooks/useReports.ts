import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api";

export interface Report {
    id: string;
    title: string;
    type: "daily" | "weekly" | "monthly" | "custom";
    status: "pending" | "generated" | "failed";
    generatedAt: string | null;
    downloadUrl: string | null;
}

export interface ReportGenerationParams {
    type: Report["type"];
    dateRange?: {
        start: string;
        end: string;
    };
}

export const useReports = () => {
    const queryClient = useQueryClient();

    const reportsQuery = useQuery({
        queryKey: ["reports"],
        queryFn: async () => apiClient.get<Report[]>("/api/reports"),
    });

    const generateReportMutation = useMutation({
        mutationFn: async (params: ReportGenerationParams) => {
            await apiClient.post("/api/reports/generate", params);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["reports"] });
        },
    });

    return {
        reports: reportsQuery.data || [],
        isLoading: reportsQuery.isLoading,
        generateReport: generateReportMutation.mutate,
        isGenerating: generateReportMutation.isPending,
    };
};

import apiClient from './client';
import { Check, CreateCheckRequest, UpdateCheckRequest, CheckResult } from '@/types/check';

export const checksApi = {
    getAll: async (params?: {
        customerId?: string;
        hostId?: string;
        type?: string;
        status?: string;
        search?: string;
    }) => {
        return apiClient.get<Check[]>('/api/checks', params);
    },

    getById: async (id: string) => {
        return apiClient.get<Check>(`/api/checks/${id}`);
    },

    create: async (check: CreateCheckRequest) => {
        return apiClient.post<Check>('/api/checks', check);
    },

    update: async (id: string, check: UpdateCheckRequest) => {
        return apiClient.put<void>(`/api/checks/${id}`, check);
    },

    delete: async (id: string) => {
        return apiClient.delete<void>(`/api/checks/${id}`);
    },

    getResults: async (id: string, limit: number = 50) => {
        return apiClient.get<CheckResult[]>(`/api/checks/${id}/results`, { limit });
    }
};

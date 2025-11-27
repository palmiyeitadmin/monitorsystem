import apiClient from './client';
import type { PagedResponse, PagedRequest } from '@/types/api';
import type { Check, CheckType } from '@/types/check';

export interface CreateCheckRequest {
    name: string;
    target: string;
    checkType: CheckType;
    intervalSeconds: number;
    timeoutSeconds: number;
    locations?: string[];
}

export interface UpdateCheckRequest extends Partial<CreateCheckRequest> {
    enabled?: boolean;
}

export const checksApi = {
    getAll: async (params: PagedRequest & { type?: CheckType }) => {
        return apiClient.get<PagedResponse<Check>>('/api/checks', params);
    },

    getById: async (id: string) => {
        return apiClient.get<Check>(`/api/checks/${id}`);
    },

    create: async (data: CreateCheckRequest) => {
        return apiClient.post<Check>('/api/checks', data);
    },

    update: async (id: string, data: UpdateCheckRequest) => {
        return apiClient.put<Check>(`/api/checks/${id}`, data);
    },

    delete: async (id: string) => {
        return apiClient.delete(`/api/checks/${id}`);
    },
};

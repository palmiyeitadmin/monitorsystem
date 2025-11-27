import { apiClient } from './client';
import type {
    Host,
    HostDetail,
    HostMetrics,
    CreateHostRequest,
    UpdateHostRequest
} from '@/types/host';
import type { PagedResponse, PagedRequest } from '@/types/api';

export const hostsApi = {
    getAll: async (params?: PagedRequest & {
        status?: string;
        customerId?: string;
        osType?: string;
        tags?: string[];
    }): Promise<PagedResponse<Host>> => {
        return apiClient.get('/api/hosts', params);
    },

    getById: async (id: string): Promise<HostDetail> => {
        return apiClient.get(`/api/hosts/${id}`);
    },

    create: async (data: CreateHostRequest): Promise<HostDetail> => {
        return apiClient.post('/api/hosts', data);
    },

    update: async (id: string, data: UpdateHostRequest): Promise<HostDetail> => {
        return apiClient.put(`/api/hosts/${id}`, data);
    },

    delete: async (id: string): Promise<void> => {
        return apiClient.delete(`/api/hosts/${id}`);
    },

    regenerateApiKey: async (id: string): Promise<{ data: string }> => {
        return apiClient.post(`/api/hosts/${id}/regenerate-api-key`);
    },

    setMaintenance: async (id: string, data: {
        enable: boolean;
        startAt?: string;
        endAt?: string;
        reason?: string;
    }): Promise<HostDetail> => {
        return apiClient.post(`/api/hosts/${id}/maintenance`, data);
    },

    toggleMonitoring: async (id: string): Promise<{ data: boolean }> => {
        return apiClient.post(`/api/hosts/${id}/toggle-monitoring`);
    },

    getMetrics: async (id: string, params?: {
        from?: string;
        to?: string;
        interval?: string;
    }): Promise<HostMetrics> => {
        return apiClient.get(`/api/hosts/${id}/metrics`, params);
    },

    getStatusCounts: async (): Promise<Record<string, number>> => {
        return apiClient.get('/api/hosts/status-counts');
    },

    getRecentlyDown: async (limit?: number): Promise<Host[]> => {
        return apiClient.get('/api/hosts/recently-down', { limit });
    },

    getHighUsage: async (limit?: number): Promise<Host[]> => {
        return apiClient.get('/api/hosts/high-usage', { limit });
    },
};

import apiClient from './client';
import type { PagedResponse, PagedRequest } from '@/types/api';
import type { StatusType } from '@/types';

export interface Service {
    id: string;
    hostId: string;
    hostName: string;
    serviceName: string;
    displayName: string;
    startupType: string;
    currentStatus: StatusType;
    lastStatusChange?: string;
    monitoringEnabled: boolean;
}

export interface ServiceActionRequest {
    action: 'Start' | 'Stop' | 'Restart';
}

export const servicesApi = {
    getAll: async (params: PagedRequest & { hostId?: string; status?: StatusType }) => {
        return apiClient.get<PagedResponse<Service>>('/api/services', params);
    },

    getById: async (id: string) => {
        return apiClient.get<Service>(`/api/services/${id}`);
    },

    performAction: async (id: string, action: ServiceActionRequest['action']) => {
        return apiClient.post(`/api/services/${id}/action`, { action });
    },
};

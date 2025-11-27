import apiClient from './client';
import type { PagedResponse } from '@/types/api';

export interface EventLog {
    id: string;
    hostId: string;
    logName: string;
    eventId: number;
    level: string;
    source: string;
    category: string;
    message: string;
    timeCreated: string;
    recordedAt: string;
}

export interface EventLogsParams {
    page: number;
    pageSize: number;
    category?: string;
    level?: string;
    fromDate?: string;
    toDate?: string;
    search?: string;
}

export const eventLogsApi = {
    getAll: async (hostId: string, params: EventLogsParams) => {
        return apiClient.get<PagedResponse<EventLog>>(
            `/api/hosts/${hostId}/eventlogs`,
            params
        );
    },

    getCategories: async (hostId: string) => {
        return apiClient.get<string[]>(`/api/hosts/${hostId}/eventlogs/categories`);
    },
};

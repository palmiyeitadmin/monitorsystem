import { apiClient } from './client';
import type {
    Incident,
    IncidentDetail,
    CreateIncidentRequest,
    AddCommentRequest,
    ResolveIncidentRequest,
    IncidentTimelineItem
} from '@/types/incident';
import type { PagedResponse, PagedRequest } from '@/types/api';

export const incidentsApi = {
    getAll: async (params?: PagedRequest & {
        status?: string;
        severity?: string;
        customerId?: string;
        assignedToId?: string;
        isOpen?: boolean;
    }): Promise<PagedResponse<Incident>> => {
        return apiClient.get('/api/incidents', params);
    },

    getById: async (id: string): Promise<IncidentDetail> => {
        return apiClient.get(`/api/incidents/${id}`);
    },

    create: async (data: CreateIncidentRequest): Promise<IncidentDetail> => {
        return apiClient.post('/api/incidents', data);
    },

    update: async (id: string, data: Partial<CreateIncidentRequest>): Promise<IncidentDetail> => {
        return apiClient.put(`/api/incidents/${id}`, data);
    },

    acknowledge: async (id: string, comment?: string): Promise<IncidentDetail> => {
        return apiClient.post(`/api/incidents/${id}/acknowledge`, { comment });
    },

    assign: async (id: string, assignToUserId: string, comment?: string): Promise<IncidentDetail> => {
        return apiClient.post(`/api/incidents/${id}/assign`, { assignToUserId, comment });
    },

    updateStatus: async (id: string, status: string, comment?: string): Promise<IncidentDetail> => {
        return apiClient.post(`/api/incidents/${id}/status`, { status, comment });
    },

    resolve: async (id: string, data: ResolveIncidentRequest): Promise<IncidentDetail> => {
        return apiClient.post(`/api/incidents/${id}/resolve`, data);
    },

    close: async (id: string): Promise<IncidentDetail> => {
        return apiClient.post(`/api/incidents/${id}/close`);
    },

    reopen: async (id: string, reason?: string): Promise<IncidentDetail> => {
        return apiClient.post(`/api/incidents/${id}/reopen`, undefined, { reason });
    },

    escalate: async (id: string, data: {
        newSeverity: string;
        assignToUserId?: string;
        reason: string;
    }): Promise<IncidentDetail> => {
        return apiClient.post(`/api/incidents/${id}/escalate`, data);
    },

    addComment: async (id: string, data: AddCommentRequest): Promise<IncidentTimelineItem> => {
        return apiClient.post(`/api/incidents/${id}/comments`, data);
    },

    getTimeline: async (id: string, includeInternal?: boolean): Promise<IncidentTimelineItem[]> => {
        return apiClient.get(`/api/incidents/${id}/timeline`, { includeInternal });
    },

    getStatusCounts: async (): Promise<Record<string, number>> => {
        return apiClient.get('/api/incidents/status-counts');
    },

    getSeverityCounts: async (): Promise<Record<string, number>> => {
        return apiClient.get('/api/incidents/severity-counts');
    },

    getStatistics: async (from?: string, to?: string): Promise<{
        totalIncidents: number;
        openIncidents: number;
        resolvedIncidents: number;
        avgTimeToAcknowledgeMinutes: number;
        avgTimeToResolveMinutes: number;
        slaBreaches: number;
    }> => {
        return apiClient.get('/api/incidents/statistics', { from, to });
    },
};

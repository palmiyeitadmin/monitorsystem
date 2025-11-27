import axios, { AxiosInstance } from 'axios';

const baseURL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

// Create axios instance
const axiosInstance: AxiosInstance = axios.create({
    baseURL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// API Client wrapper
class ApiClient {
    private axios: AxiosInstance;

    constructor(instance: AxiosInstance) {
        this.axios = instance;

        // Setup request interceptor
        this.axios.interceptors.request.use(
            (config) => {
                const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;
                if (token) {
                    config.headers.Authorization = `Bearer ${token}`;
                }
                return config;
            },
            (error) => Promise.reject(error)
        );

        // Setup response interceptor
        this.axios.interceptors.response.use(
            (response) => response.data,
            async (error) => {
                if (error.response?.status === 401) {
                    this.setToken(null);
                    if (typeof window !== 'undefined') {
                        window.location.href = '/login';
                    }
                }
                return Promise.reject(error.response?.data || error);
            }
        );
    }

    setToken(token: string | null) {
        if (typeof window !== 'undefined') {
            if (token) {
                localStorage.setItem('token', token);
            } else {
                localStorage.removeItem('token');
            }
        }
    }

    get<T>(url: string, params?: any): Promise<T> {
        return this.axios.get(url, { params });
    }

    post<T>(url: string, data?: any): Promise<T> {
        return this.axios.post(url, data);
    }

    put<T>(url: string, data?: any): Promise<T> {
        return this.axios.put(url, data);
    }

    delete<T>(url: string): Promise<T> {
        return this.axios.delete(url);
    }
}

export const apiClient = new ApiClient(axiosInstance);

// API endpoints
export const api = {
    auth: {
        login: async (email: string, password: string) => {
            const res = await apiClient.post<LoginApiResponse>('/api/auth/login', { email, password });
            return {
                token: res.accessToken,
                refreshToken: res.refreshToken,
                user: res.user,
            };
        },
        logout: () => apiClient.post('/api/auth/logout'),
        me: () => apiClient.get<any>('/api/auth/me'),
    },

    dashboard: {
        getSummary: () => apiClient.get<DashboardSummary>('/api/dashboard/summary'),
        getHealth: () => apiClient.get<SystemHealth>('/api/dashboard/health'),
    },

    hosts: {
        list: (params?: PagedRequest) => apiClient.get<PagedResponse<Host>>('/api/hosts', params),
        get: (id: string) => apiClient.get<Host>(`/api/hosts/${id}`),
        create: (data: CreateHostRequest) => apiClient.post<Host>('/api/hosts', data),
        update: (id: string, data: UpdateHostRequest) => apiClient.put<Host>(`/api/hosts/${id}`, data),
        delete: (id: string) => apiClient.delete(`/api/hosts/${id}`),
    },

    incidents: {
        list: (params?: PagedRequest) => apiClient.get<PagedResponse<Incident>>('/api/incidents', params),
        get: (id: string) => apiClient.get<Incident>(`/api/incidents/${id}`),
        update: (id: string, data: any) => apiClient.put<Incident>(`/api/incidents/${id}`, data),
    },

    customers: {
        list: () => apiClient.get<Customer[]>('/api/customers'),
    },

    locations: {
        list: () => apiClient.get<Location[]>('/api/locations'),
    },

    services: {
        list: (params?: PagedRequest) => apiClient.get<PagedResponse<Service>>('/api/services', params),
    },

    checks: {
        list: (params?: PagedRequest) => apiClient.get<PagedResponse<Check>>('/api/checks', params),
        create: (data: Partial<Check>) => apiClient.post<Check>('/api/checks', data),
    },

    notifications: {
        list: () => apiClient.get<Notification[]>('/api/notifications'),
    },
};

// TypeScript types
export interface PagedRequest {
    page?: number;
    pageSize?: number;
    search?: string;
    sortBy?: string;
    sortOrder?: 'asc' | 'desc';
}

export interface PagedResponse<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
}

export interface DashboardSummary {
    totalHosts: number;
    hostsUp: number;
    hostsDown: number;
    hostsWarning: number;
    totalServices: number;
    servicesHealthy: number;
    totalChecks: number;
    checksOk: number;
    checksFailing: number;
    totalIncidents: number;
    incidentsOpen: number;
    avgResponseTime: number;
    uptime30d: number;
}

export interface SystemHealth {
    status: string;
    lastUpdated: string;
}

export interface Host {
    id: string;
    name: string;
    hostname?: string;
    osType: string;
    category: string;
    tags?: string[];
    primaryIp?: string;
    publicIp?: string;
    currentStatus: string;
    statusDisplay?: string;
    lastSeenAt?: string | null;
    monitoringEnabled?: boolean;
    maintenanceMode?: boolean;
    apiKey?: string;
    uptimeSeconds?: number;
    cpuPercent?: number;
    ramPercent?: number;
    customer?: { id: string; name: string };
    location?: { id: string; name: string; city?: string; country?: string };
    metrics?: {
        cpuPercent?: number;
        ramPercent?: number;
        uptimeSeconds?: number;
    };
}

export interface Incident {
    id: string;
    title: string;
    description?: string;
    severity: string;
    status: string;
    assignedToName?: string;
    hostName?: string;
    createdAt: string;
    resolvedAt?: string;
}

export interface Customer {
    id: string;
    name: string;
    contactName?: string;
    contactEmail?: string;
    contactPhone?: string;
    city?: string;
    country?: string;
    industry?: string;
}

export interface Service {
    id: string;
    serviceName?: string;
    displayName?: string;
    serviceType?: string;
    currentStatus?: string;
    status?: string;
    hostId?: string;
    hostName?: string;
}

export interface Check {
    id: string;
    name: string;
    type: string;
    status: string;
    url?: string;
    target?: string;
    intervalSeconds?: number;
    lastRun?: string | null;
    hostId?: string;
}

export interface CreateHostRequest {
    name: string;
    ipAddress: string;
    osType?: string;
    category?: string;
    customerId?: string;
    locationId?: string;
}

export interface UpdateHostRequest extends Partial<CreateHostRequest> { }

export interface Location {
    id: string;
    name: string;
    category: string;
    providerName?: string;
    city?: string;
    country?: string;
    isActive: boolean;
}

export interface Notification {
    id: string;
    channel: string;
    recipient: string;
    subject: string;
    status: string;
    createdAt: string;
    sentAt?: string;
    errorMessage?: string;
    retryCount: number;
}

export interface LoginApiResponse {
    accessToken: string;
    refreshToken: string;
    user: any;
}

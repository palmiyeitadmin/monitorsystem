import * as signalR from '@microsoft/signalr';

const HUB_URL = process.env.NEXT_PUBLIC_SIGNALR_URL || 'http://localhost:5000/hubs/monitoring';

class SignalRService {
    private connection: signalR.HubConnection | null = null;
    private reconnectAttempts = 0;
    private maxReconnectAttempts = 10;
    private listeners: Map<string, Set<(...args: unknown[]) => void>> = new Map();

    async connect(accessToken?: string): Promise<void> {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            return;
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(HUB_URL, {
                accessTokenFactory: () => accessToken || '',
                withCredentials: true,
            })
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: (retryContext) => {
                    if (retryContext.previousRetryCount >= this.maxReconnectAttempts) {
                        return null;
                    }
                    return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
                },
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.setupConnectionEvents();
        await this.startConnection();
    }

    private setupConnectionEvents(): void {
        if (!this.connection) return;

        this.connection.onreconnecting((error) => {
            console.log('SignalR reconnecting...', error);
            this.reconnectAttempts++;
        });

        this.connection.onreconnected((connectionId) => {
            console.log('SignalR reconnected', connectionId);
            this.reconnectAttempts = 0;
            this.resubscribeAll();
        });

        this.connection.onclose((error) => {
            console.log('SignalR connection closed', error);
        });
    }

    private async startConnection(): Promise<void> {
        try {
            await this.connection?.start();
            console.log('SignalR connected');
            this.reconnectAttempts = 0;
        } catch (error) {
            console.error('SignalR connection error:', error);
            throw error;
        }
    }

    async disconnect(): Promise<void> {
        if (this.connection) {
            await this.connection.stop();
            this.connection = null;
        }
    }

    // Subscribe to groups
    async joinDashboardGroup(): Promise<void> {
        await this.connection?.invoke('JoinDashboardGroup');
    }

    async leaveDashboardGroup(): Promise<void> {
        await this.connection?.invoke('LeaveDashboardGroup');
    }

    async joinHostGroup(hostId: string): Promise<void> {
        await this.connection?.invoke('JoinHostGroup', hostId);
    }

    async leaveHostGroup(hostId: string): Promise<void> {
        await this.connection?.invoke('LeaveHostGroup', hostId);
    }

    async joinCustomerGroup(customerId: string): Promise<void> {
        await this.connection?.invoke('JoinCustomerGroup', customerId);
    }

    async leaveCustomerGroup(customerId: string): Promise<void> {
        await this.connection?.invoke('LeaveCustomerGroup', customerId);
    }

    // Event handlers
    on<T>(event: string, callback: (data: T) => void): () => void {
        if (!this.listeners.has(event)) {
            this.listeners.set(event, new Set());
        }

        const wrappedCallback = callback as (...args: unknown[]) => void;
        this.listeners.get(event)!.add(wrappedCallback);
        this.connection?.on(event, wrappedCallback);

        return () => {
            this.listeners.get(event)?.delete(wrappedCallback);
            this.connection?.off(event, wrappedCallback);
        };
    }

    onHostUpdate(callback: (data: HostUpdateEvent) => void): () => void {
        return this.on('HostUpdate', callback);
    }

    onServiceUpdate(callback: (data: ServiceUpdateEvent) => void): () => void {
        return this.on('ServiceUpdate', callback);
    }

    onCheckResult(callback: (data: CheckResultEvent) => void): () => void {
        return this.on('CheckResult', callback);
    }

    onIncidentCreated(callback: (data: IncidentEvent) => void): () => void {
        return this.on('IncidentCreated', callback);
    }

    onIncidentUpdated(callback: (data: IncidentEvent) => void): () => void {
        return this.on('IncidentUpdated', callback);
    }

    private resubscribeAll(): void {
        this.listeners.forEach((callbacks, event) => {
            callbacks.forEach((callback) => {
                this.connection?.on(event, callback);
            });
        });
    }
}

// Event types
export interface HostUpdateEvent {
    hostId: string;
    hostName: string;
    currentStatus: string;
    cpuPercent: number;
    ramPercent: number;
    lastSeenAt: string;
    statusChangedAt?: string;
}

export interface ServiceUpdateEvent {
    serviceId: string;
    serviceName: string;
    hostId: string;
    currentStatus: string;
    displayName: string;
}

export interface CheckResultEvent {
    checkId: string;
    checkName: string;
    checkType: string;
    currentStatus: string;
    responseTimeMs: number;
    lastCheckAt: string;
    success: boolean;
    errorMessage?: string;
}

export interface IncidentEvent {
    incidentId: string;
    incidentNumber: string;
    title: string;
    severity: string;
    status: string;
    isAutoCreated?: boolean;
    updatedAt?: string;
}

export const signalRService = new SignalRService();
export default signalRService;

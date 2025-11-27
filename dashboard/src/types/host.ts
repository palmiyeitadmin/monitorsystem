import { StatusType, OsType } from './common';

export interface Host {
    id: string;
    name: string;
    hostname: string;
    description?: string;
    osType: OsType;
    osVersion?: string;
    primaryIp?: string;
    publicIp?: string;
    currentStatus: StatusType;
    lastSeenAt?: string;
    statusChangedAt?: string;
    cpuPercent?: number;
    ramPercent?: number;
    monitoringEnabled: boolean;
    maintenanceMode: boolean;
    tags: string[];
    customerId?: string;
    customerName?: string;
    locationId?: string;
    locationName?: string;
    createdAt: string;
    updatedAt: string;
}

export interface HostDetail extends Host {
    apiKey: string;
    agentVersion?: string;
    checkIntervalSeconds: number;
    metrics: HostCurrentMetrics;
    thresholds: HostThresholds;
    monitoringSettings: HostMonitoringSettings;
    disks: HostDisk[];
    services: HostService[];
    statistics: HostStatistics;
}

export interface HostCurrentMetrics {
    cpuPercent?: number;
    ramPercent?: number;
    ramUsedMb?: number;
    ramTotalMb?: number;
    uptimeSeconds?: number;
    processCount?: number;
    uptimeDisplay?: string;
}

export interface HostThresholds {
    cpuWarning: number;
    cpuCritical: number;
    ramWarning: number;
    ramCritical: number;
    diskWarning: number;
    diskCritical: number;
}

export interface HostMonitoringSettings {
    alertOnDown: boolean;
    alertDelaySeconds: number;
    alertOnHighCpu: boolean;
    alertOnHighRam: boolean;
    alertOnHighDisk: boolean;
}

export interface HostDisk {
    id: string;
    name: string;
    mountPoint: string;
    fileSystem: string;
    totalGb: number;
    usedGb: number;
    freeGb: number;
    usedPercent: number;
}

export interface HostService {
    id: string;
    serviceName: string;
    displayName: string;
    serviceType: string;
    currentStatus: StatusType;
    monitoringEnabled: boolean;
}

export interface HostStatistics {
    uptime24h: number;
    uptime7d: number;
    uptime30d: number;
    uptimePercentage30d: number;
    avgCpu24h: number;
    avgRam24h: number;
    incidentCount30d: number;
}

export interface HostMetrics {
    hostId: string;
    cpuHistory: MetricDataPoint[];
    ramHistory: MetricDataPoint[];
    diskHistory: MetricDataPoint[];
    networkHistory: MetricDataPoint[];
}

export interface MetricDataPoint {
    timestamp: string;
    value: number;
}

export interface CreateHostRequest {
    name: string;
    hostname: string;
    description?: string;
    osType: OsType;
    customerId?: string;
    locationId?: string;
    tags?: string[];
    checkIntervalSeconds?: number;
    monitoringEnabled?: boolean;
}

export interface UpdateHostRequest extends CreateHostRequest {
    thresholds?: HostThresholds;
    monitoringSettings?: HostMonitoringSettings;
}

import { StatusType } from './common';
export type CheckType = 'HTTP' | 'TCP' | 'Ping' | 'DNS';

export interface Check {
    id: string;
    name: string;
    checkType: CheckType;
    target: string;
    currentStatus: StatusType;
    lastCheckTime?: string;
    lastResponseTimeMs?: number;
    lastError?: string;
    monitoringEnabled: boolean;
    intervalSeconds: number;
    sslExpiresAt?: string;
    sslDaysUntilExpiry?: number;
    customerId?: string;
    customerName?: string;
    tags: string[];
    uptime24h?: number;
    uptime7d?: number;
    createdAt: string;
}

export interface CheckDetail extends Check {
    description?: string;
    httpSettings?: HttpCheckSettings;
    tcpSettings?: TcpCheckSettings;
    dnsSettings?: DnsCheckSettings;
    sslSettings?: SslCheckSettings;
    timeoutSeconds: number;
    retryCount: number;
    retryDelaySeconds: number;
    failureThreshold: number;
    currentFailureCount: number;
    sslInfo?: SslInfo;
    statistics: CheckStatistics;
}

export interface HttpCheckSettings {
    method: string;
    headers?: Record<string, string>;
    body?: string;
    expectedStatusCodes: number[];
    expectedKeyword?: string;
    keywordShouldExist: boolean;
}

export interface TcpCheckSettings {
    port: number;
}

export interface DnsCheckSettings {
    recordType: string;
    expectedResult?: string;
}

export interface SslCheckSettings {
    enabled: boolean;
    warningDays: number;
    criticalDays: number;
}

export interface SslInfo {
    expiresAt?: string;
    daysUntilExpiry?: number;
    issuer?: string;
    subject?: string;
    status: string;
}

export interface CheckStatistics {
    uptime24h: number;
    uptime7d: number;
    uptime30d: number;
    avgResponseTimeMs: number;
    minResponseTimeMs: number;
    maxResponseTimeMs: number;
    checksLast24h: number;
    failuresLast24h: number;
    uptimePercentage24h: number;
    uptimePercentage7d: number;
    uptimePercentage30d: number;
}

export interface CheckResult {
    id: number;
    success: boolean;
    status: StatusType;
    responseTimeMs: number;
    checkedAt: string;
    httpStatusCode?: number;
    httpStatusMessage?: string;
    keywordFound?: boolean;
    sslValid?: boolean;
    sslDaysUntilExpiry?: number;
    dnsResult?: string;
    errorMessage?: string;
    errorType?: string;
}

export interface CreateCheckRequest {
    name: string;
    type: CheckType;
    target: string;
    description?: string;
    customerId?: string;
    hostId?: string;
    tags?: string[];
    httpMethod?: string;
    httpHeaders?: Record<string, string>;
    expectedStatusCodes?: number[];
    expectedKeyword?: string;
    checkSsl?: boolean;
    sslExpiryWarningDays?: number;
    sslExpiryCriticalDays?: number;
    tcpPort?: number;
    dnsRecordType?: string;
    expectedDnsResult?: string;
    intervalSeconds?: number;
    timeoutSeconds?: number;
    retryCount?: number;
    failureThreshold?: number;
    monitoringEnabled?: boolean;
    alertOnFailure?: boolean;
    alertOnSslExpiry?: boolean;
}

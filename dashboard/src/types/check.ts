export interface Check {
    id: string;
    name: string;
    checkType: 'HTTP' | 'TCP' | 'Ping' | 'DNS';
    target: string;
    intervalSeconds: number;
    timeoutSeconds: number;
    monitoringEnabled: boolean;
    currentStatus: 'Up' | 'Down' | 'Degraded' | 'Unknown';
    lastCheckAt?: string;
    lastResponseTimeMs?: number;
    lastStatusCode?: number;
    lastErrorMessage?: string;
    monitorSsl: boolean;
    sslDaysRemaining?: number;
    hostId?: string;
    customerId?: string;

    // Extended properties
    httpMethod?: string;
    expectedStatusCode?: number;
    expectedKeyword?: string;
    keywordShouldExist?: boolean;
    sslExpiryWarningDays?: number;
    tcpPort?: number;
    history?: CheckResult[];
}

export interface CreateCheckRequest {
    name: string;
    checkType: 'HTTP' | 'TCP' | 'Ping' | 'DNS';
    target: string;
    intervalSeconds: number;
    timeoutSeconds: number;
    monitoringEnabled: boolean;

    // HTTP
    httpMethod?: string;
    requestBody?: string;
    requestHeaders?: Record<string, string>;
    expectedStatusCode?: number;
    expectedKeyword?: string;
    keywordShouldExist?: boolean;
    monitorSsl?: boolean;
    sslExpiryWarningDays?: number;

    // TCP
    tcpPort?: number;

    hostId?: string;
    customerId?: string;
}

export interface UpdateCheckRequest extends Partial<CreateCheckRequest> { }

export interface CheckResult {
    id: string;
    status: string;
    responseTimeMs?: number;
    statusCode?: number;
    errorMessage?: string;
    checkedAt: string;
}

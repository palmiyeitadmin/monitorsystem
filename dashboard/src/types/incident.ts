export type IncidentStatus = 'New' | 'Acknowledged' | 'InProgress' | 'Resolved' | 'Closed';
export type IncidentSeverity = 'Critical' | 'High' | 'Medium' | 'Low' | 'Info';

export interface Incident {
    id: string;
    incidentId: string;
    title: string;
    severity: IncidentSeverity;
    status: IncidentStatus;
    category?: string;
    isAutoCreated: boolean;
    sourceType?: string;
    sourceName?: string;
    assignedTo?: UserSummary;
    slaResponseBreached: boolean;
    slaResolutionBreached: boolean;
    createdAt: string;
    acknowledgedAt?: string;
    resolvedAt?: string;
    duration?: string;
    customerId?: string;
    customerName?: string;
}

export interface IncidentDetail extends Incident {
    description?: string;
    sourceId?: string;
    assignedAt?: string;
    acknowledgedBy?: UserSummary;
    resolvedBy?: UserSummary;
    resolutionNotes?: string;
    rootCause?: string;
    closedBy?: UserSummary;
    closedAt?: string;
    sla: SlaInfo;
    impact?: string;
    affectedUsersCount?: number;
    firstOccurredAt?: string;
    lastOccurredAt?: string;
    occurrenceCount: number;
    resources: IncidentResource[];
    timeline: IncidentTimelineItem[];
    priorityScore: number;
}

export interface SlaInfo {
    responseDue?: string;
    resolutionDue?: string;
    responseBreached: boolean;
    resolutionBreached: boolean;
    timeToResponseDue?: string;
    timeToResolutionDue?: string;
}

export interface IncidentResource {
    id: string;
    resourceType: string;
    resourceId: string;
    resourceName: string;
}

export interface IncidentTimelineItem {
    id: string;
    eventType: string;
    title?: string;
    content?: string;
    oldValue?: string;
    newValue?: string;
    isInternal: boolean;
    isSystemGenerated: boolean;
    user?: UserSummary;
    createdAt: string;
}

export interface UserSummary {
    id: string;
    fullName: string;
    avatarUrl?: string;
}

export interface CreateIncidentRequest {
    title: string;
    description?: string;
    severity: IncidentSeverity;
    category?: string;
    customerId?: string;
    assignedToId?: string;
    impact?: string;
    affectedUsersCount?: number;
    resources?: CreateIncidentResourceRequest[];
}

export interface CreateIncidentResourceRequest {
    resourceType: string;
    resourceId: string;
    resourceName: string;
}

export interface AddCommentRequest {
    content: string;
    isInternal?: boolean;
}

export interface ResolveIncidentRequest {
    resolutionNotes?: string;
    rootCause?: string;
}

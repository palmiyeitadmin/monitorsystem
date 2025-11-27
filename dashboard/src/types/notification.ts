export type NotificationType = 'Email' | 'SMS' | 'Webhook' | 'Slack' | 'Teams' | 'Telegram';

export interface NotificationChannel {
    id: string;
    name: string;
    type: NotificationType;
    target: string;
    isEnabled: boolean;
    isVerified: boolean;
    config?: Record<string, string>;
    createdAt: string;
}

export interface NotificationRule {
    id: string;
    name: string;
    isEnabled: boolean;
    channels: string[]; // Channel IDs
    events: string[]; // Event types (e.g., 'HostDown', 'IncidentCreated')
    severity: string[]; // Severities to alert on
    tags: string[]; // Tags to filter by
    customerId?: string;
}

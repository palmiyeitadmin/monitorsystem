'use client';

import { useEffect, useState } from 'react';

interface NotificationData {
    id: string;
    timestamp: Date;
    channel: 'email' | 'sms' | 'telegram' | 'webhook' | 'slack';
    recipient: string;
    subject: string;
    message: string;
    relatedTo?: string;
    relatedToType?: 'incident' | 'check' | 'deployment';
    status: 'delivered' | 'failed' | 'pending' | 'retrying';
    incidentId?: string;
    checkId?: string;
    deploymentId?: string;
    customer?: string;
    rule?: string;
    deliveryLog?: {
        timestamp: Date;
        status: string;
        message: string;
    }[];
    retryCount?: number;
    maxRetries?: number;
    nextRetry?: Date;
}

export default function NotificationsPage() {
    const [notifications, setNotifications] = useState<NotificationData[]>([
        {
            id: '1',
            timestamp: new Date('2023-10-27T10:45:12Z'),
            channel: 'email',
            recipient: 'admin@example.com',
            subject: 'CRITICAL: Server CPU > 95%',
            message: '[ERA Monitor] CRITICAL: Server CPU utilization has exceeded 95% on server \'prod-db-01\'. Incident #5821 has been created.',
            relatedTo: 'Incident #5821',
            relatedToType: 'incident',
            status: 'delivered',
            incidentId: '5821',
            customer: 'Global Tech Inc.',
            rule: 'Critical CPU Alert',
        },
        {
            id: '2',
            timestamp: new Date('2023-10-27T10:45:10Z'),
            channel: 'sms',
            recipient: '+15551234567',
            subject: 'CRITICAL: Server CPU > 95%',
            message: '[ERA Monitor] CRITICAL: Server CPU utilization has exceeded 95% on server \'prod-db-01\'. Incident #5821 has been created.',
            relatedTo: 'Incident #5821',
            relatedToType: 'incident',
            status: 'failed',
            incidentId: '5821',
            customer: 'Global Tech Inc.',
            rule: 'Critical CPU Alert',
            deliveryLog: [
                { timestamp: new Date('2023-10-27T10:45:10Z'), status: 'attempting', message: 'Attempting delivery...' },
                { timestamp: new Date('2023-10-27T10:45:12Z'), status: 'failed', message: 'Network error: Connection timed out' },
            ],
            retryCount: 1,
            maxRetries: 3,
            nextRetry: new Date(Date.now() + 5 * 60 * 1000),
        },
        {
            id: '3',
            timestamp: new Date('2023-10-27T10:44:55Z'),
            channel: 'telegram',
            recipient: '@oncall_engineer',
            subject: 'WARNING: Latency spike detected',
            message: '[ERA Monitor] WARNING: Latency spike detected on API Gateway. Average response time increased by 150%.',
            relatedTo: 'Check \'API Gateway\'',
            relatedToType: 'check',
            status: 'pending',
            checkId: 'api-gateway-check',
            customer: 'Global Tech Inc.',
            rule: 'Latency Alert',
        },
        {
            id: '4',
            timestamp: new Date('2023-10-27T10:43:01Z'),
            channel: 'webhook',
            recipient: 'https://hooks.slack.com/...',
            subject: 'INFO: Deployment Succeeded',
            message: '[ERA Monitor] INFO: Deployment of API Gateway v2.1.0 completed successfully.',
            relatedTo: 'Deployment #998',
            relatedToType: 'deployment',
            status: 'retrying',
            deploymentId: '998',
            customer: 'Global Tech Inc.',
            rule: 'Deployment Notifications',
            deliveryLog: [
                { timestamp: new Date('2023-10-27T10:43:01Z'), status: 'attempting', message: 'Attempting delivery...' },
            ],
            retryCount: 1,
            maxRetries: 3,
            nextRetry: new Date(Date.now() + 2 * 60 * 1000),
        },
    ]);
    const [loading, setLoading] = useState(false);
    const [searchIncidentId, setSearchIncidentId] = useState('');
    const [searchRecipient, setSearchRecipient] = useState('');
    const [dateRange, setDateRange] = useState('24h');
    const [channelFilter, setChannelFilter] = useState('all');
    const [statusFilter, setStatusFilter] = useState('all');
    const [customerFilter, setCustomerFilter] = useState('all');
    const [selectedNotification, setSelectedNotification] = useState<NotificationData | null>(null);
    const [showDetailsPanel, setShowDetailsPanel] = useState(false);

    const getChannelIcon = (channel: string) => {
        switch (channel?.toLowerCase()) {
            case 'email':
                return 'email';
            case 'sms':
                return 'sms';
            case 'telegram':
                return 'chat';
            case 'webhook':
                return 'api';
            case 'slack':
                return 'forum';
            default:
                return 'notifications';
        }
    };

    const getStatusColor = (status: string) => {
        switch (status?.toLowerCase()) {
            case 'delivered':
                return 'bg-green-500/20 text-green-500 border-green-500/30';
            case 'failed':
                return 'bg-red-500/20 text-red-500 border-red-500/30';
            case 'pending':
                return 'bg-yellow-500/20 text-yellow-500 border-yellow-500/30';
            case 'retrying':
                return 'bg-blue-500/20 text-blue-500 border-blue-500/30';
            default:
                return 'bg-gray-500/20 text-gray-500 border-gray-500/30';
        }
    };

    const getStatusIcon = (status: string) => {
        switch (status?.toLowerCase()) {
            case 'delivered':
                return 'check_circle';
            case 'failed':
                return 'cancel';
            case 'pending':
                return 'hourglass_top';
            case 'retrying':
                return 'autorenew';
            default:
                return 'help';
        }
    };

    const formatTimestamp = (date: Date) => {
        return date.toLocaleString('en-US', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit',
            hour12: false,
        });
    };

    const filteredNotifications = notifications.filter(notification => {
        const matchesIncidentId = !searchIncidentId || notification.incidentId?.includes(searchIncidentId);
        const matchesRecipient = !searchRecipient || notification.recipient.toLowerCase().includes(searchRecipient.toLowerCase());
        const matchesChannel = channelFilter === 'all' || notification.channel === channelFilter;
        const matchesStatus = statusFilter === 'all' || notification.status === statusFilter;
        const matchesCustomer = customerFilter === 'all' || notification.customer === customerFilter;

        return matchesIncidentId && matchesRecipient && matchesChannel && matchesStatus && matchesCustomer;
    });

    const NotificationDetailsPanel = ({ notification, onClose }: { notification: NotificationData; onClose: () => void }) => {
        return (
            <div className="fixed inset-y-0 right-0 w-full max-w-lg bg-[#1b2b32] border-l border-[#365663] shadow-2xl p-8 transform translate-x-0 overflow-y-auto z-50">
                <div className="space-y-8">
                    {/* Header */}
                    <div className="flex justify-between items-start">
                        <div className="flex items-center gap-4">
                            <div className="w-12 h-12 bg-[#28aae2]/20 rounded-lg flex items-center justify-center">
                                <span className="material-symbols-outlined text-[#28aae2] text-3xl">
                                    {getChannelIcon(notification.channel)}
                                </span>
                            </div>
                            <div>
                                <h2 className="text-white text-xl font-bold capitalize">{notification.channel} Notification</h2>
                                <p className="text-gray-400 text-sm">{formatTimestamp(notification.timestamp)}</p>
                            </div>
                        </div>
                        <button onClick={onClose} className="text-gray-400 hover:text-white">
                            <span className="material-symbols-outlined">close</span>
                        </button>
                    </div>

                    {/* Status Badge */}
                    <div className="flex justify-center">
                        <span className={`inline-flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm font-medium ${getStatusColor(notification.status)}`}>
                            <span className="material-symbols-outlined text-base">
                                {getStatusIcon(notification.status)}
                            </span>
                            {notification.status.charAt(0).toUpperCase() + notification.status.slice(1)}
                        </span>
                    </div>

                    {/* Delivery Information */}
                    <div className="space-y-4">
                        <h3 className="text-sm font-bold uppercase tracking-wider text-gray-400">Delivery Information</h3>
                        <div className="grid grid-cols-3 gap-4 text-sm">
                            <div className="text-gray-400">Recipient</div>
                            <div className="col-span-2 text-white font-medium">{notification.recipient}</div>

                            <div className="text-gray-400">Subject</div>
                            <div className="col-span-2 text-white font-medium">{notification.subject}</div>

                            <div className="text-gray-400">Sent At</div>
                            <div className="col-span-2 text-white font-medium">{formatTimestamp(notification.timestamp)}</div>

                            <div className="text-gray-400">Message ID</div>
                            <div className="col-span-2 text-white font-medium truncate">{notification.id}</div>
                        </div>
                    </div>

                    {/* Content Preview */}
                    <div className="space-y-4">
                        <h3 className="text-sm font-bold uppercase tracking-wider text-gray-400">Content Preview</h3>
                        <div className="p-4 bg-[#121d21] border border-[#365663] rounded-lg text-sm text-gray-300">
                            {notification.message}
                        </div>
                    </div>

                    {/* Related Information */}
                    <div className="space-y-4">
                        <h3 className="text-sm font-bold uppercase tracking-wider text-gray-400">Related Information</h3>
                        <div className="grid grid-cols-3 gap-4 text-sm">
                            {notification.relatedToType && (
                                <>
                                    <div className="text-gray-400 capitalize">{notification.relatedToType}</div>
                                    <div className="col-span-2 text-white font-medium text-[#28aae2] hover:underline cursor-pointer">
                                        {notification.relatedTo}
                                    </div>
                                </>
                            )}

                            {notification.rule && (
                                <>
                                    <div className="text-gray-400">Rule</div>
                                    <div className="col-span-2 text-white font-medium text-[#28aae2] hover:underline cursor-pointer">
                                        {notification.rule}
                                    </div>
                                </>
                            )}

                            {notification.customer && (
                                <>
                                    <div className="text-gray-400">Customer</div>
                                    <div className="col-span-2 text-white font-medium text-[#28aae2] hover:underline cursor-pointer">
                                        {notification.customer}
                                    </div>
                                </>
                            )}
                        </div>
                    </div>

                    {/* Delivery Log */}
                    {notification.deliveryLog && notification.deliveryLog.length > 0 && (
                        <div className="space-y-4">
                            <h3 className="text-sm font-bold uppercase tracking-wider text-gray-400">Delivery Log</h3>
                            <div className="space-y-2 text-sm text-gray-400">
                                {notification.deliveryLog.map((log, index) => (
                                    <p key={index}>
                                        <span className="font-medium text-gray-300">[{formatTimestamp(log.timestamp)}] {log.status}:</span> {log.message}
                                    </p>
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Retry Information */}
                    {notification.status === 'failed' && notification.retryCount !== undefined && (
                        <div className="space-y-4">
                            <h3 className="text-sm font-bold uppercase tracking-wider text-gray-400">Retry Information</h3>
                            <div className="grid grid-cols-3 gap-4 text-sm">
                                <div className="text-gray-400">Error</div>
                                <div className="col-span-2 font-medium text-red-500">
                                    {notification.deliveryLog?.[notification.deliveryLog.length - 1]?.message || 'Unknown error'}
                                </div>

                                <div className="text-gray-400">Retry Count</div>
                                <div className="col-span-2 font-medium">
                                    {notification.retryCount} / {notification.maxRetries || 3}
                                </div>

                                <div className="text-gray-400">Next Retry</div>
                                <div className="col-span-2 font-medium">
                                    {notification.nextRetry ? formatTimestamp(notification.nextRetry) : 'N/A'}
                                </div>
                            </div>

                            <div className="flex gap-4">
                                <button className="flex-1 flex items-center justify-center gap-2 h-10 px-4 rounded-lg bg-[#28aae2] hover:bg-[#2196d4] text-[#121d21] text-sm font-bold transition-colors">
                                    <span className="material-symbols-outlined">replay</span>
                                    Retry Now
                                </button>
                                <button className="flex-1 px-4 py-2 bg-[#365663] hover:bg-[#445566] text-white text-sm font-medium rounded-lg transition-colors">
                                    Cancel Retries
                                </button>
                            </div>
                        </div>
                    )}
                </div>
            </div>
        );
    };

    return (
        <>
            {/* Page Header */}
            <div className="flex flex-wrap justify-between items-center gap-4 mb-6">
                <div className="flex flex-col gap-1">
                    <h1 className="text-white text-4xl font-black tracking-[-0.033em]">Notification History</h1>
                    <p className="text-gray-400 text-base">View all sent alerts, notifications, and their delivery status</p>
                </div>
                <button className="flex items-center gap-2 h-10 px-4 rounded-lg bg-[#28aae2] hover:bg-[#2196d4] text-[#121d21] text-sm font-bold transition-colors">
                    <span className="material-symbols-outlined">download</span>
                    <span className="truncate">Export to CSV</span>
                </button>
            </div>

            {/* Stats Cards */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
                <div className="bg-[#1b2b32] rounded-xl p-6 border border-[#365663]">
                    <p className="text-gray-400 text-base mb-2">Total Sent (24h)</p>
                    <div className="flex items-baseline gap-2">
                        <p className="text-white text-3xl font-bold">1,234</p>
                        <div className="flex items-center text-green-500 text-sm font-medium">
                            <span className="material-symbols-outlined text-base">arrow_upward</span>
                            <span>12%</span>
                        </div>
                    </div>
                </div>

                <div className="bg-[#1b2b32] rounded-xl p-6 border border-[#365663]">
                    <p className="text-gray-400 text-base mb-2">Delivered</p>
                    <div className="flex items-baseline gap-2">
                        <p className="text-white text-3xl font-bold">1,150</p>
                        <div className="flex items-center text-green-500 text-sm font-medium">
                            <span className="material-symbols-outlined text-base">arrow_upward</span>
                            <span>15%</span>
                        </div>
                    </div>
                </div>

                <div className="bg-[#1b2b32] rounded-xl p-6 border border-[#365663]">
                    <p className="text-gray-400 text-base mb-2">Failed</p>
                    <div className="flex items-baseline gap-2">
                        <p className="text-white text-3xl font-bold">64</p>
                        <div className="flex items-center text-red-500 text-sm font-medium">
                            <span className="material-symbols-outlined text-base">arrow_downward</span>
                            <span>5%</span>
                        </div>
                    </div>
                </div>

                <div className="bg-[#1b2b32] rounded-xl p-6 border border-[#365663]">
                    <p className="text-gray-400 text-base mb-2">Pending</p>
                    <div className="flex items-baseline gap-2">
                        <p className="text-white text-3xl font-bold">20</p>
                        <div className="flex items-center text-yellow-500 text-sm font-medium">
                            <span className="material-symbols-outlined text-base">arrow_upward</span>
                            <span>2%</span>
                        </div>
                    </div>
                </div>
            </div>

            {/* Filters */}
            <div className="flex flex-wrap gap-3 mb-6">
                <button className="flex h-9 shrink-0 items-center justify-center gap-2 rounded-lg bg-[#1b2b32] border border-[#365663] px-4 text-sm font-medium hover:border-[#28aae2]/50 transition-colors">
                    <span>Date Range: Last 24 hours</span>
                    <span className="material-symbols-outlined text-gray-400">expand_more</span>
                </button>

                <button className="flex h-9 shrink-0 items-center justify-center gap-2 rounded-lg bg-[#1b2b32] border border-[#365663] px-4 text-sm font-medium hover:border-[#28aae2]/50 transition-colors">
                    <span>Channel: All</span>
                    <span className="material-symbols-outlined text-gray-400">expand_more</span>
                </button>

                <button className="flex h-9 shrink-0 items-center justify-center gap-2 rounded-lg bg-[#1b2b32] border border-[#365663] px-4 text-sm font-medium hover:border-[#28aae2]/50 transition-colors">
                    <span>Status: All</span>
                    <span className="material-symbols-outlined text-gray-400">expand_more</span>
                </button>

                <div className="relative">
                    <span className="material-symbols-outlined text-gray-400 absolute left-3 top-1/2 transform -translate-y-1/2">search</span>
                    <input
                        type="text"
                        placeholder="Search by Incident ID"
                        value={searchIncidentId}
                        onChange={(e) => setSearchIncidentId(e.target.value)}
                        className="h-9 w-48 rounded-lg bg-[#1b2b32] border border-[#365663] pl-10 pr-3 text-sm text-white placeholder-gray-400 focus:outline-none focus:border-[#28aae2] transition-colors"
                    />
                </div>

                <div className="relative">
                    <span className="material-symbols-outlined text-gray-400 absolute left-3 top-1/2 transform -translate-y-1/2">search</span>
                    <input
                        type="text"
                        placeholder="Search by Recipient"
                        value={searchRecipient}
                        onChange={(e) => setSearchRecipient(e.target.value)}
                        className="h-9 w-48 rounded-lg bg-[#1b2b32] border border-[#365663] pl-10 pr-3 text-sm text-white placeholder-gray-400 focus:outline-none focus:border-[#28aae2] transition-colors"
                    />
                </div>

                <button className="flex h-9 shrink-0 items-center justify-center gap-2 rounded-lg bg-[#1b2b32] border border-[#365663] px-4 text-sm font-medium hover:border-[#28aae2]/50 transition-colors">
                    <span>Customer: All Customers</span>
                    <span className="material-symbols-outlined text-gray-400">expand_more</span>
                </button>
            </div>

            {/* Notifications Table */}
            <div className="overflow-hidden rounded-xl border border-[#365663] bg-[#121d21]">
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-[#365663]">
                        <thead className="bg-[#1b2b32]/50">
                            <tr>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Time</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Channel</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Recipient</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Subject/Message</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Related To</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Status</th>
                                <th className="relative px-6 py-3">
                                    <span className="sr-only">Actions</span>
                                </th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-[#365663]">
                            {filteredNotifications.map((notification, index) => (
                                <tr
                                    key={notification.id}
                                    className={`${index % 2 === 0 ? 'bg-[#1b2b32]/20' : ''} hover:bg-[#1b2b32]/50 transition-colors cursor-pointer`}
                                    onClick={() => {
                                        setSelectedNotification(notification);
                                        setShowDetailsPanel(true);
                                    }}
                                >
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                                        {formatTimestamp(notification.timestamp)}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-white font-medium capitalize">
                                        {notification.channel}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                                        {notification.recipient}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                                        {notification.subject}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-[#28aae2] hover:underline">
                                        {notification.relatedTo}
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                                        <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium ${getStatusColor(notification.status)}`}>
                                            <span className="material-symbols-outlined text-sm">
                                                {getStatusIcon(notification.status)}
                                            </span>
                                            {notification.status}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                        <button
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                setSelectedNotification(notification);
                                                setShowDetailsPanel(true);
                                            }}
                                            className="text-gray-400 hover:text-white p-2 hover:bg-[#253c46] rounded-full transition-colors"
                                        >
                                            <span className="material-symbols-outlined">more_vert</span>
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Notification Details Panel */}
            {showDetailsPanel && selectedNotification && (
                <NotificationDetailsPanel
                    notification={selectedNotification}
                    onClose={() => {
                        setShowDetailsPanel(false);
                        setSelectedNotification(null);
                    }}
                />
            )}
        </>
    );
}

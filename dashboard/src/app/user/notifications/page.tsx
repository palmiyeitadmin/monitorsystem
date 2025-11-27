'use client'

import { useState } from 'react'

interface Notification {
    id: string
    time: string
    channel: 'email' | 'sms' | 'webhook' | 'telegram'
    recipient: string
    subject: string
    relatedTo: string
    status: 'delivered' | 'failed' | 'pending'
    messageId?: string
    sentAt?: string
    deliveredAt?: string
}

export default function UserNotifications() {
    const [dateRange, setDateRange] = useState('Last 24 hours')
    const [channelFilter, setChannelFilter] = useState('All')
    const [statusFilter, setStatusFilter] = useState('All')
    const [searchTerm, setSearchTerm] = useState('')
    const [selectedNotification, setSelectedNotification] = useState<Notification | null>(null)

    const notifications: Notification[] = [
        {
            id: '1',
            time: '2023-10-27 10:05 AM',
            channel: 'email',
            recipient: 'user@example.com',
            subject: 'CRITICAL: High CPU Usage on Server-01',
            relatedTo: 'Incident #5821',
            status: 'delivered',
            messageId: 'f4a2...c3b1',
            sentAt: '10:05:10 AM',
            deliveredAt: '10:05:12 AM'
        },
        {
            id: '2',
            time: '2023-10-27 10:02 AM',
            channel: 'sms',
            recipient: '+15551234567',
            subject: 'CRITICAL: High CPU Usage on Server-01',
            relatedTo: 'Incident #5821',
            status: 'failed'
        },
        {
            id: '3',
            time: '2023-10-27 09:58 AM',
            channel: 'webhook',
            recipient: 'https://hooks.slack.com/...',
            subject: 'Service \'API-Gateway\' is down.',
            relatedTo: 'Incident #5820',
            status: 'pending'
        },
        {
            id: '4',
            time: '2023-10-27 09:45 AM',
            channel: 'telegram',
            recipient: '@johndoe',
            subject: 'WARNING: Disk space on DB-03 is at 85%',
            relatedTo: 'Incident #5819',
            status: 'delivered'
        }
    ]

    const filteredNotifications = notifications.filter(notification => {
        const matchesSearch = notification.subject.toLowerCase().includes(searchTerm.toLowerCase()) ||
            notification.relatedTo.toLowerCase().includes(searchTerm.toLowerCase())
        const matchesChannel = channelFilter === 'All' || notification.channel === channelFilter
        const matchesStatus = statusFilter === 'All' || notification.status === statusFilter
        return matchesSearch && matchesChannel && matchesStatus
    })

    const getStatusColor = (status: string) => {
        switch (status) {
            case 'delivered':
                return 'text-green-400'
            case 'failed':
                return 'text-red-400'
            case 'pending':
                return 'text-yellow-400'
            default:
                return 'text-gray-400'
        }
    }

    const getStatusBgColor = (status: string) => {
        switch (status) {
            case 'delivered':
                return 'bg-green-500/10'
            case 'failed':
                return 'bg-red-500/10'
            case 'pending':
                return 'bg-yellow-500/10'
            default:
                return 'bg-gray-500/10'
        }
    }

    const getStatusIcon = (status: string) => {
        switch (status) {
            case 'delivered':
                return 'check_circle'
            case 'failed':
                return 'cancel'
            case 'pending':
                return 'hourglass_top'
            default:
                return 'help'
        }
    }

    const getChannelIcon = (channel: string) => {
        switch (channel) {
            case 'email':
                return 'mail'
            case 'sms':
                return 'sms'
            case 'webhook':
                return 'link'
            case 'telegram':
                return 'send'
            default:
                return 'notifications'
        }
    }

    return (
        <div className="p-6 lg:p-8 bg-gray-50 dark:bg-gray-900 min-h-screen">
            {/* Page Header */}
            <div className="flex flex-wrap items-end justify-between gap-4 mb-6">
                <div className="flex flex-col gap-1">
                    <h1 className="text-3xl font-bold text-gray-900 dark:text-white tracking-tight">
                        My Notification History
                    </h1>
                    <p className="text-gray-600 dark:text-gray-400 text-base">
                        View all alerts and notifications related to your resources and their delivery status
                    </p>
                </div>
            </div>

            <div className="flex flex-col gap-6">
                {/* Filters Row */}
                <div className="flex flex-wrap items-center gap-3">
                    {/* Date Range Filter */}
                    <button className="flex h-9 items-center justify-center gap-x-2 rounded-lg bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 px-3 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                        <span className="material-symbols-outlined text-lg">calendar_today</span>
                        <span>Date Range: {dateRange}</span>
                        <span className="material-symbols-outlined text-lg">expand_more</span>
                    </button>

                    {/* Channel Filter */}
                    <button className="flex h-9 items-center justify-center gap-x-2 rounded-lg bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 px-3 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                        <span className="material-symbols-outlined text-lg">send</span>
                        <span>Channel: {channelFilter}</span>
                        <span className="material-symbols-outlined text-lg">expand_more</span>
                    </button>

                    {/* Status Filter */}
                    <button className="flex h-9 items-center justify-center gap-x-2 rounded-lg bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 px-3 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                        <span className="material-symbols-outlined text-lg">task_alt</span>
                        <span>Status: {statusFilter}</span>
                        <span className="material-symbols-outlined text-lg">expand_more</span>
                    </button>

                    {/* Search */}
                    <div className="relative flex-1 min-w-[200px]">
                        <span className="material-symbols-outlined pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">
                            search
                        </span>
                        <input
                            type="text"
                            className="h-9 w-full rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 pl-10 pr-3 text-sm text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
                            placeholder="Search by Incident ID"
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                    </div>
                </div>

                {/* Stats Cards */}
                <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
                    <div className="flex flex-col gap-1 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-4">
                        <p className="text-sm font-medium text-gray-600 dark:text-gray-400">Total Sent (24h)</p>
                        <p className="text-3xl font-bold text-gray-900 dark:text-white">1,234</p>
                    </div>
                    <div className="flex flex-col gap-1 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-4">
                        <p className="text-sm font-medium text-gray-600 dark:text-gray-400">Delivered</p>
                        <p className="text-3xl font-bold text-green-600 dark:text-green-400">1,200</p>
                    </div>
                    <div className="flex flex-col gap-1 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-4">
                        <p className="text-sm font-medium text-gray-600 dark:text-gray-400">Failed</p>
                        <p className="text-3xl font-bold text-red-600 dark:text-red-400">14</p>
                    </div>
                    <div className="flex flex-col gap-1 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-4">
                        <p className="text-sm font-medium text-gray-600 dark:text-gray-400">Pending</p>
                        <p className="text-3xl font-bold text-yellow-600 dark:text-yellow-400">20</p>
                    </div>
                </div>

                {/* Notifications Table */}
                <div className="overflow-hidden rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
                    <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                        <thead className="bg-gray-50 dark:bg-gray-700/50">
                            <tr>
                                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                    Time
                                </th>
                                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                    Channel
                                </th>
                                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                    Recipient
                                </th>
                                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                    Subject/Message
                                </th>
                                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                    Related To
                                </th>
                                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                    Status
                                </th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                            {filteredNotifications.map((notification) => (
                                <tr
                                    key={notification.id}
                                    className="hover:bg-gray-50 dark:hover:bg-gray-700/50 cursor-pointer transition-colors"
                                    onClick={() => setSelectedNotification(notification)}
                                >
                                    <td className="whitespace-nowrap px-4 py-4 text-sm text-gray-600 dark:text-gray-400">
                                        {notification.time}
                                    </td>
                                    <td className="whitespace-nowrap px-4 py-4 text-sm text-gray-600 dark:text-gray-400">
                                        <div className="flex items-center gap-2">
                                            <span className="material-symbols-outlined text-lg">
                                                {getChannelIcon(notification.channel)}
                                            </span>
                                            {notification.channel.charAt(0).toUpperCase() + notification.channel.slice(1)}
                                        </div>
                                    </td>
                                    <td className="whitespace-nowrap px-4 py-4 text-sm text-gray-600 dark:text-gray-400">
                                        {notification.recipient}
                                    </td>
                                    <td className="px-4 py-4 text-sm font-medium text-gray-900 dark:text-white">
                                        {notification.subject}
                                    </td>
                                    <td className="whitespace-nowrap px-4 py-4 text-sm text-gray-600 dark:text-gray-400">
                                        {notification.relatedTo}
                                    </td>
                                    <td className="whitespace-nowrap px-4 py-4 text-sm text-gray-600 dark:text-gray-400">
                                        <div className={`inline-flex items-center gap-x-1.5 rounded-full ${getStatusBgColor(notification.status)} px-2 py-1 text-xs font-medium ${getStatusColor(notification.status)}`}>
                                            <span className="material-symbols-outlined text-sm" style={{ fontVariationSettings: "'FILL' 1" }}>
                                                {getStatusIcon(notification.status)}
                                            </span>
                                            {notification.status.charAt(0).toUpperCase() + notification.status.slice(1)}
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Notification Detail Panel */}
            {selectedNotification && (
                <div className="fixed inset-0 z-50 overflow-hidden">
                    <div
                        className="absolute inset-0 bg-black bg-opacity-50"
                        onClick={() => setSelectedNotification(null)}
                    />
                    <div className="absolute right-0 top-0 h-full w-full max-w-md bg-white dark:bg-gray-800 shadow-xl">
                        <div className="flex h-full flex-col overflow-y-auto">
                            {/* Panel Header */}
                            <div className="p-6 border-b border-gray-200 dark:border-gray-700">
                                <div className="flex items-start justify-between">
                                    <div className="flex items-center gap-3">
                                        <span className="material-symbols-outlined text-2xl text-primary">
                                            {getChannelIcon(selectedNotification.channel)}
                                        </span>
                                        <h2 className="text-lg font-medium text-gray-900 dark:text-white capitalize">
                                            {selectedNotification.channel} Notification
                                        </h2>
                                    </div>
                                    <button
                                        className="rounded-md text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 focus:outline-none focus:ring-2 focus:ring-primary"
                                        onClick={() => setSelectedNotification(null)}
                                    >
                                        <span className="material-symbols-outlined">close</span>
                                    </button>
                                </div>
                                <div className="mt-2 flex items-center gap-4">
                                    <div className={`inline-flex items-center gap-x-1.5 rounded-full ${getStatusBgColor(selectedNotification.status)} px-2 py-1 text-xs font-medium ${getStatusColor(selectedNotification.status)}`}>
                                        <span className="material-symbols-outlined text-sm" style={{ fontVariationSettings: "'FILL' 1" }}>
                                            {getStatusIcon(selectedNotification.status)}
                                        </span>
                                        {selectedNotification.status.charAt(0).toUpperCase() + selectedNotification.status.slice(1)}
                                    </div>
                                    <p className="text-sm text-gray-600 dark:text-gray-400">
                                        {selectedNotification.time}
                                    </p>
                                </div>
                            </div>

                            {/* Panel Content */}
                            <div className="flex-1 p-6">
                                <div className="flex flex-col gap-6">
                                    {/* Delivery Information */}
                                    <div>
                                        <h3 className="font-medium text-gray-900 dark:text-white mb-3">Delivery Information</h3>
                                        <dl className="divide-y divide-gray-200 dark:divide-gray-700 border-y border-gray-200 dark:border-gray-700">
                                            <div className="flex justify-between py-3 text-sm">
                                                <dt className="text-gray-600 dark:text-gray-400">Recipient</dt>
                                                <dd className="text-gray-900 dark:text-white">{selectedNotification.recipient}</dd>
                                            </div>
                                            <div className="flex justify-between py-3 text-sm">
                                                <dt className="text-gray-600 dark:text-gray-400">Subject</dt>
                                                <dd className="text-gray-900 dark:text-white">{selectedNotification.subject}</dd>
                                            </div>
                                            {selectedNotification.sentAt && (
                                                <div className="flex justify-between py-3 text-sm">
                                                    <dt className="text-gray-600 dark:text-gray-400">Sent At</dt>
                                                    <dd className="text-gray-900 dark:text-white">{selectedNotification.sentAt}</dd>
                                                </div>
                                            )}
                                            {selectedNotification.deliveredAt && (
                                                <div className="flex justify-between py-3 text-sm">
                                                    <dt className="text-gray-600 dark:text-gray-400">Delivered At</dt>
                                                    <dd className="text-gray-900 dark:text-white">{selectedNotification.deliveredAt}</dd>
                                                </div>
                                            )}
                                            {selectedNotification.messageId && (
                                                <div className="flex justify-between py-3 text-sm">
                                                    <dt className="text-gray-600 dark:text-gray-400">Message ID</dt>
                                                    <dd className="text-gray-900 dark:text-white truncate">{selectedNotification.messageId}</dd>
                                                </div>
                                            )}
                                        </dl>
                                    </div>

                                    {/* Content Preview */}
                                    <div>
                                        <h3 className="font-medium text-gray-900 dark:text-white mb-3">Content Preview</h3>
                                        <div className="rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900 p-4 text-sm text-gray-900 dark:text-white">
                                            <p><strong>Alert:</strong> {selectedNotification.subject.split(':')[0]}</p>
                                            <p><strong>Resource:</strong> Server-01</p>
                                            <p><strong>Metric:</strong> CPU Usage</p>
                                            <p><strong>Value:</strong> 98.5%</p>
                                            <p className="mt-2">The CPU usage on Server-01 has exceeded critical threshold of 95%.</p>
                                        </div>
                                    </div>

                                    {/* Related Information */}
                                    <div>
                                        <h3 className="font-medium text-gray-900 dark:text-white mb-3">Related Information</h3>
                                        <dl className="divide-y divide-gray-200 dark:divide-gray-700 border-y border-gray-200 dark:border-gray-700">
                                            <div className="flex justify-between py-3 text-sm">
                                                <dt className="text-gray-600 dark:text-gray-400">Incident</dt>
                                                <dd>
                                                    <a className="text-primary hover:underline" href="#">
                                                        {selectedNotification.relatedTo}
                                                    </a>
                                                </dd>
                                            </div>
                                            <div className="flex justify-between py-3 text-sm">
                                                <dt className="text-gray-600 dark:text-gray-400">Notification Rule</dt>
                                                <dd className="text-gray-900 dark:text-white">Critical CPU Alerts</dd>
                                            </div>
                                        </dl>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    )
}
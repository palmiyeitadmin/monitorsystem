'use client'

import { useState, useEffect } from 'react'
import { useAuth } from '@/lib/auth-context'

interface Resource {
    id: string
    name: string
    type: 'server' | 'website' | 'service'
    status: 'up' | 'down' | 'degraded'
    uptime: number
}

interface Incident {
    id: string
    title: string
    severity: 'critical' | 'warning' | 'info'
    status: 'investigating' | 'monitoring' | 'resolved'
    createdAt: string
}

export default function UserDashboard() {
    const { user } = useAuth()
    const [resources, setResources] = useState<Resource[]>([
        {
            id: '1',
            name: 'Production Web Server',
            type: 'server',
            status: 'up',
            uptime: 99.99
        },
        {
            id: '2',
            name: 'Database Cluster A',
            type: 'server',
            status: 'down',
            uptime: 98.21
        },
        {
            id: '3',
            name: 'api.eramonitor.com',
            type: 'website',
            status: 'up',
            uptime: 100
        },
        {
            id: '4',
            name: 'CDN Asset Delivery',
            type: 'service',
            status: 'degraded',
            uptime: 99.91
        }
    ])

    const [incidents, setIncidents] = useState<Incident[]>([
        {
            id: '1',
            title: 'Database Latency Issue',
            severity: 'critical',
            status: 'investigating',
            createdAt: '15 minutes ago'
        },
        {
            id: '2',
            title: 'API Endpoint 5xx Errors',
            severity: 'warning',
            status: 'monitoring',
            createdAt: '2 hours ago'
        },
        {
            id: '3',
            title: 'Login Service Outage',
            severity: 'info',
            status: 'resolved',
            createdAt: '1 day ago'
        }
    ])

    const stats = {
        hosts: {
            total: 10,
            up: 9,
            down: 1
        },
        websites: {
            total: 5,
            up: 5,
            down: 0
        },
        incidents: {
            active: 1
        },
        uptime: {
            overall: 99.98
        }
    }

    const getStatusColor = (status: string) => {
        switch (status) {
            case 'up':
            case 'resolved':
                return 'text-green-600 dark:text-green-400'
            case 'down':
            case 'critical':
                return 'text-red-600 dark:text-red-400'
            case 'degraded':
            case 'warning':
                return 'text-yellow-600 dark:text-yellow-400'
            default:
                return 'text-gray-600 dark:text-gray-400'
        }
    }

    const getStatusBgColor = (status: string) => {
        switch (status) {
            case 'up':
            case 'resolved':
                return 'bg-green-100 dark:bg-green-900/20 text-green-800 dark:text-green-300'
            case 'down':
            case 'critical':
                return 'bg-red-100 dark:bg-red-900/20 text-red-800 dark:text-red-300'
            case 'degraded':
            case 'warning':
                return 'bg-yellow-100 dark:bg-yellow-900/20 text-yellow-800 dark:text-yellow-300'
            default:
                return 'bg-gray-100 dark:bg-gray-900/20 text-gray-800 dark:text-gray-300'
        }
    }

    const getStatusDot = (status: string) => {
        switch (status) {
            case 'up':
            case 'resolved':
                return 'bg-green-500'
            case 'down':
            case 'critical':
                return 'bg-red-500'
            case 'degraded':
            case 'warning':
                return 'bg-yellow-500'
            default:
                return 'bg-gray-500'
        }
    }

    const formatRelativeTime = (dateString: string) => {
        return dateString
    }

    return (
        <div className="p-6 lg:p-10 bg-gray-50 dark:bg-gray-900 min-h-screen">
            {/* Welcome Section */}
            <div className="mb-8">
                <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
                    Welcome back, {user?.fullName?.split(' ')[0] || 'User'}!
                </h1>
                <p className="text-gray-600 dark:text-gray-400">
                    Here's a summary of your monitored resources.
                </p>
            </div>

            {/* Stats Grid */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
                {/* My Hosts */}
                <div className="bg-white dark:bg-gray-800 rounded-xl p-6 border border-gray-200 dark:border-gray-700">
                    <h3 className="text-gray-700 dark:text-gray-300 text-base font-medium mb-2">
                        My Hosts
                    </h3>
                    <p className="text-2xl font-bold text-gray-900 dark:text-white mb-1">
                        {stats.hosts.total}
                    </p>
                    <p className="text-sm text-gray-500 dark:text-gray-400">
                        <span className="text-green-600 dark:text-green-400">{stats.hosts.up} Up</span> /{' '}
                        <span className="text-red-600 dark:text-red-400">{stats.hosts.down} Down</span>
                    </p>
                </div>

                {/* My Websites */}
                <div className="bg-white dark:bg-gray-800 rounded-xl p-6 border border-gray-200 dark:border-gray-700">
                    <h3 className="text-gray-700 dark:text-gray-300 text-base font-medium mb-2">
                        My Websites
                    </h3>
                    <p className="text-2xl font-bold text-gray-900 dark:text-white mb-1">
                        {stats.websites.total}
                    </p>
                    <p className="text-sm text-gray-500 dark:text-gray-400">
                        <span className="text-green-600 dark:text-green-400">{stats.websites.up} Up</span> /{' '}
                        <span className="text-red-600 dark:text-red-400">{stats.websites.down} Down</span>
                    </p>
                </div>

                {/* Active Incidents */}
                <div className="bg-white dark:bg-gray-800 rounded-xl p-6 border border-gray-200 dark:border-gray-700">
                    <h3 className="text-gray-700 dark:text-gray-300 text-base font-medium mb-2">
                        Active Incidents
                    </h3>
                    <p className={`text-2xl font-bold mb-1 ${getStatusColor('critical')}`}>
                        {stats.incidents.active}
                    </p>
                    <p className="text-sm text-gray-500 dark:text-gray-400">
                        View details
                    </p>
                </div>

                {/* Overall Uptime */}
                <div className="bg-white dark:bg-gray-800 rounded-xl p-6 border border-gray-200 dark:border-gray-700">
                    <h3 className="text-gray-700 dark:text-gray-300 text-base font-medium mb-2">
                        Overall Uptime (30d)
                    </h3>
                    <p className="text-2xl font-bold text-gray-900 dark:text-white mb-1">
                        {stats.uptime.overall}%
                    </p>
                    <p className="text-sm text-gray-500 dark:text-gray-400">
                        Excellent performance
                    </p>
                </div>
            </div>

            {/* Main Content Grid */}
            <div className="grid grid-cols-1 xl:grid-cols-3 gap-8">
                {/* Resource Status Table */}
                <div className="xl:col-span-2">
                    <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-4">
                        Resource Status
                    </h2>
                    <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">
                        <div className="overflow-x-auto">
                            <table className="w-full">
                                <thead className="bg-gray-50 dark:bg-gray-700/50">
                                    <tr>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                            Status
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                            Resource Name
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                            Type
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                            Uptime
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                            Actions
                                        </th>
                                    </tr>
                                </thead>
                                <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                                    {resources.map((resource) => (
                                        <tr
                                            key={resource.id}
                                            className="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors"
                                        >
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <span className={`flex items-center gap-2 text-sm ${getStatusColor(resource.status)}`}>
                                                    <div className={`h-2.5 w-2.5 rounded-full ${getStatusDot(resource.status)}`}></div>
                                                    {resource.status.charAt(0).toUpperCase() + resource.status.slice(1)}
                                                </span>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <div className="text-sm font-medium text-gray-900 dark:text-white">
                                                    {resource.name}
                                                </div>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <div className="text-sm text-gray-500 dark:text-gray-400 capitalize">
                                                    {resource.type}
                                                </div>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <div className="text-sm text-gray-500 dark:text-gray-400">
                                                    {resource.uptime}%
                                                </div>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <button className="text-sm font-medium text-primary hover:underline">
                                                    Details
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>

                {/* Recent Incidents */}
                <div className="xl:col-span-1">
                    <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-4">
                        Recent Incidents
                    </h2>
                    <div className="space-y-4">
                        {incidents.map((incident) => (
                            <div
                                key={incident.id}
                                className="bg-white dark:bg-gray-800 rounded-xl p-4 border border-gray-200 dark:border-gray-700"
                            >
                                <div className="flex justify-between items-start mb-2">
                                    <h3 className="font-semibold text-gray-900 dark:text-white">
                                        {incident.title}
                                    </h3>
                                    <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${getStatusBgColor(incident.severity)}`}>
                                        {incident.severity.charAt(0).toUpperCase() + incident.severity.slice(1)}
                                    </span>
                                </div>
                                <p className="text-sm text-gray-500 dark:text-gray-400 mb-2">
                                    Status:{' '}
                                    <span className="font-medium text-gray-700 dark:text-gray-300 capitalize">
                                        {incident.status}
                                    </span>
                                </p>
                                <p className="text-xs text-gray-400 dark:text-gray-500">
                                    {formatRelativeTime(incident.createdAt)}
                                </p>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    )
}
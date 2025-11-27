'use client'

import { useState } from 'react'

interface Website {
    id: string
    url: string
    status: 'up' | 'down' | 'warning'
    responseTime?: number
    sslDays: number
    lastCheck: string
    uptime: number
}

export default function UserWebsites() {
    const [searchTerm, setSearchTerm] = useState('')

    const websites: Website[] = [
        {
            id: '1',
            url: 'eramonitor.com',
            status: 'up',
            responseTime: 85,
            sslDays: 85,
            lastCheck: '30 secs ago',
            uptime: 100
        },
        {
            id: '2',
            url: 'cloud-infra.dev',
            status: 'down',
            sslDays: 32,
            lastCheck: '2 mins ago',
            uptime: 95
        },
        {
            id: '3',
            url: 'tech-services.io',
            status: 'warning',
            responseTime: 550,
            sslDays: 12,
            lastCheck: '1 min ago',
            uptime: 99
        }
    ]

    const filteredWebsites = websites.filter(website =>
        website.url.toLowerCase().includes(searchTerm.toLowerCase())
    )

    const getStatusColor = (status: string) => {
        switch (status) {
            case 'up':
                return 'text-green-600 dark:text-green-400'
            case 'down':
                return 'text-red-600 dark:text-red-400'
            case 'warning':
                return 'text-yellow-600 dark:text-yellow-400'
            default:
                return 'text-gray-600 dark:text-gray-400'
        }
    }

    const getStatusBgColor = (status: string) => {
        switch (status) {
            case 'up':
                return 'bg-green-100 dark:bg-green-500/20 text-green-800 dark:text-green-400'
            case 'down':
                return 'bg-red-100 dark:bg-red-500/20 text-red-800 dark:text-red-400'
            case 'warning':
                return 'bg-yellow-100 dark:bg-yellow-500/20 text-yellow-800 dark:text-yellow-400'
            default:
                return 'bg-gray-100 dark:bg-gray-500/20 text-gray-800 dark:text-gray-400'
        }
    }

    const getStatusDot = (status: string) => {
        switch (status) {
            case 'up':
                return 'bg-green-500'
            case 'down':
                return 'bg-red-500'
            case 'warning':
                return 'bg-yellow-500'
            default:
                return 'bg-gray-500'
        }
    }

    const getUptimeColor = (uptime: number) => {
        if (uptime >= 99) return 'bg-green-500'
        if (uptime >= 95) return 'bg-yellow-500'
        return 'bg-red-500'
    }

    return (
        <div className="p-6 lg:p-8 bg-gray-50 dark:bg-gray-900 min-h-screen">
            <div className="max-w-7xl mx-auto">
                {/* Page Header */}
                <div className="flex flex-wrap justify-between items-center gap-4 mb-6">
                    <div className="flex flex-col">
                        <h1 className="text-4xl font-black text-gray-900 dark:text-white tracking-tight">
                            My Websites
                        </h1>
                        <p className="text-gray-500 dark:text-gray-400 mt-1">
                            Monitor status, response time, and SSL of your websites.
                        </p>
                    </div>
                    <button className="flex items-center justify-center gap-2 rounded-lg h-10 px-4 bg-primary text-white text-sm font-bold hover:bg-primary/90 transition-colors">
                        <span className="material-symbols-outlined text-base">add</span>
                        <span className="truncate">Add New Website</span>
                    </button>
                </div>

                {/* Search Bar */}
                <div className="mb-6">
                    <div className="relative">
                        <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 dark:text-gray-500">
                            search
                        </span>
                        <input
                            type="text"
                            className="block w-full pl-12 pr-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
                            placeholder="Search by URL or name..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                    </div>
                </div>

                {/* Websites Table */}
                <div className="@container">
                    <div className="overflow-hidden rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
                        <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                            <thead className="bg-gray-50 dark:bg-gray-700/50">
                                <tr>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                        URL
                                    </th>
                                    <th className="table-col-status px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                        Status
                                    </th>
                                    <th className="table-col-response px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                        Response Time
                                    </th>
                                    <th className="table-col-ssl px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                        SSL
                                    </th>
                                    <th className="table-col-check px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                        Last Check
                                    </th>
                                    <th className="table-col-uptime px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                                        Uptime (24h)
                                    </th>
                                    <th className="relative px-6 py-3">
                                        <span className="sr-only">Actions</span>
                                    </th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                                {filteredWebsites.map((website) => (
                                    <tr
                                        key={website.id}
                                        className="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors"
                                    >
                                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">
                                            {website.url}
                                        </td>
                                        <td className="table-col-status px-6 py-4 whitespace-nowrap">
                                            <span className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-medium ${getStatusBgColor(website.status)}`}>
                                                <span className={`size-2 rounded-full ${getStatusDot(website.status)}`}></span>
                                                {website.status.charAt(0).toUpperCase() + website.status.slice(1)}
                                            </span>
                                        </td>
                                        <td className="table-col-response px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                                            {website.responseTime ? `${website.responseTime} ms` : 'N/A'}
                                        </td>
                                        <td className="table-col-ssl px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                                            {website.sslDays} days
                                        </td>
                                        <td className="table-col-check px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                                            {website.lastCheck}
                                        </td>
                                        <td className="table-col-uptime px-6 py-4 whitespace-nowrap">
                                            <div className="flex items-center gap-3">
                                                <div className="w-24 overflow-hidden rounded-full bg-gray-200 dark:bg-gray-700">
                                                    <div
                                                        className={`h-1.5 rounded-full transition-all duration-300 ${getUptimeColor(website.uptime)}`}
                                                        style={{ width: `${website.uptime}%` }}
                                                    ></div>
                                                </div>
                                                <p className="text-gray-900 dark:text-white text-sm font-medium">
                                                    {website.uptime}%
                                                </p>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                            <button className="text-gray-500 dark:text-gray-400 hover:text-primary dark:hover:text-primary transition-colors">
                                                <span className="material-symbols-outlined">more_vert</span>
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                </div>

                {/* Empty State */}
                {filteredWebsites.length === 0 && (
                    <div className="mt-8">
                        <div className="flex flex-col items-center gap-6 rounded-lg border-2 border-dashed border-gray-300 dark:border-gray-600 px-6 py-20 text-center">
                            <span className="material-symbols-outlined text-5xl text-gray-400 dark:text-gray-500">
                                cloud_off
                            </span>
                            <div className="flex max-w-md flex-col items-center gap-2">
                                <p className="text-gray-900 dark:text-white text-lg font-bold tracking-tight">
                                    {searchTerm ? 'No websites found' : 'No websites are being monitored yet'}
                                </p>
                                <p className="text-gray-500 dark:text-gray-400 text-sm">
                                    {searchTerm
                                        ? 'Try adjusting your search criteria.'
                                        : 'Get started by adding your first website to monitor its uptime, performance, and SSL certificate.'}
                                </p>
                            </div>
                            <button className="flex items-center justify-center gap-2 rounded-lg h-10 px-4 bg-primary text-white text-sm font-bold hover:bg-primary/90 transition-colors">
                                <span className="material-symbols-outlined text-base">add</span>
                                <span className="truncate">
                                    {searchTerm ? 'Clear Search' : 'Add Your First Website'}
                                </span>
                            </button>
                        </div>
                    </div>
                )}

                {/* Responsive Table Styles */}
                <style jsx>{`
          @container (max-width: 1024px) {
            .table-col-uptime { display: none; }
          }
          @container (max-width: 860px) {
            .table-col-check { display: none; }
          }
          @container (max-width: 768px) {
            .table-col-ssl { display: none; }
          }
          @container (max-width: 640px) {
            .table-col-response { display: none; }
          }
        `}</style>
            </div>
        </div>
    )
}
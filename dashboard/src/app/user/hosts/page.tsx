'use client'

import { useState } from 'react'

interface Server {
    id: string
    name: string
    location: string
    status: 'up' | 'down' | 'warning'
    cpu?: number
    ram?: number
    disk?: number
    lastSeen: string
}

export default function UserHosts() {
    const [searchTerm, setSearchTerm] = useState('')
    const [statusFilter, setStatusFilter] = useState<'all' | 'up' | 'down'>('all')

    const servers: Server[] = [
        {
            id: '1',
            name: 'Production Web Server 01',
            location: 'AWS / US-West-2',
            status: 'up',
            cpu: 65,
            ram: 40,
            disk: 80,
            lastSeen: '2 minutes ago'
        },
        {
            id: '2',
            name: 'Staging DB Server',
            location: 'Azure / US-East-1',
            status: 'down',
            lastSeen: '1 hour ago'
        },
        {
            id: '3',
            name: 'Analytics Cluster Node 03',
            location: 'GCP / EU-West-1',
            status: 'warning',
            cpu: 92,
            ram: 85,
            disk: 55,
            lastSeen: '30 seconds ago'
        },
        {
            id: '4',
            name: 'Internal Tools Server',
            location: 'On-Prem / Datacenter A',
            status: 'up',
            cpu: 15,
            ram: 25,
            disk: 30,
            lastSeen: '5 minutes ago'
        }
    ]

    const filteredServers = servers.filter(server => {
        const matchesSearch = server.name.toLowerCase().includes(searchTerm.toLowerCase())
        const matchesStatus = statusFilter === 'all' || server.status === statusFilter
        return matchesSearch && matchesStatus
    })

    const getStatusColor = (status: string) => {
        switch (status) {
            case 'up':
                return 'text-green-500'
            case 'down':
                return 'text-red-500'
            case 'warning':
                return 'text-yellow-500'
            default:
                return 'text-gray-500'
        }
    }

    const getStatusBgColor = (status: string) => {
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

    const getProgressBarColor = (value: number, type: string) => {
        if (value >= 90) return 'bg-red-500'
        if (value >= 80) return 'bg-yellow-500'
        return 'bg-primary'
    }

    return (
        <div className="p-6 lg:p-8 bg-gray-50 dark:bg-gray-900 min-h-screen">
            {/* Page Header */}
            <div className="mb-8">
                <h1 className="text-4xl font-black text-gray-900 dark:text-white mb-2">
                    My Servers
                </h1>
                <p className="text-gray-600 dark:text-gray-400">
                    Monitor and manage your server infrastructure
                </p>
            </div>

            {/* Search and Filters */}
            <div className="flex flex-col md:flex-row gap-4 items-center mb-8">
                {/* Search Bar */}
                <div className="w-full md:w-1/2 lg:w-1/3">
                    <div className="relative">
                        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <span className="material-symbols-outlined text-gray-500 dark:text-gray-400">search</span>
                        </div>
                        <input
                            type="text"
                            className="block w-full pl-10 pr-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
                            placeholder="Search by server name..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                    </div>
                </div>

                {/* Status Filters */}
                <div className="flex gap-2">
                    <button
                        onClick={() => setStatusFilter('all')}
                        className={`flex h-8 items-center justify-center gap-x-2 rounded-lg px-3 transition-colors ${statusFilter === 'all'
                                ? 'bg-primary text-white'
                                : 'bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-white hover:bg-gray-300 dark:hover:bg-gray-600'
                            }`}
                    >
                        <span className="text-sm font-medium">All Statuses</span>
                    </button>
                    <button
                        onClick={() => setStatusFilter('up')}
                        className={`flex h-8 items-center justify-center gap-x-2 rounded-lg px-3 transition-colors ${statusFilter === 'up'
                                ? 'bg-primary text-white'
                                : 'bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-white hover:bg-gray-300 dark:hover:bg-gray-600'
                            }`}
                    >
                        <span className="text-sm font-medium">Up</span>
                    </button>
                    <button
                        onClick={() => setStatusFilter('down')}
                        className={`flex h-8 items-center justify-center gap-x-2 rounded-lg px-3 transition-colors ${statusFilter === 'down'
                                ? 'bg-primary text-white'
                                : 'bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-white hover:bg-gray-300 dark:hover:bg-gray-600'
                            }`}
                    >
                        <span className="text-sm font-medium">Down</span>
                    </button>
                </div>
            </div>

            {/* Server Cards Grid */}
            {filteredServers.length > 0 ? (
                <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6">
                    {filteredServers.map((server) => (
                        <div
                            key={server.id}
                            className="flex flex-col rounded-xl bg-white dark:bg-gray-800 p-5 shadow-sm hover:shadow-lg hover:ring-2 hover:ring-primary/50 transition-all cursor-pointer"
                        >
                            {/* Header */}
                            <div className="flex items-center justify-between mb-4">
                                <h3 className="text-lg font-bold text-gray-900 dark:text-white">
                                    {server.name}
                                </h3>
                                <div className="flex items-center gap-2">
                                    <div className={`w-3 h-3 rounded-full ${getStatusBgColor(server.status)}`}></div>
                                    <span className={`text-sm font-medium ${getStatusColor(server.status)}`}>
                                        {server.status.charAt(0).toUpperCase() + server.status.slice(1)}
                                    </span>
                                </div>
                            </div>

                            {/* Location */}
                            <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
                                {server.location}
                            </p>

                            {/* Resource Usage */}
                            <div className="flex flex-col gap-3">
                                {/* CPU */}
                                <div>
                                    <div className="flex justify-between text-xs text-gray-500 dark:text-gray-400 mb-1">
                                        <span>CPU</span>
                                        <span>{server.cpu ? `${server.cpu}%` : '--%'}</span>
                                    </div>
                                    <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-1.5">
                                        <div
                                            className={`h-1.5 rounded-full transition-all duration-300 ${server.cpu ? getProgressBarColor(server.cpu, 'cpu') : 'bg-gray-400'
                                                }`}
                                            style={{ width: server.cpu ? `${server.cpu}%` : '0%' }}
                                        ></div>
                                    </div>
                                </div>

                                {/* RAM */}
                                <div>
                                    <div className="flex justify-between text-xs text-gray-500 dark:text-gray-400 mb-1">
                                        <span>RAM</span>
                                        <span>{server.ram ? `${server.ram}%` : '--%'}</span>
                                    </div>
                                    <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-1.5">
                                        <div
                                            className={`h-1.5 rounded-full transition-all duration-300 ${server.ram ? getProgressBarColor(server.ram, 'ram') : 'bg-gray-400'
                                                }`}
                                            style={{ width: server.ram ? `${server.ram}%` : '0%' }}
                                        ></div>
                                    </div>
                                </div>

                                {/* Disk */}
                                <div>
                                    <div className="flex justify-between text-xs text-gray-500 dark:text-gray-400 mb-1">
                                        <span>Disk</span>
                                        <span>{server.disk ? `${server.disk}%` : '--%'}</span>
                                    </div>
                                    <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-1.5">
                                        <div
                                            className={`h-1.5 rounded-full transition-all duration-300 ${server.disk ? getProgressBarColor(server.disk, 'disk') : 'bg-gray-400'
                                                }`}
                                            style={{ width: server.disk ? `${server.disk}%` : '0%' }}
                                        ></div>
                                    </div>
                                </div>
                            </div>

                            {/* Last Seen */}
                            <p className="text-xs text-gray-500 dark:text-gray-400 mt-4 text-right">
                                Last seen: {server.lastSeen}
                            </p>
                        </div>
                    ))}
                </div>
            ) : (
                /* Empty State */
                <div className="flex flex-col items-center justify-center p-12 bg-white dark:bg-gray-800 rounded-xl border border-dashed border-gray-300 dark:border-gray-600">
                    <span className="material-symbols-outlined text-5xl text-gray-400 dark:text-gray-500">
                        cloud_off
                    </span>
                    <h3 className="mt-4 text-xl font-semibold text-gray-900 dark:text-white">
                        No Servers Found
                    </h3>
                    <p className="mt-2 text-sm text-gray-500 dark:text-gray-400">
                        {searchTerm || statusFilter !== 'all'
                            ? 'Try adjusting your search or filter criteria.'
                            : 'Get started by adding a new monitor to your account.'}
                    </p>
                    <button className="mt-6 flex items-center justify-center gap-2 rounded-lg h-10 px-5 bg-primary text-white text-sm font-bold hover:bg-primary/90 transition-colors">
                        <span className="material-symbols-outlined text-xl">add</span>
                        <span>Add New Monitor</span>
                    </button>
                </div>
            )}
        </div>
    )
}
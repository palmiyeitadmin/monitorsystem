'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import type { Service, PagedResponse } from '@/lib/api';
import { formatDistanceToNow } from 'date-fns';
import { tr } from 'date-fns/locale';

interface ServiceData {
    id: string;
    serviceName: string;
    serviceType: string;
    hostName: string;
    currentStatus: string;
    lastCheck: Date;
}

export default function ServicesPage() {
    const [services, setServices] = useState<ServiceData[]>([
        {
            id: '1',
            serviceName: 'WebApp-Main-Pool',
            serviceType: 'IIS App Pool',
            hostName: 'PROD-WEB-01',
            currentStatus: 'running',
            lastCheck: new Date(Date.now() - 2 * 60 * 1000),
        },
        {
            id: '2',
            serviceName: 'AuthenticationService',
            serviceType: 'Windows Service',
            hostName: 'AUTH-SRV-01',
            currentStatus: 'running',
            lastCheck: new Date(Date.now() - 3 * 60 * 1000),
        },
        {
            id: '3',
            serviceName: 'PostgresDB-Container',
            serviceType: 'Docker Container',
            hostName: 'DB-SRV-02',
            currentStatus: 'stopped',
            lastCheck: new Date(Date.now() - 15 * 60 * 1000),
        },
        {
            id: '4',
            serviceName: 'Nginx-Proxy',
            serviceType: 'Systemd Unit',
            hostName: 'PROD-WEB-02',
            currentStatus: 'warning',
            lastCheck: new Date(Date.now() - 5 * 60 * 1000),
        },
        {
            id: '5',
            serviceName: 'ReportingSite',
            serviceType: 'IIS Site',
            hostName: 'BI-SRV-01',
            currentStatus: 'running',
            lastCheck: new Date(Date.now() - 1 * 60 * 1000),
        }
    ]);
    const [loading, setLoading] = useState(false);
    const [viewMode, setViewMode] = useState<'grouped' | 'flat'>('flat');
    const [search, setSearch] = useState('');

    const getStatusColor = (status: string) => {
        const normalized = (status || '').toLowerCase();
        switch (normalized) {
            case 'healthy':
            case 'running':
            case 'up':
                return 'text-success';
            case 'stopped':
            case 'down':
                return 'text-danger';
            case 'warning':
            case 'degraded':
                return 'text-warning';
            default:
                return 'text-gray-400';
        }
    };

    const getStatusBgColor = (status: string) => {
        const normalized = (status || '').toLowerCase();
        switch (normalized) {
            case 'healthy':
            case 'running':
            case 'up':
                return 'bg-success';
            case 'stopped':
            case 'down':
                return 'bg-danger';
            case 'warning':
            case 'degraded':
                return 'bg-warning';
            default:
                return 'bg-gray-400';
        }
    };

    const filteredServices = services.filter(service =>
        service.serviceName?.toLowerCase().includes(search.toLowerCase()) ||
        service.serviceType?.toLowerCase().includes(search.toLowerCase()) ||
        service.hostName?.toLowerCase().includes(search.toLowerCase())
    );

    return (
        <div className="flex flex-col h-full">
            {/* Page Heading */}
            <div className="flex flex-wrap justify-between items-center gap-4 mb-6">
                <h1 className="text-gray-900 dark:text-white text-3xl font-bold tracking-tight">Services Management</h1>
                {/* ToolBar */}
                <div className="flex items-center gap-2">
                    <button className="flex items-center justify-center rounded-lg h-10 w-10 border border-border-light dark:border-border-dark bg-surface dark:bg-surface hover:bg-accent dark:hover:bg-accent transition-all duration-200 hover:scale-105">
                        <span className="material-symbols-outlined text-muted-foreground dark:text-muted-foreground">download</span>
                    </button>
                    <button className="flex items-center justify-center rounded-lg h-10 w-10 border border-border-light dark:border-border-dark bg-surface dark:bg-surface hover:bg-accent dark:hover:bg-accent transition-all duration-200 hover:scale-105">
                        <span className="material-symbols-outlined text-muted-foreground dark:text-muted-foreground">tune</span>
                    </button>
                    <button className="flex items-center justify-center overflow-hidden rounded-lg h-10 bg-primary text-background-dark gap-2 text-sm font-bold min-w-0 px-4 hover:bg-primary/90 transition-all duration-200 hover:scale-105 shadow-lg hover:shadow-primary/20">
                        <span className="material-symbols-outlined text-background-dark fill">add</span>
                        <span className="truncate">Add Service</span>
                    </button>
                </div>
            </div>

            {/* Secondary Header */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                {/* SegmentedButtons */}
                <div className="flex h-10 items-center justify-center rounded-lg bg-muted dark:bg-muted p-1 border border-border-light dark:border-border-dark">
                    <label className={`flex cursor-pointer h-full grow items-center justify-center overflow-hidden rounded-md px-2 transition-all duration-200 ${viewMode === 'grouped'
                        ? 'bg-surface dark:bg-surface shadow-sm text-foreground dark:text-white'
                        : 'text-muted-foreground dark:text-muted-foreground hover:bg-surface/50 dark:hover:bg-surface/50'
                        }`}>
                        <span className="truncate">Grouped by host</span>
                        <input
                            className="invisible w-0"
                            name="view-toggle"
                            type="radio"
                            value="grouped"
                            checked={viewMode === 'grouped'}
                            onChange={() => setViewMode('grouped')}
                        />
                    </label>
                    <label className={`flex cursor-pointer h-full grow items-center justify-center overflow-hidden rounded-md px-2 transition-all duration-200 ${viewMode === 'flat'
                        ? 'bg-surface dark:bg-surface shadow-sm text-foreground dark:text-white'
                        : 'text-muted-foreground dark:text-muted-foreground hover:bg-surface/50 dark:hover:bg-surface/50'
                        }`}>
                        <span className="truncate">Flat list view</span>
                        <input
                            className="invisible w-0"
                            name="view-toggle"
                            type="radio"
                            value="flat"
                            checked={viewMode === 'flat'}
                            onChange={() => setViewMode('flat')}
                        />
                    </label>
                </div>
                {/* Search Input */}
                <div className="relative">
                    <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground dark:text-muted-foreground">search</span>
                    <input
                        className="w-full h-10 pl-10 pr-4 rounded-lg border border-border-light dark:border-border-dark bg-surface dark:bg-surface focus:ring-2 focus:ring-primary/50 focus:border-primary/80 transition-all duration-200"
                        placeholder="Search by service name..."
                        type="text"
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                </div>
            </div>

            {/* Chips/Filters */}
            <div className="flex items-center gap-3 pb-4 border-b border-border-light dark:border-border-dark">
                <p className="text-sm font-medium text-muted-foreground dark:text-muted-foreground">Filters:</p>
                {['Type', 'Status', 'Host', 'Customer'].map((filter) => (
                    <button key={filter} className="flex h-8 shrink-0 items-center justify-center gap-x-2 rounded-lg bg-muted dark:bg-muted pl-3 pr-2 text-foreground dark:text-white hover:bg-accent dark:hover:bg-accent transition-all duration-200 hover:scale-105">
                        <p className="text-sm font-medium leading-normal">{filter}</p>
                        <span className="material-symbols-outlined text-muted-foreground dark:text-muted-foreground" style={{ fontSize: '20px' }}>expand_more</span>
                    </button>
                ))}
            </div>

            {/* Data Table */}
            <div className="overflow-x-auto mt-4">
                <div className="rounded-lg border border-border-light dark:border-border-dark bg-surface dark:bg-surface hover:border-primary/30 transition-all duration-300">
                    <table className="w-full text-sm text-left text-muted-foreground dark:text-muted-foreground">
                        <thead className="text-xs text-muted-foreground dark:text-muted-foreground uppercase bg-muted dark:bg-muted border-b border-border-light dark:border-border-dark">
                            <tr>
                                <th className="px-6 py-3 rounded-l-lg" scope="col">
                                    <div className="flex items-center gap-1 cursor-pointer hover:text-primary transition-colors duration-200">Service Name <span className="material-symbols-outlined" style={{ fontSize: '16px' }}>arrow_upward</span></div>
                                </th>
                                <th className="px-6 py-3" scope="col">Type</th>
                                <th className="px-6 py-3" scope="col">Host</th>
                                <th className="px-6 py-3" scope="col">Status</th>
                                <th className="px-6 py-3" scope="col">Last Check</th>
                                <th className="px-6 py-3 rounded-r-lg" scope="col"></th>
                            </tr>
                        </thead>
                        <tbody>
                            {loading ? (
                                <tr>
                                    <td colSpan={6} className="px-6 py-8 text-center">
                                        <div className="flex items-center justify-center gap-3">
                                            <div className="w-8 h-8 border-4 border-primary/20 border-t-primary rounded-full animate-spin"></div>
                                            <span className="text-muted-foreground">Loading...</span>
                                        </div>
                                    </td>
                                </tr>
                            ) : filteredServices.length === 0 ? (
                                <tr>
                                    <td colSpan={6} className="px-6 py-8 text-center">
                                        <div className="flex flex-col items-center gap-3">
                                            <span className="material-symbols-outlined text-4xl text-muted-foreground">lan</span>
                                            <span className="text-lg font-medium">No services found</span>
                                            <span className="text-sm text-muted-foreground">Try adjusting your search criteria</span>
                                        </div>
                                    </td>
                                </tr>
                            ) : (
                                filteredServices.map((service) => (
                                    <tr key={service.id} className="bg-surface dark:bg-surface border-b border-border-light dark:border-border-dark hover:bg-accent/50 dark:hover:bg-accent/50 transition-all duration-200">
                                        <td className="px-6 py-4 font-medium text-foreground dark:text-white whitespace-nowrap">{service.serviceName}</td>
                                        <td className="px-6 py-4 text-muted-foreground dark:text-muted-foreground">{service.serviceType}</td>
                                        <td className="px-6 py-4 text-muted-foreground dark:text-muted-foreground">{service.hostName}</td>
                                        <td className="px-6 py-4">
                                            <div className="flex items-center gap-2">
                                                <div className={`h-2.5 w-2.5 rounded-full ${getStatusBgColor(service.currentStatus)} ${service.currentStatus !== 'stopped' ? 'animate-pulse' : ''}`}></div>
                                                <span className={getStatusColor(service.currentStatus)}>{service.currentStatus ? service.currentStatus.charAt(0).toUpperCase() + service.currentStatus.slice(1) : ''}</span>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 text-muted-foreground dark:text-muted-foreground">
                                            {formatDistanceToNow(service.lastCheck, { addSuffix: true, locale: tr })}
                                        </td>
                                        <td className="px-6 py-4 text-right">
                                            <button className="p-1 rounded-full hover:bg-accent dark:hover:bg-accent transition-all duration-200 hover:scale-110">
                                                <span className="material-symbols-outlined">more_vert</span>
                                            </button>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Pagination */}
            <nav aria-label="Table navigation" className="flex items-center justify-between pt-4">
                <span className="text-sm font-normal text-muted-foreground dark:text-muted-foreground">
                    Showing <span className="font-semibold text-foreground dark:text-white">1-{filteredServices.length}</span> of <span className="font-semibold text-foreground dark:text-white">{filteredServices.length}</span>
                </span>
                <div className="flex items-center gap-2">
                    <button className="px-3 h-8 ml-0 leading-tight text-muted-foreground bg-surface dark:bg-surface border border-border-light dark:border-border-dark rounded-l-lg hover:bg-accent dark:hover:bg-accent hover:text-foreground dark:hover:text-white transition-all duration-200 hover:scale-105 flex items-center">
                        Previous
                    </button>
                    <button className="px-3 h-8 leading-tight text-muted-foreground bg-surface dark:bg-surface border border-border-light dark:border-border-dark hover:bg-accent dark:hover:bg-accent hover:text-foreground dark:hover:text-white transition-all duration-200 hover:scale-105 flex items-center">
                        1
                    </button>
                    <button className="px-3 h-8 leading-tight text-background-dark bg-primary border border-primary shadow-lg flex items-center">
                        2
                    </button>
                    <button className="px-3 h-8 leading-tight text-muted-foreground bg-surface dark:bg-surface border border-border-light dark:border-border-dark hover:bg-accent dark:hover:bg-accent hover:text-foreground dark:hover:text-white transition-all duration-200 hover:scale-105 flex items-center">
                        3
                    </button>
                    <button className="px-3 h-8 leading-tight text-muted-foreground bg-surface dark:bg-surface border border-border-light dark:border-border-dark rounded-r-lg hover:bg-accent dark:hover:bg-accent hover:text-foreground dark:hover:text-white transition-all duration-200 hover:scale-105 flex items-center">
                        Next
                    </button>
                </div>
            </nav>
        </div>
    );
}

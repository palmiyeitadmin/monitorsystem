'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import type { Host, PagedResponse } from '@/lib/api';
import { formatDistanceToNow } from 'date-fns';
import { tr } from 'date-fns/locale';

export default function HostsPage() {
    const [hosts, setHosts] = useState<Host[]>([]);
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [search, setSearch] = useState('');

    useEffect(() => {
        loadHosts();
    }, [page, search]);

    const loadHosts = async () => {
        try {
            const response: PagedResponse<Host> = await api.hosts.list({ page, pageSize: 10, search });
            setHosts(response.items);
            setTotalPages(response.totalPages);
        } catch (error) {
            console.error('Failed to load hosts:', error);
        } finally {
            setLoading(false);
        }
    };

    const getStatusColor = (status: string) => {
        const normalized = (status || '').toLowerCase();
        switch (normalized) {
            case 'online':
            case 'up':
            case 'running':
                return 'text-[#28A745]';
            case 'offline':
            case 'down':
            case 'stopped':
                return 'text-[#DC3545]';
            case 'warning':
            case 'degraded':
                return 'text-[#FFC107]';
            default:
                return 'text-gray-400';
        }
    };

    const getOsIconUrl = (osType: string) => {
        const normalized = (osType || '').toLowerCase();
        if (normalized.includes('windows')) {
            return "https://lh3.googleusercontent.com/aida-public/AB6AXuAuGWB2ZvmUY5ULufJa9vUbLbgew4Vuo3I-YGmPmv_0xrV4LOCJ7LdvhrOso-0PMD9NOXgkvnKnUGJ7QvYX2zFEMLCzx8F6xyrdXnMN1QegqvPGSzKIUpLyLrGt_0OQqahnwMHkIk006rD2Yu4Oj7Ac82wEmKzo5JZYxb2dJLdiA6w8f0ITxq_TnQNut99pueHEeqB4prn9QtfYs7pkpcVaikfaYKBcfH2sqqiP7VUqvVeDnJHUJBWFrjP-SoNN3uwlii_SphIaFqo";
        }
        return "https://lh3.googleusercontent.com/aida-public/AB6AXuDVFHNTTsl-HEFA0wHpsGLM9tUaLrv703eg9d1y4bkvQi9JJkoAWhzKx8xEtZLc1linLo3tgAIxrftAw8JwuUDKemH_4YyETutIpAFmR6eARb9uEJTI2Bhm6l6bVmrzB-HjOiYWaCfqFaPMFZV8JIiObcRaW-1IrUeziXAEXbjYud6pOkW72DZEER9aq_IIi__izTQieAKPurhQxSZr0JRCdiH-M8ilR0Hy6nV3t-V2X5veVNWNbcGxzKrv10OT8Z_v68EL0DIDCtI";
    };

    return (
        <div className="flex flex-col h-full">
            {/* Page Header */}
            <div className="flex flex-wrap justify-between items-center gap-4 mb-6">
                <h1 className="text-gray-900 dark:text-white text-4xl font-black leading-tight tracking-[-0.033em]">Hosts Management</h1>
            </div>

            {/* Toolbar */}
            <div className="flex justify-between items-center gap-2 px-4 py-3 bg-surface dark:bg-surface-dark rounded-lg mb-6 border border-border-light dark:border-border-dark hover:border-primary/30 transition-all duration-300">
                <div className="flex items-center gap-2">
                    <div className="relative">
                        <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 dark:text-gray-500">search</span>
                        <input
                            className="pl-10 pr-4 py-2 w-64 bg-background-light dark:bg-background-dark border-transparent focus:border-primary focus:ring-primary/50 rounded-md text-sm text-gray-900 dark:text-white placeholder-gray-500 transition-all duration-200"
                            placeholder="Search by name or IP..."
                            type="text"
                            value={search}
                            onChange={(e) => setSearch(e.target.value)}
                        />
                    </div>
                    <button className="p-2 text-gray-500 dark:text-white hover:bg-white/10 dark:hover:bg-white/10 rounded-md transition-all duration-200 hover:scale-105">
                        <span className="material-symbols-outlined">filter_list</span>
                    </button>
                    <button className="flex items-center justify-center rounded-lg h-10 bg-primary/20 text-primary/50 gap-2 text-sm font-bold leading-normal tracking-[0.015em] px-4 cursor-not-allowed opacity-60" disabled>
                        <span className="material-symbols-outlined">group_work</span>
                        <span className="truncate">Bulk Actions</span>
                    </button>
                </div>
                <button className="flex items-center justify-center rounded-lg h-10 bg-primary text-background-dark gap-2 text-sm font-bold leading-normal tracking-[0.015em] min-w-0 px-4 hover:bg-primary/90 transition-all duration-200 hover:scale-105 shadow-lg hover:shadow-primary/20">
                    <span className="material-symbols-outlined fill text-background-dark">add</span>
                    <span className="truncate">Add New Host</span>
                </button>
            </div>

            {/* Table */}
            <div className="flex-grow overflow-x-auto">
                <div className="overflow-hidden rounded-lg border border-border-light dark:border-border-dark bg-surface dark:bg-surface-dark hover:border-primary/30 transition-all duration-300">
                    <table className="w-full text-left">
                        <thead className="bg-surface dark:bg-surface-dark border-b border-border-light dark:border-border-dark">
                            <tr>
                                <th className="p-4 w-12">
                                    <input className="rounded bg-muted dark:bg-muted border-border-light dark:border-border-dark text-primary focus:ring-primary/50 focus:ring-offset-surface transition-all duration-200" type="checkbox" />
                                </th>
                                <th className="px-4 py-3 text-foreground dark:text-white text-sm font-semibold leading-normal">Status</th>
                                <th className="px-4 py-3 text-foreground dark:text-white text-sm font-semibold leading-normal">Hostname</th>
                                <th className="px-4 py-3 text-foreground dark:text-white text-sm font-semibold leading-normal">IP Address</th>
                                <th className="px-4 py-3 text-foreground dark:text-white text-sm font-semibold leading-normal w-14">OS</th>
                                <th className="px-4 py-3 text-foreground dark:text-white text-sm font-semibold leading-normal">Location</th>
                                <th className="px-4 py-3 text-foreground dark:text-white text-sm font-semibold leading-normal">Customer</th>
                                <th className="px-4 py-3 text-foreground dark:text-white text-sm font-semibold leading-normal">Last Seen</th>
                                <th className="px-4 py-3 text-foreground dark:text-white text-sm font-semibold leading-normal">CPU/RAM</th>
                                <th className="px-4 py-3 text-foreground dark:text-white text-sm font-semibold leading-normal">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {loading ? (
                                <tr>
                                    <td colSpan={10} className="px-4 py-8 text-center">
                                        <div className="flex items-center justify-center gap-3">
                                            <div className="w-8 h-8 border-4 border-primary/20 border-t-primary rounded-full animate-spin"></div>
                                            <span className="text-muted-foreground">Loading...</span>
                                        </div>
                                    </td>
                                </tr>
                            ) : hosts.length === 0 ? (
                                <tr>
                                    <td colSpan={10} className="px-4 py-8 text-center text-muted-foreground">
                                        <div className="flex flex-col items-center gap-3">
                                            <span className="material-symbols-outlined text-4xl text-muted-foreground">dns</span>
                                            <span className="text-lg font-medium">No hosts found</span>
                                            <span className="text-sm text-muted-foreground">Try adjusting your search criteria</span>
                                        </div>
                                    </td>
                                </tr>
                            ) : (
                                hosts.map((host) => (
                                    <tr key={host.id} className="border-t border-border-light dark:border-border-dark hover:bg-white/5 dark:hover:bg-white/5 transition-all duration-200">
                                        <td className="p-4">
                                            <input className="rounded bg-muted dark:bg-muted border-border-light dark:border-border-dark text-primary focus:ring-primary/50 focus:ring-offset-surface transition-all duration-200" type="checkbox" />
                                        </td>
                                        <td className={`px-4 py-2 text-sm font-normal leading-normal ${getStatusColor(host.currentStatus)}`}>
                                            <span className="flex items-center gap-2">
                                                <div className={`size-2.5 rounded-full bg-current ${host.currentStatus !== 'down' ? 'animate-pulse' : ''}`}></div>
                                                {host.statusDisplay || host.currentStatus}
                                            </span>
                                        </td>
                                        <td className="px-4 py-2 text-foreground dark:text-white text-sm font-normal leading-normal font-mono">{host.name}</td>
                                        <td className="px-4 py-2 text-muted-foreground dark:text-muted-foreground text-sm font-normal leading-normal font-mono">{host.primaryIp || 'N/A'}</td>
                                        <td className="px-4 py-2 text-sm font-normal leading-normal">
                                            <div
                                                className="bg-center bg-no-repeat aspect-square bg-cover rounded-full w-8 ring-2 ring-border-light dark:ring-border-dark"
                                                style={{ backgroundImage: `url("${getOsIconUrl(host.osType)}")` }}
                                            ></div>
                                        </td>
                                        <td className="px-4 py-2 text-muted-foreground dark:text-muted-foreground text-sm font-normal leading-normal">{host.location?.name || 'N/A'}</td>
                                        <td className="px-4 py-2 text-muted-foreground dark:text-muted-foreground text-sm font-normal leading-normal">{host.customer?.name || 'N/A'}</td>
                                        <td className="px-4 py-2 text-muted-foreground dark:text-muted-foreground text-sm font-normal leading-normal">
                                            {host.lastSeenAt ? formatDistanceToNow(new Date(host.lastSeenAt), { addSuffix: true, locale: tr }) : 'Never'}
                                        </td>
                                        <td className="px-4 py-2 text-sm font-normal leading-normal">
                                            <div className="flex items-center gap-3">
                                                <div className="w-24 overflow-hidden rounded-full bg-muted dark:bg-muted h-1.5">
                                                    <div
                                                        className={`h-full rounded-full transition-all duration-500 ${host.cpuPercent && host.cpuPercent > 80 ? 'bg-destructive' :
                                                                host.cpuPercent && host.cpuPercent > 60 ? 'bg-warning' :
                                                                    'bg-success'
                                                            }`}
                                                        style={{ width: `${host.cpuPercent || 0}%` }}
                                                    ></div>
                                                </div>
                                                <p className="text-foreground dark:text-white text-sm font-medium leading-normal">{host.cpuPercent || 0}%</p>
                                            </div>
                                        </td>
                                        <td className="px-4 py-2 text-muted-foreground dark:text-muted-foreground">
                                            <button className="p-2 hover:bg-white/10 dark:hover:bg-white/10 rounded-full transition-all duration-200 hover:scale-110">
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
            <div className="flex items-center justify-between p-4 mt-auto">
                <span className="text-sm text-muted-foreground dark:text-muted-foreground">
                    Showing {hosts.length} hosts
                </span>
                <div className="flex items-center gap-2">
                    <button
                        onClick={() => setPage(Math.max(1, page - 1))}
                        disabled={page === 1}
                        className="flex size-10 items-center justify-center text-muted-foreground dark:text-muted-foreground disabled:opacity-50 hover:bg-white/10 dark:hover:bg-white/10 rounded-lg transition-all duration-200"
                    >
                        <span className="material-symbols-outlined text-lg">chevron_left</span>
                    </button>

                    {[...Array(Math.min(5, totalPages))].map((_, i) => (
                        <button
                            key={i + 1}
                            onClick={() => setPage(i + 1)}
                            className={`text-sm font-normal leading-normal flex size-10 items-center justify-center rounded-lg transition-all duration-200 ${page === i + 1
                                    ? 'text-background-dark bg-primary font-bold shadow-lg'
                                    : 'text-muted-foreground dark:text-muted-foreground hover:bg-white/10 dark:hover:bg-white/10'
                                }`}
                        >
                            {i + 1}
                        </button>
                    ))}

                    {totalPages > 5 && <span className="text-sm font-normal leading-normal flex size-10 items-center justify-center text-muted-foreground dark:text-muted-foreground">...</span>}

                    <button
                        onClick={() => setPage(Math.min(totalPages, page + 1))}
                        disabled={page === totalPages}
                        className="flex size-10 items-center justify-center text-muted-foreground dark:text-muted-foreground disabled:opacity-50 hover:bg-white/10 dark:hover:bg-white/10 rounded-lg transition-all duration-200"
                    >
                        <span className="material-symbols-outlined text-lg">chevron_right</span>
                    </button>
                </div>
            </div>
        </div>
    );
}

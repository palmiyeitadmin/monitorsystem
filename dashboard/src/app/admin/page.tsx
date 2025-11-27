'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import type { DashboardSummary } from '@/lib/api';

export default function DashboardPage() {
    const [summary, setSummary] = useState<DashboardSummary | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [sidebarOpen, setSidebarOpen] = useState(true);

    useEffect(() => {
        loadDashboard();
    }, []);

    const loadDashboard = async () => {
        try {
            const data = await api.dashboard.getSummary();
            setSummary(data);
        } catch (err: any) {
            setError(err.message || 'Dashboard yüklenemedi');
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <div className="flex flex-col items-center gap-4">
                    <div className="w-12 h-12 border-4 border-primary/20 border-t-primary rounded-full animate-spin"></div>
                    <div className="text-white">Yükleniyor...</div>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <div className="text-red-400">{error}</div>
            </div>
        );
    }

    return (
        <>
            {/* Page Header */}
            <div className="flex flex-wrap justify-between gap-4 items-center mb-8">
                <h1 className="text-white text-4xl font-black leading-tight tracking-[-0.033em] min-w-72">Admin Dashboard</h1>
                <div className="flex flex-wrap gap-3">
                    <button className="flex items-center justify-center gap-2 h-10 px-4 rounded-lg bg-white/10 hover:bg-white/20 text-white text-sm font-medium transition-all duration-200 hover:scale-105">
                        <span className="material-symbols-outlined text-lg">add</span> Add Host
                    </button>
                    <button className="flex items-center justify-center gap-2 h-10 px-4 rounded-lg bg-white/10 hover:bg-white/20 text-white text-sm font-medium transition-all duration-200 hover:scale-105">
                        <span className="material-symbols-outlined text-lg">playlist_add_check</span> Add Check
                    </button>
                    <button className="flex items-center justify-center gap-2 h-10 px-4 rounded-lg bg-primary hover:bg-primary/90 text-background-dark text-sm font-bold transition-all duration-200 hover:scale-105 shadow-lg">
                        View All Incidents
                    </button>
                </div>
            </div>

            {/* Stats Cards */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
                <div className="stat-card flex flex-col gap-2 rounded-lg p-6 border border-white/10 bg-surface dark:bg-surface-dark hover:border-primary/30 transition-all duration-300 hover:shadow-lg hover:shadow-primary/10">
                    <div className="flex items-center justify-between">
                        <p className="text-gray-300 text-base font-medium leading-normal">Total Hosts</p>
                        <span className="material-symbols-outlined text-primary/60">dns</span>
                    </div>
                    <p className="text-white tracking-light text-3xl font-bold leading-tight">{summary?.totalHosts || 1_254}</p>
                    <div className="flex items-center gap-2 text-xs text-green-400">
                        <span className="material-symbols-outlined text-sm">trending_up</span>
                        <span>+12% from last month</span>
                    </div>
                </div>
                <div className="stat-card flex flex-col gap-2 rounded-lg p-6 border border-white/10 bg-surface dark:bg-surface-dark hover:border-primary/30 transition-all duration-300 hover:shadow-lg hover:shadow-primary/10">
                    <div className="flex items-center justify-between">
                        <p className="text-gray-300 text-base font-medium leading-normal">Services Monitored</p>
                        <span className="material-symbols-outlined text-primary/60">lan</span>
                    </div>
                    <p className="text-white tracking-light text-3xl font-bold leading-tight">{summary?.totalServices || 8_732}</p>
                    <div className="flex items-center gap-2 text-xs text-green-400">
                        <span className="material-symbols-outlined text-sm">trending_up</span>
                        <span>+8% from last month</span>
                    </div>
                </div>
                <div className="stat-card flex flex-col gap-2 rounded-lg p-6 border border-white/10 bg-surface dark:bg-surface-dark hover:border-danger/30 transition-all duration-300 hover:shadow-lg hover:shadow-danger/10">
                    <div className="flex items-center justify-between">
                        <p className="text-gray-300 text-base font-medium leading-normal">Active Incidents</p>
                        <span className="material-symbols-outlined text-danger/60">error</span>
                    </div>
                    <p className="text-danger tracking-light text-3xl font-bold leading-tight">{summary?.incidentsOpen || 3}</p>
                    <div className="flex items-center gap-2 text-xs text-danger">
                        <span className="material-symbols-outlined text-sm">trending_up</span>
                        <span>2 critical, 1 warning</span>
                    </div>
                </div>
                <div className="stat-card flex flex-col gap-2 rounded-lg p-6 border border-white/10 bg-surface dark:bg-surface-dark hover:border-success/30 transition-all duration-300 hover:shadow-lg hover:shadow-success/10">
                    <div className="flex items-center justify-between">
                        <p className="text-gray-300 text-base font-medium leading-normal">Uptime (30d)</p>
                        <span className="material-symbols-outlined text-success/60">check_circle</span>
                    </div>
                    <p className="text-success tracking-light text-3xl font-bold leading-tight">{summary?.uptime30d?.toFixed(2) || '99.98'}%</p>
                    <div className="flex items-center gap-2 text-xs text-success">
                        <span className="material-symbols-outlined text-sm">trending_up</span>
                        <span>+0.2% improvement</span>
                    </div>
                </div>
            </div>

            {/* System Health & Recent Incidents */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <div className="lg:col-span-2 flex flex-col gap-6">
                    {/* System Health */}
                    <div className="health-chart flex flex-col gap-4 rounded-lg border border-white/10 bg-surface dark:bg-surface-dark p-6 hover:border-primary/30 transition-all duration-300">
                        <div className="flex items-center justify-between">
                            <h3 className="text-white text-lg font-medium leading-normal">System Health</h3>
                            <div className="flex items-center gap-2">
                                <span className="w-2 h-2 rounded-full bg-success animate-pulse"></span>
                                <span className="text-xs text-success">Live</span>
                            </div>
                        </div>
                        <div className="flex items-baseline gap-4">
                            <p className="text-success tracking-light text-[32px] font-bold leading-tight truncate">Normal</p>
                            <div className="flex gap-1 items-center">
                                <p className="text-gray-400 text-sm font-normal leading-normal">Last 24 Hours</p>
                                <p className="text-success text-sm font-medium leading-normal">+0.2%</p>
                            </div>
                        </div>
                        <div className="flex min-h-[220px] flex-1 flex-col gap-8 py-4">
                            <svg fill="none" height="100%" preserveAspectRatio="none" viewBox="0 0 500 150" width="100%" xmlns="http://www.w3.org/2000/svg">
                                <defs>
                                    <linearGradient gradientUnits="userSpaceOnUse" id="paint0_linear_chart" x1="250" x2="250" y1="1" y2="149">
                                        <stop stopColor="#28aae2" stopOpacity="0.3"></stop>
                                        <stop offset="1" stopColor="#28aae2" stopOpacity="0"></stop>
                                    </linearGradient>
                                </defs>
                                <path d="M0 109C18.1538 109 18.1538 21 36.3077 21C54.4615 21 54.4615 41 72.6154 41C90.7692 41 90.7692 93 108.923 93C127.077 93 127.077 33 145.231 33C163.385 33 163.385 101 181.538 101C199.692 101 199.692 61 217.846 61C236 61 236 45 254.154 45C272.308 45 272.308 121 290.462 121C308.615 121 308.615 149 326.769 149C344.923 149 344.923 1 363.077 1C381.231 1 381.231 81 399.385 81C417.538 81 417.538 129 435.692 129C453.846 129 453.846 25 500 25V149H0V109Z" fill="url(#paint0_linear_chart)"></path>
                                <path d="M0 109C18.1538 109 18.1538 21 36.3077 21C54.4615 21 54.4615 41 72.6154 41C90.7692 41 90.7692 93 108.923 93C127.077 93 127.077 33 145.231 33C163.385 33 163.385 101 181.538 101C199.692 101 199.692 61 217.846 61C236 61 236 45 254.154 45C272.308 45 272.308 121 290.462 121C308.615 121 308.615 149 326.769 149C344.923 149 344.923 1 363.077 1C381.231 1 381.231 81 399.385 81C417.538 81 417.538 129 435.692 129C453.846 129 453.846 25 500 25" stroke="#28aae2" strokeLinecap="round" strokeWidth="3"></path>
                            </svg>
                            <div className="flex justify-between -mt-4">
                                <p className="text-gray-400 text-xs font-bold">24h ago</p>
                                <p className="text-gray-400 text-xs font-bold">18h</p>
                                <p className="text-gray-400 text-xs font-bold">12h</p>
                                <p className="text-gray-400 text-xs font-bold">6h</p>
                                <p className="text-gray-400 text-xs font-bold">Now</p>
                            </div>
                        </div>
                    </div>

                    {/* Recent Incidents */}
                    <div className="incidents-table flex flex-col gap-4 rounded-lg border border-white/10 bg-surface dark:bg-surface-dark p-6 hover:border-primary/30 transition-all duration-300">
                        <div className="flex items-center justify-between">
                            <h3 className="text-white text-lg font-medium leading-normal">Recent Incidents</h3>
                            <button className="text-primary hover:text-primary/80 text-sm font-medium">View All</button>
                        </div>
                        <div className="overflow-x-auto">
                            <table className="w-full text-left">
                                <thead>
                                    <tr className="border-b border-white/10">
                                        <th className="py-3 px-4 text-sm font-semibold text-gray-300">ID</th>
                                        <th className="py-3 px-4 text-sm font-semibold text-gray-300">Service</th>
                                        <th className="py-3 px-4 text-sm font-semibold text-gray-300">Status</th>
                                        <th className="py-3 px-4 text-sm font-semibold text-gray-300">Timestamp</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr className="border-b border-white/10 hover:bg-white/5 transition-colors duration-200">
                                        <td className="py-3 px-4 text-sm text-gray-200 font-mono">INC-84321</td>
                                        <td className="py-3 px-4 text-sm text-gray-200">API Gateway</td>
                                        <td className="py-3 px-4 text-sm">
                                            <span className="inline-flex items-center gap-1.5 rounded-full bg-danger/20 px-2 py-1 text-xs font-medium text-danger">
                                                <span className="w-1.5 h-1.5 rounded-full bg-danger animate-pulse"></span>
                                                Critical
                                            </span>
                                        </td>
                                        <td className="py-3 px-4 text-sm text-gray-400">2 min ago</td>
                                    </tr>
                                    <tr className="border-b border-white/10 hover:bg-white/5 transition-colors duration-200">
                                        <td className="py-3 px-4 text-sm text-gray-200 font-mono">INC-84320</td>
                                        <td className="py-3 px-4 text-sm text-gray-200">Authentication Service</td>
                                        <td className="py-3 px-4 text-sm">
                                            <span className="inline-flex items-center gap-1.5 rounded-full bg-danger/20 px-2 py-1 text-xs font-medium text-danger">
                                                <span className="w-1.5 h-1.5 rounded-full bg-danger animate-pulse"></span>
                                                Critical
                                            </span>
                                        </td>
                                        <td className="py-3 px-4 text-sm text-gray-400">5 min ago</td>
                                    </tr>
                                    <tr className="border-b border-white/10 hover:bg-white/5 transition-colors duration-200">
                                        <td className="py-3 px-4 text-sm text-gray-200 font-mono">INC-84319</td>
                                        <td className="py-3 px-4 text-sm text-gray-200">Database Cluster #3</td>
                                        <td className="py-3 px-4 text-sm">
                                            <span className="inline-flex items-center gap-1.5 rounded-full bg-warning/20 px-2 py-1 text-xs font-medium text-warning">
                                                <span className="w-1.5 h-1.5 rounded-full bg-warning animate-pulse"></span>
                                                Warning
                                            </span>
                                        </td>
                                        <td className="py-3 px-4 text-sm text-gray-400">45 min ago</td>
                                    </tr>
                                    <tr className="hover:bg-white/5 transition-colors duration-200">
                                        <td className="py-3 px-4 text-sm text-gray-200 font-mono">INC-84318</td>
                                        <td className="py-3 px-4 text-sm text-gray-200">Load Balancer EU-West-1</td>
                                        <td className="py-3 px-4 text-sm">
                                            <span className="inline-flex items-center gap-1.5 rounded-full bg-success/20 px-2 py-1 text-xs font-medium text-success">
                                                <span className="w-1.5 h-1.5 rounded-full bg-success"></span>
                                                Resolved
                                            </span>
                                        </td>
                                        <td className="py-3 px-4 text-sm text-gray-400">2 hours ago</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>

                {/* Datacenter Status */}
                <div className="flex flex-col gap-6">
                    <div className="datacenter-status flex flex-col gap-4 rounded-lg border border-white/10 bg-surface dark:bg-surface-dark p-6 hover:border-primary/30 transition-all duration-300">
                        <h3 className="text-white text-lg font-medium leading-normal">Datacenter Status</h3>
                        <div className="w-full bg-center bg-no-repeat aspect-video bg-cover rounded-lg object-cover" style={{ backgroundImage: 'url("https://lh3.googleusercontent.com/aida-public/AB6AXuAWoYjoPTtZWKyewb9K4xMKkQ3skzcLVcSN30Y7yETuH04Rk3wChxHTwj6fXmXdm1KWGGkGWS2lfuf8bpKyz74qZjGb7ICMU-S10daeW9yck77714VOyXq8Vpng4mGuHrk_nPrxyO25GkCIZWUiQsxl5ejR5adboy4CHq8f9qzfMlAPpyig5_Rc_pWk3LyMl62ZHPEzwm2_reXs5U_5m98wnQlSimeUGrFiZSOEUMpyseSSLKi0nHdR2bUgpMDJAcQmd1cP-kJ9Z2Q")' }}>
                            <div className="w-full h-full flex items-center justify-center bg-black/20 rounded-lg">
                                <span className="text-white text-sm font-medium">World Map</span>
                            </div>
                        </div>
                        <div className="flex flex-col gap-2 mt-2">
                            <div className="flex items-center justify-between text-sm">
                                <div className="flex items-center gap-2">
                                    <span className="size-2 rounded-full bg-success animate-pulse"></span>
                                    <span>Operational</span>
                                </div>
                                <span className="font-medium">12</span>
                            </div>
                            <div className="flex items-center justify-between text-sm">
                                <div className="flex items-center gap-2">
                                    <span className="size-2 rounded-full bg-warning animate-pulse"></span>
                                    <span>Degraded</span>
                                </div>
                                <span className="font-medium">1</span>
                            </div>
                            <div className="flex items-center justify-between text-sm">
                                <div className="flex items-center gap-2">
                                    <span className="size-2 rounded-full bg-danger animate-pulse"></span>
                                    <span>Outage</span>
                                </div>
                                <span className="font-medium">2</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}

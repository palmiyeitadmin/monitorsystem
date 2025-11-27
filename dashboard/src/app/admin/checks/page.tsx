'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import type { Check, PagedResponse } from '@/lib/api';

interface CheckData {
    id: string;
    name: string;
    type: 'http' | 'ping' | 'tcp' | 'ssl' | 'dns';
    url?: string;
    target?: string;
    status: 'passing' | 'failing' | 'warning';
    lastCheck: Date;
    interval: number;
    timeout: number;
    retries: number;
    notifications: boolean;
    tags: string[];
}

export default function ChecksPage() {
    const [checks, setChecks] = useState<CheckData[]>([
        {
            id: '1',
            name: 'API Gateway Health',
            type: 'http',
            url: 'https://api.example.com/health',
            status: 'passing',
            lastCheck: new Date(Date.now() - 2 * 60 * 1000),
            interval: 60,
            timeout: 10,
            retries: 3,
            notifications: true,
            tags: ['api', 'critical'],
        },
        {
            id: '2',
            name: 'Database Connection',
            type: 'tcp',
            target: 'db.example.com:5432',
            status: 'passing',
            lastCheck: new Date(Date.now() - 1 * 60 * 1000),
            interval: 30,
            timeout: 5,
            retries: 2,
            notifications: true,
            tags: ['database', 'critical'],
        },
        {
            id: '3',
            name: 'Website Homepage',
            type: 'http',
            url: 'https://example.com',
            status: 'failing',
            lastCheck: new Date(Date.now() - 5 * 60 * 1000),
            interval: 60,
            timeout: 10,
            retries: 3,
            notifications: true,
            tags: ['website'],
        },
        {
            id: '4',
            name: 'SSL Certificate',
            type: 'ssl',
            target: 'example.com',
            status: 'warning',
            lastCheck: new Date(Date.now() - 30 * 60 * 1000),
            interval: 3600,
            timeout: 10,
            retries: 1,
            notifications: true,
            tags: ['security'],
        },
        {
            id: '5',
            name: 'DNS Resolution',
            type: 'dns',
            target: 'example.com',
            status: 'passing',
            lastCheck: new Date(Date.now() - 15 * 60 * 1000),
            interval: 300,
            timeout: 5,
            retries: 2,
            notifications: false,
            tags: ['network'],
        },
    ]);
    const [loading, setLoading] = useState(false);
    const [activeTab, setActiveTab] = useState('all');
    const [searchTerm, setSearchTerm] = useState('');
    const [statusFilter, setStatusFilter] = useState('all');
    const [showCreateModal, setShowCreateModal] = useState(false);

    const getStatusColor = (status: string) => {
        switch (status?.toLowerCase()) {
            case 'passing':
                return 'bg-green-500/20 text-green-500 border-green-500/30';
            case 'failing':
                return 'bg-red-500/20 text-red-500 border-red-500/30';
            case 'warning':
                return 'bg-yellow-500/20 text-yellow-500 border-yellow-500/30';
            default:
                return 'bg-gray-500/20 text-gray-500 border-gray-500/30';
        }
    };

    const getStatusIcon = (status: string) => {
        switch (status?.toLowerCase()) {
            case 'passing':
                return 'check_circle';
            case 'failing':
                return 'error';
            case 'warning':
                return 'warning';
            default:
                return 'help';
        }
    };

    const getTypeIcon = (type: string) => {
        switch (type?.toLowerCase()) {
            case 'http':
                return 'language';
            case 'ping':
                return 'network_ping';
            case 'tcp':
                return 'lan';
            case 'ssl':
                return 'lock';
            case 'dns':
                return 'dns';
            default:
                return 'help';
        }
    };

    const getRelativeTime = (date: Date) => {
        const now = new Date();
        const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);

        if (diffInSeconds < 60) {
            return 'just now';
        } else if (diffInSeconds < 3600) {
            const minutes = Math.floor(diffInSeconds / 60);
            return `${minutes}m ago`;
        } else if (diffInSeconds < 86400) {
            const hours = Math.floor(diffInSeconds / 3600);
            return `${hours}h ago`;
        } else {
            const days = Math.floor(diffInSeconds / 86400);
            return `${days}d ago`;
        }
    };

    const filteredChecks = checks.filter(check => {
        const matchesSearch = check.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
            check.tags.some(tag => tag.toLowerCase().includes(searchTerm.toLowerCase()));

        const matchesStatus = statusFilter === 'all' || check.status === statusFilter;

        return matchesSearch && matchesStatus;
    });

    const CreateCheckModal = ({ onClose }: { onClose: () => void }) => {
        const [formData, setFormData] = useState({
            name: '',
            type: 'http',
            url: '',
            target: '',
            interval: 60,
            timeout: 10,
            retries: 3,
            notifications: true,
            tags: '',
        });

        const handleSubmit = (e: React.FormEvent) => {
            e.preventDefault();
            // Handle form submission here
            console.log('Creating check:', formData);
            onClose();
        };

        return (
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
                <div className="bg-[#1b2b32] rounded-lg p-6 w-full max-w-md border border-[#365663]">
                    <div className="flex justify-between items-center mb-4">
                        <h2 className="text-white text-xl font-bold">Create New Check</h2>
                        <button onClick={onClose} className="text-gray-400 hover:text-white">
                            <span className="material-symbols-outlined">close</span>
                        </button>
                    </div>

                    <form onSubmit={handleSubmit} className="space-y-4">
                        <div>
                            <label className="text-gray-400 text-sm">Check Name</label>
                            <input
                                type="text"
                                value={formData.name}
                                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                className="w-full mt-1 px-3 py-2 bg-[#121d21] border border-[#365663] rounded-lg text-white focus:outline-none focus:border-[#28aae2]"
                                required
                            />
                        </div>

                        <div>
                            <label className="text-gray-400 text-sm">Type</label>
                            <select
                                value={formData.type}
                                onChange={(e) => setFormData({ ...formData, type: e.target.value })}
                                className="w-full mt-1 px-3 py-2 bg-[#121d21] border border-[#365663] rounded-lg text-white focus:outline-none focus:border-[#28aae2]"
                            >
                                <option value="http">HTTP</option>
                                <option value="ping">Ping</option>
                                <option value="tcp">TCP</option>
                                <option value="ssl">SSL</option>
                                <option value="dns">DNS</option>
                            </select>
                        </div>

                        {formData.type === 'http' ? (
                            <div>
                                <label className="text-gray-400 text-sm">URL</label>
                                <input
                                    type="url"
                                    value={formData.url}
                                    onChange={(e) => setFormData({ ...formData, url: e.target.value })}
                                    className="w-full mt-1 px-3 py-2 bg-[#121d21] border border-[#365663] rounded-lg text-white focus:outline-none focus:border-[#28aae2]"
                                    required
                                />
                            </div>
                        ) : (
                            <div>
                                <label className="text-gray-400 text-sm">Target</label>
                                <input
                                    type="text"
                                    value={formData.target}
                                    onChange={(e) => setFormData({ ...formData, target: e.target.value })}
                                    className="w-full mt-1 px-3 py-2 bg-[#121d21] border border-[#365663] rounded-lg text-white focus:outline-none focus:border-[#28aae2]"
                                    required
                                />
                            </div>
                        )}

                        <div className="grid grid-cols-3 gap-4">
                            <div>
                                <label className="text-gray-400 text-sm">Interval (s)</label>
                                <input
                                    type="number"
                                    value={formData.interval}
                                    onChange={(e) => setFormData({ ...formData, interval: parseInt(e.target.value) })}
                                    className="w-full mt-1 px-3 py-2 bg-[#121d21] border border-[#365663] rounded-lg text-white focus:outline-none focus:border-[#28aae2]"
                                    min="10"
                                    required
                                />
                            </div>

                            <div>
                                <label className="text-gray-400 text-sm">Timeout (s)</label>
                                <input
                                    type="number"
                                    value={formData.timeout}
                                    onChange={(e) => setFormData({ ...formData, timeout: parseInt(e.target.value) })}
                                    className="w-full mt-1 px-3 py-2 bg-[#121d21] border border-[#365663] rounded-lg text-white focus:outline-none focus:border-[#28aae2]"
                                    min="1"
                                    required
                                />
                            </div>

                            <div>
                                <label className="text-gray-400 text-sm">Retries</label>
                                <input
                                    type="number"
                                    value={formData.retries}
                                    onChange={(e) => setFormData({ ...formData, retries: parseInt(e.target.value) })}
                                    className="w-full mt-1 px-3 py-2 bg-[#121d21] border border-[#365663] rounded-lg text-white focus:outline-none focus:border-[#28aae2]"
                                    min="0"
                                    max="10"
                                    required
                                />
                            </div>
                        </div>

                        <div>
                            <label className="text-gray-400 text-sm">Tags (comma separated)</label>
                            <input
                                type="text"
                                value={formData.tags}
                                onChange={(e) => setFormData({ ...formData, tags: e.target.value })}
                                className="w-full mt-1 px-3 py-2 bg-[#121d21] border border-[#365663] rounded-lg text-white focus:outline-none focus:border-[#28aae2]"
                                placeholder="api, critical, production"
                            />
                        </div>

                        <div className="flex items-center gap-2">
                            <input
                                type="checkbox"
                                id="notifications"
                                checked={formData.notifications}
                                onChange={(e) => setFormData({ ...formData, notifications: e.target.checked })}
                                className="w-4 h-4 rounded border-[#365663] bg-[#121d21] text-[#28aae2] focus:ring-[#28aae2]"
                            />
                            <label htmlFor="notifications" className="text-white text-sm">
                                Enable notifications
                            </label>
                        </div>

                        <div className="flex gap-3 mt-6">
                            <button
                                type="submit"
                                className="flex-1 px-4 py-2 bg-[#28aae2] hover:bg-[#2196d4] text-[#121d21] text-sm font-bold rounded-lg transition-colors"
                            >
                                Create Check
                            </button>
                            <button
                                type="button"
                                onClick={onClose}
                                className="flex-1 px-4 py-2 bg-[#365663] hover:bg-[#445566] text-white text-sm font-medium rounded-lg transition-colors"
                            >
                                Cancel
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        );
    };

    return (
        <>
            {/* Page Header */}
            <div className="flex flex-wrap justify-between items-center gap-4 mb-6">
                <h1 className="text-white text-4xl font-black tracking-[-0.033em]">Checks Management</h1>
                <button
                    onClick={() => setShowCreateModal(true)}
                    className="flex items-center gap-2 h-10 px-4 rounded-lg bg-[#28aae2] hover:bg-[#2196d4] text-[#121d21] text-sm font-bold transition-colors"
                >
                    <span className="material-symbols-outlined fill">add</span>
                    Create Check
                </button>
            </div>

            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
                <div className="bg-[#1b2b32] rounded-lg p-4 border border-[#365663]">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-gray-400 text-sm">Total Checks</p>
                            <p className="text-white text-2xl font-bold">{checks.length}</p>
                        </div>
                        <div className="w-12 h-12 bg-[#28aae2]/20 rounded-lg flex items-center justify-center">
                            <span className="material-symbols-outlined text-[#28aae2]">monitoring</span>
                        </div>
                    </div>
                </div>

                <div className="bg-[#1b2b32] rounded-lg p-4 border border-[#365663]">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-gray-400 text-sm">Passing</p>
                            <p className="text-white text-2xl font-bold">{checks.filter(c => c.status === 'passing').length}</p>
                        </div>
                        <div className="w-12 h-12 bg-green-500/20 rounded-lg flex items-center justify-center">
                            <span className="material-symbols-outlined text-green-500">check_circle</span>
                        </div>
                    </div>
                </div>

                <div className="bg-[#1b2b32] rounded-lg p-4 border border-[#365663]">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-gray-400 text-sm">Failing</p>
                            <p className="text-white text-2xl font-bold">{checks.filter(c => c.status === 'failing').length}</p>
                        </div>
                        <div className="w-12 h-12 bg-red-500/20 rounded-lg flex items-center justify-center">
                            <span className="material-symbols-outlined text-red-500">error</span>
                        </div>
                    </div>
                </div>

                <div className="bg-[#1b2b32] rounded-lg p-4 border border-[#365663]">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-gray-400 text-sm">Warnings</p>
                            <p className="text-white text-2xl font-bold">{checks.filter(c => c.status === 'warning').length}</p>
                        </div>
                        <div className="w-12 h-12 bg-yellow-500/20 rounded-lg flex items-center justify-center">
                            <span className="material-symbols-outlined text-yellow-500">warning</span>
                        </div>
                    </div>
                </div>
            </div>

            {/* Tabs */}
            <div className="border-b border-[#365663] mb-6">
                <nav className="flex space-x-8">
                    <button
                        onClick={() => setActiveTab('all')}
                        className={`py-2 px-1 border-b-2 font-medium text-sm transition-colors ${activeTab === 'all'
                                ? 'border-[#28aae2] text-white'
                                : 'border-transparent text-gray-400 hover:text-white'
                            }`}
                    >
                        All Checks
                    </button>
                    <button
                        onClick={() => setActiveTab('http')}
                        className={`py-2 px-1 border-b-2 font-medium text-sm transition-colors ${activeTab === 'http'
                                ? 'border-[#28aae2] text-white'
                                : 'border-transparent text-gray-400 hover:text-white'
                            }`}
                    >
                        HTTP
                    </button>
                    <button
                        onClick={() => setActiveTab('ping')}
                        className={`py-2 px-1 border-b-2 font-medium text-sm transition-colors ${activeTab === 'ping'
                                ? 'border-[#28aae2] text-white'
                                : 'border-transparent text-gray-400 hover:text-white'
                            }`}
                    >
                        Ping
                    </button>
                    <button
                        onClick={() => setActiveTab('tcp')}
                        className={`py-2 px-1 border-b-2 font-medium text-sm transition-colors ${activeTab === 'tcp'
                                ? 'border-[#28aae2] text-white'
                                : 'border-transparent text-gray-400 hover:text-white'
                            }`}
                    >
                        TCP
                    </button>
                    <button
                        onClick={() => setActiveTab('ssl')}
                        className={`py-2 px-1 border-b-2 font-medium text-sm transition-colors ${activeTab === 'ssl'
                                ? 'border-[#28aae2] text-white'
                                : 'border-transparent text-gray-400 hover:text-white'
                            }`}
                    >
                        SSL
                    </button>
                    <button
                        onClick={() => setActiveTab('dns')}
                        className={`py-2 px-1 border-b-2 font-medium text-sm transition-colors ${activeTab === 'dns'
                                ? 'border-[#28aae2] text-white'
                                : 'border-transparent text-gray-400 hover:text-white'
                            }`}
                    >
                        DNS
                    </button>
                </nav>
            </div>

            {/* Filters */}
            <div className="flex flex-wrap gap-4 mb-6">
                <div className="flex-1 min-w-[200px]">
                    <div className="relative">
                        <span className="material-symbols-outlined absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 text-sm">
                            search
                        </span>
                        <input
                            type="text"
                            placeholder="Search checks..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2 bg-[#1b2b32] border border-[#365663] rounded-lg text-white placeholder-gray-400 focus:outline-none focus:border-[#28aae2] transition-colors"
                        />
                    </div>
                </div>

                <select
                    value={statusFilter}
                    onChange={(e) => setStatusFilter(e.target.value)}
                    className="px-4 py-2 bg-[#1b2b32] border border-[#365663] rounded-lg text-white focus:outline-none focus:border-[#28aae2] transition-colors"
                >
                    <option value="all">All Status</option>
                    <option value="passing">Passing</option>
                    <option value="failing">Failing</option>
                    <option value="warning">Warning</option>
                </select>
            </div>

            {/* Checks List */}
            <div className="overflow-hidden rounded-lg border border-[#365663] bg-[#121d21]">
                {loading ? (
                    <div className="px-4 py-8 text-center text-gray-400">Loading...</div>
                ) : filteredChecks.length === 0 ? (
                    <div className="px-4 py-8 text-center text-gray-400">No checks found</div>
                ) : (
                    <div className="divide-y divide-[#365663]">
                        {filteredChecks.map((check) => (
                            <div key={check.id} className="p-4 hover:bg-[#1b2b32]/50 transition-colors">
                                <div className="flex items-start justify-between">
                                    <div className="flex items-start gap-3">
                                        <div className={`w-10 h-10 rounded-lg flex items-center justify-center ${getStatusColor(check.status)}`}>
                                            <span className="material-symbols-outlined text-lg">
                                                {getStatusIcon(check.status)}
                                            </span>
                                        </div>

                                        <div className="flex-1">
                                            <div className="flex items-center gap-2 mb-1">
                                                <h3 className="text-white font-medium">{check.name}</h3>
                                                <span className={`inline-flex items-center gap-1 px-2 py-1 rounded text-xs font-medium ${getStatusColor(check.status)}`}>
                                                    {check.status}
                                                </span>
                                            </div>

                                            <div className="flex items-center gap-4 text-sm text-gray-400 mb-2">
                                                <div className="flex items-center gap-1">
                                                    <span className="material-symbols-outlined text-sm">
                                                        {getTypeIcon(check.type)}
                                                    </span>
                                                    <span className="uppercase">{check.type}</span>
                                                </div>

                                                <span>•</span>

                                                <span>Every {check.interval}s</span>

                                                <span>•</span>

                                                <span>Timeout: {check.timeout}s</span>
                                            </div>

                                            <div className="text-sm text-[#95b7c6]">
                                                {check.url || check.target}
                                            </div>

                                            {check.tags.length > 0 && (
                                                <div className="flex gap-2 mt-2">
                                                    {check.tags.map((tag, index) => (
                                                        <span key={index} className="px-2 py-1 bg-[#365663] text-gray-300 text-xs rounded">
                                                            {tag}
                                                        </span>
                                                    ))}
                                                </div>
                                            )}
                                        </div>
                                    </div>

                                    <div className="flex items-center gap-2">
                                        <button className="p-2 hover:bg-[#253c46] rounded-full text-[#95b7c6] transition-colors">
                                            <span className="material-symbols-outlined">edit</span>
                                        </button>
                                        <button className="p-2 hover:bg-[#253c46] rounded-full text-[#95b7c6] transition-colors">
                                            <span className="material-symbols-outlined">more_vert</span>
                                        </button>
                                    </div>
                                </div>

                                <div className="mt-3 text-xs text-gray-500">
                                    Last checked: {getRelativeTime(check.lastCheck)}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>

            {/* Create Check Modal */}
            {showCreateModal && (
                <CreateCheckModal onClose={() => setShowCreateModal(false)} />
            )}
        </>
    );
}

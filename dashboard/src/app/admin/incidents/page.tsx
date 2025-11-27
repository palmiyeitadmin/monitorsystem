'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import type { Incident, PagedResponse } from '@/lib/api';

interface IncidentData {
    id: string;
    title: string;
    severity: 'critical' | 'high' | 'medium' | 'low';
    status: 'new' | 'acknowledged' | 'in_progress' | 'resolved';
    service: string;
    createdAt: Date;
    assignedTo?: string;
    assignedToAvatar?: string;
}

export default function IncidentsPage() {
    const [incidents, setIncidents] = useState<IncidentData[]>([
        {
            id: '84321',
            title: 'Host DOWN: PROD-WEB-01',
            severity: 'critical',
            status: 'new',
            service: 'API Gateway',
            createdAt: new Date(Date.now() - 2 * 60 * 1000),
            assignedTo: 'John Doe',
            assignedToAvatar: 'https://picsum.photos/seed/user1/40/40.jpg',
        },
        {
            id: '84320',
            title: 'High CPU: DB-MASTER-03',
            severity: 'critical',
            status: 'new',
            service: 'Authentication Service',
            createdAt: new Date(Date.now() - 5 * 60 * 1000),
            assignedTo: 'Jane Smith',
        },
        {
            id: '84319',
            title: 'Database Cluster #3',
            severity: 'high',
            status: 'new',
            service: 'Database Cluster #3',
            createdAt: new Date(Date.now() - 45 * 60 * 1000),
        },
        {
            id: '84318',
            title: 'Service Unreachable: AUTH-01',
            severity: 'high',
            status: 'acknowledged',
            service: 'Authentication Service',
            createdAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000),
            assignedTo: 'Mike Johnson',
            assignedToAvatar: 'https://picsum.photos/seed/user2/40/40.jpg',
        },
        {
            id: '84317',
            title: 'Disk Space Alert: BACKUP-02',
            severity: 'medium',
            status: 'acknowledged',
            service: 'Backup Service',
            createdAt: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000),
            assignedTo: 'Sarah Wilson',
            assignedToAvatar: 'https://picsum.photos/seed/user3/40/40.jpg',
        },
        {
            id: '84316',
            title: 'Login Service Failure',
            severity: 'low',
            status: 'in_progress',
            service: 'Authentication Service',
            createdAt: new Date(Date.now() - 4 * 24 * 60 * 60 * 1000),
            assignedTo: 'Tom Brown',
            assignedToAvatar: 'https://picsum.photos/seed/user4/40/40.jpg',
        },
    ]);
    const [loading, setLoading] = useState(false);

    const getSeverityColor = (severity: string) => {
        switch (severity?.toLowerCase()) {
            case 'critical':
                return 'bg-red-500/20 text-red-500 border-red-500/30';
            case 'high':
                return 'bg-orange-500/20 text-orange-500 border-orange-500/30';
            case 'medium':
                return 'bg-yellow-500/20 text-yellow-500 border-yellow-500/30';
            case 'low':
                return 'bg-blue-500/20 text-blue-500 border-blue-500/30';
            default:
                return 'bg-gray-500/20 text-gray-500 border-gray-500/30';
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

    const getIncidentsByStatus = (status: string) => {
        return incidents.filter(incident => incident.status === status);
    };

    const IncidentCard = ({ incident }: { incident: IncidentData }) => (
        <div className="bg-[#1b2b32] rounded-lg p-4 mb-3 border border-[#365663] hover:border-[#28aae2]/50 transition-colors cursor-pointer">
            <div className="flex items-start justify-between mb-2">
                <span className={`inline-flex items-center gap-1.5 px-2 py-1 rounded-full text-xs font-medium border ${getSeverityColor(incident.severity)}`}>
                    <span className="w-1.5 h-1.5 rounded-full bg-current"></span>
                    {incident.severity}
                </span>
                <button className="text-gray-400 hover:text-white">
                    <span className="material-symbols-outlined text-sm">more_horiz</span>
                </button>
            </div>

            <h3 className="text-white font-medium mb-2">{incident.title}</h3>

            <div className="text-xs text-gray-400 mb-3">
                <span>#{incident.id}</span> • <span>{incident.service}</span>
            </div>

            {incident.assignedTo && (
                <div className="flex items-center gap-2 mt-3 pt-3 border-t border-[#365663]">
                    {incident.assignedToAvatar ? (
                        <img
                            src={incident.assignedToAvatar}
                            alt={incident.assignedTo}
                            className="w-6 h-6 rounded-full"
                        />
                    ) : (
                        <div className="w-6 h-6 rounded-full bg-[#28aae2]/20 text-[#28aae2] flex items-center justify-center text-xs font-medium">
                            {incident.assignedTo.charAt(0)}
                        </div>
                    )}
                    <span className="text-xs text-gray-400">{incident.assignedTo}</span>
                </div>
            )}

            <div className="text-xs text-gray-500 mt-2">
                {getRelativeTime(incident.createdAt)}
            </div>
        </div>
    );

    const KanbanColumn = ({ title, status, count, color }: { title: string; status: string; count: number; color: string }) => (
        <div className="flex-1 min-w-[280px]">
            <div className="flex items-center justify-between mb-4">
                <h2 className="text-white font-medium flex items-center gap-2">
                    <span className={`w-2 h-2 rounded-full ${color}`}></span>
                    {title}
                </h2>
                <span className="text-gray-400 text-sm">{count}</span>
            </div>

            <div className="min-h-[400px] bg-[#121d21]/50 rounded-lg p-3 border border-[#365663]">
                {getIncidentsByStatus(status).map(incident => (
                    <IncidentCard key={incident.id} incident={incident} />
                ))}

                {getIncidentsByStatus(status).length === 0 && (
                    <div className="text-center py-8 text-gray-500 text-sm">
                        No incidents
                    </div>
                )}
            </div>
        </div>
    );

    return (
        <>
            {/* Page Header */}
            <div className="flex flex-wrap justify-between items-center gap-4 mb-6">
                <h1 className="text-white text-4xl font-black tracking-[-0.033em]">Incidents Management</h1>
                <button className="flex items-center gap-2 h-10 px-4 rounded-lg bg-[#28aae2] hover:bg-[#2196d4] text-[#121d21] text-sm font-bold transition-colors">
                    <span className="material-symbols-outlined fill">add</span>
                    Create Incident
                </button>
            </div>

            {/* Kanban Board */}
            <div className="flex gap-4 overflow-x-auto pb-4">
                <KanbanColumn
                    title="New"
                    status="new"
                    count={getIncidentsByStatus('new').length}
                    color="bg-red-500"
                />
                <KanbanColumn
                    title="Acknowledged"
                    status="acknowledged"
                    count={getIncidentsByStatus('acknowledged').length}
                    color="bg-yellow-500"
                />
                <KanbanColumn
                    title="In Progress"
                    status="in_progress"
                    count={getIncidentsByStatus('in_progress').length}
                    color="bg-blue-500"
                />
                <KanbanColumn
                    title="Resolved"
                    status="resolved"
                    count={getIncidentsByStatus('resolved').length}
                    color="bg-green-500"
                />
            </div>
        </>
    );
}

'use client';

import { useQuery } from '@tanstack/react-query';
import { Server, Activity, Globe, AlertTriangle } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { cn } from '@/lib/utils';
import { apiClient } from '@/lib/api/client';

interface StatusOverviewData {
    totalHosts: number;
    hostsUp: number;
    hostsDown: number;
    hostsWarning: number;
    totalServices: number;
    servicesUp: number;
    servicesDown: number;
    totalChecks: number;
    checksPassing: number;
    checksFailing: number;
    openIncidents: number;
    criticalIncidents: number;
    overallHealth: number;
}

async function fetchStatusOverview(): Promise<StatusOverviewData> {
    return apiClient.get<StatusOverviewData>('/api/dashboard/status-overview');
}

export function StatusOverview() {
    const { data, isLoading, error } = useQuery({
        queryKey: ['dashboard', 'status-overview'],
        queryFn: fetchStatusOverview,
        refetchInterval: 30000, // Refresh every 30 seconds
    });

    if (isLoading) {
        return <StatusOverviewSkeleton />;
    }

    if (error) {
        return (
            <Card className="col-span-full">
                <CardContent className="p-6">
                    <p className="text-destructive">Failed to load status overview</p>
                </CardContent>
            </Card>
        );
    }

    const stats = [
        {
            name: 'Hosts',
            icon: Server,
            total: data?.totalHosts || 0,
            up: data?.hostsUp || 0,
            down: data?.hostsDown || 0,
            warning: data?.hostsWarning || 0,
        },
        {
            name: 'Services',
            icon: Activity,
            total: data?.totalServices || 0,
            up: data?.servicesUp || 0,
            down: data?.servicesDown || 0,
        },
        {
            name: 'Checks',
            icon: Globe,
            total: data?.totalChecks || 0,
            up: data?.checksPassing || 0,
            down: data?.checksFailing || 0,
        },
        {
            name: 'Incidents',
            icon: AlertTriangle,
            total: data?.openIncidents || 0,
            critical: data?.criticalIncidents || 0,
        },
    ];

    return (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            {stats.map((stat) => (
                <Card key={stat.name}>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">{stat.name}</CardTitle>
                        <stat.icon className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{stat.total}</div>
                        <div className="flex items-center gap-2 mt-1 text-xs text-muted-foreground">
                            {stat.up !== undefined && (
                                <span className="flex items-center gap-1">
                                    <span className="h-2 w-2 rounded-full bg-status-up" />
                                    {stat.up} up
                                </span>
                            )}
                            {stat.down !== undefined && stat.down > 0 && (
                                <span className="flex items-center gap-1">
                                    <span className="h-2 w-2 rounded-full bg-status-down" />
                                    {stat.down} down
                                </span>
                            )}
                            {stat.warning !== undefined && stat.warning > 0 && (
                                <span className="flex items-center gap-1">
                                    <span className="h-2 w-2 rounded-full bg-status-warning" />
                                    {stat.warning} warning
                                </span>
                            )}
                            {stat.critical !== undefined && stat.critical > 0 && (
                                <span className="flex items-center gap-1 text-destructive">
                                    {stat.critical} critical
                                </span>
                            )}
                        </div>
                    </CardContent>
                </Card>
            ))}
        </div>
    );
}

function StatusOverviewSkeleton() {
    return (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            {[...Array(4)].map((_, i) => (
                <Card key={i}>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <Skeleton className="h-4 w-20" />
                        <Skeleton className="h-4 w-4" />
                    </CardHeader>
                    <CardContent>
                        <Skeleton className="h-8 w-16" />
                        <Skeleton className="h-3 w-32 mt-2" />
                    </CardContent>
                </Card>
            ))}
        </div>
    );
}

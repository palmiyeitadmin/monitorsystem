'use client';

import { use } from 'react';
import { useQuery } from '@tanstack/react-query';
import { hostsApi } from '@/lib/api/hosts';
import { HostHeader } from '@/components/hosts/host-header';
import { HostMetrics } from '@/components/hosts/host-metrics';
import { DiskList } from '@/components/hosts/host-disks';
import { HostSettingsForm } from '@/components/hosts/host-settings-form';
import { HostApplications } from '@/components/hosts/host-applications';
import { HostEventLogs } from '@/components/hosts/host-event-logs';
import { ServiceList } from '@/components/services/service-list';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { useHostSignalR } from '@/hooks/use-signalr';
import { notFound, useSearchParams } from 'next/navigation';

interface HostPageProps {
    params: Promise<{
        id: string;
    }>;
}

export default function HostPage({ params }: HostPageProps) {
    const { id } = use(params);
    const searchParams = useSearchParams();
    const defaultTab = searchParams.get('tab') || 'overview';

    // Validate id exists
    if (!id) {
        return <HostPageSkeleton />;
    }

    const { data: host, isLoading, error } = useQuery({
        queryKey: ['host', id],
        queryFn: () => hostsApi.getById(id),
        enabled: !!id, // Only run query if ID exists
    });

    // Connect to SignalR group for this host
    useHostSignalR(id);

    if (isLoading) {
        return <HostPageSkeleton />;
    }

    if (error || !host) {
        return notFound();
    }

    return (
        <div className="space-y-6">
            <HostHeader host={host} />

            <Tabs defaultValue={defaultTab} className="space-y-4">
                <TabsList>
                    <TabsTrigger value="overview">Overview</TabsTrigger>
                    <TabsTrigger value="metrics">Metrics</TabsTrigger>
                    <TabsTrigger value="services">Services</TabsTrigger>
                    <TabsTrigger value="applications">Applications</TabsTrigger>
                    <TabsTrigger value="eventlogs">Event Logs</TabsTrigger>
                    <TabsTrigger value="disks">Disks</TabsTrigger>
                    <TabsTrigger value="settings">Settings</TabsTrigger>
                </TabsList>

                <TabsContent value="overview" className="space-y-4">
                    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                        <Card>
                            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                                <CardTitle className="text-sm font-medium">Uptime</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <div className="text-2xl font-bold">
                                    {host.metrics?.uptimeDisplay || 'Unknown'}
                                </div>
                                <p className="text-xs text-muted-foreground">Current uptime</p>
                            </CardContent>
                        </Card>
                        <Card>
                            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                                <CardTitle className="text-sm font-medium">CPU</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <div className="text-2xl font-bold">
                                    {host.metrics?.cpuPercent?.toFixed(1) || 0}%
                                </div>
                                <p className="text-xs text-muted-foreground">Current usage</p>
                            </CardContent>
                        </Card>
                        <Card>
                            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                                <CardTitle className="text-sm font-medium">RAM</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <div className="text-2xl font-bold">
                                    {host.metrics?.ramPercent?.toFixed(1) || 0}%
                                </div>
                                <p className="text-xs text-muted-foreground">
                                    {host.metrics?.ramUsedMb ? (host.metrics.ramUsedMb / 1024).toFixed(1) : 0} GB / {host.metrics?.ramTotalMb ? (host.metrics.ramTotalMb / 1024).toFixed(1) : 0} GB
                                </p>
                            </CardContent>
                        </Card>
                        <Card>
                            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                                <CardTitle className="text-sm font-medium">Incidents</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <div className="text-2xl font-bold">
                                    {host.statistics?.incidentCount30d}
                                </div>
                                <p className="text-xs text-muted-foreground">Last 30 days</p>
                            </CardContent>
                        </Card>
                        <Card>
                            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                                <CardTitle className="text-sm font-medium">Network (In/Out)</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <div className="text-2xl font-bold">
                                    {host.metrics?.networkInBytes ? (host.metrics.networkInBytes / 1024 / 1024).toFixed(2) : '0.00'} / {host.metrics?.networkOutBytes ? (host.metrics.networkOutBytes / 1024 / 1024).toFixed(2) : '0.00'} MB
                                </div>
                                <p className="text-xs text-muted-foreground">Total transferred</p>
                            </CardContent>
                        </Card>
                        <Card>
                            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                                <CardTitle className="text-sm font-medium">Processes</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <div className="text-2xl font-bold">
                                    {host.metrics?.processCount || 0}
                                </div>
                                <p className="text-xs text-muted-foreground">Running processes</p>
                            </CardContent>
                        </Card>
                    </div>

                    <HostMetrics hostId={host.id} />
                </TabsContent>

                <TabsContent value="metrics">
                    <HostMetrics hostId={host.id} />
                </TabsContent>

                <TabsContent value="services">
                    <Card>
                        <CardHeader>
                            <CardTitle>Services</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <ServiceList hostId={host.id} />
                        </CardContent>
                    </Card>
                </TabsContent>

                <TabsContent value="applications">
                    <HostApplications services={host.services} />
                </TabsContent>

                <TabsContent value="eventlogs">
                    <HostEventLogs hostId={host.id} />
                </TabsContent>

                <TabsContent value="disks">
                    <Card>
                        <CardHeader>
                            <CardTitle>Disks</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <DiskList hostId={host.id} disks={host.disks} />
                        </CardContent>
                    </Card>
                </TabsContent>

                <TabsContent value="settings">
                    <Card>
                        <CardHeader>
                            <CardTitle>Host Settings</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <HostSettingsForm host={host} />
                        </CardContent>
                    </Card>
                </TabsContent>
            </Tabs>
        </div>
    );
}

function HostPageSkeleton() {
    return (
        <div className="space-y-6">
            <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
                <div className="flex items-center gap-4">
                    <Skeleton className="h-10 w-10 rounded-md" />
                    <div>
                        <Skeleton className="h-8 w-48 mb-2" />
                        <Skeleton className="h-4 w-64" />
                    </div>
                </div>
                <div className="flex gap-2">
                    <Skeleton className="h-9 w-24" />
                    <Skeleton className="h-9 w-9" />
                </div>
            </div>
            <Skeleton className="h-10 w-full max-w-md" />
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                {[...Array(4)].map((_, i) => (
                    <Skeleton key={i} className="h-32" />
                ))}
            </div>
            <Skeleton className="h-[400px]" />
        </div>
    );
}

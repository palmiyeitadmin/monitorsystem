'use client';

import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Server, Cpu, MemoryStick, Clock } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { StatusBadge } from '@/components/common/status-badge';
import { Skeleton } from '@/components/ui/skeleton';
import { cn } from '@/lib/utils';
import { formatDistanceToNow } from 'date-fns';
import { hostsApi } from '@/lib/api/hosts';
import type { Host } from '@/types/host';

interface HostGridProps {
    limit?: number;
    customerId?: string;
}

export function HostGrid({ limit = 12, customerId }: HostGridProps) {
    const { data, isLoading } = useQuery({
        queryKey: ['dashboard', 'host-grid', { limit, customerId }],
        queryFn: async () => {
            const response = await hostsApi.getAll({
                pageSize: limit,
                customerId: customerId,
            });
            return response;
        },
        refetchInterval: 30000,
    });

    if (isLoading) {
        return <HostGridSkeleton count={limit} />;
    }

    const hosts: Host[] = data?.items || [];

    if (hosts.length === 0) {
        return (
            <Card>
                <CardContent className="flex flex-col items-center justify-center py-10">
                    <Server className="h-12 w-12 text-muted-foreground mb-4" />
                    <p className="text-muted-foreground">No hosts found</p>
                    <Link href="/hosts/new" className="text-primary hover:underline mt-2">
                        Add your first host
                    </Link>
                </CardContent>
            </Card>
        );
    }

    return (
        <Card>
            <CardHeader>
                <div className="flex items-center justify-between">
                    <CardTitle>Hosts</CardTitle>
                    <Link href="/hosts" className="text-sm text-primary hover:underline">
                        View all
                    </Link>
                </div>
            </CardHeader>
            <CardContent>
                <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                    {hosts.map((host) => (
                        <HostGridItem key={host.id} host={host} />
                    ))}
                </div>
            </CardContent>
        </Card>
    );
}

function HostGridItem({ host }: { host: Host }) {
    const isDown = host.currentStatus === 'Down';
    const isWarning = host.currentStatus === 'Warning' || host.currentStatus === 'Degraded';

    return (
        <Link href={`/hosts/${host.id}`}>
            <div
                className={cn(
                    'rounded-lg border p-3 transition-colors hover:bg-accent',
                    isDown && 'border-status-down bg-status-down/5',
                    isWarning && 'border-status-warning bg-status-warning/5'
                )}
            >
                <div className="flex items-start justify-between mb-2">
                    <div className="flex items-center gap-2">
                        <Server className="h-4 w-4 text-muted-foreground" />
                        <span className="font-medium text-sm truncate max-w-[120px]">
                            {host.name}
                        </span>
                    </div>
                    <StatusBadge status={host.currentStatus} size="sm" />
                </div>

                <div className="space-y-1 text-xs text-muted-foreground">
                    <div className="flex items-center justify-between">
                        <span className="flex items-center gap-1">
                            <Cpu className="h-3 w-3" /> CPU
                        </span>
                        <span className={cn(
                            host.cpuPercent && host.cpuPercent > 90 && 'text-destructive font-medium'
                        )}>
                            {host.cpuPercent?.toFixed(1) || '--'}%
                        </span>
                    </div>
                    <div className="flex items-center justify-between">
                        <span className="flex items-center gap-1">
                            <MemoryStick className="h-3 w-3" /> RAM
                        </span>
                        <span className={cn(
                            host.ramPercent && host.ramPercent > 90 && 'text-destructive font-medium'
                        )}>
                            {host.ramPercent?.toFixed(1) || '--'}%
                        </span>
                    </div>
                    {host.lastSeenAt && (
                        <div className="flex items-center justify-between pt-1 border-t">
                            <span className="flex items-center gap-1">
                                <Clock className="h-3 w-3" />
                            </span>
                            <span>
                                {formatDistanceToNow(new Date(host.lastSeenAt), { addSuffix: true })}
                            </span>
                        </div>
                    )}
                </div>
            </div>
        </Link>
    );
}

function HostGridSkeleton({ count }: { count: number }) {
    return (
        <Card>
            <CardHeader>
                <Skeleton className="h-6 w-20" />
            </CardHeader>
            <CardContent>
                <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                    {[...Array(count)].map((_, i) => (
                        <div key={i} className="rounded-lg border p-3">
                            <div className="flex items-center justify-between mb-2">
                                <Skeleton className="h-4 w-24" />
                                <Skeleton className="h-5 w-12" />
                            </div>
                            <div className="space-y-2">
                                <Skeleton className="h-3 w-full" />
                                <Skeleton className="h-3 w-full" />
                                <Skeleton className="h-3 w-2/3" />
                            </div>
                        </div>
                    ))}
                </div>
            </CardContent>
        </Card>
    );
}

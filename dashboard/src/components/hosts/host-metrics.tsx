'use client';

import { useQuery } from '@tanstack/react-query';
import { MetricChart } from '@/components/charts/metric-chart';
import { hostsApi } from '@/lib/api/hosts';
import { Skeleton } from '@/components/ui/skeleton';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

interface HostMetricsProps {
    hostId: string;
}

export function HostMetrics({ hostId }: HostMetricsProps) {
    const { data, isLoading } = useQuery({
        queryKey: ['host-metrics', hostId],
        queryFn: () => hostsApi.getMetrics(hostId),
        refetchInterval: 10000, // Refresh every 10 seconds
    });

    if (isLoading) {
        return <HostMetricsSkeleton />;
    }

    if (!data) {
        return (
            <Card>
                <CardContent className="p-6 text-center text-muted-foreground">
                    No metrics available for this host.
                </CardContent>
            </Card>
        );
    }

    return (
        <div className="grid gap-4 md:grid-cols-2">
            <MetricChart
                data={data.cpuHistory}
                title="CPU Usage"
                unit="%"
                color="#29ABE2"
                maxValue={100}
            />
            <MetricChart
                data={data.ramHistory}
                title="RAM Usage"
                unit="%"
                color="#8b5cf6"
                maxValue={100}
            />
            <MetricChart
                data={data.diskHistory}
                title="Disk Usage"
                unit="%"
                color="#f97316"
                maxValue={100}
            />
            <MetricChart
                data={data.networkHistory}
                title="Network Traffic"
                unit=" MB/s"
                color="#10b981"
            />
        </div>
    );
}

function HostMetricsSkeleton() {
    return (
        <div className="grid gap-4 md:grid-cols-2">
            {[...Array(4)].map((_, i) => (
                <Card key={i}>
                    <CardHeader className="flex flex-row items-center justify-between pb-2">
                        <Skeleton className="h-5 w-24" />
                        <Skeleton className="h-8 w-16" />
                    </CardHeader>
                    <CardContent>
                        <Skeleton className="h-[300px] w-full" />
                    </CardContent>
                </Card>
            ))}
        </div>
    );
}

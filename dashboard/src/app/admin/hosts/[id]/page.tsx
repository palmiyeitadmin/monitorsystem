"use client";

import { useParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { StatusBadge } from "@/components/common/StatusBadge";
import { Activity, Cpu, HardDrive, Server } from "lucide-react";
import { formatDistanceToNow } from "date-fns";

export default function HostDetailPage() {
    const params = useParams();
    const id = params.id as string;

    const { data: host, isLoading } = useQuery({
        queryKey: ["host", id],
        queryFn: async () => {
            return api.hosts.get(id);
        },
    });

    if (isLoading) {
        return <div>Loading...</div>;
    }

    if (!host) {
        return <div>Host not found</div>;
    }

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">{host.name}</h2>
                    <p className="text-muted-foreground">{host.primaryIp || host.publicIp || "IP bilinmiyor"}</p>
                </div>
                <StatusBadge status={host.statusDisplay || host.currentStatus} className="text-lg px-4 py-1" />
            </div>

            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Status</CardTitle>
                        <Activity className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold capitalize">{host.statusDisplay || host.currentStatus}</div>
                        <p className="text-xs text-muted-foreground">
                            Last seen {host.lastSeenAt ? formatDistanceToNow(new Date(host.lastSeenAt), { addSuffix: true }) : "Never"}
                        </p>
                    </CardContent>
                </Card>
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Uptime</CardTitle>
                        <Server className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{host.metrics?.uptimeSeconds ? `${Math.round((host.metrics.uptimeSeconds / 3600) * 10) / 10}h` : "--"}</div>
                        <p className="text-xs text-muted-foreground">
                            Last 30 days
                        </p>
                    </CardContent>
                </Card>
                {/* Placeholders for real metrics */}
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">CPU Usage</CardTitle>
                        <Cpu className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{host.metrics?.cpuPercent ?? "--"}%</div>
                        <p className="text-xs text-muted-foreground">
                            Real-time
                        </p>
                    </CardContent>
                </Card>
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Memory</CardTitle>
                        <HardDrive className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{host.metrics?.ramPercent ?? "--"}%</div>
                        <p className="text-xs text-muted-foreground">
                            Real-time
                        </p>
                    </CardContent>
                </Card>
            </div>

            {/* Additional sections for Services, Checks, Incidents would go here */}
        </div>
    );
}

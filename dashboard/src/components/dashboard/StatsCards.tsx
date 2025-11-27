"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Activity, Server, ShieldAlert, Globe } from "lucide-react";
import { useHosts } from "@/hooks/useHosts";
import { useServices } from "@/hooks/useServices";
import { useIncidents } from "@/hooks/useIncidents";
import type { Host, Incident, Service } from "@/lib/api";

export function StatsCards() {
    const { hosts } = useHosts();
    const { services } = useServices();
    const { incidents } = useIncidents();

    const activeIncidents = incidents.filter((i: Incident) => i.status !== "resolved").length;
    const hostsUp = hosts.filter((h: Host) => (h.currentStatus || "").toLowerCase() === "up").length;
    const servicesUp = services.filter((s: Service) => (s.currentStatus || s.status || "").toLowerCase() === "up").length;

    return (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">Total Hosts</CardTitle>
                    <Server className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                    <div className="text-2xl font-bold">{hosts.length}</div>
                    <p className="text-xs text-muted-foreground">
                        {hostsUp} Online
                    </p>
                </CardContent>
            </Card>
            <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">Services</CardTitle>
                    <Globe className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                    <div className="text-2xl font-bold">{services.length}</div>
                    <p className="text-xs text-muted-foreground">
                        {servicesUp} Operational
                    </p>
                </CardContent>
            </Card>
            <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">Active Incidents</CardTitle>
                    <ShieldAlert className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                    <div className="text-2xl font-bold">{activeIncidents}</div>
                    <p className="text-xs text-muted-foreground">
                        Requires attention
                    </p>
                </CardContent>
            </Card>
            <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">System Health</CardTitle>
                    <Activity className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                    <div className="text-2xl font-bold">98.5%</div>
                    <p className="text-xs text-muted-foreground">
                        +2.1% from last month
                    </p>
                </CardContent>
            </Card>
        </div>
    );
}

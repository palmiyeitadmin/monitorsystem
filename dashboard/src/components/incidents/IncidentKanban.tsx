"use client";

import { Incident, useIncidents } from "@/hooks/useIncidents";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { formatDistanceToNow } from "date-fns";
import { AlertTriangle, CheckCircle, Clock, type LucideIcon } from "lucide-react";

export function IncidentKanban() {
    const { incidents, isLoading } = useIncidents();

    if (isLoading) return <div>Loading...</div>;

    const columns = {
        open: incidents.filter((i) => i.status === "open"),
        acknowledged: incidents.filter((i) => i.status === "acknowledged"),
        resolved: incidents.filter((i) => i.status === "resolved"),
    };

    return (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 h-full">
            <KanbanColumn
                title="Open"
                incidents={columns.open}
                icon={AlertTriangle}
                color="text-red-500"
                bg="bg-red-50 dark:bg-red-900/10"
            />
            <KanbanColumn
                title="Acknowledged"
                incidents={columns.acknowledged}
                icon={Clock}
                color="text-yellow-500"
                bg="bg-yellow-50 dark:bg-yellow-900/10"
            />
            <KanbanColumn
                title="Resolved"
                incidents={columns.resolved}
                icon={CheckCircle}
                color="text-green-500"
                bg="bg-green-50 dark:bg-green-900/10"
            />
        </div>
    );
}

function KanbanColumn({
    title,
    incidents,
    icon: Icon,
    color,
    bg,
}: {
    title: string;
    incidents: Incident[];
    icon: LucideIcon;
    color: string;
    bg: string;
}) {
    return (
        <div className={`flex flex-col rounded-lg p-4 ${bg} h-full`}>
            <div className="flex items-center gap-2 mb-4">
                <Icon className={`h-5 w-5 ${color}`} />
                <h3 className="font-semibold text-lg">{title}</h3>
                <Badge variant="secondary" className="ml-auto">
                    {incidents.length}
                </Badge>
            </div>
            <div className="space-y-3 overflow-y-auto flex-1">
                {incidents.map((incident) => (
                    <IncidentCard key={incident.id} incident={incident} />
                ))}
            </div>
        </div>
    );
}

function IncidentCard({ incident }: { incident: Incident }) {
    return (
        <Card className="cursor-pointer hover:shadow-md transition-shadow">
            <CardHeader className="p-4 pb-2">
                <div className="flex justify-between items-start">
                    <CardTitle className="text-sm font-medium leading-tight">
                        {incident.title}
                    </CardTitle>
                    <Badge
                        variant={incident.severity === "critical" ? "destructive" : "outline"}
                        className="text-xs"
                    >
                        {incident.severity}
                    </Badge>
                </div>
            </CardHeader>
            <CardContent className="p-4 pt-2">
                <p className="text-xs text-muted-foreground mb-2 line-clamp-2">
                    {incident.description}
                </p>
                <div className="flex justify-between items-center text-xs text-muted-foreground">
                    <span>{incident.hostName}</span>
                    <span>{formatDistanceToNow(new Date(incident.createdAt), { addSuffix: true })}</span>
                </div>
            </CardContent>
        </Card>
    );
}



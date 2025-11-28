"use client";

import { Check } from "@/types/check";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
} from "@/components/ui/tooltip";
import { Activity, Edit, Globe, MoreHorizontal, Pause, Play, Server, Trash } from "lucide-react";
import { formatDistanceToNow } from "date-fns";
import { cn } from "@/lib/utils";

interface CheckCardProps {
    check: Check;
    onEdit: (check: Check) => void;
    onDelete: (id: string) => void;
    onToggle: (check: Check) => void;
}

export function CheckCard({ check, onEdit, onDelete, onToggle }: CheckCardProps) {
    const getStatusColor = (status: string) => {
        switch (status) {
            case "Up": return "bg-emerald-500/10 text-white border-emerald-500/20";
            case "Down": return "bg-rose-500/10 text-white border-rose-500/20";
            case "Degraded": return "bg-amber-500/10 text-white border-amber-500/20";
            default: return "bg-slate-500/10 text-white border-slate-500/20";
        }
    };

    const getStatusBg = (status: string) => {
        switch (status) {
            case "Up": return "bg-emerald-500";
            case "Down": return "bg-rose-500";
            case "Degraded": return "bg-amber-500";
            default: return "bg-slate-500";
        }
    };

    const getTypeIcon = (type: string) => {
        switch (type) {
            case "HTTP": return <Globe className="h-4 w-4" />;
            case "TCP": return <Server className="h-4 w-4" />;
            case "Ping": return <Activity className="h-4 w-4" />;
            case "DNS": return <Activity className="h-4 w-4" />; // Use Activity for DNS for now or find better icon
            default: return <Activity className="h-4 w-4" />;
        }
    };

    // Prepare history bars (ensure we have 20 slots, fill with empty if needed)
    // History comes sorted by CheckedAt desc (newest first)
    // We want to display oldest -> newest (left -> right)
    const history = check.history || [];
    const displayHistory = [...history].reverse(); // Now oldest -> newest

    // Fill remaining slots if less than 20
    const totalSlots = 20;
    const emptySlots = Math.max(0, totalSlots - displayHistory.length);
    const slots = [
        ...Array(emptySlots).fill(null),
        ...displayHistory
    ];

    return (
        <div className="group relative overflow-hidden rounded-xl border bg-card p-4 transition-all hover:shadow-md">
            {/* Status Indicator Line */}
            <div className={cn("absolute left-0 top-0 bottom-0 w-1", getStatusBg(check.currentStatus))} />

            <div className="flex flex-col gap-4 pl-2">
                {/* Header */}
                <div className="flex items-start justify-between">
                    <div className="flex items-center gap-3">
                        <div className={cn("flex h-8 w-8 items-center justify-center rounded-lg bg-secondary")}>
                            {getTypeIcon(check.checkType)}
                        </div>
                        <div>
                            <h3 className="font-semibold leading-none tracking-tight">{check.name}</h3>
                            <a
                                href={check.checkType === 'HTTP' ? check.target : '#'}
                                target={check.checkType === 'HTTP' ? "_blank" : undefined}
                                rel="noopener noreferrer"
                                className="text-sm text-muted-foreground hover:underline hover:text-primary mt-1 block"
                            >
                                {check.target}
                            </a>
                        </div>
                    </div>

                    <div className="flex items-center gap-2">
                        <Badge variant="outline" className={cn("capitalize", getStatusColor(check.currentStatus))}>
                            {check.currentStatus}
                        </Badge>
                        <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                                <Button variant="ghost" size="icon" className="h-8 w-8">
                                    <MoreHorizontal className="h-4 w-4" />
                                    <span className="sr-only">Open menu</span>
                                </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                                <DropdownMenuLabel>Actions</DropdownMenuLabel>
                                <DropdownMenuItem onClick={() => onEdit(check)}>
                                    <Edit className="mr-2 h-4 w-4" /> Edit
                                </DropdownMenuItem>
                                <DropdownMenuItem onClick={() => onToggle(check)}>
                                    {check.monitoringEnabled ? (
                                        <>
                                            <Pause className="mr-2 h-4 w-4" /> Pause
                                        </>
                                    ) : (
                                        <>
                                            <Play className="mr-2 h-4 w-4" /> Resume
                                        </>
                                    )}
                                </DropdownMenuItem>
                                <DropdownMenuSeparator />
                                <DropdownMenuItem
                                    className="text-red-600 focus:text-red-600"
                                    onClick={() => onDelete(check.id)}
                                >
                                    <Trash className="mr-2 h-4 w-4" /> Delete
                                </DropdownMenuItem>
                            </DropdownMenuContent>
                        </DropdownMenu>
                    </div>
                </div>

                {/* Heartbeat Bar */}
                <div className="flex items-end gap-[2px] h-8 mt-2">
                    <TooltipProvider delayDuration={0}>
                        {slots.map((result, i) => (
                            <Tooltip key={i}>
                                <TooltipTrigger asChild>
                                    <div
                                        className={cn(
                                            "flex-1 rounded-sm transition-all hover:scale-110 hover:opacity-80",
                                            result ? getStatusBg(result.status) : "bg-secondary/50",
                                            "h-full" // Fixed height for now, could be dynamic based on response time
                                        )}
                                        style={{
                                            height: result ? '100%' : '100%', // Could map response time to height
                                            opacity: result ? 1 : 0.3
                                        }}
                                    />
                                </TooltipTrigger>
                                {result && (
                                    <TooltipContent>
                                        <div className="text-xs">
                                            <p className="font-semibold">{result.status}</p>
                                            <p>{new Date(result.checkedAt).toLocaleString()}</p>
                                            {result.responseTimeMs && <p>{result.responseTimeMs}ms</p>}
                                            {result.errorMessage && <p className="text-red-500">{result.errorMessage}</p>}
                                        </div>
                                    </TooltipContent>
                                )}
                            </Tooltip>
                        ))}
                    </TooltipProvider>
                </div>

                {/* Footer Stats */}
                <div className="flex items-center justify-between text-xs text-muted-foreground mt-1">
                    <div className="flex gap-4">
                        <span className="flex items-center gap-1">
                            <Activity className="h-3 w-3" />
                            {check.lastResponseTimeMs ? `${check.lastResponseTimeMs}ms` : '-'}
                        </span>
                        <span>
                            Last check: {check.lastCheckAt ? formatDistanceToNow(new Date(check.lastCheckAt), { addSuffix: true }) : 'Never'}
                        </span>
                    </div>
                    {check.sslDaysRemaining !== undefined && check.sslDaysRemaining !== null && (
                        <span className={cn(
                            check.sslDaysRemaining < 30 ? "text-amber-500" : "text-emerald-500"
                        )}>
                            SSL: {check.sslDaysRemaining} days
                        </span>
                    )}
                </div>
            </div>
        </div>
    );
}

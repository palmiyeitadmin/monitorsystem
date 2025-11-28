"use client";

import { useState } from "react";
import { Check } from "@/types/check";
import { checksApi } from "@/lib/api/checks";
import { toast } from "sonner";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { MoreHorizontal, Play, Pause, Trash, Edit, Globe, Activity, Server } from "lucide-react";
import { formatDistanceToNow } from "date-fns";
import { CheckFormDialog } from "./check-form-dialog";

interface CheckListProps {
    checks: Check[];
    onRefresh: () => void;
}

export function CheckList({ checks, onRefresh }: CheckListProps) {
    const [editingCheck, setEditingCheck] = useState<Check | undefined>(undefined);
    const [isFormOpen, setIsFormOpen] = useState(false);

    const handleDelete = async (id: string) => {
        if (!confirm("Are you sure you want to delete this check?")) return;
        try {
            await checksApi.delete(id);
            toast.success("Check deleted");
            onRefresh();
        } catch (error) {
            console.error(error);
            toast.error("Failed to delete check");
        }
    };

    const handleToggle = async (check: Check) => {
        try {
            await checksApi.update(check.id, { monitoringEnabled: !check.monitoringEnabled });
            toast.success(`Monitoring ${check.monitoringEnabled ? "paused" : "resumed"}`);
            onRefresh();
        } catch (error) {
            console.error(error);
            toast.error("Failed to update status");
        }
    };

    const getStatusColor = (status: string) => {
        switch (status) {
            case "Up": return "bg-green-500/10 text-green-500 hover:bg-green-500/20";
            case "Down": return "bg-red-500/10 text-red-500 hover:bg-red-500/20";
            case "Degraded": return "bg-yellow-500/10 text-yellow-500 hover:bg-yellow-500/20";
            default: return "bg-gray-500/10 text-gray-500 hover:bg-gray-500/20";
        }
    };

    const getTypeIcon = (type: string) => {
        switch (type) {
            case "HTTP": return <Globe className="h-4 w-4" />;
            case "TCP": return <Server className="h-4 w-4" />;
            case "Ping": return <Activity className="h-4 w-4" />;
            default: return <Activity className="h-4 w-4" />;
        }
    };

    return (
        <>
            <div className="rounded-md border bg-card">
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Name</TableHead>
                            <TableHead>Target</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead>Last Check</TableHead>
                            <TableHead>Response Time</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {checks.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={6} className="h-24 text-center">
                                    No checks found. Create one to start monitoring.
                                </TableCell>
                            </TableRow>
                        ) : (
                            checks.map((check) => (
                                <TableRow key={check.id}>
                                    <TableCell className="font-medium">
                                        <div className="flex items-center gap-2">
                                            {getTypeIcon(check.checkType)}
                                            <div className="flex flex-col">
                                                <span>{check.name}</span>
                                                <span className="text-xs text-muted-foreground">{check.checkType}</span>
                                            </div>
                                        </div>
                                    </TableCell>
                                    <TableCell className="font-mono text-sm">{check.target}</TableCell>
                                    <TableCell>
                                        <Badge variant="outline" className={getStatusColor(check.currentStatus)}>
                                            {check.currentStatus}
                                        </Badge>
                                    </TableCell>
                                    <TableCell>
                                        {check.lastCheckAt ? (
                                            <span className="text-sm text-muted-foreground">
                                                {formatDistanceToNow(new Date(check.lastCheckAt), { addSuffix: true })}
                                            </span>
                                        ) : (
                                            <span className="text-sm text-muted-foreground">-</span>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {check.lastResponseTimeMs ? (
                                            <span className="font-mono text-sm">{check.lastResponseTimeMs}ms</span>
                                        ) : (
                                            <span className="text-sm text-muted-foreground">-</span>
                                        )}
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <DropdownMenu>
                                            <DropdownMenuTrigger asChild>
                                                <Button variant="ghost" className="h-8 w-8 p-0">
                                                    <span className="sr-only">Open menu</span>
                                                    <MoreHorizontal className="h-4 w-4" />
                                                </Button>
                                            </DropdownMenuTrigger>
                                            <DropdownMenuContent align="end">
                                                <DropdownMenuLabel>Actions</DropdownMenuLabel>
                                                <DropdownMenuItem onClick={() => {
                                                    setEditingCheck(check);
                                                    setIsFormOpen(true);
                                                }}>
                                                    <Edit className="mr-2 h-4 w-4" /> Edit
                                                </DropdownMenuItem>
                                                <DropdownMenuItem onClick={() => handleToggle(check)}>
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
                                                    onClick={() => handleDelete(check.id)}
                                                >
                                                    <Trash className="mr-2 h-4 w-4" /> Delete
                                                </DropdownMenuItem>
                                            </DropdownMenuContent>
                                        </DropdownMenu>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </div>

            <CheckFormDialog
                open={isFormOpen}
                onOpenChange={(open) => {
                    setIsFormOpen(open);
                    if (!open) setEditingCheck(undefined);
                }}
                check={editingCheck}
                onSuccess={onRefresh}
            />
        </>
    );
}

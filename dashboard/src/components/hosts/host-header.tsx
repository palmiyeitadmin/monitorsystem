'use client';

import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import {
    ArrowLeft,
    RefreshCw,
    Power,
    Trash2,
    MoreVertical,
    Play,
    Pause,
    Terminal
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { StatusBadge } from '@/components/common/status-badge';
import { toast } from 'sonner';
import { hostsApi } from '@/lib/api/hosts';
import type { HostDetail } from '@/types/host';
import { formatDistanceToNow } from 'date-fns';

interface HostHeaderProps {
    host: HostDetail;
}

export function HostHeader({ host }: HostHeaderProps) {
    const router = useRouter();
    const queryClient = useQueryClient();

    const deleteMutation = useMutation({
        mutationFn: hostsApi.delete,
        onSuccess: () => {
            toast.success('Host deleted successfully');
            router.push('/hosts');
        },
        onError: (error: Error) => {
            toast.error(`Failed to delete host: ${error.message}`);
        },
    });

    const maintenanceMutation = useMutation({
        mutationFn: (enable: boolean) => hostsApi.setMaintenance(host.id, { enable }),
        onSuccess: (_, enable) => {
            toast.success(`Maintenance mode ${enable ? 'enabled' : 'disabled'}`);
            queryClient.invalidateQueries({ queryKey: ['host', host.id] });
        },
        onError: (error: Error) => {
            toast.error(`Failed to update maintenance mode: ${error.message}`);
        },
    });

    const toggleMonitoringMutation = useMutation({
        mutationFn: hostsApi.toggleMonitoring,
        onSuccess: () => {
            toast.success('Monitoring status updated');
            queryClient.invalidateQueries({ queryKey: ['host', host.id] });
        },
        onError: (error: Error) => {
            toast.error(`Failed to update monitoring status: ${error.message}`);
        },
    });

    return (
        <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
            <div className="flex items-center gap-4">
                <Button variant="ghost" size="icon" onClick={() => router.back()}>
                    <ArrowLeft className="h-4 w-4" />
                </Button>
                <div>
                    <div className="flex items-center gap-3">
                        <h1 className="text-2xl font-bold tracking-tight">{host.name}</h1>
                        <StatusBadge status={host.currentStatus} showPulse />
                    </div>
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                        <span>{host.hostname}</span>
                        <span>•</span>
                        <span>{host.primaryIp}</span>
                        <span>•</span>
                        <span>{host.osType} {host.osVersion}</span>
                        {host.lastSeenAt && (
                            <>
                                <span>•</span>
                                <span>Last seen {formatDistanceToNow(new Date(host.lastSeenAt), { addSuffix: true })}</span>
                            </>
                        )}
                    </div>
                </div>
            </div>

            <div className="flex items-center gap-2">
                <Button
                    variant="outline"
                    size="sm"
                    onClick={() => queryClient.invalidateQueries({ queryKey: ['host', host.id] })}
                >
                    <RefreshCw className="mr-2 h-4 w-4" />
                    Refresh
                </Button>

                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Button variant="outline" size="icon">
                            <MoreVertical className="h-4 w-4" />
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                        <DropdownMenuLabel>Actions</DropdownMenuLabel>
                        <DropdownMenuItem onClick={() => toggleMonitoringMutation.mutate(host.id)}>
                            {host.monitoringEnabled ? (
                                <>
                                    <Pause className="mr-2 h-4 w-4" /> Pause Monitoring
                                </>
                            ) : (
                                <>
                                    <Play className="mr-2 h-4 w-4" /> Resume Monitoring
                                </>
                            )}
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => maintenanceMutation.mutate(!host.maintenanceMode)}>
                            <Terminal className="mr-2 h-4 w-4" />
                            {host.maintenanceMode ? 'Disable Maintenance' : 'Enable Maintenance'}
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem
                            className="text-destructive focus:text-destructive"
                            onClick={() => {
                                if (confirm('Are you sure you want to delete this host?')) {
                                    deleteMutation.mutate(host.id);
                                }
                            }}
                        >
                            <Trash2 className="mr-2 h-4 w-4" /> Delete Host
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>
        </div>
    );
}

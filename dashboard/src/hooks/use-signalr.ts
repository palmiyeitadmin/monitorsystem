import { useEffect, useCallback } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import signalRService, {
    HostUpdateEvent,
    IncidentEvent,
    CheckResultEvent
} from '@/lib/signalr';
import { useAuthStore } from '@/stores/auth-store';
import { toast } from 'sonner';

export function useSignalR() {
    const queryClient = useQueryClient();
    const { accessToken, isAuthenticated } = useAuthStore();

    useEffect(() => {
        if (!isAuthenticated || !accessToken) {
            return;
        }

        const connect = async () => {
            try {
                await signalRService.connect(accessToken);
                await signalRService.joinDashboardGroup();
            } catch (error) {
                console.error('Failed to connect to SignalR:', error);
            }
        };

        connect();

        return () => {
            signalRService.leaveDashboardGroup();
            signalRService.disconnect();
        };
    }, [isAuthenticated, accessToken]);

    // Host updates
    useEffect(() => {
        const unsubscribe = signalRService.onHostUpdate((data: HostUpdateEvent) => {
            // Update host in cache
            queryClient.invalidateQueries({ queryKey: ['hosts'] });
            queryClient.invalidateQueries({ queryKey: ['host', data.hostId] });
            queryClient.invalidateQueries({ queryKey: ['dashboard', 'status-overview'] });

            // Show notification for status changes
            if (data.currentStatus === 'Down') {
                toast.error(`Host Down: ${data.hostName}`, {
                    description: 'The host is no longer responding.',
                });
            } else if (data.currentStatus === 'Up' && data.statusChangedAt) {
                toast.success(`Host Recovered: ${data.hostName}`, {
                    description: 'The host is back online.',
                });
            }
        });

        return unsubscribe;
    }, [queryClient]);

    // Incident updates
    useEffect(() => {
        const unsubscribeCreated = signalRService.onIncidentCreated((data: IncidentEvent) => {
            queryClient.invalidateQueries({ queryKey: ['incidents'] });
            queryClient.invalidateQueries({ queryKey: ['dashboard', 'status-overview'] });

            toast.warning(`New Incident: ${data.incidentNumber}`, {
                description: data.title,
            });
        });

        const unsubscribeUpdated = signalRService.onIncidentUpdated((data: IncidentEvent) => {
            queryClient.invalidateQueries({ queryKey: ['incidents'] });
            queryClient.invalidateQueries({ queryKey: ['incident', data.incidentId] });
        });

        return () => {
            unsubscribeCreated();
            unsubscribeUpdated();
        };
    }, [queryClient]);

    // Check results
    useEffect(() => {
        const unsubscribe = signalRService.onCheckResult((data: CheckResultEvent) => {
            queryClient.invalidateQueries({ queryKey: ['checks'] });
            queryClient.invalidateQueries({ queryKey: ['check', data.checkId] });

            if (!data.success && data.currentStatus === 'Down') {
                toast.error(`Check Failed: ${data.checkName}`, {
                    description: data.errorMessage || 'Check is failing.',
                });
            }
        });

        return unsubscribe;
    }, [queryClient]);
}

export function useHostSignalR(hostId: string) {
    const queryClient = useQueryClient();

    useEffect(() => {
        signalRService.joinHostGroup(hostId);

        return () => {
            signalRService.leaveHostGroup(hostId);
        };
    }, [hostId]);

    useEffect(() => {
        const unsubscribe = signalRService.onHostUpdate((data: HostUpdateEvent) => {
            if (data.hostId === hostId) {
                queryClient.invalidateQueries({ queryKey: ['host', hostId] });
                queryClient.invalidateQueries({ queryKey: ['host-metrics', hostId] });
            }
        });

        return unsubscribe;
    }, [hostId, queryClient]);
}

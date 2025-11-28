'use client';

import { useQuery } from '@tanstack/react-query';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
} from '@/components/ui/dialog';
import { servicesApi } from '@/lib/api/services';
import { StatusBadge } from '@/components/common/status-badge';
import { Skeleton } from '@/components/ui/skeleton';
import { ScrollArea } from '@/components/ui/scroll-area';

interface ServiceDetailsModalProps {
    serviceId: string | null;
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export function ServiceDetailsModal({ serviceId, open, onOpenChange }: ServiceDetailsModalProps) {
    const { data: service, isLoading } = useQuery({
        queryKey: ['service', serviceId],
        queryFn: () => serviceId ? servicesApi.getById(serviceId) : null,
        enabled: !!serviceId && open,
    });

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-2xl">
                <DialogHeader>
                    <DialogTitle>Service Details</DialogTitle>
                </DialogHeader>

                {isLoading ? (
                    <div className="space-y-4">
                        <Skeleton className="h-8 w-1/3" />
                        <Skeleton className="h-24 w-full" />
                        <Skeleton className="h-40 w-full" />
                    </div>
                ) : service ? (
                    <div className="space-y-6">
                        <div className="grid grid-cols-2 gap-4">
                            <div>
                                <h4 className="text-sm font-medium text-muted-foreground">Name</h4>
                                <p className="text-base">{service.serviceName}</p>
                            </div>
                            <div>
                                <h4 className="text-sm font-medium text-muted-foreground">Display Name</h4>
                                <p className="text-base">{service.displayName}</p>
                            </div>
                            <div>
                                <h4 className="text-sm font-medium text-muted-foreground">Status</h4>
                                <div className="mt-1">
                                    <StatusBadge status={service.currentStatus} />
                                </div>
                            </div>
                            <div>
                                <h4 className="text-sm font-medium text-muted-foreground">Startup Type</h4>
                                <p className="text-base">{service.startupType}</p>
                            </div>
                            {service.description && (
                                <div className="col-span-2">
                                    <h4 className="text-sm font-medium text-muted-foreground">Description</h4>
                                    <p className="text-sm text-muted-foreground mt-1">{service.description}</p>
                                </div>
                            )}
                        </div>

                        {service.config && Object.keys(service.config).length > 0 && (
                            <div>
                                <h4 className="text-sm font-medium text-muted-foreground mb-2">Configuration</h4>
                                <ScrollArea className="h-[300px] w-full rounded-md border p-4 bg-muted/50">
                                    <pre className="text-xs font-mono">
                                        {JSON.stringify(service.config, null, 2)}
                                    </pre>
                                </ScrollArea>
                            </div>
                        )}
                    </div>
                ) : (
                    <div className="text-center text-muted-foreground">
                        Service not found.
                    </div>
                )}
            </DialogContent>
        </Dialog>
    );
}

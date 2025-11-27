'use client';

import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { StatusBadge } from '@/components/common/status-badge';
import { servicesApi, type Service } from '@/lib/api/services';
import { MoreVertical, Play, Square, RotateCw, Search } from 'lucide-react';
import { toast } from 'sonner';
import { useDebounce } from '@/hooks/use-debounce';

interface ServiceListProps {
    hostId?: string;
}

export function ServiceList({ hostId }: ServiceListProps) {
    const [search, setSearch] = useState('');
    const debouncedSearch = useDebounce(search, 500);
    const queryClient = useQueryClient();

    const { data, isLoading } = useQuery({
        queryKey: ['services', hostId, debouncedSearch],
        queryFn: () => servicesApi.getAll({
            pageNumber: 1,
            pageSize: 50,
            hostId,
            search: debouncedSearch
        }),
    });

    const actionMutation = useMutation({
        mutationFn: ({ id, action }: { id: string; action: 'Start' | 'Stop' | 'Restart' }) =>
            servicesApi.performAction(id, action),
        onSuccess: (_, { action }) => {
            toast.success(`Service ${action.toLowerCase()} command sent`);
            queryClient.invalidateQueries({ queryKey: ['services'] });
        },
        onError: (error: Error) => {
            toast.error(`Failed to perform action: ${error.message}`);
        },
    });

    if (isLoading) {
        return <div>Loading services...</div>;
    }

    return (
        <div className="space-y-4">
            <div className="flex items-center gap-2">
                <div className="relative flex-1 max-w-sm">
                    <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                    <Input
                        placeholder="Search services..."
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        className="pl-8"
                    />
                </div>
            </div>

            <div className="rounded-md border">
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Name</TableHead>
                            <TableHead>Display Name</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead>Startup Type</TableHead>
                            <TableHead className="w-[50px]"></TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {data?.items.map((service) => (
                            <TableRow key={service.id}>
                                <TableCell className="font-medium">{service.serviceName}</TableCell>
                                <TableCell>{service.displayName}</TableCell>
                                <TableCell>
                                    <StatusBadge status={service.currentStatus} />
                                </TableCell>
                                <TableCell>{service.startupType}</TableCell>
                                <TableCell>
                                    <DropdownMenu>
                                        <DropdownMenuTrigger asChild>
                                            <Button variant="ghost" size="icon">
                                                <MoreVertical className="h-4 w-4" />
                                            </Button>
                                        </DropdownMenuTrigger>
                                        <DropdownMenuContent align="end">
                                            <DropdownMenuItem onClick={() => actionMutation.mutate({ id: service.id, action: 'Start' })}>
                                                <Play className="mr-2 h-4 w-4" /> Start
                                            </DropdownMenuItem>
                                            <DropdownMenuItem onClick={() => actionMutation.mutate({ id: service.id, action: 'Stop' })}>
                                                <Square className="mr-2 h-4 w-4" /> Stop
                                            </DropdownMenuItem>
                                            <DropdownMenuItem onClick={() => actionMutation.mutate({ id: service.id, action: 'Restart' })}>
                                                <RotateCw className="mr-2 h-4 w-4" /> Restart
                                            </DropdownMenuItem>
                                        </DropdownMenuContent>
                                    </DropdownMenu>
                                </TableCell>
                            </TableRow>
                        ))}
                        {data?.items.length === 0 && (
                            <TableRow>
                                <TableCell colSpan={5} className="h-24 text-center">
                                    No services found.
                                </TableCell>
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
            </div>
        </div>
    );
}

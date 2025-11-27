'use client';

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { formatDistanceToNow } from 'date-fns';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import { Input } from '@/components/ui/input';
import { apiClient } from '@/lib/api/client';
import { AlertCircle, CheckCircle2, Clock, Search } from 'lucide-react';
import { cn } from '@/lib/utils';

// Define Incident type locally for now if not in types
interface Incident {
    id: string;
    title: string;
    description: string;
    status: 'Open' | 'Investigating' | 'Resolved';
    severity: 'Low' | 'Medium' | 'High' | 'Critical';
    createdAt: string;
    resolvedAt?: string;
    hostName?: string;
    serviceName?: string;
}


export function IncidentList() {
    const [statusFilter, setStatusFilter] = useState<string>('all');
    const [searchQuery, setSearchQuery] = useState('');

    // Mock API call - replace with actual API
    const { data: incidents, isLoading } = useQuery({
        queryKey: ['incidents'],
        queryFn: async () => {
            // return apiClient.get<Incident[]>('/incidents');
            // Returning mock data for now
            return [
                {
                    id: '1',
                    title: 'High CPU Usage',
                    description: 'CPU usage exceeded 90% for 5 minutes',
                    status: 'Open',
                    severity: 'High',
                    createdAt: new Date(Date.now() - 1000 * 60 * 30).toISOString(),
                    hostName: 'web-server-01',
                },
                {
                    id: '2',
                    title: 'Service Down',
                    description: 'Nginx service stopped unexpectedly',
                    status: 'Resolved',
                    severity: 'Critical',
                    createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24).toISOString(),
                    resolvedAt: new Date(Date.now() - 1000 * 60 * 60 * 23).toISOString(),
                    hostName: 'lb-01',
                    serviceName: 'nginx',
                },
                {
                    id: '3',
                    title: 'Disk Space Low',
                    description: 'Free disk space is below 10%',
                    status: 'Investigating',
                    severity: 'Medium',
                    createdAt: new Date(Date.now() - 1000 * 60 * 60 * 2).toISOString(),
                    hostName: 'db-01',
                },
            ] as Incident[];
        },
    });

    const filteredIncidents = incidents?.filter((incident) => {
        const matchesStatus = statusFilter === 'all' || incident.status.toLowerCase() === statusFilter;
        const matchesSearch =
            incident.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
            incident.description.toLowerCase().includes(searchQuery.toLowerCase()) ||
            incident.hostName?.toLowerCase().includes(searchQuery.toLowerCase());
        return matchesStatus && matchesSearch;
    });

    const getSeverityColor = (severity: string) => {
        switch (severity) {
            case 'Critical': return 'destructive';
            case 'High': return 'destructive'; // Or orange if available
            case 'Medium': return 'default'; // Or yellow
            case 'Low': return 'secondary';
            default: return 'secondary';
        }
    };

    const getStatusIcon = (status: string) => {
        switch (status) {
            case 'Open': return <AlertCircle className="h-4 w-4 text-red-500" />;
            case 'Investigating': return <Clock className="h-4 w-4 text-yellow-500" />;
            case 'Resolved': return <CheckCircle2 className="h-4 w-4 text-green-500" />;
            default: return <AlertCircle className="h-4 w-4" />;
        }
    };

    return (
        <div className="space-y-4">
            <div className="flex flex-col sm:flex-row gap-4 justify-between">
                <div className="relative w-full sm:w-64">
                    <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                    <Input
                        placeholder="Search incidents..."
                        className="pl-8"
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                    />
                </div>
                <Select value={statusFilter} onValueChange={setStatusFilter}>
                    <SelectTrigger className="w-[180px]">
                        <SelectValue placeholder="Filter by status" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="all">All Statuses</SelectItem>
                        <SelectItem value="open">Open</SelectItem>
                        <SelectItem value="investigating">Investigating</SelectItem>
                        <SelectItem value="resolved">Resolved</SelectItem>
                    </SelectContent>
                </Select>
            </div>

            <div className="rounded-md border">
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Status</TableHead>
                            <TableHead>Severity</TableHead>
                            <TableHead>Title</TableHead>
                            <TableHead>Host</TableHead>
                            <TableHead>Created</TableHead>
                            <TableHead>Resolved</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={6} className="text-center py-8">
                                    Loading incidents...
                                </TableCell>
                            </TableRow>
                        ) : filteredIncidents?.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={6} className="text-center py-8 text-muted-foreground">
                                    No incidents found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            filteredIncidents?.map((incident) => (
                                <TableRow key={incident.id}>
                                    <TableCell>
                                        <div className="flex items-center gap-2">
                                            {getStatusIcon(incident.status)}
                                            <span>{incident.status}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <Badge variant={getSeverityColor(incident.severity) as any}>
                                            {incident.severity}
                                        </Badge>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex flex-col">
                                            <span className="font-medium">{incident.title}</span>
                                            <span className="text-xs text-muted-foreground">{incident.description}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell>{incident.hostName || '-'}</TableCell>
                                    <TableCell className="text-muted-foreground">
                                        {formatDistanceToNow(new Date(incident.createdAt), { addSuffix: true })}
                                    </TableCell>
                                    <TableCell className="text-muted-foreground">
                                        {incident.resolvedAt
                                            ? formatDistanceToNow(new Date(incident.resolvedAt), { addSuffix: true })
                                            : '-'}
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </div>
        </div>
    );
}

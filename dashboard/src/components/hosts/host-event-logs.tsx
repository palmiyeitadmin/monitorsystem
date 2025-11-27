'use client';

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from '@/components/ui/table';
import { Search, AlertCircle, AlertTriangle, Info } from 'lucide-react';
import { cn } from '@/lib/utils';
import { eventLogsApi, type EventLog } from '@/lib/api/eventlogs';
import { useDebounce } from '@/hooks/use-debounce';

interface HostEventLogsProps {
    hostId: string;
}

const LEVEL_CONFIG = {
    Critical: { icon: AlertCircle, color: 'text-red-500', bg: 'bg-red-500/10' },
    Error: { icon: AlertTriangle, color: 'text-orange-500', bg: 'bg-orange-500/10' },
    Warning: { icon: AlertTriangle, color: 'text-yellow-500', bg: 'bg-yellow-500/10' },
    Information: { icon: Info, color: 'text-blue-500', bg: 'bg-blue-500/10' },
};

export function HostEventLogs({ hostId }: HostEventLogsProps) {
    const [search, setSearch] = useState('');
    const [page, setPage] = useState(1);
    const [category, setCategory] = useState<string>('all');
    const [level, setLevel] = useState<string>('all');
    const pageSize = 50;

    const debouncedSearch = useDebounce(search, 500);

    // Fetch event logs
    const { data, isLoading } = useQuery({
        queryKey: ['eventlogs', hostId, page, category, level, debouncedSearch],
        queryFn: () =>
            eventLogsApi.getAll(hostId, {
                page,
                pageSize,
                category: category === 'all' ? undefined : category,
                level: level === 'all' ? undefined : level,
                search: debouncedSearch || undefined,
            }),
    });

    // Fetch categories for filter
    const { data: categories } = useQuery({
        queryKey: ['eventlog-categories', hostId],
        queryFn: () => eventLogsApi.getCategories(hostId),
    });

    const totalPages = data ? Math.ceil(data.totalCount / pageSize) : 0;

    const formatDate = (date: string) => {
        return new Date(date).toLocaleString('tr-TR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });
    };

    return (
        <div className="space-y-4">
            {/* Filters */}
            <Card>
                <CardContent className="pt-6">
                    <div className="flex flex-col gap-4 md:flex-row">
                        {/* Search */}
                        <div className="relative flex-1">
                            <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                            <Input
                                placeholder="Search in source or message..."
                                value={search}
                                onChange={(e) => setSearch(e.target.value)}
                                className="pl-9"
                            />
                        </div>

                        {/* Category Filter */}
                        <Select value={category} onValueChange={setCategory}>
                            <SelectTrigger className="w-full md:w-[180px]">
                                <SelectValue placeholder="All Categories" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="all">All Categories</SelectItem>
                                {categories?.map((cat: string) => (
                                    <SelectItem key={cat} value={cat}>
                                        {cat}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>

                        {/* Level Filter */}
                        <Select value={level} onValueChange={setLevel}>
                            <SelectTrigger className="w-full md:w-[180px]">
                                <SelectValue placeholder="All Levels" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="all">All Levels</SelectItem>
                                <SelectItem value="Critical">Critical</SelectItem>
                                <SelectItem value="Error">Error</SelectItem>
                                <SelectItem value="Warning">Warning</SelectItem>
                                <SelectItem value="Information">Information</SelectItem>
                            </SelectContent>
                        </Select>
                    </div>
                </CardContent>
            </Card>

            {/* Event Logs Table */}
            <Card>
                <CardHeader>
                    <CardTitle>
                        Event Logs
                        {data && ` (${data.totalCount} total)`}
                    </CardTitle>
                </CardHeader>
                <CardContent>
                    {isLoading ? (
                        <div className="text-center py-8">Loading event logs...</div>
                    ) : !data || data.items.length === 0 ? (
                        <div className="text-center py-8 text-muted-foreground">
                            No event logs found
                        </div>
                    ) : (
                        <>
                            <div className="rounded-md border">
                                <Table>
                                    <TableHeader>
                                        <TableRow>
                                            <TableHead className="w-[100px]">Level</TableHead>
                                            <TableHead className="w-[120px]">Event ID</TableHead>
                                            <TableHead className="w-[150px]">Category</TableHead>
                                            <TableHead className="w-[180px]">Source</TableHead>
                                            <TableHead>Message</TableHead>
                                            <TableHead className="w-[160px]">Time</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {data.items.map((log: EventLog) => {
                                            const levelConfig = LEVEL_CONFIG[log.level as keyof typeof LEVEL_CONFIG];
                                            const Icon = levelConfig?.icon || Info;

                                            return (
                                                <TableRow key={log.id}>
                                                    <TableCell>
                                                        <div
                                                            className={cn(
                                                                'flex items-center gap-2 rounded-md px-2 py-1',
                                                                levelConfig?.bg
                                                            )}
                                                        >
                                                            <Icon
                                                                className={cn('h-4 w-4', levelConfig?.color)}
                                                            />
                                                            <span className={cn('text-xs font-medium', levelConfig?.color)}>
                                                                {log.level}
                                                            </span>
                                                        </div>
                                                    </TableCell>
                                                    <TableCell className="font-mono text-sm">
                                                        {log.eventId}
                                                    </TableCell>
                                                    <TableCell>
                                                        <span className="inline-flex items-center rounded-md bg-primary/10 px-2 py-1 text-xs font-medium text-primary">
                                                            {log.category}
                                                        </span>
                                                    </TableCell>
                                                    <TableCell className="text-sm">{log.source}</TableCell>
                                                    <TableCell className="max-w-md truncate text-sm">
                                                        {log.message}
                                                    </TableCell>
                                                    <TableCell className="text-sm text-muted-foreground">
                                                        {formatDate(log.timeCreated)}
                                                    </TableCell>
                                                </TableRow>
                                            );
                                        })}
                                    </TableBody>
                                </Table>
                            </div>

                            {/* Pagination */}
                            {totalPages > 1 && (
                                <div className="flex items-center justify-between px-2 mt-4">
                                    <div className="text-sm text-muted-foreground">
                                        Showing {(page - 1) * pageSize + 1} to{' '}
                                        {Math.min(page * pageSize, data.totalCount)} of {data.totalCount} logs
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            onClick={() => setPage((p) => Math.max(1, p - 1))}
                                            disabled={page === 1}
                                        >
                                            Previous
                                        </Button>
                                        <div className="text-sm">
                                            Page {page} of {totalPages}
                                        </div>
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                                            disabled={page === totalPages}
                                        >
                                            Next
                                        </Button>
                                    </div>
                                </div>
                            )}
                        </>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}

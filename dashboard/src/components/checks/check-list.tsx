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
import { checksApi } from '@/lib/api/checks';
import { MoreVertical, Trash2, Edit, Search, Plus } from 'lucide-react';
import { toast } from 'sonner';
import { useDebounce } from '@/hooks/use-debounce';
import { formatDistanceToNow } from 'date-fns';
import { CheckDialog } from './check-dialog';

export function CheckList() {
    const [search, setSearch] = useState('');
    const [dialogOpen, setDialogOpen] = useState(false);
    const [selectedCheck, setSelectedCheck] = useState<any>(null); // Use proper type if possible, or cast later
    const debouncedSearch = useDebounce(search, 500);
    const queryClient = useQueryClient();

    const { data, isLoading } = useQuery({
        queryKey: ['checks', debouncedSearch],
        queryFn: () => checksApi.getAll({
            pageNumber: 1,
            pageSize: 50,
            search: debouncedSearch
        }),
    });

    const deleteMutation = useMutation({
        mutationFn: checksApi.delete,
        onSuccess: () => {
            toast.success('Check deleted successfully');
            queryClient.invalidateQueries({ queryKey: ['checks'] });
        },
        onError: (error: Error) => {
            toast.error(`Failed to delete check: ${error.message}`);
        },
    });

    const handleEdit = (check: any) => {
        setSelectedCheck(check);
        setDialogOpen(true);
    };

    const handleCreate = () => {
        setSelectedCheck(null);
        setDialogOpen(true);
    };

    if (isLoading) {
        return <div>Loading checks...</div>;
    }

    return (
        <div className="space-y-4">
            <div className="flex items-center gap-2">
                <div className="relative flex-1 max-w-sm">
                    <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                    <Input
                        placeholder="Search checks..."
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        className="pl-8"
                    />
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="mr-2 h-4 w-4" /> Add Check
                </Button>
            </div>

            <div className="rounded-md border">
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Name</TableHead>
                            <TableHead>Target</TableHead>
                            <TableHead>Type</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead>Last Check</TableHead>
                            <TableHead className="w-[50px]"></TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {data?.items.map((check) => (
                            <TableRow key={check.id}>
                                <TableCell className="font-medium">{check.name}</TableCell>
                                <TableCell>{check.target}</TableCell>
                                <TableCell>{check.checkType}</TableCell>
                                <TableCell>
                                    <StatusBadge status={check.currentStatus} />
                                </TableCell>
                                <TableCell>
                                    {check.lastCheckTime
                                        ? formatDistanceToNow(new Date(check.lastCheckTime), { addSuffix: true })
                                        : 'Never'}
                                </TableCell>
                                <TableCell>
                                    <DropdownMenu>
                                        <DropdownMenuTrigger asChild>
                                            <Button variant="ghost" size="icon">
                                                <MoreVertical className="h-4 w-4" />
                                            </Button>
                                        </DropdownMenuTrigger>
                                        <DropdownMenuContent align="end">
                                            <DropdownMenuItem onClick={() => handleEdit(check)}>
                                                <Edit className="mr-2 h-4 w-4" /> Edit
                                            </DropdownMenuItem>
                                            <DropdownMenuItem
                                                className="text-destructive focus:text-destructive"
                                                onClick={() => {
                                                    if (confirm('Are you sure you want to delete this check?')) {
                                                        deleteMutation.mutate(check.id);
                                                    }
                                                }}
                                            >
                                                <Trash2 className="mr-2 h-4 w-4" /> Delete
                                            </DropdownMenuItem>
                                        </DropdownMenuContent>
                                    </DropdownMenu>
                                </TableCell>
                            </TableRow>
                        ))}
                        {data?.items.length === 0 && (
                            <TableRow>
                                <TableCell colSpan={6} className="h-24 text-center">
                                    No checks found.
                                </TableCell>
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
            </div>

            <CheckDialog
                open={dialogOpen}
                onOpenChange={setDialogOpen}
                check={selectedCheck}
            />
        </div>
    );
}

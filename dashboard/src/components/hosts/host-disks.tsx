'use client';

import { Progress } from '@/components/ui/progress';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from '@/components/ui/table';
import { formatBytes } from '@/lib/utils';
import { HardDrive } from 'lucide-react';
import { HostDisk } from '@/types/host';

interface DiskListProps {
    hostId: string;
    disks?: HostDisk[];
}

export function DiskList({ hostId, disks = [] }: DiskListProps) {
    // Calculate aggregate usage
    const totalSpace = disks.reduce((acc, disk) => acc + disk.totalGb, 0);
    const totalUsed = disks.reduce((acc, disk) => acc + disk.usedGb, 0);
    const aggregateUsage = totalSpace > 0 ? (totalUsed / totalSpace) * 100 : 0;

    return (
        <div className="space-y-6">
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                <div className="rounded-xl border bg-card text-card-foreground shadow">
                    <div className="p-6 flex flex-row items-center justify-between space-y-0 pb-2">
                        <h3 className="tracking-tight text-sm font-medium">Total Disk Usage</h3>
                        <HardDrive className="h-4 w-4 text-muted-foreground" />
                    </div>
                    <div className="p-6 pt-0">
                        <div className="text-2xl font-bold">{aggregateUsage.toFixed(1)}%</div>
                        <Progress value={aggregateUsage} className="mt-2" />
                        <p className="text-xs text-muted-foreground mt-2">
                            {totalUsed.toFixed(0)} GB used of {totalSpace.toFixed(0)} GB
                        </p>
                    </div>
                </div>
            </div>

            <div className="rounded-md border">
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Mount Point</TableHead>
                            <TableHead>Filesystem</TableHead>
                            <TableHead>Total</TableHead>
                            <TableHead>Used</TableHead>
                            <TableHead>Free</TableHead>
                            <TableHead>Usage</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {disks.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={6} className="text-center text-muted-foreground">
                                    No disks found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            disks.map((disk) => (
                                <TableRow key={disk.id}>
                                    <TableCell className="font-medium">{disk.mountPoint}</TableCell>
                                    <TableCell>{disk.fileSystem}</TableCell>
                                    <TableCell>{disk.totalGb} GB</TableCell>
                                    <TableCell>{disk.usedGb} GB</TableCell>
                                    <TableCell>{disk.freeGb} GB</TableCell>
                                    <TableCell className="w-[200px]">
                                        <div className="flex items-center gap-2">
                                            <Progress value={disk.usedPercent} />
                                            <span className="text-xs text-muted-foreground">{disk.usedPercent.toFixed(0)}%</span>
                                        </div>
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

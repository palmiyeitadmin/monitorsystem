"use client";

import { ColumnDef } from "@tanstack/react-table";
import { Host, useHosts } from "@/hooks/useHosts";
import { DataTable } from "@/components/common/DataTable";
import { StatusBadge } from "@/components/common/StatusBadge";
import { Button } from "@/components/ui/button";
import { MoreHorizontal, Trash } from "lucide-react";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useState } from "react";
import { ConfirmDialog } from "@/components/common/ConfirmDialog";
import { formatDistanceToNow } from "date-fns";

export function HostsTable() {
    const { hosts, deleteHost } = useHosts();
    const [deleteId, setDeleteId] = useState<string | null>(null);

    const columns: ColumnDef<Host>[] = [
        {
            accessorKey: "name",
            header: "Name",
        },
        {
            accessorKey: "ipAddress",
            header: "IP Address",
        },
        {
            accessorKey: "status",
            header: "Status",
            cell: ({ row }) => <StatusBadge status={row.getValue("status")} />,
        },
        {
            accessorKey: "lastSeen",
            header: "Last Seen",
            cell: ({ row }) => {
                const date = row.getValue("lastSeen") as string | null;
                if (!date) return "Never";
                return formatDistanceToNow(new Date(date), { addSuffix: true });
            },
        },
        {
            id: "actions",
            cell: ({ row }) => {
                const host = row.original;

                return (
                    <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                            <Button variant="ghost" className="h-8 w-8 p-0">
                                <span className="sr-only">Open menu</span>
                                <MoreHorizontal className="h-4 w-4" />
                            </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                            <DropdownMenuLabel>Actions</DropdownMenuLabel>
                            <DropdownMenuItem
                                onClick={() => host.apiKey && navigator.clipboard.writeText(host.apiKey)}
                            >
                                Copy API Key
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                                className="text-red-600"
                                onClick={() => setDeleteId(host.id)}
                            >
                                <Trash className="mr-2 h-4 w-4" />
                                Delete Host
                            </DropdownMenuItem>
                        </DropdownMenuContent>
                    </DropdownMenu>
                );
            },
        },
    ];

    return (
        <>
            <DataTable columns={columns} data={hosts} searchKey="name" />

            <ConfirmDialog
                open={!!deleteId}
                onOpenChange={(open) => !open && setDeleteId(null)}
                title="Delete Host"
                description="Are you sure you want to delete this host? This action cannot be undone."
                onConfirm={() => {
                    if (deleteId) {
                        deleteHost(deleteId);
                        setDeleteId(null);
                    }
                }}
                variant="destructive"
                confirmText="Delete"
            />
        </>
    );
}

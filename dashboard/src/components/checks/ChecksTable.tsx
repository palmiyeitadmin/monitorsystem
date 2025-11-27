"use client";

import { ColumnDef } from "@tanstack/react-table";
import { Check, useChecks } from "@/hooks/useChecks";
import { DataTable } from "@/components/common/DataTable";
import { StatusBadge } from "@/components/common/StatusBadge";
import { formatDistanceToNow } from "date-fns";
import { Badge } from "@/components/ui/badge";

export function ChecksTable() {
    const { checks } = useChecks();

    const columns: ColumnDef<Check>[] = [
        {
            accessorKey: "name",
            header: "Name",
        },
        {
            accessorKey: "type",
            header: "Type",
            cell: ({ row }) => (
                <Badge variant="secondary" className="uppercase">
                    {row.getValue("type")}
                </Badge>
            ),
        },
        {
            accessorKey: "target",
            header: "Target",
        },
        {
            accessorKey: "status",
            header: "Status",
            cell: ({ row }) => <StatusBadge status={row.getValue("status")} />,
        },
        {
            accessorKey: "intervalSeconds",
            header: "Interval (s)",
        },
        {
            accessorKey: "lastRun",
            header: "Last Run",
            cell: ({ row }) => {
                const date = row.getValue("lastRun") as string | null;
                if (!date) return "Never";
                return formatDistanceToNow(new Date(date), { addSuffix: true });
            },
        },
    ];

    return <DataTable columns={columns} data={checks} searchKey="name" />;
}

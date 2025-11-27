"use client";

import { ColumnDef } from "@tanstack/react-table";
import { Service, useServices } from "@/hooks/useServices";
import { DataTable } from "@/components/common/DataTable";
import { StatusBadge } from "@/components/common/StatusBadge";
import { formatDistanceToNow } from "date-fns";

export function ServicesTable() {
    const { services } = useServices();

    const columns: ColumnDef<Service>[] = [
        {
            accessorKey: "name",
            header: "Service Name",
        },
        {
            accessorKey: "hostName",
            header: "Host",
        },
        {
            accessorKey: "port",
            header: "Port",
        },
        {
            accessorKey: "status",
            header: "Status",
            cell: ({ row }) => <StatusBadge status={row.getValue("status")} />,
        },
        {
            accessorKey: "lastChecked",
            header: "Last Checked",
            cell: ({ row }) => {
                const date = row.getValue("lastChecked") as string | null;
                if (!date) return "Never";
                return formatDistanceToNow(new Date(date), { addSuffix: true });
            },
        },
    ];

    return <DataTable columns={columns} data={services} searchKey="name" />;
}

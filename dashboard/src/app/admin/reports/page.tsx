"use client";

import { useReports } from "@/hooks/useReports";
import { Button } from "@/components/ui/button";
import { DataTable } from "@/components/common/DataTable";
import { StatusBadge } from "@/components/common/StatusBadge";
import { ColumnDef } from "@tanstack/react-table";
import { Report } from "@/hooks/useReports";
import { formatDistanceToNow } from "date-fns";
import { Download, FileText } from "lucide-react";

export default function ReportsPage() {
    const { reports, generateReport, isGenerating } = useReports();

    const columns: ColumnDef<Report>[] = [
        {
            accessorKey: "title",
            header: "Report Title",
            cell: ({ row }) => (
                <div className="flex items-center gap-2">
                    <FileText className="h-4 w-4 text-muted-foreground" />
                    {row.getValue("title")}
                </div>
            ),
        },
        {
            accessorKey: "type",
            header: "Type",
            cell: ({ row }) => <span className="capitalize">{row.getValue("type")}</span>,
        },
        {
            accessorKey: "status",
            header: "Status",
            cell: ({ row }) => <StatusBadge status={row.getValue("status")} />,
        },
        {
            accessorKey: "generatedAt",
            header: "Generated",
            cell: ({ row }) => {
                const date = row.getValue("generatedAt") as string | null;
                if (!date) return "Pending";
                return formatDistanceToNow(new Date(date), { addSuffix: true });
            },
        },
        {
            id: "actions",
            cell: ({ row }) => {
                const report = row.original;
                if (report.status !== "generated" || !report.downloadUrl) return null;

                return (
                    <Button variant="ghost" size="sm" asChild>
                        <a href={report.downloadUrl} download>
                            <Download className="mr-2 h-4 w-4" />
                            Download
                        </a>
                    </Button>
                );
            },
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">Reports</h2>
                    <p className="text-muted-foreground">
                        Generate and download system performance reports.
                    </p>
                </div>
                <Button onClick={() => generateReport({ type: "monthly" })} disabled={isGenerating}>
                    {isGenerating ? "Generating..." : "Generate Report"}
                </Button>
            </div>
            <DataTable columns={columns} data={reports} searchKey="title" />
        </div>
    );
}

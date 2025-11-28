"use client";

import { useEffect, useState } from "react";
import { Check } from "@/types/check";
import { checksApi } from "@/lib/api/checks";
import { CheckList } from "@/components/checks/check-list";
import { CheckFormDialog } from "@/components/checks/check-form-dialog";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";

export default function ChecksPage() {
    const [checks, setChecks] = useState<Check[]>([]);
    const [loading, setLoading] = useState(true);
    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const [search, setSearch] = useState("");
    const [typeFilter, setTypeFilter] = useState<string>("all");

    const fetchChecks = async () => {
        try {
            setLoading(true);
            const data = await checksApi.getAll({
                search: search || undefined,
                type: typeFilter !== "all" ? typeFilter : undefined,
            });
            setChecks(data);
        } catch (error) {
            console.error(error);
            toast.error("Failed to fetch checks");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchChecks();
    }, [search, typeFilter]);

    return (
        <div className="flex-1 space-y-4 p-8 pt-6">
            <div className="flex items-center justify-between space-y-2">
                <h2 className="text-3xl font-bold tracking-tight">Uptime Monitors</h2>
                <div className="flex items-center space-x-2">
                    <Button onClick={() => setIsCreateOpen(true)}>
                        <Plus className="mr-2 h-4 w-4" /> New Check
                    </Button>
                </div>
            </div>

            <div className="flex items-center space-x-2">
                <Input
                    placeholder="Search checks..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    className="h-8 w-[150px] lg:w-[250px]"
                />
                <Select value={typeFilter} onValueChange={setTypeFilter}>
                    <SelectTrigger className="h-8 w-[150px]">
                        <SelectValue placeholder="Filter by type" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="all">All Types</SelectItem>
                        <SelectItem value="HTTP">HTTP(s)</SelectItem>
                        <SelectItem value="TCP">TCP Port</SelectItem>
                        <SelectItem value="Ping">Ping</SelectItem>
                        <SelectItem value="DNS">DNS</SelectItem>
                    </SelectContent>
                </Select>
            </div>

            {loading ? (
                <div className="flex items-center justify-center h-64">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
                </div>
            ) : (
                <CheckList checks={checks} onRefresh={fetchChecks} />
            )}

            <CheckFormDialog
                open={isCreateOpen}
                onOpenChange={setIsCreateOpen}
                onSuccess={fetchChecks}
            />
        </div>
    );
}

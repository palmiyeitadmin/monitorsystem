"use client";

import { useState } from "react";
import { Check } from "@/types/check";
import { checksApi } from "@/lib/api/checks";
import { toast } from "sonner";
import { CheckCard } from "./check-card";
import { Activity } from "lucide-react";
import { CheckFormDialog } from "./check-form-dialog";

interface CheckListProps {
    checks: Check[];
    onRefresh: () => void;
}

export function CheckList({ checks, onRefresh }: CheckListProps) {
    const [editingCheck, setEditingCheck] = useState<Check | undefined>(undefined);
    const [isFormOpen, setIsFormOpen] = useState(false);

    const handleDelete = async (id: string) => {
        if (!confirm("Are you sure you want to delete this check?")) return;
        try {
            await checksApi.delete(id);
            toast.success("Check deleted");
            onRefresh();
        } catch (error) {
            console.error(error);
            toast.error("Failed to delete check");
        }
    };

    const handleToggle = async (check: Check) => {
        try {
            await checksApi.update(check.id, { monitoringEnabled: !check.monitoringEnabled });
            toast.success(`Monitoring ${check.monitoringEnabled ? "paused" : "resumed"}`);
            onRefresh();
        } catch (error) {
            console.error(error);
            toast.error("Failed to update status");
        }
    };

    return (
        <>
            {checks.length === 0 ? (
                <div className="flex h-[400px] flex-col items-center justify-center rounded-md border border-dashed p-8 text-center animate-in fade-in-50">
                    <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-secondary">
                        <Activity className="h-6 w-6 text-muted-foreground" />
                    </div>
                    <h3 className="mt-4 text-lg font-semibold">No checks created</h3>
                    <p className="mb-4 mt-2 text-sm text-muted-foreground">
                        Start monitoring your services by creating your first check.
                    </p>
                </div>
            ) : (
                <div className="grid gap-4 md:grid-cols-1 lg:grid-cols-2 xl:grid-cols-3">
                    {checks.map((check) => (
                        <CheckCard
                            key={check.id}
                            check={check}
                            onEdit={(c) => {
                                setEditingCheck(c);
                                setIsFormOpen(true);
                            }}
                            onDelete={handleDelete}
                            onToggle={handleToggle}
                        />
                    ))}
                </div>
            )}

            <CheckFormDialog
                open={isFormOpen}
                onOpenChange={(open) => {
                    setIsFormOpen(open);
                    if (!open) setEditingCheck(undefined);
                }}
                check={editingCheck}
                onSuccess={onRefresh}
            />
        </>
    );
}

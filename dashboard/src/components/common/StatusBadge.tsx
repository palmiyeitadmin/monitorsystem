import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

type StatusType = "up" | "down" | "warning" | "maintenance" | "unknown" | "success" | "error" | "pending";

interface StatusBadgeProps {
    status: StatusType | string;
    className?: string;
    variant?: "default" | "secondary" | "destructive" | "outline";
}

const statusStyles: Record<string, string> = {
    up: "bg-green-100 text-green-800 hover:bg-green-100 dark:bg-green-900/30 dark:text-green-400",
    success: "bg-green-100 text-green-800 hover:bg-green-100 dark:bg-green-900/30 dark:text-green-400",
    down: "bg-red-100 text-red-800 hover:bg-red-100 dark:bg-red-900/30 dark:text-red-400",
    error: "bg-red-100 text-red-800 hover:bg-red-100 dark:bg-red-900/30 dark:text-red-400",
    warning: "bg-yellow-100 text-yellow-800 hover:bg-yellow-100 dark:bg-yellow-900/30 dark:text-yellow-400",
    maintenance: "bg-blue-100 text-blue-800 hover:bg-blue-100 dark:bg-blue-900/30 dark:text-blue-400",
    pending: "bg-gray-100 text-gray-800 hover:bg-gray-100 dark:bg-gray-800 dark:text-gray-400",
    unknown: "bg-gray-100 text-gray-800 hover:bg-gray-100 dark:bg-gray-800 dark:text-gray-400",
};

export function StatusBadge({ status, className, variant = "outline" }: StatusBadgeProps) {
    const normalizedStatus = (status ?? "unknown").toString().toLowerCase();
    const styleClass = statusStyles[normalizedStatus] || statusStyles.unknown;

    return (
        <Badge
            variant={variant}
            className={cn("capitalize font-medium border-0", styleClass, className)}
        >
            {status}
        </Badge>
    );
}

import { cn } from "@/lib/utils";

type StatusType = "up" | "down" | "warning" | "maintenance" | "unknown" | "success" | "error" | "pending";

interface StatusDotProps {
    status: StatusType | string;
    className?: string;
    pulse?: boolean;
}

const statusColorMap: Record<string, string> = {
    up: "bg-green-500",
    success: "bg-green-500",
    down: "bg-red-500",
    error: "bg-red-500",
    warning: "bg-yellow-500",
    maintenance: "bg-blue-500",
    pending: "bg-gray-400",
    unknown: "bg-gray-300",
};

export function StatusDot({ status, className, pulse = false }: StatusDotProps) {
    const normalizedStatus = status.toLowerCase();
    const colorClass = statusColorMap[normalizedStatus] || statusColorMap.unknown;

    return (
        <div className={cn("relative flex h-3 w-3", className)}>
            {pulse && (normalizedStatus === "down" || normalizedStatus === "warning") && (
                <span
                    className={cn(
                        "animate-ping absolute inline-flex h-full w-full rounded-full opacity-75",
                        colorClass
                    )}
                ></span>
            )}
            <span className={cn("relative inline-flex rounded-full h-3 w-3", colorClass)}></span>
        </div>
    );
}

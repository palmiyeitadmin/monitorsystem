import { cn } from '@/lib/utils';
import type { StatusType } from '@/types';

interface StatusBadgeProps {
    status: StatusType;
    size?: 'sm' | 'md' | 'lg';
    showPulse?: boolean;
    className?: string;
}

const statusConfig: Record<StatusType, { label: string; className: string }> = {
    Up: {
        label: 'Up',
        className: 'bg-status-up/10 text-status-up border-status-up/20',
    },
    Down: {
        label: 'Down',
        className: 'bg-status-down/10 text-status-down border-status-down/20',
    },
    Warning: {
        label: 'Warning',
        className: 'bg-status-warning/10 text-status-warning border-status-warning/20',
    },
    Degraded: {
        label: 'Degraded',
        className: 'bg-status-warning/10 text-status-warning border-status-warning/20',
    },
    Maintenance: {
        label: 'Maintenance',
        className: 'bg-status-maintenance/10 text-status-maintenance border-status-maintenance/20',
    },
    Unknown: {
        label: 'Unknown',
        className: 'bg-status-unknown/10 text-status-unknown border-status-unknown/20',
    },
};

const sizeClasses = {
    sm: 'text-xs px-1.5 py-0.5',
    md: 'text-sm px-2 py-1',
    lg: 'text-base px-3 py-1.5',
};

export function StatusBadge({
    status,
    size = 'md',
    showPulse = false,
    className
}: StatusBadgeProps) {
    const config = statusConfig[status] || statusConfig.Unknown;

    return (
        <span
            className={cn(
                'inline-flex items-center gap-1.5 rounded-full border font-medium',
                config.className,
                sizeClasses[size],
                className
            )}
        >
            {showPulse && (
                <span className="relative flex h-2 w-2">
                    {status === 'Up' && (
                        <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-status-up opacity-75" />
                    )}
                    <span
                        className={cn(
                            'relative inline-flex rounded-full h-2 w-2',
                            status === 'Up' && 'bg-status-up',
                            status === 'Down' && 'bg-status-down',
                            status === 'Warning' && 'bg-status-warning',
                            status === 'Degraded' && 'bg-status-warning',
                            status === 'Maintenance' && 'bg-status-maintenance',
                            status === 'Unknown' && 'bg-status-unknown'
                        )}
                    />
                </span>
            )}
            {config.label}
        </span>
    );
}

import { cn } from '@/lib/utils';
import type { IncidentSeverity } from '@/types/incident';

interface SeverityBadgeProps {
    severity: IncidentSeverity;
    size?: 'sm' | 'md' | 'lg';
    className?: string;
}

const severityConfig: Record<IncidentSeverity, { label: string; className: string }> = {
    Critical: {
        label: 'Critical',
        className: 'bg-severity-critical/10 text-severity-critical border-severity-critical/20',
    },
    High: {
        label: 'High',
        className: 'bg-severity-high/10 text-severity-high border-severity-high/20',
    },
    Medium: {
        label: 'Medium',
        className: 'bg-severity-medium/10 text-severity-medium border-severity-medium/20',
    },
    Low: {
        label: 'Low',
        className: 'bg-severity-low/10 text-severity-low border-severity-low/20',
    },
    Info: {
        label: 'Info',
        className: 'bg-severity-info/10 text-severity-info border-severity-info/20',
    },
};

const sizeClasses = {
    sm: 'text-xs px-1.5 py-0.5',
    md: 'text-sm px-2 py-1',
    lg: 'text-base px-3 py-1.5',
};

export function SeverityBadge({ severity, size = 'md', className }: SeverityBadgeProps) {
    const config = severityConfig[severity] || severityConfig.Medium;

    return (
        <span
            className={cn(
                'inline-flex items-center rounded-full border font-medium',
                config.className,
                sizeClasses[size],
                className
            )}
        >
            {config.label}
        </span>
    );
}

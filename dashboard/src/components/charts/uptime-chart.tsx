'use client';

import { useMemo } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';
import { cn } from '@/lib/utils';
import { format, subDays } from 'date-fns';

interface UptimeDay {
    date: string;
    uptimePercent: number;
    outages: number;
    status: 'up' | 'partial' | 'down' | 'maintenance' | 'unknown';
}

interface UptimeChartProps {
    data: UptimeDay[];
    days?: number;
    title?: string;
    className?: string;
}

export function UptimeChart({ data, days = 90, title = 'Uptime', className }: UptimeChartProps) {
    const chartDays = useMemo(() => {
        const result: UptimeDay[] = [];
        const today = new Date();

        for (let i = days - 1; i >= 0; i--) {
            const date = format(subDays(today, i), 'yyyy-MM-dd');
            const dayData = data.find((d) => d.date === date);

            result.push(
                dayData || {
                    date,
                    uptimePercent: 100,
                    outages: 0,
                    status: 'unknown',
                }
            );
        }

        return result;
    }, [data, days]);

    const averageUptime = useMemo(() => {
        const validDays = chartDays.filter((d) => d.status !== 'unknown');
        if (validDays.length === 0) return 100;
        return validDays.reduce((sum, d) => sum + d.uptimePercent, 0) / validDays.length;
    }, [chartDays]);

    const getStatusColor = (day: UptimeDay) => {
        if (day.status === 'unknown') return 'bg-muted';
        if (day.status === 'maintenance') return 'bg-status-maintenance';
        if (day.uptimePercent >= 99.9) return 'bg-status-up';
        if (day.uptimePercent >= 95) return 'bg-status-warning';
        return 'bg-status-down';
    };

    return (
        <Card className={className}>
            <CardHeader>
                <div className="flex items-center justify-between">
                    <CardTitle className="text-base">{title}</CardTitle>
                    <span className="text-2xl font-bold text-primary">
                        {averageUptime.toFixed(2)}%
                    </span>
                </div>
            </CardHeader>
            <CardContent>
                <div className="flex gap-[2px] flex-wrap">
                    {chartDays.map((day, index) => (
                        <Tooltip key={day.date}>
                            <TooltipTrigger>
                                <div
                                    className={cn(
                                        'w-3 h-8 rounded-sm transition-colors hover:opacity-80',
                                        getStatusColor(day)
                                    )}
                                />
                            </TooltipTrigger>
                            <TooltipContent>
                                <div className="text-sm">
                                    <p className="font-medium">{format(new Date(day.date), 'MMM dd, yyyy')}</p>
                                    <p>Uptime: {day.uptimePercent.toFixed(2)}%</p>
                                    {day.outages > 0 && <p>Outages: {day.outages}</p>}
                                </div>
                            </TooltipContent>
                        </Tooltip>
                    ))}
                </div>
                <div className="flex items-center justify-between mt-4 text-xs text-muted-foreground">
                    <span>{days} days ago</span>
                    <div className="flex items-center gap-4">
                        <span className="flex items-center gap-1">
                            <span className="w-3 h-3 rounded-sm bg-status-up" />
                            100%
                        </span>
                        <span className="flex items-center gap-1">
                            <span className="w-3 h-3 rounded-sm bg-status-warning" />
                            95-99%
                        </span>
                        <span className="flex items-center gap-1">
                            <span className="w-3 h-3 rounded-sm bg-status-down" />
                            &lt;95%
                        </span>
                    </div>
                    <span>Today</span>
                </div>
            </CardContent>
        </Card>
    );
}

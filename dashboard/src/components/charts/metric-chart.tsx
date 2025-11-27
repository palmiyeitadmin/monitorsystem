'use client';

import { useMemo } from 'react';
import {
    Area,
    AreaChart,
    CartesianGrid,
    ResponsiveContainer,
    Tooltip,
    XAxis,
    YAxis,
} from 'recharts';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { format } from 'date-fns';
import type { MetricDataPoint } from '@/types/host';
import { cn } from '@/lib/utils';

interface MetricChartProps {
    data: MetricDataPoint[];
    title: string;
    unit: string;
    color?: string;
    height?: number;
    maxValue?: number;
    className?: string;
}

export function MetricChart({
    data,
    title,
    unit,
    color = '#29ABE2',
    height = 300,
    maxValue,
    className
}: MetricChartProps) {
    const chartData = useMemo(() => {
        return data.map((point) => ({
            timestamp: new Date(point.timestamp).getTime(),
            value: point.value,
        }));
    }, [data]);

    const formatXAxis = (timestamp: number) => {
        return format(new Date(timestamp), 'HH:mm');
    };

    const formatTooltip = (value: number) => {
        return [`${value.toFixed(1)}${unit}`, title];
    };

    const formatTooltipLabel = (timestamp: number) => {
        return format(new Date(timestamp), 'MMM dd, HH:mm:ss');
    };

    const currentValue = chartData.length > 0 ? chartData[chartData.length - 1].value : 0;

    return (
        <Card className={cn("overflow-hidden", className)}>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
                <CardTitle className="text-base font-medium">{title}</CardTitle>
                <span className="text-2xl font-bold">
                    {currentValue.toFixed(1)}{unit}
                </span>
            </CardHeader>
            <CardContent>
                <div style={{ height }}>
                    <ResponsiveContainer width="100%" height="100%">
                        <AreaChart
                            data={chartData}
                            margin={{ top: 10, right: 10, left: 0, bottom: 0 }}
                        >
                            <defs>
                                <linearGradient id={`gradient-${title}`} x1="0" y1="0" x2="0" y2="1">
                                    <stop offset="5%" stopColor={color} stopOpacity={0.3} />
                                    <stop offset="95%" stopColor={color} stopOpacity={0} />
                                </linearGradient>
                            </defs>
                            <CartesianGrid strokeDasharray="3 3" className="stroke-border" vertical={false} />
                            <XAxis
                                dataKey="timestamp"
                                tickFormatter={formatXAxis}
                                fontSize={12}
                                tickLine={false}
                                axisLine={false}
                                className="fill-muted-foreground"
                                minTickGap={30}
                            />
                            <YAxis
                                domain={[0, maxValue || 'auto']}
                                tickFormatter={(v) => `${v}${unit}`}
                                fontSize={12}
                                tickLine={false}
                                axisLine={false}
                                className="fill-muted-foreground"
                                width={45}
                            />
                            <Tooltip
                                formatter={formatTooltip}
                                labelFormatter={formatTooltipLabel}
                                contentStyle={{
                                    backgroundColor: 'hsl(var(--card))',
                                    border: '1px solid hsl(var(--border))',
                                    borderRadius: '6px',
                                    boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)',
                                }}
                                itemStyle={{ color: 'hsl(var(--foreground))' }}
                            />
                            <Area
                                type="monotone"
                                dataKey="value"
                                stroke={color}
                                strokeWidth={2}
                                fill={`url(#gradient-${title})`}
                                animationDuration={500}
                            />
                        </AreaChart>
                    </ResponsiveContainer>
                </div>
            </CardContent>
        </Card>
    );
}

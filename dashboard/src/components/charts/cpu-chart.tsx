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

interface CpuChartProps {
    data: MetricDataPoint[];
    title?: string;
    height?: number;
}

export function CpuChart({ data, title = 'CPU Usage', height = 300 }: CpuChartProps) {
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
        return [`${value.toFixed(1)}%`, 'CPU'];
    };

    const formatTooltipLabel = (timestamp: number) => {
        return format(new Date(timestamp), 'MMM dd, HH:mm:ss');
    };

    return (
        <Card>
            <CardHeader>
                <CardTitle className="text-base">{title}</CardTitle>
            </CardHeader>
            <CardContent>
                <ResponsiveContainer width="100%" height={height}>
                    <AreaChart
                        data={chartData}
                        margin={{ top: 10, right: 10, left: 0, bottom: 0 }}
                    >
                        <defs>
                            <linearGradient id="cpuGradient" x1="0" y1="0" x2="0" y2="1">
                                <stop offset="5%" stopColor="#29ABE2" stopOpacity={0.3} />
                                <stop offset="95%" stopColor="#29ABE2" stopOpacity={0} />
                            </linearGradient>
                        </defs>
                        <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                        <XAxis
                            dataKey="timestamp"
                            tickFormatter={formatXAxis}
                            fontSize={12}
                            tickLine={false}
                            axisLine={false}
                            className="fill-muted-foreground"
                        />
                        <YAxis
                            domain={[0, 100]}
                            tickFormatter={(v) => `${v}%`}
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
                            }}
                        />
                        <Area
                            type="monotone"
                            dataKey="value"
                            stroke="#29ABE2"
                            strokeWidth={2}
                            fill="url(#cpuGradient)"
                        />
                    </AreaChart>
                </ResponsiveContainer>
            </CardContent>
        </Card>
    );
}

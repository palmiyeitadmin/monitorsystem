"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";

const data = [
    {
        time: "00:00",
        uptime: 100,
        latency: 24,
    },
    {
        time: "04:00",
        uptime: 99.9,
        latency: 28,
    },
    {
        time: "08:00",
        uptime: 100,
        latency: 22,
    },
    {
        time: "12:00",
        uptime: 98.5,
        latency: 45,
    },
    {
        time: "16:00",
        uptime: 100,
        latency: 25,
    },
    {
        time: "20:00",
        uptime: 100,
        latency: 23,
    },
    {
        time: "24:00",
        uptime: 100,
        latency: 24,
    },
];

export function SystemHealthChart() {
    return (
        <Card className="col-span-4">
            <CardHeader>
                <CardTitle>System Health Overview</CardTitle>
            </CardHeader>
            <CardContent className="pl-2">
                <ResponsiveContainer width="100%" height={350}>
                    <LineChart data={data}>
                        <XAxis
                            dataKey="time"
                            stroke="#888888"
                            fontSize={12}
                            tickLine={false}
                            axisLine={false}
                        />
                        <YAxis
                            stroke="#888888"
                            fontSize={12}
                            tickLine={false}
                            axisLine={false}
                            tickFormatter={(value) => `${value}%`}
                        />
                        <Tooltip />
                        <Line
                            type="monotone"
                            dataKey="uptime"
                            stroke="#22c55e"
                            strokeWidth={2}
                            activeDot={{ r: 8 }}
                        />
                        <Line
                            type="monotone"
                            dataKey="latency"
                            stroke="#3b82f6"
                            strokeWidth={2}
                        />
                    </LineChart>
                </ResponsiveContainer>
            </CardContent>
        </Card>
    );
}

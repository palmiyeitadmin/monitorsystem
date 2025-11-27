'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import { Calendar } from '@/components/ui/calendar';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import { CalendarIcon, Download } from 'lucide-react';
import { format } from 'date-fns';
import { cn } from '@/lib/utils';
import { toast } from 'sonner';

export function ReportGenerator() {
    const [reportType, setReportType] = useState('uptime');
    const [date, setDate] = useState<Date | undefined>(new Date());

    const handleGenerate = () => {
        toast.promise(
            new Promise((resolve) => setTimeout(resolve, 2000)),
            {
                loading: 'Generating report...',
                success: 'Report generated successfully',
                error: 'Failed to generate report',
            }
        );
    };

    return (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            <Card>
                <CardHeader>
                    <CardTitle>Uptime Report</CardTitle>
                    <CardDescription>
                        Generate detailed uptime statistics for all hosts and checks.
                    </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="space-y-2">
                        <label className="text-sm font-medium">Date Range</label>
                        <Popover>
                            <PopoverTrigger asChild>
                                <Button
                                    variant={"outline"}
                                    className={cn(
                                        "w-full justify-start text-left font-normal",
                                        !date && "text-muted-foreground"
                                    )}
                                >
                                    <CalendarIcon className="mr-2 h-4 w-4" />
                                    {date ? format(date, "PPP") : <span>Pick a date</span>}
                                </Button>
                            </PopoverTrigger>
                            <PopoverContent className="w-auto p-0">
                                <Calendar
                                    mode="single"
                                    selected={date}
                                    onSelect={setDate}
                                    initialFocus
                                />
                            </PopoverContent>
                        </Popover>
                    </div>
                    <Button className="w-full" onClick={handleGenerate}>
                        <Download className="mr-2 h-4 w-4" />
                        Generate PDF
                    </Button>
                </CardContent>
            </Card>

            <Card>
                <CardHeader>
                    <CardTitle>Incident Report</CardTitle>
                    <CardDescription>
                        Summary of all incidents, resolution times, and severity.
                    </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="space-y-2">
                        <label className="text-sm font-medium">Period</label>
                        <Select defaultValue="30d">
                            <SelectTrigger>
                                <SelectValue placeholder="Select period" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="7d">Last 7 days</SelectItem>
                                <SelectItem value="30d">Last 30 days</SelectItem>
                                <SelectItem value="90d">Last 3 months</SelectItem>
                            </SelectContent>
                        </Select>
                    </div>
                    <Button className="w-full" variant="secondary" onClick={handleGenerate}>
                        <Download className="mr-2 h-4 w-4" />
                        Generate CSV
                    </Button>
                </CardContent>
            </Card>

            <Card>
                <CardHeader>
                    <CardTitle>Resource Usage</CardTitle>
                    <CardDescription>
                        Historical CPU, RAM, and Disk usage trends.
                    </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="space-y-2">
                        <label className="text-sm font-medium">Format</label>
                        <Select defaultValue="pdf">
                            <SelectTrigger>
                                <SelectValue placeholder="Select format" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="pdf">PDF Document</SelectItem>
                                <SelectItem value="excel">Excel Spreadsheet</SelectItem>
                            </SelectContent>
                        </Select>
                    </div>
                    <Button className="w-full" variant="outline" onClick={handleGenerate}>
                        <Download className="mr-2 h-4 w-4" />
                        Generate Report
                    </Button>
                </CardContent>
            </Card>
        </div>
    );
}

'use client';

import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { hostsApi } from '@/lib/api/hosts';
import {
    Form,
    FormControl,
    FormDescription,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { Separator } from '@/components/ui/separator';
import { toast } from 'sonner';
import { Copy, Check, Key } from 'lucide-react';
import { useState } from 'react';
import type { HostDetail } from '@/types/host';

const hostSettingsSchema = z.object({
    checkIntervalSeconds: z.coerce.number().min(10, 'Minimum interval is 10 seconds'),
    monitoringEnabled: z.boolean(),

    // Thresholds
    cpuWarning: z.coerce.number().min(0).max(100),
    cpuCritical: z.coerce.number().min(0).max(100),
    ramWarning: z.coerce.number().min(0).max(100),
    ramCritical: z.coerce.number().min(0).max(100),
    diskWarning: z.coerce.number().min(0).max(100),
    diskCritical: z.coerce.number().min(0).max(100),

    // Monitoring Settings
    alertOnDown: z.boolean(),
    alertDelaySeconds: z.coerce.number().min(0),
    alertOnHighCpu: z.boolean(),
    alertOnHighRam: z.boolean(),
    alertOnHighDisk: z.boolean(),
});

type HostSettingsValues = z.infer<typeof hostSettingsSchema>;

interface HostSettingsFormProps {
    host: HostDetail;
}

export function HostSettingsForm({ host }: HostSettingsFormProps) {
    const queryClient = useQueryClient();

    const form = useForm<HostSettingsValues>({
        resolver: zodResolver(hostSettingsSchema) as any,
        defaultValues: {
            checkIntervalSeconds: host.checkIntervalSeconds,
            monitoringEnabled: host.monitoringEnabled,

            cpuWarning: host.thresholds?.cpuWarning ?? 80,
            cpuCritical: host.thresholds?.cpuCritical ?? 90,
            ramWarning: host.thresholds?.ramWarning ?? 80,
            ramCritical: host.thresholds?.ramCritical ?? 90,
            diskWarning: host.thresholds?.diskWarning ?? 80,
            diskCritical: host.thresholds?.diskCritical ?? 90,

            alertOnDown: host.monitoringSettings?.alertOnDown ?? true,
            alertDelaySeconds: host.monitoringSettings?.alertDelaySeconds ?? 300,
            alertOnHighCpu: host.monitoringSettings?.alertOnHighCpu ?? true,
            alertOnHighRam: host.monitoringSettings?.alertOnHighRam ?? true,
            alertOnHighDisk: host.monitoringSettings?.alertOnHighDisk ?? true,
        },
    });

    const updateMutation = useMutation({
        mutationFn: (values: HostSettingsValues) => {
            return hostsApi.update(host.id, {
                name: host.name, // Required by API but not changed here
                hostname: host.hostname, // Required by API
                osType: host.osType, // Required by API
                checkIntervalSeconds: values.checkIntervalSeconds,
                monitoringEnabled: values.monitoringEnabled,
                thresholds: {
                    cpuWarning: values.cpuWarning,
                    cpuCritical: values.cpuCritical,
                    ramWarning: values.ramWarning,
                    ramCritical: values.ramCritical,
                    diskWarning: values.diskWarning,
                    diskCritical: values.diskCritical,
                },
                monitoringSettings: {
                    alertOnDown: values.alertOnDown,
                    alertDelaySeconds: values.alertDelaySeconds,
                    alertOnHighCpu: values.alertOnHighCpu,
                    alertOnHighRam: values.alertOnHighRam,
                    alertOnHighDisk: values.alertOnHighDisk,
                },
            });
        },
        onSuccess: () => {
            toast.success('Host settings updated successfully');
            queryClient.invalidateQueries({ queryKey: ['host', host.id] });
        },
        onError: (error: Error) => {
            toast.error(`Failed to update settings: ${error.message}`);
        },
    });

    const onSubmit = (values: HostSettingsValues) => {
        updateMutation.mutate(values);
    };

    const deleteMutation = useMutation({
        mutationFn: () => hostsApi.delete(host.id),
        onSuccess: () => {
            toast.success('Host deleted successfully');
            queryClient.invalidateQueries({ queryKey: ['hosts'] });
            window.location.href = '/hosts'; // Force redirect
        },
        onError: (error: Error) => {
            toast.error(`Failed to delete host: ${error.message}`);
        },
    });

    const handleDelete = () => {
        if (confirm('Are you sure you want to delete this host? This action cannot be undone.')) {
            deleteMutation.mutate();
        }
    };

    const [copied, setCopied] = useState(false);

    const handleCopyApiKey = () => {
        navigator.clipboard.writeText(host.apiKey);
        setCopied(true);
        toast.success('API Key copied to clipboard');
        setTimeout(() => setCopied(false), 2000);
    };

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">

                <div className="space-y-4">
                    <h3 className="text-lg font-medium flex items-center gap-2">
                        <Key className="h-5 w-5" />
                        Connection Details
                    </h3>
                    <div className="rounded-lg border bg-card text-card-foreground shadow-sm p-6 space-y-4">
                        <div className="grid grid-cols-1 gap-4">
                            <div className="space-y-2">
                                <FormLabel>API Endpoint</FormLabel>
                                <div className="flex rounded-md shadow-sm ring-offset-background">
                                    <Input
                                        readOnly
                                        value={`${process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'}/api`}
                                        className="bg-muted font-mono text-sm"
                                    />
                                </div>
                                <FormDescription>
                                    Enter this URL in the Agent settings.
                                </FormDescription>
                            </div>

                            <div className="space-y-2">
                                <FormLabel>API Key</FormLabel>
                                <div className="flex space-x-2">
                                    <Input
                                        readOnly
                                        value={host.apiKey}
                                        type="password"
                                        className="bg-muted font-mono text-sm flex-1"
                                    />
                                    <Button
                                        type="button"
                                        variant="outline"
                                        size="icon"
                                        onClick={handleCopyApiKey}
                                        className="shrink-0"
                                    >
                                        {copied ? (
                                            <Check className="h-4 w-4 text-green-500" />
                                        ) : (
                                            <Copy className="h-4 w-4" />
                                        )}
                                    </Button>
                                </div>
                                <FormDescription>
                                    This key is required for the agent to authenticate. Keep it secret.
                                </FormDescription>
                            </div>
                        </div>
                    </div>
                </div>

                <Separator />

                <div className="space-y-4">
                    <h3 className="text-lg font-medium">General Settings</h3>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField
                            control={form.control}
                            name="checkIntervalSeconds"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>Check Interval (seconds)</FormLabel>
                                    <FormControl>
                                        <Input type="number" {...field} />
                                    </FormControl>
                                    <FormDescription>
                                        How often the agent reports status.
                                    </FormDescription>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />
                        <FormField
                            control={form.control}
                            name="monitoringEnabled"
                            render={({ field }) => (
                                <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                                    <div className="space-y-0.5">
                                        <FormLabel className="text-base">Monitoring Enabled</FormLabel>
                                        <FormDescription>
                                            Enable or disable monitoring for this host.
                                        </FormDescription>
                                    </div>
                                    <FormControl>
                                        <Checkbox
                                            checked={field.value}
                                            onCheckedChange={field.onChange}
                                        />
                                    </FormControl>
                                </FormItem>
                            )}
                        />
                    </div>
                </div>

                <Separator />

                <div className="space-y-4">
                    <h3 className="text-lg font-medium">Thresholds</h3>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                        {/* CPU */}
                        <div className="space-y-4">
                            <h4 className="text-sm font-semibold">CPU</h4>
                            <FormField
                                control={form.control}
                                name="cpuWarning"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Warning (%)</FormLabel>
                                        <FormControl>
                                            <Input type="number" {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <FormField
                                control={form.control}
                                name="cpuCritical"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Critical (%)</FormLabel>
                                        <FormControl>
                                            <Input type="number" {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>

                        {/* RAM */}
                        <div className="space-y-4">
                            <h4 className="text-sm font-semibold">RAM</h4>
                            <FormField
                                control={form.control}
                                name="ramWarning"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Warning (%)</FormLabel>
                                        <FormControl>
                                            <Input type="number" {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <FormField
                                control={form.control}
                                name="ramCritical"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Critical (%)</FormLabel>
                                        <FormControl>
                                            <Input type="number" {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>

                        {/* Disk */}
                        <div className="space-y-4">
                            <h4 className="text-sm font-semibold">Disk</h4>
                            <FormField
                                control={form.control}
                                name="diskWarning"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Warning (%)</FormLabel>
                                        <FormControl>
                                            <Input type="number" {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <FormField
                                control={form.control}
                                name="diskCritical"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Critical (%)</FormLabel>
                                        <FormControl>
                                            <Input type="number" {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>
                    </div>
                </div>

                <Separator />

                <div className="space-y-4">
                    <h3 className="text-lg font-medium">Alerts</h3>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField
                            control={form.control}
                            name="alertOnDown"
                            render={({ field }) => (
                                <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                                    <FormControl>
                                        <Checkbox
                                            checked={field.value}
                                            onCheckedChange={field.onChange}
                                        />
                                    </FormControl>
                                    <div className="space-y-1 leading-none">
                                        <FormLabel>Alert when Down</FormLabel>
                                    </div>
                                </FormItem>
                            )}
                        />
                        <FormField
                            control={form.control}
                            name="alertDelaySeconds"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>Alert Delay (seconds)</FormLabel>
                                    <FormControl>
                                        <Input type="number" {...field} />
                                    </FormControl>
                                    <FormDescription>
                                        Wait before sending down alert.
                                    </FormDescription>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />
                        <FormField
                            control={form.control}
                            name="alertOnHighCpu"
                            render={({ field }) => (
                                <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                                    <FormControl>
                                        <Checkbox
                                            checked={field.value}
                                            onCheckedChange={field.onChange}
                                        />
                                    </FormControl>
                                    <div className="space-y-1 leading-none">
                                        <FormLabel>Alert on High CPU</FormLabel>
                                    </div>
                                </FormItem>
                            )}
                        />
                        <FormField
                            control={form.control}
                            name="alertOnHighRam"
                            render={({ field }) => (
                                <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                                    <FormControl>
                                        <Checkbox
                                            checked={field.value}
                                            onCheckedChange={field.onChange}
                                        />
                                    </FormControl>
                                    <div className="space-y-1 leading-none">
                                        <FormLabel>Alert on High RAM</FormLabel>
                                    </div>
                                </FormItem>
                            )}
                        />
                        <FormField
                            control={form.control}
                            name="alertOnHighDisk"
                            render={({ field }) => (
                                <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                                    <FormControl>
                                        <Checkbox
                                            checked={field.value}
                                            onCheckedChange={field.onChange}
                                        />
                                    </FormControl>
                                    <div className="space-y-1 leading-none">
                                        <FormLabel>Alert on High Disk</FormLabel>
                                    </div>
                                </FormItem>
                            )}
                        />
                    </div>
                </div>

                <div className="flex justify-end">
                    <Button type="submit" disabled={updateMutation.isPending}>
                        {updateMutation.isPending ? 'Saving...' : 'Save Changes'}
                    </Button>
                </div>

                <Separator />

                <div className="space-y-4">
                    <h3 className="text-lg font-medium text-destructive">Danger Zone</h3>
                    <div className="rounded-md border border-destructive/20 bg-destructive/5 p-4">
                        <div className="flex items-center justify-between">
                            <div>
                                <h4 className="font-medium text-destructive">Delete Host</h4>
                                <p className="text-sm text-muted-foreground">
                                    Permanently remove this host and all its data. This action cannot be undone.
                                </p>
                            </div>
                            <Button
                                type="button"
                                variant="destructive"
                                onClick={handleDelete}
                                disabled={deleteMutation.isPending}
                            >
                                {deleteMutation.isPending ? 'Deleting...' : 'Delete Host'}
                            </Button>
                        </div>
                    </div>
                </div>
            </form>
        </Form>
    );
}

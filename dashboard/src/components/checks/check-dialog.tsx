'use client';

import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from '@/components/ui/dialog';
import {
    Form,
    FormControl,
    FormDescription,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from '@/components/ui/form';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { checksApi } from '@/lib/api/checks';
import { checkFormSchema, type CheckFormValues } from './check-form-schema';
import type { Check } from '@/types/check';
import { toast } from 'sonner';

interface CheckDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    check?: Check | null;
}

export function CheckDialog({ open, onOpenChange, check }: CheckDialogProps) {
    const queryClient = useQueryClient();
    const isEditing = !!check;

    const form = useForm<CheckFormValues>({
        resolver: zodResolver(checkFormSchema) as any,
        defaultValues: {
            name: '',
            target: '',
            checkType: 'HTTP',
            intervalSeconds: 60,
            timeoutSeconds: 10,
            monitoringEnabled: true,
            httpMethod: 'GET',
            checkSsl: true,
            sslExpiryWarningDays: 30,
        },
    });

    // Reset form when dialog opens/closes or check changes
    useEffect(() => {
        if (open) {
            if (check) {
                form.reset({
                    name: check.name,
                    target: check.target,
                    checkType: check.checkType,
                    intervalSeconds: check.intervalSeconds,
                    timeoutSeconds: 30, // Default or from detail
                    monitoringEnabled: check.monitoringEnabled,
                    httpMethod: 'GET', // Should come from check details
                    checkSsl: true, // Should come from check details
                    sslExpiryWarningDays: 30, // Should come from check details
                });
            } else {
                form.reset({
                    name: '',
                    target: '',
                    checkType: 'HTTP',
                    intervalSeconds: 60,
                    timeoutSeconds: 10,
                    monitoringEnabled: true,
                    httpMethod: 'GET',
                    checkSsl: true,
                    sslExpiryWarningDays: 30,
                });
            }
        }
    }, [open, check, form]);

    const createMutation = useMutation({
        mutationFn: checksApi.create,
        onSuccess: () => {
            toast.success('Check created successfully');
            queryClient.invalidateQueries({ queryKey: ['checks'] });
            onOpenChange(false);
        },
        onError: (error: Error) => {
            toast.error(`Failed to create check: ${error.message}`);
        },
    });

    const updateMutation = useMutation({
        mutationFn: (values: CheckFormValues) => checksApi.update(check!.id, values),
        onSuccess: () => {
            toast.success('Check updated successfully');
            queryClient.invalidateQueries({ queryKey: ['checks'] });
            onOpenChange(false);
        },
        onError: (error: Error) => {
            toast.error(`Failed to update check: ${error.message}`);
        },
    });

    const onSubmit = (values: CheckFormValues) => {
        const payload: any = { ...values };

        if (values.expectedStatusCodes) {
            payload.expectedStatusCodes = values.expectedStatusCodes.split(',').map(s => parseInt(s.trim())).filter(n => !isNaN(n));
        }

        if (isEditing) {
            updateMutation.mutate(payload);
        } else {
            createMutation.mutate(payload);
        }
    };

    const checkType = form.watch('checkType');

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>{isEditing ? 'Edit Check' : 'Create Check'}</DialogTitle>
                    <DialogDescription>
                        Configure the settings for your uptime check.
                    </DialogDescription>
                </DialogHeader>

                <Form {...form}>
                    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                        <div className="grid grid-cols-2 gap-4">
                            <FormField
                                control={form.control}
                                name="name"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Name</FormLabel>
                                        <FormControl>
                                            <Input placeholder="My Website" {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <FormField
                                control={form.control}
                                name="checkType"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Type</FormLabel>
                                        <Select onValueChange={field.onChange} defaultValue={field.value}>
                                            <FormControl>
                                                <SelectTrigger>
                                                    <SelectValue placeholder="Select type" />
                                                </SelectTrigger>
                                            </FormControl>
                                            <SelectContent>
                                                <SelectItem value="HTTP">HTTP(s)</SelectItem>
                                                <SelectItem value="TCP">TCP Port</SelectItem>
                                                <SelectItem value="Ping">Ping (ICMP)</SelectItem>
                                                <SelectItem value="DNS">DNS</SelectItem>
                                            </SelectContent>
                                        </Select>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>

                        <FormField
                            control={form.control}
                            name="target"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>Target (URL, IP, or Hostname)</FormLabel>
                                    <FormControl>
                                        <Input placeholder="https://example.com" {...field} />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />

                        <div className="grid grid-cols-2 gap-4">
                            <FormField
                                control={form.control}
                                name="intervalSeconds"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Interval (seconds)</FormLabel>
                                        <FormControl>
                                            <Input type="number" {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <FormField
                                control={form.control}
                                name="timeoutSeconds"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Timeout (seconds)</FormLabel>
                                        <FormControl>
                                            <Input type="number" {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>

                        {checkType === 'HTTP' && (
                            <div className="space-y-4 border rounded-md p-4 bg-muted/50">
                                <h4 className="font-medium text-sm">HTTP Settings</h4>
                                <div className="grid grid-cols-2 gap-4">
                                    <FormField
                                        control={form.control}
                                        name="httpMethod"
                                        render={({ field }) => (
                                            <FormItem>
                                                <FormLabel>Method</FormLabel>
                                                <Select onValueChange={field.onChange} defaultValue={field.value}>
                                                    <FormControl>
                                                        <SelectTrigger>
                                                            <SelectValue />
                                                        </SelectTrigger>
                                                    </FormControl>
                                                    <SelectContent>
                                                        <SelectItem value="GET">GET</SelectItem>
                                                        <SelectItem value="POST">POST</SelectItem>
                                                        <SelectItem value="HEAD">HEAD</SelectItem>
                                                    </SelectContent>
                                                </Select>
                                                <FormMessage />
                                            </FormItem>
                                        )}
                                    />
                                    <FormField
                                        control={form.control}
                                        name="expectedStatusCodes"
                                        render={({ field }) => (
                                            <FormItem>
                                                <FormLabel>Expected Status Codes</FormLabel>
                                                <FormControl>
                                                    <Input placeholder="200, 201, 301" {...field} />
                                                </FormControl>
                                                <FormDescription>Comma separated</FormDescription>
                                                <FormMessage />
                                            </FormItem>
                                        )}
                                    />
                                </div>
                                <FormField
                                    control={form.control}
                                    name="checkSsl"
                                    render={({ field }) => (
                                        <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                                            <FormControl>
                                                <Checkbox
                                                    checked={field.value}
                                                    onCheckedChange={field.onChange}
                                                />
                                            </FormControl>
                                            <div className="space-y-1 leading-none">
                                                <FormLabel>Monitor SSL Certificate</FormLabel>
                                                <FormDescription>
                                                    Alert when certificate is expiring or invalid
                                                </FormDescription>
                                            </div>
                                        </FormItem>
                                    )}
                                />
                            </div>
                        )}

                        {checkType === 'TCP' && (
                            <div className="space-y-4 border rounded-md p-4 bg-muted/50">
                                <h4 className="font-medium text-sm">TCP Settings</h4>
                                <FormField
                                    control={form.control}
                                    name="tcpPort"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Port</FormLabel>
                                            <FormControl>
                                                <Input type="number" placeholder="80" {...field} />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                            </div>
                        )}

                        <FormField
                            control={form.control}
                            name="monitoringEnabled"
                            render={({ field }) => (
                                <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                                    <FormControl>
                                        <Checkbox
                                            checked={field.value}
                                            onCheckedChange={field.onChange}
                                        />
                                    </FormControl>
                                    <div className="space-y-1 leading-none">
                                        <FormLabel>Enabled</FormLabel>
                                        <FormDescription>
                                            Start monitoring immediately after creation
                                        </FormDescription>
                                    </div>
                                </FormItem>
                            )}
                        />

                        <DialogFooter>
                            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
                                Cancel
                            </Button>
                            <Button type="submit" disabled={createMutation.isPending || updateMutation.isPending}>
                                {isEditing ? 'Update Check' : 'Create Check'}
                            </Button>
                        </DialogFooter>
                    </form>
                </Form>
            </DialogContent>
        </Dialog>
    );
}

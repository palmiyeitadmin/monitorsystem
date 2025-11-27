'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { hostsApi } from '@/lib/api/hosts';
import { useRouter } from 'next/navigation';
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
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { toast } from 'sonner';
import { OsType } from '@/types/common';

const createHostSchema = z.object({
    name: z.string().min(1, 'Name is required'),
    hostname: z.string().min(1, 'Hostname or IP is required'),
    osType: z.nativeEnum(OsType),
    description: z.string().optional(),
    checkIntervalSeconds: z.coerce.number().min(10, 'Minimum interval is 10 seconds'),
    monitoringEnabled: z.boolean(),
});

type CreateHostValues = z.infer<typeof createHostSchema>;

export function CreateHostForm() {
    const router = useRouter();
    const queryClient = useQueryClient();

    const form = useForm<CreateHostValues>({
        resolver: zodResolver(createHostSchema) as any,
        defaultValues: {
            name: '',
            hostname: '',
            osType: OsType.Linux,
            description: '',
            checkIntervalSeconds: 60,
            monitoringEnabled: true,
        },
    });

    const createMutation = useMutation({
        mutationFn: (values: CreateHostValues) => {
            return hostsApi.create(values);
        },
        onSuccess: () => {
            toast.success('Host created successfully');
            queryClient.invalidateQueries({ queryKey: ['hosts'] });
            router.push('/hosts');
        },
        onError: (error: Error) => {
            toast.error(`Failed to create host: ${error.message}`);
        },
    });

    const onSubmit = (values: CreateHostValues) => {
        createMutation.mutate(values);
    };

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8 max-w-2xl">
                <div className="space-y-4">
                    <FormField
                        control={form.control}
                        name="name"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Name</FormLabel>
                                <FormControl>
                                    <Input placeholder="My Server" {...field} />
                                </FormControl>
                                <FormDescription>
                                    A friendly name for this host.
                                </FormDescription>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="hostname"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Hostname or IP</FormLabel>
                                <FormControl>
                                    <Input placeholder="192.168.1.100" {...field} />
                                </FormControl>
                                <FormDescription>
                                    The IP address or hostname of the server.
                                </FormDescription>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="osType"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Operating System</FormLabel>
                                <Select
                                    onValueChange={(val) => field.onChange(parseInt(val))}
                                    defaultValue={field.value.toString()}
                                >
                                    <FormControl>
                                        <SelectTrigger>
                                            <SelectValue placeholder="Select OS" />
                                        </SelectTrigger>
                                    </FormControl>
                                    <SelectContent>
                                        <SelectItem value={OsType.Linux.toString()}>Linux</SelectItem>
                                        <SelectItem value={OsType.Windows.toString()}>Windows</SelectItem>
                                        <SelectItem value={OsType.macOS.toString()}>macOS</SelectItem>
                                        <SelectItem value={OsType.Other.toString()}>Other</SelectItem>
                                    </SelectContent>
                                </Select>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="description"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Description</FormLabel>
                                <FormControl>
                                    <Input placeholder="Optional description" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

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
                                    <FormMessage />
                                </FormItem>
                            )}
                        />

                        <FormField
                            control={form.control}
                            name="monitoringEnabled"
                            render={({ field }) => (
                                <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4 mt-8">
                                    <div className="space-y-0.5">
                                        <FormLabel className="text-base">Monitoring Enabled</FormLabel>
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

                <div className="flex justify-end gap-4">
                    <Button variant="outline" type="button" onClick={() => router.back()}>
                        Cancel
                    </Button>
                    <Button type="submit" disabled={createMutation.isPending}>
                        {createMutation.isPending ? 'Creating...' : 'Create Host'}
                    </Button>
                </div>
            </form>
        </Form>
    );
}

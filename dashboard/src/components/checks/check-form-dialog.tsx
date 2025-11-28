"use client";

import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Check, CreateCheckRequest } from "@/types/check";
import { checksApi } from "@/lib/api/checks";
import { toast } from "sonner";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import {
    Form,
    FormControl,
    FormDescription,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

const checkSchema = z.object({
    name: z.string().min(2, "Name must be at least 2 characters"),
    checkType: z.enum(["HTTP", "TCP", "Ping", "DNS"]),
    target: z.string().min(1, "Target is required"),
    intervalSeconds: z.coerce.number().min(10, "Minimum interval is 10 seconds"),
    timeoutSeconds: z.coerce.number().min(1, "Minimum timeout is 1 second"),
    monitoringEnabled: z.boolean(),

    // HTTP
    httpMethod: z.string().optional(),
    expectedStatusCode: z.coerce.number().optional(),
    expectedKeyword: z.string().optional(),
    keywordShouldExist: z.boolean().optional(),
    monitorSsl: z.boolean().optional(),
    sslExpiryWarningDays: z.coerce.number().optional(),

    // TCP
    tcpPort: z.coerce.number().optional(),
});

type CheckFormValues = z.infer<typeof checkSchema>;

interface CheckFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    check?: Check;
    onSuccess: () => void;
}

export function CheckFormDialog({
    open,
    onOpenChange,
    check,
    onSuccess,
}: CheckFormDialogProps) {
    const [activeTab, setActiveTab] = useState("general");

    const form = useForm<any>({
        resolver: zodResolver(checkSchema),
        defaultValues: {
            name: "",
            checkType: "HTTP",
            target: "",
            intervalSeconds: 60,
            timeoutSeconds: 30,
            monitoringEnabled: true,
            httpMethod: "GET",
            expectedStatusCode: 200,
            keywordShouldExist: true,
            monitorSsl: true,
            sslExpiryWarningDays: 14,
        },
    });

    const checkType = form.watch("checkType");

    useEffect(() => {
        if (check) {
            form.reset({
                name: check.name,
                checkType: check.checkType,
                target: check.target,
                intervalSeconds: check.intervalSeconds,
                timeoutSeconds: check.timeoutSeconds,
                monitoringEnabled: check.monitoringEnabled,
                monitorSsl: check.monitorSsl,
                httpMethod: check.httpMethod || "GET", // Ensure defaults
                expectedStatusCode: check.expectedStatusCode || 200,
                keywordShouldExist: check.keywordShouldExist ?? true,
                sslExpiryWarningDays: check.sslExpiryWarningDays || 14,
                expectedKeyword: check.expectedKeyword || "",
                tcpPort: check.tcpPort,
            });
        } else {
            form.reset({
                name: "",
                checkType: "HTTP",
                target: "",
                intervalSeconds: 60,
                timeoutSeconds: 30,
                monitoringEnabled: true,
                httpMethod: "GET",
                expectedStatusCode: 200,
                keywordShouldExist: true,
                monitorSsl: true,
                sslExpiryWarningDays: 14,
            });
        }
    }, [check, form, open]);

    const onSubmit = async (values: any) => {
        try {
            // Clean up values based on type
            const payload: any = { ...values };

            if (payload.checkType !== "HTTP") {
                delete payload.httpMethod;
                delete payload.expectedStatusCode;
                delete payload.expectedKeyword;
                delete payload.keywordShouldExist;
                delete payload.monitorSsl;
                delete payload.sslExpiryWarningDays;
            }

            if (payload.checkType !== "TCP") {
                delete payload.tcpPort;
            }

            if (check) {
                await checksApi.update(check.id, payload);
                toast.success("Check updated successfully");
            } else {
                await checksApi.create(payload as CreateCheckRequest);
                toast.success("Check created successfully");
            }
            onSuccess();
            onOpenChange(false);
        } catch (error) {
            console.error(error);
            toast.error("Failed to save check");
        }
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>{check ? "Edit Check" : "Create New Check"}</DialogTitle>
                    <DialogDescription>
                        Configure monitoring settings for your external services.
                    </DialogDescription>
                </DialogHeader>

                <Form {...form}>
                    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
                        <Tabs value={activeTab} onValueChange={setActiveTab}>
                            <TabsList className="grid w-full grid-cols-2">
                                <TabsTrigger value="general">General</TabsTrigger>
                                <TabsTrigger value="advanced">Advanced Configuration</TabsTrigger>
                            </TabsList>

                            <TabsContent value="general" className="space-y-4">
                                <FormField
                                    control={form.control}
                                    name="name"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Friendly Name</FormLabel>
                                            <FormControl>
                                                <Input placeholder="e.g. Corporate Website" {...field} />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <div className="grid grid-cols-2 gap-4">
                                    <FormField
                                        control={form.control}
                                        name="checkType"
                                        render={({ field }) => (
                                            <FormItem>
                                                <FormLabel>Check Type</FormLabel>
                                                <Select
                                                    onValueChange={field.onChange}
                                                    defaultValue={field.value}
                                                    disabled={!!check}
                                                >
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

                                    <FormField
                                        control={form.control}
                                        name="target"
                                        render={({ field }) => (
                                            <FormItem>
                                                <FormLabel>Target</FormLabel>
                                                <FormControl>
                                                    <Input
                                                        placeholder={
                                                            checkType === "HTTP" ? "https://example.com" :
                                                                checkType === "TCP" ? "db.example.com:5432" :
                                                                    "example.com"
                                                        }
                                                        {...field}
                                                    />
                                                </FormControl>
                                                <FormDescription>
                                                    {checkType === "HTTP" ? "Full URL including scheme" : "Hostname or IP"}
                                                </FormDescription>
                                                <FormMessage />
                                            </FormItem>
                                        )}
                                    />
                                </div>

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

                                <FormField
                                    control={form.control}
                                    name="monitoringEnabled"
                                    render={({ field }) => (
                                        <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                                            <div className="space-y-0.5">
                                                <FormLabel className="text-base">Enabled</FormLabel>
                                                <FormDescription>
                                                    Active monitoring for this check
                                                </FormDescription>
                                            </div>
                                            <FormControl>
                                                <Switch
                                                    checked={field.value}
                                                    onCheckedChange={field.onChange}
                                                />
                                            </FormControl>
                                        </FormItem>
                                    )}
                                />
                            </TabsContent>

                            <TabsContent value="advanced" className="space-y-4">
                                {checkType === "HTTP" && (
                                    <>
                                        <div className="grid grid-cols-2 gap-4">
                                            <FormField
                                                control={form.control}
                                                name="httpMethod"
                                                render={({ field }) => (
                                                    <FormItem>
                                                        <FormLabel>HTTP Method</FormLabel>
                                                        <Select
                                                            onValueChange={field.onChange}
                                                            defaultValue={field.value}
                                                        >
                                                            <FormControl>
                                                                <SelectTrigger>
                                                                    <SelectValue placeholder="Method" />
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
                                                name="expectedStatusCode"
                                                render={({ field }) => (
                                                    <FormItem>
                                                        <FormLabel>Expected Status</FormLabel>
                                                        <FormControl>
                                                            <Input type="number" {...field} />
                                                        </FormControl>
                                                        <FormMessage />
                                                    </FormItem>
                                                )}
                                            />
                                        </div>

                                        <FormField
                                            control={form.control}
                                            name="monitorSsl"
                                            render={({ field }) => (
                                                <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                                                    <div className="space-y-0.5">
                                                        <FormLabel className="text-base">SSL Monitoring</FormLabel>
                                                        <FormDescription>
                                                            Track certificate expiry
                                                        </FormDescription>
                                                    </div>
                                                    <FormControl>
                                                        <Switch
                                                            checked={field.value}
                                                            onCheckedChange={field.onChange}
                                                        />
                                                    </FormControl>
                                                </FormItem>
                                            )}
                                        />

                                        <FormField
                                            control={form.control}
                                            name="expectedKeyword"
                                            render={({ field }) => (
                                                <FormItem>
                                                    <FormLabel>Expected Keyword (Optional)</FormLabel>
                                                    <FormControl>
                                                        <Input placeholder="Text that must exist in response" {...field} />
                                                    </FormControl>
                                                    <FormMessage />
                                                </FormItem>
                                            )}
                                        />
                                    </>
                                )}

                                {checkType === "TCP" && (
                                    <FormField
                                        control={form.control}
                                        name="tcpPort"
                                        render={({ field }) => (
                                            <FormItem>
                                                <FormLabel>TCP Port</FormLabel>
                                                <FormControl>
                                                    <Input type="number" placeholder="80" {...field} />
                                                </FormControl>
                                                <FormDescription>
                                                    Override port if not in target
                                                </FormDescription>
                                                <FormMessage />
                                            </FormItem>
                                        )}
                                    />
                                )}
                            </TabsContent>
                        </Tabs>

                        <DialogFooter>
                            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
                                Cancel
                            </Button>
                            <Button type="submit">
                                {check ? "Save Changes" : "Create Check"}
                            </Button>
                        </DialogFooter>
                    </form>
                </Form>
            </DialogContent>
        </Dialog>
    );
}

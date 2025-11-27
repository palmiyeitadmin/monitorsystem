import { z } from 'zod';

export const checkFormSchema = z.object({
    name: z.string().min(1, 'Name is required'),
    target: z.string().min(1, 'Target is required'),
    checkType: z.enum(['HTTP', 'TCP', 'Ping', 'DNS']),
    intervalSeconds: z.coerce.number().min(10, 'Interval must be at least 10 seconds'),
    timeoutSeconds: z.coerce.number().min(1, 'Timeout must be at least 1 second'),
    monitoringEnabled: z.boolean().default(true),

    // HTTP Specific
    httpMethod: z.string().optional(),
    expectedStatusCodes: z.string().optional(), // Comma separated numbers
    expectedKeyword: z.string().optional(),
    checkSsl: z.boolean().optional(),
    sslExpiryWarningDays: z.coerce.number().optional(),

    // TCP Specific
    tcpPort: z.coerce.number().optional(),

    // DNS Specific
    dnsRecordType: z.string().optional(),
    expectedDnsResult: z.string().optional(),
});

export type CheckFormValues = z.infer<typeof checkFormSchema>;

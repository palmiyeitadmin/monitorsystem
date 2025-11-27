'use client';

import { ServiceList } from '@/components/services/service-list';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

export default function ServicesPage() {
    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-3xl font-bold tracking-tight">Services</h2>
                <p className="text-muted-foreground">
                    Manage services across all monitored hosts.
                </p>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>All Services</CardTitle>
                </CardHeader>
                <CardContent>
                    <ServiceList />
                </CardContent>
            </Card>
        </div>
    );
}

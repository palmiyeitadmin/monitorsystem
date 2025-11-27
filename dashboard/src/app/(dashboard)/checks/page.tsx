'use client';

import { CheckList } from '@/components/checks/check-list';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

export default function ChecksPage() {
    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-3xl font-bold tracking-tight">Uptime Checks</h2>
                <p className="text-muted-foreground">
                    Monitor websites, APIs, and servers from multiple locations.
                </p>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>All Checks</CardTitle>
                </CardHeader>
                <CardContent>
                    <CheckList />
                </CardContent>
            </Card>
        </div>
    );
}

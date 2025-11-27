'use client';

import { HostGrid } from '@/components/dashboard/host-grid';
import { Button } from '@/components/ui/button';
import { Plus } from 'lucide-react';
import Link from 'next/link';

export default function HostsPage() {
    return (
        <div className="space-y-8">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">Hosts</h2>
                    <p className="text-muted-foreground">
                        Manage and monitor your servers and infrastructure.
                    </p>
                </div>
                <Button asChild>
                    <Link href="/hosts/new">
                        <Plus className="mr-2 h-4 w-4" />
                        Add Host
                    </Link>
                </Button>
            </div>

            <HostGrid limit={100} />
        </div>
    );
}

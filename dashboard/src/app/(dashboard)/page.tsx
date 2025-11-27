import { StatusOverview } from '@/components/dashboard/status-overview';
import { HostGrid } from '@/components/dashboard/host-grid';
import { Separator } from '@/components/ui/separator';

export default function DashboardPage() {
    return (
        <div className="space-y-8">
            <div>
                <h2 className="text-3xl font-bold tracking-tight">Dashboard</h2>
                <p className="text-muted-foreground">
                    Overview of your infrastructure status and health.
                </p>
            </div>

            <StatusOverview />

            <Separator />

            <HostGrid limit={8} />
        </div>
    );
}

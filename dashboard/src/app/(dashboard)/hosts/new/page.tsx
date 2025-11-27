import { CreateHostForm } from '@/components/hosts/create-host-form';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

export default function NewHostPage() {
    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-3xl font-bold tracking-tight">Add New Host</h2>
                <p className="text-muted-foreground">
                    Register a new server to monitor.
                </p>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Host Details</CardTitle>
                </CardHeader>
                <CardContent>
                    <CreateHostForm />
                </CardContent>
            </Card>
        </div>
    );
}

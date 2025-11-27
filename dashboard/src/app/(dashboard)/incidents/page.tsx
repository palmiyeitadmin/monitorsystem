import { IncidentList } from '@/components/incidents/incident-list';

export default function IncidentsPage() {
    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-3xl font-bold tracking-tight">Incidents</h1>
                <p className="text-muted-foreground">
                    View and manage system incidents and alerts.
                </p>
            </div>
            <IncidentList />
        </div>
    );
}

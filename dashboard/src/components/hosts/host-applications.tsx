'use client';

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { StatusBadge } from '@/components/common/status-badge';
import { Database, Server, Container, HardDrive, Globe } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { HostService } from '@/types/host';

interface HostApplicationsProps {
    services: HostService[];
}

interface ApplicationService {
    name: string;
    displayName: string;
    icon: React.ReactNode;
    category: 'database' | 'webserver' | 'infrastructure';
    serviceNames: string[]; // Possible service names to match
    subItems?: { name: string; status: string }[];
}

const APPLICATION_DEFINITIONS: ApplicationService[] = [
    {
        name: 'SQL Server',
        displayName: 'Microsoft SQL Server',
        icon: <Database className="h-5 w-5" />,
        category: 'database',
        serviceNames: ['MSSQLSERVER', 'SQLSERVERAGENT', 'MSSQLServer', 'SQLServer'],
    },
    {
        name: 'MySQL',
        displayName: 'MySQL Database',
        icon: <Database className="h-5 w-5" />,
        category: 'database',
        serviceNames: ['MySQL', 'MySQL80', 'MySQL57', 'mysqld'],
    },
    {
        name: 'PostgreSQL',
        displayName: 'PostgreSQL Database',
        icon: <Database className="h-5 w-5" />,
        category: 'database',
        serviceNames: ['postgresql', 'PostgreSQL', 'postgres'],
    },
    {
        name: 'IIS',
        displayName: 'Internet Information Services',
        icon: <Server className="h-5 w-5" />,
        category: 'webserver',
        serviceNames: ['W3SVC', 'WAS', 'IISADMIN'],
    },
    {
        name: 'Nginx',
        displayName: 'Nginx Web Server',
        icon: <Globe className="h-5 w-5" />,
        category: 'webserver',
        serviceNames: ['nginx', 'nginx.service'],
    },
    {
        name: 'Apache',
        displayName: 'Apache HTTP Server',
        icon: <Globe className="h-5 w-5" />,
        category: 'webserver',
        serviceNames: ['httpd', 'apache2', 'Apache2.4'],
    },
    {
        name: 'Tomcat',
        displayName: 'Apache Tomcat',
        icon: <Server className="h-5 w-5" />,
        category: 'webserver',
        serviceNames: ['Tomcat', 'Tomcat9', 'Tomcat10', 'tomcat'],
    },
    {
        name: 'Docker',
        displayName: 'Docker Engine',
        icon: <Container className="h-5 w-5" />,
        category: 'infrastructure',
        serviceNames: ['Docker', 'docker', 'docker.service', 'com.docker.service'],
    },
    {
        name: 'Veeam',
        displayName: 'Veeam Backup',
        icon: <HardDrive className="h-5 w-5" />,
        category: 'infrastructure',
        serviceNames: ['VeeamBackupSvc', 'VeeamBrokerSvc', 'VeeamCatalogSvc', 'VeeamCloudSvc'],
    },
];

export function HostApplications({ services }: HostApplicationsProps) {
    const getApplicationStatus = (app: ApplicationService) => {
        const matchingServices = services.filter(s =>
            app.serviceNames.some(name =>
                s.serviceName.toLowerCase().includes(name.toLowerCase()) ||
                s.displayName?.toLowerCase().includes(name.toLowerCase())
            )
        );

        if (matchingServices.length === 0) {
            return { isActive: false, status: 'Unknown' as const, service: null };
        }

        // Get the primary service (first match)
        const primaryService = matchingServices[0];
        const isActive = primaryService.currentStatus === 'Up';

        // For IIS, collect sites and app pools
        let subItems: { name: string; status: string }[] | undefined;
        if (app.name === 'IIS') {
            subItems = services
                .filter(s => s.serviceType === 'IIS_Site' || s.serviceType === 'IIS_AppPool')
                .map(s => ({
                    name: s.displayName || s.serviceName,
                    status: s.currentStatus,
                }));
        }

        return {
            isActive,
            status: primaryService.currentStatus,
            service: primaryService,
            subItems,
        };
    };

    const categorizedApps = {
        database: APPLICATION_DEFINITIONS.filter(a => a.category === 'database'),
        webserver: APPLICATION_DEFINITIONS.filter(a => a.category === 'webserver'),
        infrastructure: APPLICATION_DEFINITIONS.filter(a => a.category === 'infrastructure'),
    };

    const renderApplicationCard = (app: ApplicationService) => {
        const { isActive, status, subItems } = getApplicationStatus(app);

        return (
            <Card
                key={app.name}
                className={cn(
                    'transition-all duration-200',
                    !isActive && 'opacity-40 grayscale'
                )}
            >
                <CardContent className="p-4">
                    <div className="flex items-start justify-between">
                        <div className="flex items-center gap-3">
                            <div
                                className={cn(
                                    'rounded-lg p-2',
                                    isActive
                                        ? 'bg-primary/10 text-primary'
                                        : 'bg-muted text-muted-foreground'
                                )}
                            >
                                {app.icon}
                            </div>
                            <div>
                                <h4 className="font-medium">{app.displayName}</h4>
                                <p className="text-sm text-muted-foreground">
                                    {isActive ? 'Installed' : 'Not Detected'}
                                </p>
                            </div>
                        </div>
                        <StatusBadge status={status} size="sm" />
                    </div>

                    {/* IIS Sub-items */}
                    {isActive && subItems && subItems.length > 0 && (
                        <div className="mt-4 space-y-2 border-t pt-3">
                            <p className="text-xs font-medium text-muted-foreground uppercase">
                                Sites & App Pools
                            </p>
                            <div className="space-y-1.5">
                                {subItems.slice(0, 5).map((item, idx) => (
                                    <div
                                        key={idx}
                                        className="flex items-center justify-between text-sm"
                                    >
                                        <span className="truncate">{item.name}</span>
                                        <StatusBadge
                                            status={item.status as any}
                                            size="sm"
                                        />
                                    </div>
                                ))}
                                {subItems.length > 5 && (
                                    <p className="text-xs text-muted-foreground">
                                        +{subItems.length - 5} more
                                    </p>
                                )}
                            </div>
                        </div>
                    )}
                </CardContent>
            </Card>
        );
    };

    return (
        <div className="space-y-6">
            {/* Databases */}
            <div>
                <h3 className="text-lg font-semibold mb-3 flex items-center gap-2">
                    <Database className="h-5 w-5 text-primary" />
                    Databases
                </h3>
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                    {categorizedApps.database.map(renderApplicationCard)}
                </div>
            </div>

            {/* Web Servers */}
            <div>
                <h3 className="text-lg font-semibold mb-3 flex items-center gap-2">
                    <Globe className="h-5 w-5 text-primary" />
                    Web Servers
                </h3>
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                    {categorizedApps.webserver.map(renderApplicationCard)}
                </div>
            </div>

            {/* Infrastructure */}
            <div>
                <h3 className="text-lg font-semibold mb-3 flex items-center gap-2">
                    <Container className="h-5 w-5 text-primary" />
                    Infrastructure
                </h3>
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                    {categorizedApps.infrastructure.map(renderApplicationCard)}
                </div>
            </div>
        </div>
    );
}

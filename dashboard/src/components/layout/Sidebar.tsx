'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { cn } from '@/lib/utils';
import {
    LayoutDashboard,
    Server,
    Activity,
    AlertTriangle,
    Users,
    Settings,
    FileText,
    Globe,
    ChevronDown,
} from 'lucide-react';
import {
    Collapsible,
    CollapsibleContent,
    CollapsibleTrigger,
} from '@/components/ui/collapsible';
import { useState } from 'react';

const navigation = [
    { name: 'Dashboard', href: '/', icon: LayoutDashboard },
    { name: 'Hosts', href: '/hosts', icon: Server },
    { name: 'Services', href: '/services', icon: Activity },
    { name: 'Checks', href: '/checks', icon: Globe },
    { name: 'Incidents', href: '/incidents', icon: AlertTriangle },
    { name: 'Customers', href: '/customers', icon: Users },
    { name: 'Reports', href: '/reports', icon: FileText },
];

const settingsNavigation = [
    { name: 'General', href: '/settings' },
    { name: 'Notifications', href: '/settings/notifications' },
    { name: 'Users', href: '/settings/users' },
    { name: 'Status Pages', href: '/status-pages' },
];

export function Sidebar() {
    const pathname = usePathname();
    const [settingsOpen, setSettingsOpen] = useState(
        pathname.startsWith('/settings') || pathname.startsWith('/status-pages')
    );

    return (
        <aside className="hidden lg:fixed lg:inset-y-0 lg:z-50 lg:flex lg:w-64 lg:flex-col">
            <div className="flex grow flex-col gap-y-5 overflow-y-auto border-r border-border bg-card px-6 pb-4">
                {/* Logo */}
                <div className="flex h-16 shrink-0 items-center">
                    <Link href="/" className="flex items-center gap-2">
                        <div className="h-8 w-8 rounded-lg bg-primary flex items-center justify-center">
                            <span className="text-primary-foreground font-bold text-lg">E</span>
                        </div>
                        <span className="font-semibold text-lg">ERA Monitor</span>
                    </Link>
                </div>

                {/* Navigation */}
                <nav className="flex flex-1 flex-col">
                    <ul role="list" className="flex flex-1 flex-col gap-y-7">
                        <li>
                            <ul role="list" className="-mx-2 space-y-1">
                                {navigation.map((item) => {
                                    const isActive = pathname === item.href ||
                                        (item.href !== '/' && pathname.startsWith(item.href));

                                    return (
                                        <li key={item.name}>
                                            <Link
                                                href={item.href}
                                                className={cn(
                                                    'group flex gap-x-3 rounded-md p-2 text-sm font-medium leading-6 transition-colors',
                                                    isActive
                                                        ? 'bg-primary text-primary-foreground'
                                                        : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
                                                )}
                                            >
                                                <item.icon className="h-5 w-5 shrink-0" />
                                                {item.name}
                                            </Link>
                                        </li>
                                    );
                                })}
                            </ul>
                        </li>

                        {/* Settings Section */}
                        <li>
                            <Collapsible open={settingsOpen} onOpenChange={setSettingsOpen}>
                                <CollapsibleTrigger className="flex w-full items-center justify-between rounded-md p-2 text-sm font-medium text-muted-foreground hover:bg-accent hover:text-accent-foreground transition-colors">
                                    <span className="flex items-center gap-x-3">
                                        <Settings className="h-5 w-5" />
                                        Settings
                                    </span>
                                    <ChevronDown className={cn(
                                        "h-4 w-4 transition-transform",
                                        settingsOpen && "rotate-180"
                                    )} />
                                </CollapsibleTrigger>
                                <CollapsibleContent className="mt-1 space-y-1">
                                    {settingsNavigation.map((item) => {
                                        const isActive = pathname === item.href;

                                        return (
                                            <Link
                                                key={item.name}
                                                href={item.href}
                                                className={cn(
                                                    'block rounded-md py-2 pl-11 pr-2 text-sm transition-colors',
                                                    isActive
                                                        ? 'bg-accent text-accent-foreground font-medium'
                                                        : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
                                                )}
                                            >
                                                {item.name}
                                            </Link>
                                        );
                                    })}
                                </CollapsibleContent>
                            </Collapsible>
                        </li>
                    </ul>
                </nav>
            </div>
        </aside>
    );
}

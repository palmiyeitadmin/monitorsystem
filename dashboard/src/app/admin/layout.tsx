'use client';

import { usePathname } from 'next/navigation';
import Link from 'next/link';
import { useAuth } from '@/lib/auth-context';
import { useState } from 'react';

export default function AdminLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    const pathname = usePathname();
    const { logout } = useAuth();
    const [sidebarOpen, setSidebarOpen] = useState(true);

    const navItems = [
        { href: '/admin', icon: 'dashboard', label: 'Dashboard' },
        { href: '/admin/hosts', icon: 'dns', label: 'Hosts' },
        { href: '/admin/services', icon: 'lan', label: 'Services' },
        { href: '/admin/checks', icon: 'task_alt', label: 'Checks' },
        { href: '/admin/incidents', icon: 'error', label: 'Incidents' },
        { href: '/admin/customers', icon: 'group', label: 'Customers' },
        { href: '/admin/users', icon: 'person', label: 'Users' },
        { href: '/admin/locations', icon: 'location_on', label: 'Locations' },
        { href: '/admin/reports', icon: 'assessment', label: 'Reports' },
    ];

    const isActive = (href: string) => {
        if (href === '/admin') {
            return pathname === '/admin';
        }
        return pathname.includes(href);
    };

    return (
        <div className="relative flex min-h-screen w-full bg-background-dark dark:bg-background-dark">
            {/* Sidebar - Hidden on mobile/tablet, visible on desktop */}
            <aside className={`hidden lg:flex flex-col bg-surface dark:bg-surface-dark border-r border-border-light dark:border-border-dark transition-all duration-300 z-50 ${sidebarOpen ? 'w-64' : 'w-20'}`}>
                <div className="flex flex-col h-full">
                    {/* Logo Section */}
                    <div className="flex items-center justify-between h-16 px-6 border-b border-border-light dark:border-border-dark">
                        <div className={`flex items-center gap-3 ${!sidebarOpen && 'justify-center'}`}>
                            <div className="size-8 text-primary flex-shrink-0">
                                <svg viewBox="0 0 48 48" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
                                    <path d="M44 4H30.6666V17.3334H17.3334V30.6666H4V44H44V4Z" />
                                </svg>
                            </div>
                            {sidebarOpen && (
                                <div className="flex flex-col">
                                    <h1 className="text-white text-base font-medium">ERA Monitor</h1>
                                    <p className="text-[#95b7c6] text-sm">Admin Panel</p>
                                </div>
                            )}
                        </div>
                        <button
                            onClick={() => setSidebarOpen(!sidebarOpen)}
                            className="p-2 rounded-lg hover:bg-white/10 transition-colors"
                        >
                            <span className="material-symbols-outlined">{sidebarOpen ? 'menu_open' : 'menu'}</span>
                        </button>
                    </div>

                    {/* Navigation */}
                    <nav className="flex-1 p-4 space-y-2">
                        {navItems.map((item) => (
                            <Link
                                key={item.href}
                                href={item.href}
                                className={`nav-item flex items-center gap-3 px-4 py-2.5 rounded-lg transition-all duration-200 ${isActive(item.href)
                                    ? 'bg-primary/20 text-primary shadow-sm'
                                    : 'text-gray-300 hover:bg-white/10 hover:text-white'
                                    }`}
                                title={!sidebarOpen ? item.label : ''}
                            >
                                <span className={`material-symbols-outlined ${isActive(item.href) ? 'fill' : ''} flex-shrink-0`}>
                                    {item.icon}
                                </span>
                                {sidebarOpen && <span className="nav-text truncate">{item.label}</span>}
                            </Link>
                        ))}
                    </nav>

                    {/* User Section */}
                    <div className="p-4 border-t border-border-light dark:border-border-dark">
                        {sidebarOpen ? (
                            <div className="space-y-2">
                                <div className="flex items-center gap-3 mb-4">
                                    <div className="size-10 rounded-full border-2 border-primary/30 flex items-center justify-center bg-surface-light dark:bg-surface-dark text-primary">
                                        <span className="material-symbols-outlined">person</span>
                                    </div>
                                    <div className="flex flex-col">
                                        <p className="text-sm font-medium text-white">Admin User</p>
                                        <p className="text-xs text-gray-400">admin@eramonitor.com</p>
                                    </div>
                                </div>
                                <Link
                                    href="/settings"
                                    className="flex items-center gap-3 px-4 py-2.5 rounded-lg text-gray-300 hover:bg-white/10 hover:text-white transition-colors"
                                >
                                    <span className="material-symbols-outlined">settings</span>
                                    <span className="nav-text">Settings</span>
                                </Link>
                                <button
                                    onClick={logout}
                                    className="flex items-center gap-3 px-4 py-2.5 rounded-lg text-gray-300 hover:bg-white/10 hover:text-white transition-colors w-full text-left"
                                >
                                    <span className="material-symbols-outlined">logout</span>
                                    <span className="nav-text">Logout</span>
                                </button>
                            </div>
                        ) : (
                            <div className="space-y-2">
                                <Link
                                    href="/settings"
                                    className="flex items-center justify-center p-3 rounded-lg text-gray-300 hover:bg-white/10 hover:text-white transition-colors"
                                    title="Settings"
                                >
                                    <span className="material-symbols-outlined">settings</span>
                                </Link>
                                <button
                                    onClick={logout}
                                    className="flex items-center justify-center p-3 rounded-lg text-gray-300 hover:bg-white/10 hover:text-white transition-colors w-full"
                                    title="Logout"
                                >
                                    <span className="material-symbols-outlined">logout</span>
                                </button>
                            </div>
                        )}
                    </div>
                </div>
            </aside>

            {/* Main Content */}
            <div className="flex-1 flex flex-col pb-24 lg:pb-0">
                {/* Header */}
                <header className="flex items-center justify-between px-4 sm:px-6 py-3 border-b border-border-light dark:border-border-dark bg-surface dark:bg-surface-dark sticky top-0 z-40">
                    <div className="flex items-center gap-4 lg:hidden">
                        <div className="size-6 text-primary">
                            <svg viewBox="0 0 48 48" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
                                <path d="M44 4H30.6666V17.3334H17.3334V30.6666H4V44H44V4Z" />
                            </svg>
                        </div>
                        <h2 className="text-white text-lg font-bold">ERA Monitor</h2>
                    </div>
                    <div className="hidden lg:block"></div>
                    <div className="flex items-center gap-2 sm:gap-4">
                        <button className="hidden sm:flex items-center justify-center gap-2 h-10 px-4 rounded-lg bg-primary text-background-dark text-sm font-bold hover:bg-primary/90 transition-all duration-200 hover:scale-105">
                            <span className="material-symbols-outlined">add</span>
                            <span>Add New Monitor</span>
                        </button>
                        <button className="relative p-2 rounded-lg hover:bg-white/10 transition-colors">
                            <span className="material-symbols-outlined">notifications</span>
                            <span className="absolute top-1 right-1 size-2 bg-danger rounded-full animate-pulse"></span>
                        </button>
                        <div className="size-10 rounded-full border-2 border-primary/30 flex items-center justify-center bg-surface-light dark:bg-surface-dark text-primary">
                            <span className="material-symbols-outlined">person</span>
                        </div>
                    </div>
                </header>

                {/* Main Content Area */}
                <main className="flex-1 overflow-y-auto p-4 sm:p-6 bg-background-light dark:bg-background-dark">
                    {children}
                </main>
            </div>

            {/* Mobile Bottom Navigation */}
            <nav className="lg:hidden fixed bottom-0 left-0 right-0 bg-surface dark:bg-surface-dark border-t border-border-light dark:border-border-dark flex justify-around items-center py-2 z-50">
                <Link href="/" className={`flex flex-col items-center justify-center w-1/5 ${isActive('/') ? 'text-primary' : 'text-gray-400 hover:text-primary'}`}>
                    <span className={`material-symbols-outlined text-2xl ${isActive('/') ? 'fill' : ''}`}>dashboard</span>
                    <span className="text-xs mt-1">Dashboard</span>
                </Link>
                <Link href="/hosts" className={`flex flex-col items-center justify-center w-1/5 ${isActive('/hosts') ? 'text-primary' : 'text-gray-400 hover:text-primary'}`}>
                    <span className={`material-symbols-outlined text-2xl ${isActive('/hosts') ? 'fill' : ''}`}>dns</span>
                    <span className="text-xs mt-1">Hosts</span>
                </Link>
                <Link href="/incidents" className={`flex flex-col items-center justify-center w-1/5 ${isActive('/incidents') ? 'text-primary' : 'text-gray-400 hover:text-primary'}`}>
                    <span className={`material-symbols-outlined text-2xl ${isActive('/incidents') ? 'fill' : ''}`}>warning</span>
                    <span className="text-xs mt-1">Incidents</span>
                </Link>
                <Link href="/checks" className={`flex flex-col items-center justify-center w-1/5 ${isActive('/checks') ? 'text-primary' : 'text-gray-400 hover:text-primary'}`}>
                    <span className={`material-symbols-outlined text-2xl ${isActive('/checks') ? 'fill' : ''}`}>checklist</span>
                    <span className="text-xs mt-1">Checks</span>
                </Link>
                <Link href="/settings" className={`flex flex-col items-center justify-center w-1/5 ${isActive('/settings') ? 'text-primary' : 'text-gray-400 hover:text-primary'}`}>
                    <span className={`material-symbols-outlined text-2xl ${isActive('/settings') ? 'fill' : ''}`}>menu</span>
                    <span className="text-xs mt-1">Menu</span>
                </Link>
            </nav>

            {/* Mobile FAB */}
            <button className="lg:hidden fixed bottom-20 right-4 bg-primary text-white rounded-full size-14 flex items-center justify-center shadow-lg hover:opacity-90 transition-opacity z-50">
                <span className="material-symbols-outlined text-3xl">add</span>
            </button>
        </div>
    );
}

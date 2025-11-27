'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import type { Customer } from '@/lib/api';

interface CustomerData {
    id: string;
    name: string;
    domain: string;
    contactEmail: string;
    contactPhone: string;
    status: 'active' | 'inactive' | 'trial';
    plan: 'basic' | 'pro' | 'enterprise';
    users: number;
    services: number;
    createdAt: Date;
    lastActive: Date;
}

export default function CustomersPage() {
    const [customers, setCustomers] = useState<CustomerData[]>([
        {
            id: '1',
            name: 'Acme Corporation',
            domain: 'acme.com',
            contactEmail: 'john.doe@acme.com',
            contactPhone: '+1 (555) 123-4567',
            status: 'active',
            plan: 'enterprise',
            users: 245,
            services: 18,
            createdAt: new Date('2023-01-15'),
            lastActive: new Date(Date.now() - 2 * 60 * 60 * 1000),
        },
        {
            id: '2',
            name: 'Tech Solutions Inc',
            domain: 'techsolutions.io',
            contactEmail: 'sarah@techsolutions.io',
            contactPhone: '+1 (555) 987-6543',
            status: 'active',
            plan: 'pro',
            users: 52,
            services: 7,
            createdAt: new Date('2023-03-22'),
            lastActive: new Date(Date.now() - 30 * 60 * 1000),
        },
        {
            id: '3',
            name: 'Digital Agency',
            domain: 'digitalagency.co',
            contactEmail: 'contact@digitalagency.co',
            contactPhone: '+1 (555) 456-7890',
            status: 'trial',
            plan: 'basic',
            users: 8,
            services: 3,
            createdAt: new Date('2023-11-10'),
            lastActive: new Date(Date.now() - 24 * 60 * 60 * 1000),
        },
        {
            id: '4',
            name: 'StartupXYZ',
            domain: 'startupxyz.app',
            contactEmail: 'founder@startupxyz.app',
            contactPhone: '+1 (555) 234-5678',
            status: 'inactive',
            plan: 'basic',
            users: 3,
            services: 2,
            createdAt: new Date('2023-05-08'),
            lastActive: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000),
        },
        {
            id: '5',
            name: 'Global Enterprises',
            domain: 'global.enterprises',
            contactEmail: 'admin@global.enterprises',
            contactPhone: '+1 (555) 345-6789',
            status: 'active',
            plan: 'enterprise',
            users: 1240,
            services: 45,
            createdAt: new Date('2022-08-15'),
            lastActive: new Date(Date.now() - 5 * 60 * 1000),
        },
    ]);
    const [loading, setLoading] = useState(false);
    const [searchTerm, setSearchTerm] = useState('');
    const [statusFilter, setStatusFilter] = useState('all');
    const [planFilter, setPlanFilter] = useState('all');
    const [selectedCustomer, setSelectedCustomer] = useState<CustomerData | null>(null);
    const [showModal, setShowModal] = useState(false);

    const getStatusColor = (status: string) => {
        switch (status?.toLowerCase()) {
            case 'active':
                return 'bg-green-500/20 text-green-500 border-green-500/30';
            case 'trial':
                return 'bg-blue-500/20 text-blue-500 border-blue-500/30';
            case 'inactive':
                return 'bg-gray-500/20 text-gray-500 border-gray-500/30';
            default:
                return 'bg-gray-500/20 text-gray-500 border-gray-500/30';
        }
    };

    const getPlanColor = (plan: string) => {
        switch (plan?.toLowerCase()) {
            case 'enterprise':
                return 'bg-purple-500/20 text-purple-500 border-purple-500/30';
            case 'pro':
                return 'bg-orange-500/20 text-orange-500 border-orange-500/30';
            case 'basic':
                return 'bg-blue-500/20 text-blue-500 border-blue-500/30';
            default:
                return 'bg-gray-500/20 text-gray-500 border-gray-500/30';
        }
    };

    const getRelativeTime = (date: Date) => {
        const now = new Date();
        const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);

        if (diffInSeconds < 60) {
            return 'just now';
        } else if (diffInSeconds < 3600) {
            const minutes = Math.floor(diffInSeconds / 60);
            return `${minutes}m ago`;
        } else if (diffInSeconds < 86400) {
            const hours = Math.floor(diffInSeconds / 3600);
            return `${hours}h ago`;
        } else {
            const days = Math.floor(diffInSeconds / 86400);
            return `${days}d ago`;
        }
    };

    const filteredCustomers = customers.filter(customer => {
        const matchesSearch = customer.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
            customer.domain.toLowerCase().includes(searchTerm.toLowerCase()) ||
            customer.contactEmail.toLowerCase().includes(searchTerm.toLowerCase());

        const matchesStatus = statusFilter === 'all' || customer.status === statusFilter;
        const matchesPlan = planFilter === 'all' || customer.plan === planFilter;

        return matchesSearch && matchesStatus && matchesPlan;
    });

    const CustomerModal = ({ customer, onClose }: { customer: CustomerData | null; onClose: () => void }) => {
        if (!customer) return null;

        return (
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
                <div className="bg-[#1b2b32] rounded-lg p-6 w-full max-w-md border border-[#365663]">
                    <div className="flex justify-between items-center mb-4">
                        <h2 className="text-white text-xl font-bold">{customer.name}</h2>
                        <button onClick={onClose} className="text-gray-400 hover:text-white">
                            <span className="material-symbols-outlined">close</span>
                        </button>
                    </div>

                    <div className="space-y-4">
                        <div>
                            <label className="text-gray-400 text-sm">Domain</label>
                            <p className="text-white">{customer.domain}</p>
                        </div>

                        <div>
                            <label className="text-gray-400 text-sm">Contact Email</label>
                            <p className="text-white">{customer.contactEmail}</p>
                        </div>

                        <div>
                            <label className="text-gray-400 text-sm">Contact Phone</label>
                            <p className="text-white">{customer.contactPhone}</p>
                        </div>

                        <div className="grid grid-cols-2 gap-4">
                            <div>
                                <label className="text-gray-400 text-sm">Status</label>
                                <div className="mt-1">
                                    <span className={`inline-flex items-center gap-1.5 px-2 py-1 rounded-full text-xs font-medium border ${getStatusColor(customer.status)}`}>
                                        {customer.status}
                                    </span>
                                </div>
                            </div>

                            <div>
                                <label className="text-gray-400 text-sm">Plan</label>
                                <div className="mt-1">
                                    <span className={`inline-flex items-center gap-1.5 px-2 py-1 rounded-full text-xs font-medium border ${getPlanColor(customer.plan)}`}>
                                        {customer.plan}
                                    </span>
                                </div>
                            </div>
                        </div>

                        <div className="grid grid-cols-2 gap-4">
                            <div>
                                <label className="text-gray-400 text-sm">Users</label>
                                <p className="text-white text-lg font-medium">{customer.users}</p>
                            </div>

                            <div>
                                <label className="text-gray-400 text-sm">Services</label>
                                <p className="text-white text-lg font-medium">{customer.services}</p>
                            </div>
                        </div>

                        <div>
                            <label className="text-gray-400 text-sm">Created</label>
                            <p className="text-white">{customer.createdAt.toLocaleDateString()}</p>
                        </div>

                        <div>
                            <label className="text-gray-400 text-sm">Last Active</label>
                            <p className="text-white">{getRelativeTime(customer.lastActive)}</p>
                        </div>
                    </div>

                    <div className="flex gap-3 mt-6">
                        <button className="flex-1 px-4 py-2 bg-[#28aae2] hover:bg-[#2196d4] text-[#121d21] text-sm font-bold rounded-lg transition-colors">
                            Edit Customer
                        </button>
                        <button
                            onClick={onClose}
                            className="flex-1 px-4 py-2 bg-[#365663] hover:bg-[#445566] text-white text-sm font-medium rounded-lg transition-colors"
                        >
                            Close
                        </button>
                    </div>
                </div>
            </div>
        );
    };

    return (
        <>
            {/* Page Header */}
            <div className="flex flex-wrap justify-between items-center gap-4 mb-6">
                <h1 className="text-white text-4xl font-black tracking-[-0.033em]">Customers Management</h1>
                <button className="flex items-center gap-2 h-10 px-4 rounded-lg bg-[#28aae2] hover:bg-[#2196d4] text-[#121d21] text-sm font-bold transition-colors">
                    <span className="material-symbols-outlined fill">add</span>
                    Add Customer
                </button>
            </div>

            {/* Filters */}
            <div className="flex flex-wrap gap-4 mb-6">
                <div className="flex-1 min-w-[200px]">
                    <div className="relative">
                        <span className="material-symbols-outlined absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 text-sm">
                            search
                        </span>
                        <input
                            type="text"
                            placeholder="Search customers..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2 bg-[#1b2b32] border border-[#365663] rounded-lg text-white placeholder-gray-400 focus:outline-none focus:border-[#28aae2] transition-colors"
                        />
                    </div>
                </div>

                <select
                    value={statusFilter}
                    onChange={(e) => setStatusFilter(e.target.value)}
                    className="px-4 py-2 bg-[#1b2b32] border border-[#365663] rounded-lg text-white focus:outline-none focus:border-[#28aae2] transition-colors"
                >
                    <option value="all">All Status</option>
                    <option value="active">Active</option>
                    <option value="trial">Trial</option>
                    <option value="inactive">Inactive</option>
                </select>

                <select
                    value={planFilter}
                    onChange={(e) => setPlanFilter(e.target.value)}
                    className="px-4 py-2 bg-[#1b2b32] border border-[#365663] rounded-lg text-white focus:outline-none focus:border-[#28aae2] transition-colors"
                >
                    <option value="all">All Plans</option>
                    <option value="basic">Basic</option>
                    <option value="pro">Pro</option>
                    <option value="enterprise">Enterprise</option>
                </select>
            </div>

            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
                <div className="bg-[#1b2b32] rounded-lg p-4 border border-[#365663]">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-gray-400 text-sm">Total Customers</p>
                            <p className="text-white text-2xl font-bold">{customers.length}</p>
                        </div>
                        <div className="w-12 h-12 bg-[#28aae2]/20 rounded-lg flex items-center justify-center">
                            <span className="material-symbols-outlined text-[#28aae2]">groups</span>
                        </div>
                    </div>
                </div>

                <div className="bg-[#1b2b32] rounded-lg p-4 border border-[#365663]">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-gray-400 text-sm">Active</p>
                            <p className="text-white text-2xl font-bold">{customers.filter(c => c.status === 'active').length}</p>
                        </div>
                        <div className="w-12 h-12 bg-green-500/20 rounded-lg flex items-center justify-center">
                            <span className="material-symbols-outlined text-green-500">check_circle</span>
                        </div>
                    </div>
                </div>

                <div className="bg-[#1b2b32] rounded-lg p-4 border border-[#365663]">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-gray-400 text-sm">Total Users</p>
                            <p className="text-white text-2xl font-bold">{customers.reduce((sum, c) => sum + c.users, 0)}</p>
                        </div>
                        <div className="w-12 h-12 bg-purple-500/20 rounded-lg flex items-center justify-center">
                            <span className="material-symbols-outlined text-purple-500">person</span>
                        </div>
                    </div>
                </div>

                <div className="bg-[#1b2b32] rounded-lg p-4 border border-[#365663]">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-gray-400 text-sm">Total Services</p>
                            <p className="text-white text-2xl font-bold">{customers.reduce((sum, c) => sum + c.services, 0)}</p>
                        </div>
                        <div className="w-12 h-12 bg-orange-500/20 rounded-lg flex items-center justify-center">
                            <span className="material-symbols-outlined text-orange-500">dns</span>
                        </div>
                    </div>
                </div>
            </div>

            {/* Customers Table */}
            <div className="overflow-hidden rounded-lg border border-[#365663] bg-[#121d21]">
                <table className="w-full text-left">
                    <thead className="bg-[#1b2b32]">
                        <tr>
                            <th className="px-4 py-3 text-white text-sm font-medium">Customer</th>
                            <th className="px-4 py-3 text-white text-sm font-medium">Status</th>
                            <th className="px-4 py-3 text-white text-sm font-medium">Plan</th>
                            <th className="px-4 py-3 text-white text-sm font-medium">Users</th>
                            <th className="px-4 py-3 text-white text-sm font-medium">Services</th>
                            <th className="px-4 py-3 text-white text-sm font-medium">Last Active</th>
                            <th className="px-4 py-3 text-white text-sm font-medium">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {loading ? (
                            <tr>
                                <td colSpan={7} className="px-4 py-8 text-center text-gray-400">Loading...</td>
                            </tr>
                        ) : filteredCustomers.length === 0 ? (
                            <tr>
                                <td colSpan={7} className="px-4 py-8 text-center text-gray-400">No customers found</td>
                            </tr>
                        ) : (
                            filteredCustomers.map((customer) => (
                                <tr key={customer.id} className="border-t border-[#365663] hover:bg-[#1b2b32]/50 transition-colors">
                                    <td className="px-4 py-3">
                                        <div>
                                            <p className="text-white font-medium">{customer.name}</p>
                                            <p className="text-[#95b7c6] text-sm">{customer.domain}</p>
                                        </div>
                                    </td>
                                    <td className="px-4 py-3">
                                        <span className={`inline-flex items-center gap-1.5 px-2 py-1 rounded-full text-xs font-medium border ${getStatusColor(customer.status)}`}>
                                            <span className="w-1.5 h-1.5 rounded-full bg-current"></span>
                                            {customer.status}
                                        </span>
                                    </td>
                                    <td className="px-4 py-3">
                                        <span className={`inline-flex items-center gap-1.5 px-2 py-1 rounded-full text-xs font-medium border ${getPlanColor(customer.plan)}`}>
                                            {customer.plan}
                                        </span>
                                    </td>
                                    <td className="px-4 py-3 text-white text-sm">{customer.users}</td>
                                    <td className="px-4 py-3 text-white text-sm">{customer.services}</td>
                                    <td className="px-4 py-3 text-[#95b7c6] text-sm">{getRelativeTime(customer.lastActive)}</td>
                                    <td className="px-4 py-3">
                                        <button
                                            onClick={() => {
                                                setSelectedCustomer(customer);
                                                setShowModal(true);
                                            }}
                                            className="p-2 hover:bg-[#253c46] rounded-full text-[#95b7c6] transition-colors"
                                        >
                                            <span className="material-symbols-outlined">more_vert</span>
                                        </button>
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            {/* Customer Modal */}
            {showModal && (
                <CustomerModal
                    customer={selectedCustomer}
                    onClose={() => {
                        setShowModal(false);
                        setSelectedCustomer(null);
                    }}
                />
            )}
        </>
    );
}

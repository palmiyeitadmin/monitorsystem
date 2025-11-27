'use client';

import { CustomerList } from '@/components/customers/customer-list';
import { Button } from '@/components/ui/button';
import { Plus } from 'lucide-react';

export default function CustomersPage() {
    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">Customers</h2>
                    <p className="text-muted-foreground">
                        Manage your customers and their contact information.
                    </p>
                </div>
                <Button>
                    <Plus className="mr-2 h-4 w-4" />
                    Add Customer
                </Button>
            </div>

            <CustomerList />
        </div>
    );
}

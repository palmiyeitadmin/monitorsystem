export interface Customer {
    id: string;
    name: string;
    email?: string;
    phone?: string;
    website?: string;
    address?: string;
    city?: string;
    country?: string;
    isActive: boolean;
    createdAt: string;
}

export interface CustomerDetail extends Customer {
    notes?: string;
    hostCount: number;
    checkCount: number;
    incidentCount: number;
    contacts: CustomerContact[];
}

export interface CustomerContact {
    id: string;
    fullName: string;
    email: string;
    phone?: string;
    role: string;
    isPrimary: boolean;
    notifyOnAlerts: boolean;
}

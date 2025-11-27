PHASE 8: Next.js Dashboard Frontend (Days 41-52)
8.1 Overview
Phase 8 focuses on implementing the Next.js 14 dashboard frontend:

Modern UI with Tailwind CSS and shadcn/ui
Real-time updates with SignalR
Interactive charts with Recharts
Responsive design (mobile-first)
Dark/Light theme support
Role-based navigation
TypeScript throughout


8.2 Project Structure
era-dashboard/
├── src/
│   ├── app/
│   │   ├── (auth)/
│   │   │   ├── login/
│   │   │   │   └── page.tsx
│   │   │   ├── register/
│   │   │   │   └── page.tsx
│   │   │   └── layout.tsx
│   │   ├── (dashboard)/
│   │   │   ├── layout.tsx
│   │   │   ├── page.tsx                    # Dashboard home
│   │   │   ├── hosts/
│   │   │   │   ├── page.tsx                # Host list
│   │   │   │   ├── [id]/
│   │   │   │   │   └── page.tsx            # Host detail
│   │   │   │   └── new/
│   │   │   │       └── page.tsx            # Create host
│   │   │   ├── services/
│   │   │   │   ├── page.tsx
│   │   │   │   └── [id]/
│   │   │   │       └── page.tsx
│   │   │   ├── checks/
│   │   │   │   ├── page.tsx
│   │   │   │   ├── [id]/
│   │   │   │   │   └── page.tsx
│   │   │   │   └── new/
│   │   │   │       └── page.tsx
│   │   │   ├── incidents/
│   │   │   │   ├── page.tsx
│   │   │   │   ├── [id]/
│   │   │   │   │   └── page.tsx
│   │   │   │   └── new/
│   │   │   │       └── page.tsx
│   │   │   ├── customers/
│   │   │   │   ├── page.tsx
│   │   │   │   └── [id]/
│   │   │   │       └── page.tsx
│   │   │   ├── reports/
│   │   │   │   ├── page.tsx
│   │   │   │   └── [id]/
│   │   │   │       └── page.tsx
│   │   │   ├── settings/
│   │   │   │   ├── page.tsx
│   │   │   │   ├── notifications/
│   │   │   │   │   └── page.tsx
│   │   │   │   ├── users/
│   │   │   │   │   └── page.tsx
│   │   │   │   └── profile/
│   │   │   │       └── page.tsx
│   │   │   └── status-pages/
│   │   │       ├── page.tsx
│   │   │       └── [id]/
│   │   │           └── page.tsx
│   │   ├── status/
│   │   │   └── [slug]/
│   │   │       └── page.tsx                # Public status page
│   │   ├── layout.tsx
│   │   ├── globals.css
│   │   └── providers.tsx
│   ├── components/
│   │   ├── ui/                             # shadcn/ui components
│   │   ├── layout/
│   │   │   ├── sidebar.tsx
│   │   │   ├── header.tsx
│   │   │   ├── mobile-nav.tsx
│   │   │   └── user-menu.tsx
│   │   ├── dashboard/
│   │   │   ├── status-overview.tsx
│   │   │   ├── host-grid.tsx
│   │   │   ├── incident-list.tsx
│   │   │   └── quick-stats.tsx
│   │   ├── hosts/
│   │   │   ├── host-card.tsx
│   │   │   ├── host-table.tsx
│   │   │   ├── host-detail.tsx
│   │   │   ├── host-metrics.tsx
│   │   │   └── host-form.tsx
│   │   ├── checks/
│   │   │   ├── check-card.tsx
│   │   │   ├── check-table.tsx
│   │   │   ├── check-form.tsx
│   │   │   └── check-history.tsx
│   │   ├── incidents/
│   │   │   ├── incident-card.tsx
│   │   │   ├── incident-table.tsx
│   │   │   ├── incident-detail.tsx
│   │   │   ├── incident-timeline.tsx
│   │   │   └── incident-form.tsx
│   │   ├── charts/
│   │   │   ├── cpu-chart.tsx
│   │   │   ├── memory-chart.tsx
│   │   │   ├── uptime-chart.tsx
│   │   │   ├── response-time-chart.tsx
│   │   │   └── heatmap.tsx
│   │   └── common/
│   │       ├── status-badge.tsx
│   │       ├── severity-badge.tsx
│   │       ├── loading.tsx
│   │       ├── error-boundary.tsx
│   │       ├── data-table.tsx
│   │       ├── pagination.tsx
│   │       ├── search-input.tsx
│   │       ├── date-range-picker.tsx
│   │       └── confirm-dialog.tsx
│   ├── hooks/
│   │   ├── use-auth.ts
│   │   ├── use-signalr.ts
│   │   ├── use-hosts.ts
│   │   ├── use-checks.ts
│   │   ├── use-incidents.ts
│   │   └── use-debounce.ts
│   ├── lib/
│   │   ├── api/
│   │   │   ├── client.ts
│   │   │   ├── auth.ts
│   │   │   ├── hosts.ts
│   │   │   ├── checks.ts
│   │   │   ├── incidents.ts
│   │   │   ├── customers.ts
│   │   │   ├── notifications.ts
│   │   │   └── reports.ts
│   │   ├── signalr.ts
│   │   ├── utils.ts
│   │   └── validations.ts
│   ├── types/
│   │   ├── api.ts
│   │   ├── host.ts
│   │   ├── check.ts
│   │   ├── incident.ts
│   │   ├── customer.ts
│   │   └── notification.ts
│   └── stores/
│       ├── auth-store.ts
│       ├── dashboard-store.ts
│       └── notification-store.ts
├── public/
│   ├── logo.svg
│   └── favicon.ico
├── tailwind.config.ts
├── next.config.js
├── tsconfig.json
└── package.json

8.3 Configuration Files
package.json
json{
  "name": "era-dashboard",
  "version": "1.0.0",
  "private": true,
  "scripts": {
    "dev": "next dev",
    "build": "next build",
    "start": "next start",
    "lint": "next lint",
    "type-check": "tsc --noEmit"
  },
  "dependencies": {
    "next": "14.2.0",
    "react": "18.3.0",
    "react-dom": "18.3.0",
    "@microsoft/signalr": "^8.0.0",
    "@tanstack/react-query": "^5.28.0",
    "@tanstack/react-table": "^8.15.0",
    "zustand": "^4.5.2",
    "recharts": "^2.12.0",
    "date-fns": "^3.6.0",
    "react-hook-form": "^7.51.0",
    "@hookform/resolvers": "^3.3.4",
    "zod": "^3.22.4",
    "clsx": "^2.1.0",
    "tailwind-merge": "^2.2.2",
    "class-variance-authority": "^0.7.0",
    "lucide-react": "^0.359.0",
    "@radix-ui/react-alert-dialog": "^1.0.5",
    "@radix-ui/react-avatar": "^1.0.4",
    "@radix-ui/react-checkbox": "^1.0.4",
    "@radix-ui/react-dialog": "^1.0.5",
    "@radix-ui/react-dropdown-menu": "^2.0.6",
    "@radix-ui/react-label": "^2.0.2",
    "@radix-ui/react-popover": "^1.0.7",
    "@radix-ui/react-select": "^2.0.0",
    "@radix-ui/react-separator": "^1.0.3",
    "@radix-ui/react-slot": "^1.0.2",
    "@radix-ui/react-switch": "^1.0.3",
    "@radix-ui/react-tabs": "^1.0.4",
    "@radix-ui/react-toast": "^1.1.5",
    "@radix-ui/react-tooltip": "^1.0.7",
    "next-themes": "^0.3.0",
    "sonner": "^1.4.0"
  },
  "devDependencies": {
    "@types/node": "^20.11.0",
    "@types/react": "^18.2.0",
    "@types/react-dom": "^18.2.0",
    "typescript": "^5.4.0",
    "tailwindcss": "^3.4.0",
    "autoprefixer": "^10.4.0",
    "postcss": "^8.4.0",
    "eslint": "^8.57.0",
    "eslint-config-next": "14.2.0"
  }
}
tailwind.config.ts
typescriptimport type { Config } from 'tailwindcss';

const config: Config = {
  darkMode: ['class'],
  content: [
    './src/pages/**/*.{js,ts,jsx,tsx,mdx}',
    './src/components/**/*.{js,ts,jsx,tsx,mdx}',
    './src/app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        border: 'hsl(var(--border))',
        input: 'hsl(var(--input))',
        ring: 'hsl(var(--ring))',
        background: 'hsl(var(--background))',
        foreground: 'hsl(var(--foreground))',
        primary: {
          DEFAULT: '#29ABE2',
          foreground: '#FFFFFF',
          50: '#E8F7FC',
          100: '#D1EFF9',
          200: '#A3DFF3',
          300: '#75CFED',
          400: '#47BFE7',
          500: '#29ABE2',
          600: '#1A8BBF',
          700: '#146A92',
          800: '#0E4A66',
          900: '#082939',
        },
        secondary: {
          DEFAULT: 'hsl(var(--secondary))',
          foreground: 'hsl(var(--secondary-foreground))',
        },
        destructive: {
          DEFAULT: 'hsl(var(--destructive))',
          foreground: 'hsl(var(--destructive-foreground))',
        },
        muted: {
          DEFAULT: 'hsl(var(--muted))',
          foreground: 'hsl(var(--muted-foreground))',
        },
        accent: {
          DEFAULT: 'hsl(var(--accent))',
          foreground: 'hsl(var(--accent-foreground))',
        },
        card: {
          DEFAULT: 'hsl(var(--card))',
          foreground: 'hsl(var(--card-foreground))',
        },
        // Status colors
        status: {
          up: '#22C55E',
          down: '#EF4444',
          warning: '#F59E0B',
          maintenance: '#8B5CF6',
          unknown: '#6B7280',
        },
        // Severity colors
        severity: {
          critical: '#DC2626',
          high: '#EA580C',
          medium: '#D97706',
          low: '#65A30D',
          info: '#0EA5E9',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        mono: ['JetBrains Mono', 'monospace'],
      },
      animation: {
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'fade-in': 'fadeIn 0.2s ease-in-out',
        'slide-in': 'slideIn 0.2s ease-out',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        slideIn: {
          '0%': { transform: 'translateY(-10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
};

export default config;
globals.css
css@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  :root {
    --background: 0 0% 100%;
    --foreground: 222.2 84% 4.9%;
    --card: 0 0% 100%;
    --card-foreground: 222.2 84% 4.9%;
    --popover: 0 0% 100%;
    --popover-foreground: 222.2 84% 4.9%;
    --primary: 199 76% 52%;
    --primary-foreground: 0 0% 100%;
    --secondary: 210 40% 96.1%;
    --secondary-foreground: 222.2 47.4% 11.2%;
    --muted: 210 40% 96.1%;
    --muted-foreground: 215.4 16.3% 46.9%;
    --accent: 210 40% 96.1%;
    --accent-foreground: 222.2 47.4% 11.2%;
    --destructive: 0 84.2% 60.2%;
    --destructive-foreground: 210 40% 98%;
    --border: 214.3 31.8% 91.4%;
    --input: 214.3 31.8% 91.4%;
    --ring: 199 76% 52%;
    --radius: 0.5rem;
  }

  .dark {
    --background: 222 47% 11%;
    --foreground: 210 40% 98%;
    --card: 217 33% 17%;
    --card-foreground: 210 40% 98%;
    --popover: 222 47% 11%;
    --popover-foreground: 210 40% 98%;
    --primary: 199 76% 52%;
    --primary-foreground: 0 0% 100%;
    --secondary: 217 33% 17%;
    --secondary-foreground: 210 40% 98%;
    --muted: 217 33% 17%;
    --muted-foreground: 215 20.2% 65.1%;
    --accent: 217 33% 17%;
    --accent-foreground: 210 40% 98%;
    --destructive: 0 62.8% 50.6%;
    --destructive-foreground: 210 40% 98%;
    --border: 217 33% 25%;
    --input: 217 33% 25%;
    --ring: 199 76% 52%;
  }
}

@layer base {
  * {
    @apply border-border;
  }
  body {
    @apply bg-background text-foreground;
  }
}

/* Custom scrollbar */
::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::-webkit-scrollbar-track {
  @apply bg-muted;
}

::-webkit-scrollbar-thumb {
  @apply bg-muted-foreground/30 rounded-full;
}

::-webkit-scrollbar-thumb:hover {
  @apply bg-muted-foreground/50;
}

8.4 Types
typescript// src/types/api.ts

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PagedRequest {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  search?: string;
}
typescript// src/types/host.ts

export type StatusType = 'Up' | 'Down' | 'Warning' | 'Degraded' | 'Maintenance' | 'Unknown';
export type OsType = 'Windows' | 'Linux' | 'macOS' | 'Other';

export interface Host {
  id: string;
  name: string;
  hostname: string;
  description?: string;
  osType: OsType;
  osVersion?: string;
  primaryIp?: string;
  publicIp?: string;
  currentStatus: StatusType;
  lastSeenAt?: string;
  statusChangedAt?: string;
  cpuPercent?: number;
  ramPercent?: number;
  monitoringEnabled: boolean;
  maintenanceMode: boolean;
  tags: string[];
  customerId?: string;
  customerName?: string;
  locationId?: string;
  locationName?: string;
  createdAt: string;
  updatedAt: string;
}

export interface HostDetail extends Host {
  apiKey: string;
  agentVersion?: string;
  checkIntervalSeconds: number;
  ramUsedMb?: number;
  ramTotalMb?: number;
  uptimeSeconds?: number;
  processCount?: number;
  thresholds: HostThresholds;
  monitoringSettings: HostMonitoringSettings;
  disks: HostDisk[];
  services: HostService[];
  statistics: HostStatistics;
}

export interface HostThresholds {
  cpuWarning: number;
  cpuCritical: number;
  ramWarning: number;
  ramCritical: number;
  diskWarning: number;
  diskCritical: number;
}

export interface HostMonitoringSettings {
  alertOnDown: boolean;
  alertDelaySeconds: number;
  alertOnHighCpu: boolean;
  alertOnHighRam: boolean;
  alertOnHighDisk: boolean;
}

export interface HostDisk {
  id: string;
  name: string;
  mountPoint: string;
  fileSystem: string;
  totalGb: number;
  usedGb: number;
  freeGb: number;
  usedPercent: number;
}

export interface HostService {
  id: string;
  serviceName: string;
  displayName: string;
  serviceType: string;
  currentStatus: StatusType;
  monitoringEnabled: boolean;
}

export interface HostStatistics {
  uptime24h: number;
  uptime7d: number;
  uptime30d: number;
  avgCpu24h: number;
  avgRam24h: number;
  incidentCount30d: number;
}

export interface HostMetrics {
  hostId: string;
  cpuHistory: MetricDataPoint[];
  ramHistory: MetricDataPoint[];
  diskHistory: MetricDataPoint[];
  networkHistory: MetricDataPoint[];
}

export interface MetricDataPoint {
  timestamp: string;
  value: number;
}

export interface CreateHostRequest {
  name: string;
  hostname: string;
  description?: string;
  osType: OsType;
  customerId?: string;
  locationId?: string;
  tags?: string[];
  checkIntervalSeconds?: number;
  monitoringEnabled?: boolean;
}

export interface UpdateHostRequest extends CreateHostRequest {
  thresholds?: HostThresholds;
  monitoringSettings?: HostMonitoringSettings;
}
typescript// src/types/check.ts

export type CheckType = 'HTTP' | 'TCP' | 'Ping' | 'DNS';

export interface Check {
  id: string;
  name: string;
  type: CheckType;
  target: string;
  currentStatus: StatusType;
  lastCheckAt?: string;
  lastResponseTimeMs?: number;
  lastError?: string;
  monitoringEnabled: boolean;
  intervalSeconds: number;
  sslExpiresAt?: string;
  sslDaysUntilExpiry?: number;
  customerId?: string;
  customerName?: string;
  tags: string[];
  uptime24h?: number;
  uptime7d?: number;
  createdAt: string;
}

export interface CheckDetail extends Check {
  description?: string;
  httpSettings?: HttpCheckSettings;
  tcpSettings?: TcpCheckSettings;
  dnsSettings?: DnsCheckSettings;
  sslSettings?: SslCheckSettings;
  timeoutSeconds: number;
  retryCount: number;
  retryDelaySeconds: number;
  failureThreshold: number;
  currentFailureCount: number;
  sslInfo?: SslInfo;
  statistics: CheckStatistics;
}

export interface HttpCheckSettings {
  method: string;
  headers?: Record<string, string>;
  body?: string;
  expectedStatusCodes: number[];
  expectedKeyword?: string;
  keywordShouldExist: boolean;
}

export interface TcpCheckSettings {
  port: number;
}

export interface DnsCheckSettings {
  recordType: string;
  expectedResult?: string;
}

export interface SslCheckSettings {
  enabled: boolean;
  warningDays: number;
  criticalDays: number;
}

export interface SslInfo {
  expiresAt?: string;
  daysUntilExpiry?: number;
  issuer?: string;
  subject?: string;
  status: string;
}

export interface CheckStatistics {
  uptime24h: number;
  uptime7d: number;
  uptime30d: number;
  avgResponseTimeMs: number;
  minResponseTimeMs: number;
  maxResponseTimeMs: number;
  checksLast24h: number;
  failuresLast24h: number;
}

export interface CheckResult {
  id: number;
  success: boolean;
  status: StatusType;
  responseTimeMs: number;
  checkedAt: string;
  httpStatusCode?: number;
  httpStatusMessage?: string;
  keywordFound?: boolean;
  sslValid?: boolean;
  sslDaysUntilExpiry?: number;
  dnsResult?: string;
  errorMessage?: string;
  errorType?: string;
}

export interface CreateCheckRequest {
  name: string;
  type: CheckType;
  target: string;
  description?: string;
  customerId?: string;
  hostId?: string;
  tags?: string[];
  httpMethod?: string;
  httpHeaders?: Record<string, string>;
  expectedStatusCodes?: number[];
  expectedKeyword?: string;
  checkSsl?: boolean;
  sslExpiryWarningDays?: number;
  sslExpiryCriticalDays?: number;
  tcpPort?: number;
  dnsRecordType?: string;
  expectedDnsResult?: string;
  intervalSeconds?: number;
  timeoutSeconds?: number;
  retryCount?: number;
  failureThreshold?: number;
  monitoringEnabled?: boolean;
  alertOnFailure?: boolean;
  alertOnSslExpiry?: boolean;
}
typescript// src/types/incident.ts

export type IncidentStatus = 'New' | 'Acknowledged' | 'InProgress' | 'Resolved' | 'Closed';
export type IncidentSeverity = 'Critical' | 'High' | 'Medium' | 'Low' | 'Info';

export interface Incident {
  id: string;
  incidentId: string;
  title: string;
  severity: IncidentSeverity;
  status: IncidentStatus;
  category?: string;
  isAutoCreated: boolean;
  sourceType?: string;
  sourceName?: string;
  assignedTo?: UserSummary;
  slaResponseBreached: boolean;
  slaResolutionBreached: boolean;
  createdAt: string;
  acknowledgedAt?: string;
  resolvedAt?: string;
  duration?: string;
  customerId?: string;
  customerName?: string;
}

export interface IncidentDetail extends Incident {
  description?: string;
  sourceId?: string;
  assignedAt?: string;
  acknowledgedBy?: UserSummary;
  resolvedBy?: UserSummary;
  resolutionNotes?: string;
  rootCause?: string;
  closedBy?: UserSummary;
  closedAt?: string;
  sla: SlaInfo;
  impact?: string;
  affectedUsersCount?: number;
  firstOccurredAt?: string;
  lastOccurredAt?: string;
  occurrenceCount: number;
  resources: IncidentResource[];
  timeline: IncidentTimelineItem[];
  priorityScore: number;
}

export interface SlaInfo {
  responseDue?: string;
  resolutionDue?: string;
  responseBreached: boolean;
  resolutionBreached: boolean;
  timeToResponseDue?: string;
  timeToResolutionDue?: string;
}

export interface IncidentResource {
  id: string;
  resourceType: string;
  resourceId: string;
  resourceName: string;
}

export interface IncidentTimelineItem {
  id: string;
  eventType: string;
  title?: string;
  content?: string;
  oldValue?: string;
  newValue?: string;
  isInternal: boolean;
  isSystemGenerated: boolean;
  user?: UserSummary;
  createdAt: string;
}

export interface UserSummary {
  id: string;
  fullName: string;
  avatarUrl?: string;
}

export interface CreateIncidentRequest {
  title: string;
  description?: string;
  severity: IncidentSeverity;
  category?: string;
  customerId?: string;
  assignedToId?: string;
  impact?: string;
  affectedUsersCount?: number;
  resources?: CreateIncidentResourceRequest[];
}

export interface CreateIncidentResourceRequest {
  resourceType: string;
  resourceId: string;
  resourceName: string;
}

export interface AddCommentRequest {
  content: string;
  isInternal?: boolean;
}

export interface ResolveIncidentRequest {
  resolutionNotes?: string;
  rootCause?: string;
}

8.5 API Client
typescript// src/lib/api/client.ts

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

interface RequestOptions extends RequestInit {
  params?: Record<string, string | number | boolean | undefined>;
}

class ApiClient {
  private baseUrl: string;
  private accessToken: string | null = null;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  setAccessToken(token: string | null) {
    this.accessToken = token;
  }

  private buildUrl(endpoint: string, params?: Record<string, string | number | boolean | undefined>): string {
    const url = new URL(`${this.baseUrl}${endpoint}`);
    
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          url.searchParams.append(key, String(value));
        }
      });
    }
    
    return url.toString();
  }

  private async request<T>(endpoint: string, options: RequestOptions = {}): Promise<T> {
    const { params, ...fetchOptions } = options;
    const url = this.buildUrl(endpoint, params);

    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...options.headers,
    };

    if (this.accessToken) {
      (headers as Record<string, string>)['Authorization'] = `Bearer ${this.accessToken}`;
    }

    const response = await fetch(url, {
      ...fetchOptions,
      headers,
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'An error occurred' }));
      throw new ApiError(response.status, error.message || 'Request failed', error);
    }

    if (response.status === 204) {
      return {} as T;
    }

    return response.json();
  }

  async get<T>(endpoint: string, params?: Record<string, string | number | boolean | undefined>): Promise<T> {
    return this.request<T>(endpoint, { method: 'GET', params });
  }

  async post<T>(endpoint: string, data?: unknown, params?: Record<string, string | number | boolean | undefined>): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
      params,
    });
  }

  async put<T>(endpoint: string, data?: unknown): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  async delete<T>(endpoint: string): Promise<T> {
    return this.request<T>(endpoint, { method: 'DELETE' });
  }
}

export class ApiError extends Error {
  status: number;
  data: unknown;

  constructor(status: number, message: string, data?: unknown) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.data = data;
  }
}

export const apiClient = new ApiClient(API_BASE_URL);
export default apiClient;
typescript// src/lib/api/hosts.ts

import { apiClient } from './client';
import type { 
  Host, 
  HostDetail, 
  HostMetrics, 
  CreateHostRequest, 
  UpdateHostRequest 
} from '@/types/host';
import type { PagedResponse, PagedRequest } from '@/types/api';

export const hostsApi = {
  getAll: async (params?: PagedRequest & {
    status?: string;
    customerId?: string;
    osType?: string;
    tags?: string[];
  }): Promise<PagedResponse<Host>> => {
    return apiClient.get('/api/hosts', params);
  },

  getById: async (id: string): Promise<HostDetail> => {
    return apiClient.get(`/api/hosts/${id}`);
  },

  create: async (data: CreateHostRequest): Promise<HostDetail> => {
    return apiClient.post('/api/hosts', data);
  },

  update: async (id: string, data: UpdateHostRequest): Promise<HostDetail> => {
    return apiClient.put(`/api/hosts/${id}`, data);
  },

  delete: async (id: string): Promise<void> => {
    return apiClient.delete(`/api/hosts/${id}`);
  },

  regenerateApiKey: async (id: string): Promise<{ data: string }> => {
    return apiClient.post(`/api/hosts/${id}/regenerate-api-key`);
  },

  setMaintenance: async (id: string, data: {
    enable: boolean;
    startAt?: string;
    endAt?: string;
    reason?: string;
  }): Promise<HostDetail> => {
    return apiClient.post(`/api/hosts/${id}/maintenance`, data);
  },

  toggleMonitoring: async (id: string): Promise<{ data: boolean }> => {
    return apiClient.post(`/api/hosts/${id}/toggle-monitoring`);
  },

  getMetrics: async (id: string, params?: {
    from?: string;
    to?: string;
    interval?: string;
  }): Promise<HostMetrics> => {
    return apiClient.get(`/api/hosts/${id}/metrics`, params);
  },

  getStatusCounts: async (): Promise<Record<string, number>> => {
    return apiClient.get('/api/hosts/status-counts');
  },

  getRecentlyDown: async (limit?: number): Promise<Host[]> => {
    return apiClient.get('/api/hosts/recently-down', { limit });
  },

  getHighUsage: async (limit?: number): Promise<Host[]> => {
    return apiClient.get('/api/hosts/high-usage', { limit });
  },
};
typescript// src/lib/api/incidents.ts

import { apiClient } from './client';
import type { 
  Incident, 
  IncidentDetail, 
  CreateIncidentRequest,
  AddCommentRequest,
  ResolveIncidentRequest,
  IncidentTimelineItem 
} from '@/types/incident';
import type { PagedResponse, PagedRequest } from '@/types/api';

export const incidentsApi = {
  getAll: async (params?: PagedRequest & {
    status?: string;
    severity?: string;
    customerId?: string;
    assignedToId?: string;
    isOpen?: boolean;
  }): Promise<PagedResponse<Incident>> => {
    return apiClient.get('/api/incidents', params);
  },

  getById: async (id: string): Promise<IncidentDetail> => {
    return apiClient.get(`/api/incidents/${id}`);
  },

  create: async (data: CreateIncidentRequest): Promise<IncidentDetail> => {
    return apiClient.post('/api/incidents', data);
  },

  update: async (id: string, data: Partial<CreateIncidentRequest>): Promise<IncidentDetail> => {
    return apiClient.put(`/api/incidents/${id}`, data);
  },

  acknowledge: async (id: string, comment?: string): Promise<IncidentDetail> => {
    return apiClient.post(`/api/incidents/${id}/acknowledge`, { comment });
  },

  assign: async (id: string, assignToUserId: string, comment?: string): Promise<IncidentDetail> => {
    return apiClient.post(`/api/incidents/${id}/assign`, { assignToUserId, comment });
  },

  updateStatus: async (id: string, status: string, comment?: string): Promise<IncidentDetail> => {
    return apiClient.post(`/api/incidents/${id}/status`, { status, comment });
  },

  resolve: async (id: string, data: ResolveIncidentRequest): Promise<IncidentDetail> => {
    return apiClient.post(`/api/incidents/${id}/resolve`, data);
  },

  close: async (id: string): Promise<IncidentDetail> => {
    return apiClient.post(`/api/incidents/${id}/close`);
  },

  reopen: async (id: string, reason?: string): Promise<IncidentDetail> => {
    return apiClient.post(`/api/incidents/${id}/reopen`, undefined, { reason });
  },

  escalate: async (id: string, data: {
    newSeverity: string;
    assignToUserId?: string;
    reason: string;
  }): Promise<IncidentDetail> => {
    return apiClient.post(`/api/incidents/${id}/escalate`, data);
  },

  addComment: async (id: string, data: AddCommentRequest): Promise<IncidentTimelineItem> => {
    return apiClient.post(`/api/incidents/${id}/comments`, data);
  },

  getTimeline: async (id: string, includeInternal?: boolean): Promise<IncidentTimelineItem[]> => {
    return apiClient.get(`/api/incidents/${id}/timeline`, { includeInternal });
  },

  getStatusCounts: async (): Promise<Record<string, number>> => {
    return apiClient.get('/api/incidents/status-counts');
  },

  getSeverityCounts: async (): Promise<Record<string, number>> => {
    return apiClient.get('/api/incidents/severity-counts');
  },

  getStatistics: async (from?: string, to?: string): Promise<{
    totalIncidents: number;
    openIncidents: number;
    resolvedIncidents: number;
    avgTimeToAcknowledgeMinutes: number;
    avgTimeToResolveMinutes: number;
    slaBreaches: number;
  }> => {
    return apiClient.get('/api/incidents/statistics', { from, to });
  },
};

8.6 SignalR Integration
typescript// src/lib/signalr.ts

import * as signalR from '@microsoft/signalr';

const HUB_URL = process.env.NEXT_PUBLIC_SIGNALR_URL || 'http://localhost:5000/hubs/monitoring';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 10;
  private listeners: Map<string, Set<(...args: unknown[]) => void>> = new Map();

  async connect(accessToken?: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => accessToken || '',
        withCredentials: true,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          if (retryContext.previousRetryCount >= this.maxReconnectAttempts) {
            return null;
          }
          return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
        },
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.setupConnectionEvents();
    await this.startConnection();
  }

  private setupConnectionEvents(): void {
    if (!this.connection) return;

    this.connection.onreconnecting((error) => {
      console.log('SignalR reconnecting...', error);
      this.reconnectAttempts++;
    });

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected', connectionId);
      this.reconnectAttempts = 0;
      this.resubscribeAll();
    });

    this.connection.onclose((error) => {
      console.log('SignalR connection closed', error);
    });
  }

  private async startConnection(): Promise<void> {
    try {
      await this.connection?.start();
      console.log('SignalR connected');
      this.reconnectAttempts = 0;
    } catch (error) {
      console.error('SignalR connection error:', error);
      throw error;
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  // Subscribe to groups
  async joinDashboardGroup(): Promise<void> {
    await this.connection?.invoke('JoinDashboardGroup');
  }

  async leaveDashboardGroup(): Promise<void> {
    await this.connection?.invoke('LeaveDashboardGroup');
  }

  async joinHostGroup(hostId: string): Promise<void> {
    await this.connection?.invoke('JoinHostGroup', hostId);
  }

  async leaveHostGroup(hostId: string): Promise<void> {
    await this.connection?.invoke('LeaveHostGroup', hostId);
  }

  async joinCustomerGroup(customerId: string): Promise<void> {
    await this.connection?.invoke('JoinCustomerGroup', customerId);
  }

  async leaveCustomerGroup(customerId: string): Promise<void> {
    await this.connection?.invoke('LeaveCustomerGroup', customerId);
  }

  // Event handlers
  on<T>(event: string, callback: (data: T) => void): () => void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    
    const wrappedCallback = callback as (...args: unknown[]) => void;
    this.listeners.get(event)!.add(wrappedCallback);
    this.connection?.on(event, wrappedCallback);

    return () => {
      this.listeners.get(event)?.delete(wrappedCallback);
      this.connection?.off(event, wrappedCallback);
    };
  }

  onHostUpdate(callback: (data: HostUpdateEvent) => void): () => void {
    return this.on('HostUpdate', callback);
  }

  onServiceUpdate(callback: (data: ServiceUpdateEvent) => void): () => void {
    return this.on('ServiceUpdate', callback);
  }

  onCheckResult(callback: (data: CheckResultEvent) => void): () => void {
    return this.on('CheckResult', callback);
  }

  onIncidentCreated(callback: (data: IncidentEvent) => void): () => void {
    return this.on('IncidentCreated', callback);
  }

  onIncidentUpdated(callback: (data: IncidentEvent) => void): () => void {
    return this.on('IncidentUpdated', callback);
  }

  private resubscribeAll(): void {
    this.listeners.forEach((callbacks, event) => {
      callbacks.forEach((callback) => {
        this.connection?.on(event, callback);
      });
    });
  }
}

// Event types
export interface HostUpdateEvent {
  hostId: string;
  hostName: string;
  currentStatus: string;
  cpuPercent: number;
  ramPercent: number;
  lastSeenAt: string;
  statusChangedAt?: string;
}

export interface ServiceUpdateEvent {
  serviceId: string;
  serviceName: string;
  hostId: string;
  currentStatus: string;
}

export interface CheckResultEvent {
  checkId: string;
  checkName: string;
  checkType: string;
  currentStatus: string;
  responseTimeMs: number;
  lastCheckAt: string;
  success: boolean;
  errorMessage?: string;
}

export interface IncidentEvent {
  incidentId: string;
  incidentNumber: string;
  title: string;
  severity: string;
  status: string;
  isAutoCreated?: boolean;
  updatedAt?: string;
}

export const signalRService = new SignalRService();
export default signalRService;
typescript// src/hooks/use-signalr.ts

import { useEffect, useCallback } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import signalRService, { 
  HostUpdateEvent, 
  IncidentEvent, 
  CheckResultEvent 
} from '@/lib/signalr';
import { useAuthStore } from '@/stores/auth-store';
import { toast } from 'sonner';

export function useSignalR() {
  const queryClient = useQueryClient();
  const { accessToken, isAuthenticated } = useAuthStore();

  useEffect(() => {
    if (!isAuthenticated || !accessToken) {
      return;
    }

    const connect = async () => {
      try {
        await signalRService.connect(accessToken);
        await signalRService.joinDashboardGroup();
      } catch (error) {
        console.error('Failed to connect to SignalR:', error);
      }
    };

    connect();

    return () => {
      signalRService.leaveDashboardGroup();
      signalRService.disconnect();
    };
  }, [isAuthenticated, accessToken]);

  // Host updates
  useEffect(() => {
    const unsubscribe = signalRService.onHostUpdate((data: HostUpdateEvent) => {
      // Update host in cache
      queryClient.invalidateQueries({ queryKey: ['hosts'] });
      queryClient.invalidateQueries({ queryKey: ['host', data.hostId] });
      queryClient.invalidateQueries({ queryKey: ['dashboard', 'status-overview'] });

      // Show notification for status changes
      if (data.currentStatus === 'Down') {
        toast.error(`Host Down: ${data.hostName}`, {
          description: 'The host is no longer responding.',
        });
      } else if (data.currentStatus === 'Up' && data.statusChangedAt) {
        toast.success(`Host Recovered: ${data.hostName}`, {
          description: 'The host is back online.',
        });
      }
    });

    return unsubscribe;
  }, [queryClient]);

  // Incident updates
  useEffect(() => {
    const unsubscribeCreated = signalRService.onIncidentCreated((data: IncidentEvent) => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard', 'status-overview'] });

      toast.warning(`New Incident: ${data.incidentNumber}`, {
        description: data.title,
      });
    });

    const unsubscribeUpdated = signalRService.onIncidentUpdated((data: IncidentEvent) => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] });
      queryClient.invalidateQueries({ queryKey: ['incident', data.incidentId] });
    });

    return () => {
      unsubscribeCreated();
      unsubscribeUpdated();
    };
  }, [queryClient]);

  // Check results
  useEffect(() => {
    const unsubscribe = signalRService.onCheckResult((data: CheckResultEvent) => {
      queryClient.invalidateQueries({ queryKey: ['checks'] });
      queryClient.invalidateQueries({ queryKey: ['check', data.checkId] });

      if (!data.success && data.currentStatus === 'Down') {
        toast.error(`Check Failed: ${data.checkName}`, {
          description: data.errorMessage || 'Check is failing.',
        });
      }
    });

    return unsubscribe;
  }, [queryClient]);
}

export function useHostSignalR(hostId: string) {
  const queryClient = useQueryClient();

  useEffect(() => {
    signalRService.joinHostGroup(hostId);

    return () => {
      signalRService.leaveHostGroup(hostId);
    };
  }, [hostId]);

  useEffect(() => {
    const unsubscribe = signalRService.onHostUpdate((data: HostUpdateEvent) => {
      if (data.hostId === hostId) {
        queryClient.invalidateQueries({ queryKey: ['host', hostId] });
        queryClient.invalidateQueries({ queryKey: ['host-metrics', hostId] });
      }
    });

    return unsubscribe;
  }, [hostId, queryClient]);
}

8.7 Layout Components
typescript// src/components/layout/sidebar.tsx

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
  Bell,
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
typescript// src/components/layout/header.tsx

'use client';

import { Bell, Menu, Moon, Sun } from 'lucide-react';
import { useTheme } from 'next-themes';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { useAuthStore } from '@/stores/auth-store';
import { Badge } from '@/components/ui/badge';
import { useRouter } from 'next/navigation';

interface HeaderProps {
  onMenuClick?: () => void;
}

export function Header({ onMenuClick }: HeaderProps) {
  const { theme, setTheme } = useTheme();
  const { user, logout } = useAuthStore();
  const router = useRouter();

  const handleLogout = async () => {
    await logout();
    router.push('/login');
  };

  const initials = user?.fullName
    ?.split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase() || 'U';

  return (
    <header className="sticky top-0 z-40 flex h-16 shrink-0 items-center gap-x-4 border-b border-border bg-card px-4 shadow-sm sm:gap-x-6 sm:px-6 lg:px-8">
      {/* Mobile menu button */}
      <Button
        variant="ghost"
        size="icon"
        className="lg:hidden"
        onClick={onMenuClick}
      >
        <Menu className="h-5 w-5" />
        <span className="sr-only">Open sidebar</span>
      </Button>

      {/* Separator */}
      <div className="h-6 w-px bg-border lg:hidden" />

      {/* Search and actions */}
      <div className="flex flex-1 gap-x-4 self-stretch lg:gap-x-6">
        <div className="flex flex-1" />

        <div className="flex items-center gap-x-4 lg:gap-x-6">
          {/* Theme toggle */}
          <Button
            variant="ghost"
            size="icon"
            onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
          >
            <Sun className="h-5 w-5 rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0" />
            <Moon className="absolute h-5 w-5 rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100" />
            <span className="sr-only">Toggle theme</span>
          </Button>

          {/* Notifications */}
          <Button variant="ghost" size="icon" className="relative">
            <Bell className="h-5 w-5" />
            <Badge 
              variant="destructive" 
              className="absolute -right-1 -top-1 h-5 w-5 rounded-full p-0 text-xs flex items-center justify-center"
            >
              3
            </Badge>
            <span className="sr-only">View notifications</span>
          </Button>

          {/* Separator */}
          <div className="hidden lg:block lg:h-6 lg:w-px lg:bg-border" />

          {/* User menu */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="relative h-8 w-8 rounded-full">
                <Avatar className="h-8 w-8">
                  <AvatarImage src={user?.avatarUrl} alt={user?.fullName} />
                  <AvatarFallback>{initials}</AvatarFallback>
                </Avatar>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-56" align="end" forceMount>
              <DropdownMenuLabel className="font-normal">
                <div className="flex flex-col space-y-1">
                  <p className="text-sm font-medium leading-none">{user?.fullName}</p>
                  <p className="text-xs leading-none text-muted-foreground">
                    {user?.email}
                  </p>
                </div>
              </DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={() => router.push('/settings/profile')}>
                Profile
              </DropdownMenuItem>
              <DropdownMenuItem onClick={() => router.push('/settings')}>
                Settings
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={handleLogout}>
                Log out
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>
    </header>
  );
}

8.8 Dashboard Components
typescript// src/components/dashboard/status-overview.tsx

'use client';

import { useQuery } from '@tanstack/react-query';
import { Server, Activity, Globe, AlertTriangle } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { cn } from '@/lib/utils';

interface StatusOverviewData {
  totalHosts: number;
  hostsUp: number;
  hostsDown: number;
  hostsWarning: number;
  totalServices: number;
  servicesUp: number;
  servicesDown: number;
  totalChecks: number;
  checksPassing: number;
  checksFailing: number;
  openIncidents: number;
  criticalIncidents: number;
  overallHealth: number;
}

async function fetchStatusOverview(): Promise<StatusOverviewData> {
  const response = await fetch('/api/dashboards/status-overview');
  if (!response.ok) throw new Error('Failed to fetch status overview');
  return response.json();
}

export function StatusOverview() {
  const { data, isLoading, error } = useQuery({
    queryKey: ['dashboard', 'status-overview'],
    queryFn: fetchStatusOverview,
    refetchInterval: 30000, // Refresh every 30 seconds
  });

  if (isLoading) {
    return <StatusOverviewSkeleton />;
  }

  if (error) {
    return (
      <Card className="col-span-full">
        <CardContent className="p-6">
          <p className="text-destructive">Failed to load status overview</p>
        </CardContent>
      </Card>
    );
  }

  const stats = [
    {
      name: 'Hosts',
      icon: Server,
      total: data?.totalHosts || 0,
      up: data?.hostsUp || 0,
      down: data?.hostsDown || 0,
      warning: data?.hostsWarning || 0,
    },
    {
      name: 'Services',
      icon: Activity,
      total: data?.totalServices || 0,
      up: data?.servicesUp || 0,
      down: data?.servicesDown || 0,
    },
    {
      name: 'Checks',
      icon: Globe,
      total: data?.totalChecks || 0,
      up: data?.checksPassing || 0,
      down: data?.checksFailing || 0,
    },
    {
      name: 'Incidents',
      icon: AlertTriangle,
      total: data?.openIncidents || 0,
      critical: data?.criticalIncidents || 0,
    },
  ];

  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      {stats.map((stat) => (
        <Card key={stat.name}>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">{stat.name}</CardTitle>
            <stat.icon className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stat.total}</div>
            <div className="flex items-center gap-2 mt-1 text-xs text-muted-foreground">
              {stat.up !== undefined && (
                <span className="flex items-center gap-1">
                  <span className="h-2 w-2 rounded-full bg-status-up" />
                  {stat.up} up
                </span>
              )}
              {stat.down !== undefined && stat.down > 0 && (
                <span className="flex items-center gap-1">
                  <span className="h-2 w-2 rounded-full bg-status-down" />
                  {stat.down} down
                </span>
              )}
              {stat.warning !== undefined && stat.warning > 0 && (
                <span className="flex items-center gap-1">
                  <span className="h-2 w-2 rounded-full bg-status-warning" />
                  {stat.warning} warning
                </span>
              )}
              {stat.critical !== undefined && stat.critical > 0 && (
                <span className="flex items-center gap-1 text-destructive">
                  {stat.critical} critical
                </span>
              )}
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

function StatusOverviewSkeleton() {
  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      {[...Array(4)].map((_, i) => (
        <Card key={i}>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <Skeleton className="h-4 w-20" />
            <Skeleton className="h-4 w-4" />
          </CardHeader>
          <CardContent>
            <Skeleton className="h-8 w-16" />
            <Skeleton className="h-3 w-32 mt-2" />
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
typescript// src/components/dashboard/host-grid.tsx

'use client';

import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { Server, Cpu, MemoryStick, Clock } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { StatusBadge } from '@/components/common/status-badge';
import { Skeleton } from '@/components/ui/skeleton';
import { cn } from '@/lib/utils';
import { formatDistanceToNow } from 'date-fns';
import type { Host } from '@/types/host';

interface HostGridProps {
  limit?: number;
  customerId?: string;
}

export function HostGrid({ limit = 12, customerId }: HostGridProps) {
  const { data, isLoading } = useQuery({
    queryKey: ['dashboard', 'host-grid', { limit, customerId }],
    queryFn: async () => {
      const params = new URLSearchParams({ pageSize: String(limit) });
      if (customerId) params.append('customerId', customerId);
      
      const response = await fetch(`/api/hosts?${params}`);
      if (!response.ok) throw new Error('Failed to fetch hosts');
      return response.json();
    },
    refetchInterval: 30000,
  });

  if (isLoading) {
    return <HostGridSkeleton count={limit} />;
  }

  const hosts: Host[] = data?.items || [];

  if (hosts.length === 0) {
    return (
      <Card>
        <CardContent className="flex flex-col items-center justify-center py-10">
          <Server className="h-12 w-12 text-muted-foreground mb-4" />
          <p className="text-muted-foreground">No hosts found</p>
          <Link href="/hosts/new" className="text-primary hover:underline mt-2">
            Add your first host
          </Link>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>Hosts</CardTitle>
          <Link href="/hosts" className="text-sm text-primary hover:underline">
            View all
          </Link>
        </div>
      </CardHeader>
      <CardContent>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {hosts.map((host) => (
            <HostGridItem key={host.id} host={host} />
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

function HostGridItem({ host }: { host: Host }) {
  const isDown = host.currentStatus === 'Down';
  const isWarning = host.currentStatus === 'Warning' || host.currentStatus === 'Degraded';

  return (
    <Link href={`/hosts/${host.id}`}>
      <div
        className={cn(
          'rounded-lg border p-3 transition-colors hover:bg-accent',
          isDown && 'border-status-down bg-status-down/5',
          isWarning && 'border-status-warning bg-status-warning/5'
        )}
      >
        <div className="flex items-start justify-between mb-2">
          <div className="flex items-center gap-2">
            <Server className="h-4 w-4 text-muted-foreground" />
            <span className="font-medium text-sm truncate max-w-[120px]">
              {host.name}
            </span>
          </div>
          <StatusBadge status={host.currentStatus} size="sm" />
        </div>

        <div className="space-y-1 text-xs text-muted-foreground">
          <div className="flex items-center justify-between">
            <span className="flex items-center gap-1">
              <Cpu className="h-3 w-3" /> CPU
            </span>
            <span className={cn(
              host.cpuPercent && host.cpuPercent > 90 && 'text-destructive font-medium'
            )}>
              {host.cpuPercent?.toFixed(1) || '--'}%
            </span>
          </div>
          <div className="flex items-center justify-between">
            <span className="flex items-center gap-1">
              <MemoryStick className="h-3 w-3" /> RAM
            </span>
            <span className={cn(
              host.ramPercent && host.ramPercent > 90 && 'text-destructive font-medium'
            )}>
              {host.ramPercent?.toFixed(1) || '--'}%
            </span>
          </div>
          {host.lastSeenAt && (
            <div className="flex items-center justify-between pt-1 border-t">
              <span className="flex items-center gap-1">
                <Clock className="h-3 w-3" />
              </span>
              <span>
                {formatDistanceToNow(new Date(host.lastSeenAt), { addSuffix: true })}
              </span>
            </div>
          )}
        </div>
      </div>
    </Link>
  );
}

function HostGridSkeleton({ count }: { count: number }) {
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-6 w-20" />
      </CardHeader>
      <CardContent>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {[...Array(count)].map((_, i) => (
            <div key={i} className="rounded-lg border p-3">
              <div className="flex items-center justify-between mb-2">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-5 w-12" />
              </div>
              <div className="space-y-2">
                <Skeleton className="h-3 w-full" />
                <Skeleton className="h-3 w-full" />
                <Skeleton className="h-3 w-2/3" />
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

8.9 Charts
typescript// src/components/charts/cpu-chart.tsx

'use client';

import { useMemo } from 'react';
import {
  Area,
  AreaChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { format } from 'date-fns';
import type { MetricDataPoint } from '@/types/host';

interface CpuChartProps {
  data: MetricDataPoint[];
  title?: string;
  height?: number;
}

export function CpuChart({ data, title = 'CPU Usage', height = 300 }: CpuChartProps) {
  const chartData = useMemo(() => {
    return data.map((point) => ({
      timestamp: new Date(point.timestamp).getTime(),
      value: point.value,
    }));
  }, [data]);

  const formatXAxis = (timestamp: number) => {
    return format(new Date(timestamp), 'HH:mm');
  };

  const formatTooltip = (value: number) => {
    return [`${value.toFixed(1)}%`, 'CPU'];
  };

  const formatTooltipLabel = (timestamp: number) => {
    return format(new Date(timestamp), 'MMM dd, HH:mm:ss');
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={height}>
          <AreaChart
            data={chartData}
            margin={{ top: 10, right: 10, left: 0, bottom: 0 }}
          >
            <defs>
              <linearGradient id="cpuGradient" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#29ABE2" stopOpacity={0.3} />
                <stop offset="95%" stopColor="#29ABE2" stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis
              dataKey="timestamp"
              tickFormatter={formatXAxis}
              fontSize={12}
              tickLine={false}
              axisLine={false}
              className="fill-muted-foreground"
            />
            <YAxis
              domain={[0, 100]}
              tickFormatter={(v) => `${v}%`}
              fontSize={12}
              tickLine={false}
              axisLine={false}
              className="fill-muted-foreground"
              width={45}
            />
            <Tooltip
              formatter={formatTooltip}
              labelFormatter={formatTooltipLabel}
              contentStyle={{
                backgroundColor: 'hsl(var(--card))',
                border: '1px solid hsl(var(--border))',
                borderRadius: '6px',
              }}
            />
            <Area
              type="monotone"
              dataKey="value"
              stroke="#29ABE2"
              strokeWidth={2}
              fill="url(#cpuGradient)"
            />
          </AreaChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
typescript// src/components/charts/uptime-chart.tsx

'use client';

import { useMemo } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';
import { cn } from '@/lib/utils';
import { format, subDays } from 'date-fns';

interface UptimeDay {
  date: string;
  uptimePercent: number;
  outages: number;
  status: 'up' | 'partial' | 'down' | 'maintenance' | 'unknown';
}

interface UptimeChartProps {
  data: UptimeDay[];
  days?: number;
  title?: string;
}

export function UptimeChart({ data, days = 90, title = 'Uptime' }: UptimeChartProps) {
  const chartDays = useMemo(() => {
    const result: UptimeDay[] = [];
    const today = new Date();
    
    for (let i = days - 1; i >= 0; i--) {
      const date = format(subDays(today, i), 'yyyy-MM-dd');
      const dayData = data.find((d) => d.date === date);
      
      result.push(
        dayData || {
          date,
          uptimePercent: 100,
          outages: 0,
          status: 'unknown',
        }
      );
    }
    
    return result;
  }, [data, days]);

  const averageUptime = useMemo(() => {
    const validDays = chartDays.filter((d) => d.status !== 'unknown');
    if (validDays.length === 0) return 100;
    return validDays.reduce((sum, d) => sum + d.uptimePercent, 0) / validDays.length;
  }, [chartDays]);

  const getStatusColor = (day: UptimeDay) => {
    if (day.status === 'unknown') return 'bg-muted';
    if (day.status === 'maintenance') return 'bg-status-maintenance';
    if (day.uptimePercent >= 99.9) return 'bg-status-up';
    if (day.uptimePercent >= 95) return 'bg-status-warning';
    return 'bg-status-down';
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="text-base">{title}</CardTitle>
          <span className="text-2xl font-bold text-primary">
            {averageUptime.toFixed(2)}%
          </span>
        </div>
      </CardHeader>
      <CardContent>
        <div className="flex gap-[2px] flex-wrap">
          {chartDays.map((day, index) => (
            <Tooltip key={day.date}>
              <TooltipTrigger>
                <div
                  className={cn(
                    'w-3 h-8 rounded-sm transition-colors hover:opacity-80',
                    getStatusColor(day)
                  )}
                />
              </TooltipTrigger>
              <TooltipContent>
                <div className="text-sm">
                  <p className="font-medium">{format(new Date(day.date), 'MMM dd, yyyy')}</p>
                  <p>Uptime: {day.uptimePercent.toFixed(2)}%</p>
                  {day.outages > 0 && <p>Outages: {day.outages}</p>}
                </div>
              </TooltipContent>
            </Tooltip>
          ))}
        </div>
        <div className="flex items-center justify-between mt-4 text-xs text-muted-foreground">
          <span>{days} days ago</span>
          <div className="flex items-center gap-4">
            <span className="flex items-center gap-1">
              <span className="w-3 h-3 rounded-sm bg-status-up" />
              100%
            </span>
            <span className="flex items-center gap-1">
              <span className="w-3 h-3 rounded-sm bg-status-warning" />
              95-99%
            </span>
            <span className="flex items-center gap-1">
              <span className="w-3 h-3 rounded-sm bg-status-down" />
              &lt;95%
            </span>
          </div>
          <span>Today</span>
        </div>
      </CardContent>
    </Card>
  );
}

8.10 Common Components
typescript// src/components/common/status-badge.tsx

import { cn } from '@/lib/utils';
import type { StatusType } from '@/types/host';

interface StatusBadgeProps {
  status: StatusType;
  size?: 'sm' | 'md' | 'lg';
  showPulse?: boolean;
  className?: string;
}

const statusConfig: Record<StatusType, { label: string; className: string }> = {
  Up: {
    label: 'Up',
    className: 'bg-status-up/10 text-status-up border-status-up/20',
  },
  Down: {
    label: 'Down',
    className: 'bg-status-down/10 text-status-down border-status-down/20',
  },
  Warning: {
    label: 'Warning',
    className: 'bg-status-warning/10 text-status-warning border-status-warning/20',
  },
  Degraded: {
    label: 'Degraded',
    className: 'bg-status-warning/10 text-status-warning border-status-warning/20',
  },
  Maintenance: {
    label: 'Maintenance',
    className: 'bg-status-maintenance/10 text-status-maintenance border-status-maintenance/20',
  },
  Unknown: {
    label: 'Unknown',
    className: 'bg-status-unknown/10 text-status-unknown border-status-unknown/20',
  },
};

const sizeClasses = {
  sm: 'text-xs px-1.5 py-0.5',
  md: 'text-sm px-2 py-1',
  lg: 'text-base px-3 py-1.5',
};

export function StatusBadge({ 
  status, 
  size = 'md', 
  showPulse = false,
  className 
}: StatusBadgeProps) {
  const config = statusConfig[status] || statusConfig.Unknown;

  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 rounded-full border font-medium',
        config.className,
        sizeClasses[size],
        className
      )}
    >
      {showPulse && (
        <span className="relative flex h-2 w-2">
          {status === 'Up' && (
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-status-up opacity-75" />
          )}
          <span
            className={cn(
              'relative inline-flex rounded-full h-2 w-2',
              status === 'Up' && 'bg-status-up',
              status === 'Down' && 'bg-status-down',
              status === 'Warning' && 'bg-status-warning',
              status === 'Degraded' && 'bg-status-warning',
              status === 'Maintenance' && 'bg-status-maintenance',
              status === 'Unknown' && 'bg-status-unknown'
            )}
          />
        </span>
      )}
      {config.label}
    </span>
  );
}
typescript// src/components/common/severity-badge.tsx

import { cn } from '@/lib/utils';
import type { IncidentSeverity } from '@/types/incident';

interface SeverityBadgeProps {
  severity: IncidentSeverity;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

const severityConfig: Record<IncidentSeverity, { label: string; className: string }> = {
  Critical: {
    label: 'Critical',
    className: 'bg-severity-critical/10 text-severity-critical border-severity-critical/20',
  },
  High: {
    label: 'High',
    className: 'bg-severity-high/10 text-severity-high border-severity-high/20',
  },
  Medium: {
    label: 'Medium',
    className: 'bg-severity-medium/10 text-severity-medium border-severity-medium/20',
  },
  Low: {
    label: 'Low',
    className: 'bg-severity-low/10 text-severity-low border-severity-low/20',
  },
  Info: {
    label: 'Info',
    className: 'bg-severity-info/10 text-severity-info border-severity-info/20',
  },
};

const sizeClasses = {
  sm: 'text-xs px-1.5 py-0.5',
  md: 'text-sm px-2 py-1',
  lg: 'text-base px-3 py-1.5',
};

export function SeverityBadge({ severity, size = 'md', className }: SeverityBadgeProps) {
  const config = severityConfig[severity] || severityConfig.Medium;

  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full border font-medium',
        config.className,
        sizeClasses[size],
        className
      )}
    >
      {config.label}
    </span>
  );
}
typescript// src/components/common/data-table.tsx

'use client';

import {
  ColumnDef,
  flexRender,
  getCoreRowModel,
  useReactTable,
  getPaginationRowModel,
  getSortedRowModel,
  SortingState,
  getFilteredRowModel,
  ColumnFiltersState,
} from '@tanstack/react-table';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useState } from 'react';
import { ChevronLeft, ChevronRight, Search } from 'lucide-react';

interface DataTableProps<TData, TValue> {
  columns: ColumnDef<TData, TValue>[];
  data: TData[];
  searchPlaceholder?: string;
  searchColumn?: string;
  pageSize?: number;
  showPagination?: boolean;
  showSearch?: boolean;
}

export function DataTable<TData, TValue>({
  columns,
  data,
  searchPlaceholder = 'Search...',
  searchColumn,
  pageSize = 10,
  showPagination = true,
  showSearch = true,
}: DataTableProps<TData, TValue>) {
  const [sorting, setSorting] = useState<SortingState>([]);
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([]);
  const [globalFilter, setGlobalFilter] = useState('');

  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
    onGlobalFilterChange: setGlobalFilter,
    state: {
      sorting,
      columnFilters,
      globalFilter,
    },
    initialState: {
      pagination: {
        pageSize,
      },
    },
  });

  return (
    <div className="space-y-4">
      {showSearch && (
        <div className="relative w-full max-w-sm">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder={searchPlaceholder}
            value={searchColumn 
              ? (table.getColumn(searchColumn)?.getFilterValue() as string) ?? ''
              : globalFilter
            }
            onChange={(e) => {
              if (searchColumn) {
                table.getColumn(searchColumn)?.setFilterValue(e.target.value);
              } else {
                setGlobalFilter(e.target.value);
              }
            }}
            className="pl-9"
          />
        </div>
      )}

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <TableHead key={header.id}>
                    {header.isPlaceholder
                      ? null
                      : flexRender(
                          header.column.columnDef.header,
                          header.getContext()
                        )}
                  </TableHead>
                ))}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {table.getRowModel().rows?.length ? (
              table.getRowModel().rows.map((row) => (
                <TableRow
                  key={row.id}
                  data-state={row.getIsSelected() && 'selected'}
                >
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id}>
                      {flexRender(
                        cell.column.columnDef.cell,
                        cell.getContext()
                      )}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell
                  colSpan={columns.length}
                  className="h-24 text-center"
                >
                  No results.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>

      {showPagination && (
        <div className="flex items-center justify-between px-2">
          <div className="text-sm text-muted-foreground">
            Page {table.getState().pagination.pageIndex + 1} of{' '}
            {table.getPageCount()}
          </div>
          <div className="flex items-center space-x-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => table.previousPage()}
              disabled={!table.getCanPreviousPage()}
            >
              <ChevronLeft className="h-4 w-4" />
              Previous
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => table.nextPage()}
              disabled={!table.getCanNextPage()}
            >
              Next
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}

8.11 Phase 8 Checklist
markdown# Phase 8 Completion Checklist

## Project Setup
- [ ] Next.js 14 with App Router
- [ ] TypeScript configuration
- [ ] Tailwind CSS with custom theme
- [ ] shadcn/ui components
- [ ] ESLint and Prettier

## Core Features
- [ ] Authentication flow (login/register/logout)
- [ ] Protected routes
- [ ] Role-based access control
- [ ] Theme toggle (dark/light)
- [ ] Responsive design

## Layout
- [ ] Sidebar navigation
- [ ] Header with user menu
- [ ] Mobile navigation
- [ ] Breadcrumbs

## Dashboard
- [ ] Status overview cards
- [ ] Host status grid
- [ ] Recent incidents list
- [ ] Quick stats
- [ ] Real-time updates

## Hosts
- [ ] Host list with filters
- [ ] Host detail page
- [ ] Host metrics charts
- [ ] Host form (create/edit)
- [ ] Maintenance mode toggle
- [ ] API key regeneration

## Services
- [ ] Service list
- [ ] Service detail
- [ ] Service status history

## Checks
- [ ] Check list with filters
- [ ] Check detail page
- [ ] Check history/results
- [ ] Check form (HTTP/TCP/Ping/DNS)
- [ ] SSL certificate info

## Incidents
- [ ] Incident list with filters
- [ ] Incident detail page
- [ ] Incident timeline
- [ ] Incident actions (acknowledge, assign, resolve)
- [ ] Add comments
- [ ] Incident form

## Customers
- [ ] Customer list
- [ ] Customer detail
- [ ] Customer hosts/checks

## Reports
- [ ] Report list
- [ ] Report form
- [ ] Report execution history
- [ ] Download reports

## Settings
- [ ] Organization settings
- [ ] Notification channels
- [ ] Notification rules
- [ ] User management
- [ ] Profile settings

## Charts
- [ ] CPU usage chart
- [ ] Memory usage chart
- [ ] Uptime chart
- [ ] Response time chart
- [ ] Availability heatmap

## Real-time
- [ ] SignalR connection
- [ ] Host update events
- [ ] Incident events
- [ ] Check result events
- [ ] Toast notifications

## API Integration
- [ ] API client with interceptors
- [ ] React Query for data fetching
- [ ] Error handling
- [ ] Loading states

## State Management
- [ ] Auth store (Zustand)
- [ ] Dashboard store
- [ ] Notification store

## Testing
- [ ] Component tests
- [ ] API mock tests
- [ ] E2E tests (Playwright)
PHASE 8: Next.js Dashboard (Days 31-42)8.1 Project Setupbashnpx create-next-app@latest dashboard --typescript --tailwind --eslint --app --src-dir
cd dashboard
npm install @tanstack/react-query axios date-fns recharts lucide-react
npm install @radix-ui/react-* # shadcn/ui dependencies
npx shadcn-ui@latest init8.2 Project Structuredashboard/
├── src/
│   ├── app/
│   │   ├── (auth)/
│   │   │   ├── login/page.tsx
│   │   │   └── forgot-password/page.tsx
│   │   │
│   │   ├── (admin)/
│   │   │   ├── layout.tsx                 # Admin layout with sidebar
│   │   │   ├── page.tsx                   # Dashboard home
│   │   │   ├── hosts/
│   │   │   │   ├── page.tsx               # Hosts list
│   │   │   │   └── [id]/page.tsx          # Host detail
│   │   │   ├── services/page.tsx
│   │   │   ├── checks/page.tsx
│   │   │   ├── incidents/
│   │   │   │   ├── page.tsx               # Kanban view
│   │   │   │   └── [id]/page.tsx          # Incident detail
│   │   │   ├── customers/page.tsx
│   │   │   ├── locations/page.tsx
│   │   │   ├── users/page.tsx
│   │   │   ├── reports/page.tsx
│   │   │   ├── notifications/page.tsx
│   │   │   └── settings/
│   │   │       ├── page.tsx
│   │   │       └── notifications/page.tsx
│   │   │
│   │   ├── (portal)/                      # Customer portal
│   │   │   ├── layout.tsx
│   │   │   ├── page.tsx                   # Portal dashboard
│   │   │   ├── hosts/page.tsx
│   │   │   ├── websites/page.tsx
│   │   │   ├── incidents/page.tsx
│   │   │   ├── reports/page.tsx
│   │   │   └── settings/page.tsx
│   │   │
│   │   ├── layout.tsx
│   │   └── globals.css
│   │
│   ├── components/
│   │   ├── ui/                            # shadcn/ui components
│   │   ├── layout/
│   │   │   ├── Sidebar.tsx
│   │   │   ├── TopBar.tsx
│   │   │   └── UserMenu.tsx
│   │   ├── dashboard/
│   │   │   ├── StatsCards.tsx
│   │   │   ├── SystemHealthChart.tsx
│   │   │   ├── DatacenterMap.tsx
│   │   │   └── RecentIncidents.tsx
│   │   ├── hosts/
│   │   │   ├── HostsTable.tsx
│   │   │   ├── HostCard.tsx
│   │   │   ├── HostDetail.tsx
│   │   │   ├── AddHostModal.tsx
│   │   │   └── MetricsCharts.tsx
│   │   ├── services/
│   │   │   ├── ServicesTable.tsx
│   │   │   └── ServiceDetailPanel.tsx
│   │   ├── checks/
│   │   │   ├── ChecksTable.tsx
│   │   │   ├── AddCheckModal.tsx
│   │   │   └── CheckResultsChart.tsx
│   │   ├── incidents/
│   │   │   ├── IncidentKanban.tsx
│   │   │   ├── IncidentCard.tsx
│   │   │   ├── IncidentDetail.tsx
│   │   │   └── IncidentTimeline.tsx
│   │   ├── customers/
│   │   │   ├── CustomersTable.tsx
│   │   │   └── CustomerModal.tsx
│   │   └── common/
│   │       ├── StatusBadge.tsx
│   │       ├── StatusDot.tsx
│   │       ├── DataTable.tsx
│   │       ├── LoadingSpinner.tsx
│   │       ├── EmptyState.tsx
│   │       └── ConfirmDialog.tsx
│   │
│   ├── hooks/
│   │   ├── useAuth.ts
│   │   ├── useHosts.ts
│   │   ├── useServices.ts
│   │   ├── useChecks.ts
│   │   ├── useIncidents.ts
│   │   └── useRealtime.ts
│   │
│   ├── lib/
│   │   ├── api.ts                         # Axios instance
│   │   ├── auth.ts                        # Auth utilities
│   │   ├── utils.ts
│   │   └── signalr.ts                     # SignalR client
│   │
│   ├── stores/
│   │   └── authStore.ts                   # Zustand store
│   │
│   └── types/
│       └── index.ts                       # TypeScript types
│
├── public/
│   └── images/
│       └── logo.png
│
└── package.json8.3 Key Components to ImplementReference the designs in /stitch_admin_dashboard_home folder for exact styling:Priority Order:
1. Layout (Sidebar, TopBar)
2. Dashboard Home (Stats, Charts, Map, Incidents)
3. Hosts Management (Table, Detail, Modal)
4. Services Management (Table, Panel)
5. Checks Management (Table, Modal)
6. Incidents (Kanban, Detail, Timeline)
7. Customers Management
8. Users & Permissions
9. Notification Settings
10. Reports
11. Customer Portal pages
12. Account Settings
13. Loading & Empty states
14. Error pages (404, 500)
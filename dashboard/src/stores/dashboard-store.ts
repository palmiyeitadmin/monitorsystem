import { create } from 'zustand';

interface DashboardState {
    isSidebarOpen: boolean;
    toggleSidebar: () => void;
    setSidebarOpen: (open: boolean) => void;
    refreshInterval: number;
    setRefreshInterval: (interval: number) => void;
}

export const useDashboardStore = create<DashboardState>((set) => ({
    isSidebarOpen: true,
    toggleSidebar: () => set((state) => ({ isSidebarOpen: !state.isSidebarOpen })),
    setSidebarOpen: (open) => set({ isSidebarOpen: open }),
    refreshInterval: 30000, // 30 seconds
    setRefreshInterval: (interval) => set({ refreshInterval: interval }),
}));

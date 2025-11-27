"use client";

import { useEffect, useState } from "react";
import { api, Location } from "@/lib/api";
import { StatusBadge } from "@/components/common/StatusBadge";

export default function LocationsPage() {
    const [locations, setLocations] = useState<Location[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const load = async () => {
            try {
                const data = await api.locations.list();
                setLocations(data);
            } catch (error) {
                console.error("Lokasyonlar yuklenemedi", error);
            } finally {
                setLoading(false);
            }
        };
        load();
    }, []);

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight text-white">Lokasyonlar</h2>
                    <p className="text-muted-foreground">Veri merkezi ve bulut bölgeleri</p>
                </div>
            </div>

            <div className="overflow-hidden rounded-lg border border-[#365663] bg-[#121d21]">
                <table className="w-full text-left">
                    <thead className="bg-[#1b2b32]">
                        <tr>
                            <th className="px-4 py-3 text-white text-sm font-medium">Ad</th>
                            <th className="px-4 py-3 text-white text-sm font-medium">Kategori</th>
                            <th className="px-4 py-3 text-white text-sm font-medium">Sehir / Ulke</th>
                            <th className="px-4 py-3 text-white text-sm font-medium">Durum</th>
                        </tr>
                    </thead>
                    <tbody>
                        {loading ? (
                            <tr>
                                <td colSpan={4} className="px-4 py-8 text-center text-gray-400">Yukleniyor...</td>
                            </tr>
                        ) : locations.length === 0 ? (
                            <tr>
                                <td colSpan={4} className="px-4 py-8 text-center text-gray-400">Lokasyon bulunamadı</td>
                            </tr>
                        ) : (
                            locations.map((loc) => (
                                <tr key={loc.id} className="border-t border-[#365663] hover:bg-[#1b2b32]/50">
                                    <td className="px-4 py-2 text-white text-sm">{loc.name}</td>
                                    <td className="px-4 py-2 text-[#95b7c6] text-sm capitalize">{loc.category?.toLowerCase()}</td>
                                    <td className="px-4 py-2 text-[#95b7c6] text-sm">
                                        {[loc.city, loc.country].filter(Boolean).join(", ") || "—"}
                                    </td>
                                    <td className="px-4 py-2">
                                        <StatusBadge status={loc.isActive ? "up" : "down"} />
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

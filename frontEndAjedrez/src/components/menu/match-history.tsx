"use client";

import { useState, useEffect } from "react";
import { getAuth } from "@/actions/get-auth";

interface Match {
    matchId: string;
    opponent: string;
    result: string;
    date: string;
}

export default function MatchHistory() {
    const [matches, setMatches] = useState<Match[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [userId, setUserId] = useState<number | null>(null);

    useEffect(() => {
        async function fetchUserId() {
            const authData = await getAuth();
            const id = authData?.decodedToken?.Id ?? null;
            setUserId(id);
        }
        fetchUserId();
    }, []);

    useEffect(() => {
        if (!userId) return;

        const fetchMatches = async () => {
            setLoading(true);
            setError(null);

            try {
                const response = await fetch(
                    `https://localhost:7218/api/User/history/${userId}?page=1&pageSize=10`,
                    {
                        method: "GET",
                        headers: {
                            "Content-Type": "application/json",
                        },
                    }
                );

                if (!response.ok) {
                    throw new Error(`Error al obtener el historial: ${response.status}`);
                }

                const data = await response.json();
                setMatches(Array.isArray(data) ? data : []);
            } catch (err: any) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };

        fetchMatches();
    }, [userId]);

    return (
        <div className="bg-foreground shadow-md rounded-lg p-4">
            <h2 className="text-xl font-semibold text-white mb-3">Historial de partidas</h2>
            {loading && <p className="text-sm text-gray-400">Cargando...</p>}
            {error && <p className="text-sm text-red-400">{error}</p>}
            {!loading && !error && matches.length === 0 && (
                <p className="text-sm text-gray-400">No hay partidas jugadas aún</p>
            )}
            {!loading && !error && matches.length > 0 && (
                <div className="overflow-y-auto max-h-64">
                    <table className="w-full text-left text-white">
                        <thead>
                            <tr className="border-b border-gray-600">
                                <th className="py-2 px-3 text-sm font-medium">Oponente</th>
                                <th className="py-2 px-3 text-sm font-medium">Resultado</th>
                                <th className="py-2 px-3 text-sm font-medium">Fecha</th>
                            </tr>
                        </thead>
                        <tbody>
                            {matches.map((match) => (
                                <tr key={match.matchId} className="border-b border-gray-700">
                                    <td className="py-2 px-3">{match.opponent}</td>
                                    <td className="py-2 px-3">
                                        <span
                                            className={`${
                                                match.result === "Win"
                                                    ? "text-green-400"
                                                    : match.result === "Loss"
                                                    ? "text-red-400"
                                                    : "text-yellow-400"
                                            }`}
                                        >
                                            {match.result}
                                        </span>
                                    </td>
                                    <td className="py-2 px-3 text-sm">
                                        {new Date(match.date).toLocaleDateString()}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
}
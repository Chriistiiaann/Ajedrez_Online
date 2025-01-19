"use client";

import React from "react";
import { useEffect, useState } from "react";

import { Button } from "@/components/ui/button";
import { Moon, Sun } from "lucide-react";

export function ToggleTheme() {

    const [isDark, setIsDark] = useState(true);

    useEffect(() => {
        const root = document.documentElement;
        if (isDark) {
        root.classList.add("dark");
        } else {
        root.classList.remove("dark");
        }
    }, [isDark]);

    return (
        <Button variant="outline" size="icon" onClick={() => setIsDark(!isDark)} className="text-">
            {isDark ? <Sun /> : <Moon />}
        </Button>
    )
}
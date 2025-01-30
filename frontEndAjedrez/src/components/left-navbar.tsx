'use client'

import React from 'react';
// import Image from 'next/image';
import Link from 'next/link';
import { Menu } from '@headlessui/react';
import { Button } from "@/components/ui/button"
import { Home, Gamepad, Users, User, Settings, ChevronDown, Globe, Bot } from 'lucide-react';

const LeftSideNavbar: React.FC = () => {
    return (
        <nav className="bg-background border-r border-white text-white h-screen w-56 flex flex-col">
        {/* Logo */}
        <div className="p-4">
            {/* <Image src="/logo.svg" alt="App Logo" width={48} height={48} /> */}
            <h1 className="text-2xl font-bold">Chess</h1>
        </div>

        {/* Main Navigation Links */}
        <ul className="flex-grow">
            <li className="px-4 py-2 hover:bg-white hover:text-background">
            <Link href="/" className="flex items-center">
                <Home className="mr-2" size={20} />
                Inicio
            </Link>
            </li>
            <li className="px-4 py-2 hover:bg-white hover:text-background">
            <Menu as="div" className="relative">
                <Menu.Button className="flex items-center w-full">
                <Gamepad className="mr-2" size={20} />
                Jugar
                <ChevronDown className="ml-auto" size={16} />
                </Menu.Button>
                <Menu.Items className="absolute left-10 top-5 w-48 bg-gray-700 rounded-md shadow-lg">
                <Menu.Item>
                    {({ active }) => (
                    <Link
                        href="/jugar/online"
                        className={`${
                        active ? 'bg-gray-600' : ''
                        } block px-4 py-2 text-sm`}
                    >
                        <Globe className="inline-block mr-2" size={16} />
                        Partida Online
                    </Link>
                    )}
                </Menu.Item>
                <Menu.Item>
                    {({ active }) => (
                    <Link
                        href="/jugar/bot"
                        className={`${
                        active ? 'bg-gray-600' : ''
                        } block px-4 py-2 text-sm`}
                    >
                        <Bot className="inline-block mr-2" size={16} />
                        Partida contra Bot
                    </Link>
                    )}
                </Menu.Item>
                </Menu.Items>
            </Menu>
            </li>
            <li className="px-4 py-2 hover:bg-white hover:text-background">
            <Link href="/amigos" className="flex items-center">
                <Users className="mr-2" size={20} />
                Amigos
            </Link>
            </li>
        </ul>

        {/* User Profile */}
        <div className="p-4">
            <Menu as="div" className="relative">
                <Link href="/login" >
                    <Button className="w-full bg-accent text-white">
                        Iniciar sesión
                    </Button>
                </Link>
            {/* <Menu.Button className="flex items-center w-full">
                <Image
                src="/user-avatar.jpg"
                alt="User Avatar"
                width={32}
                height={32}
                className="rounded-full mr-2"
                />
                <span>User Name</span>
                <ChevronDown className="ml-auto" size={16} />
            </Menu.Button>
            <Menu.Items className="absolute bottom-full left-0 w-full bg-gray-700 rounded-md shadow-lg">
                <Menu.Item>
                {({ active }) => (
                    <Link
                    href="/perfil"
                    className={`${
                        active ? 'bg-gray-600' : ''
                    } block px-4 py-2 text-sm`}
                    >
                    <User className="inline-block mr-2" size={16} />
                    Perfil
                    </Link>
                )}
                </Menu.Item>
                <Menu.Item>
                {({ active }) => (
                    <Link
                    href="/ajustes"
                    className={`${
                        active ? 'bg-gray-600' : ''
                    } block px-4 py-2 text-sm`}
                    >
                    <Settings className="inline-block mr-2" size={16} />
                    Ajustes
                    </Link>
                )}
                </Menu.Item>
            </Menu.Items> */}
            </Menu>
        </div>
        </nav>
    );
};

export default LeftSideNavbar;


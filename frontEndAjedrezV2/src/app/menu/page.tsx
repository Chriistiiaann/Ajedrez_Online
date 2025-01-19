import { Suspense } from 'react'
import UserProfile from '@/components/menu/user-profile'
import FriendsList from '@/components/menu/friends-list'
import InboxButton from '@/components/menu/inbox-button'
import GlobalStats from '@/components/menu/global-stats'

export default function MenuPage() {
    return (
        <div className="container mx-auto p-4 space-y-6">
        <UserProfile />
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="md:col-span-2">
            <Suspense fallback={<div>Loading friends...</div>}>
                <FriendsList />
            </Suspense>
            </div>
            <div className="space-y-6">
            <InboxButton />
            <Suspense fallback={<div>Loading stats...</div>}>
                <GlobalStats />
            </Suspense>
            </div>
        </div>
        </div>
    )
}


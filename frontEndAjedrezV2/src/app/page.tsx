import { cookies } from "next/headers";

import PlayNow from "@/components/playnow-cta";

import Spline from '@splinetool/react-spline/next';
import { redirect } from "next/navigation";

export default async function Home() {

  const cookieStore = await cookies();
  
  const userData = cookieStore.get("userData")?.value ?? null

  if (!userData) {

    return (
      <div className="flex flex-col justify-center">
        <div className="w-full h-screen bg-background">
        <Spline
          scene="https://prod.spline.design/arWUhO8PDdkyqwVA/scene.splinecode" 
        />
        </div>
        <div>
          <PlayNow />
        </div>
      </div>
    )
  } else {
    redirect("/menu")
  }
}

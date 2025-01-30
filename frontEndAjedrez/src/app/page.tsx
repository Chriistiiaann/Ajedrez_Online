import { cookies } from "next/headers";

import PlayNow from "@/components/playnow-cta";

import Spline from '@splinetool/react-spline/next';
import { redirect } from "next/navigation";

export default async function Home() {

  const cookieStore = await cookies();
  
  const userData = cookieStore.get("userData")?.value ?? null

  if (!userData) {
    return (
      <div className="flex flex-col justify-center relative">
          {/* Contenedor de la escena de Spline */}
          <div className="w-full h-screen bg-background relative">
              <Spline
                  scene="https://prod.spline.design/arWUhO8PDdkyqwVA/scene.splinecode" 
              />
              
              {/* Contenedor rosa con posición absoluta en la parte inferior derecha */}
              <div className="absolute bottom-4 right-4 m-4 p-4 bg-dark-bg w-[60] h-[20]">
                <span className="bg-dark-bg ">arreglar</span>
              </div>
          </div>

          {/* Componente PlayNow debajo de la escena */}
          <div className="mt-4">
              <PlayNow />
          </div>
      </div>
  );
  } else {
    redirect("/menu")
  }
}

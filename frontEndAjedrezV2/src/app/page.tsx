// import Hero from "@/components/hero";
import PlayNow from "@/components/playnow-cta";

import Spline from '@splinetool/react-spline/next';

export default function Home() {
  return (
    <div className="flex flex-col justify-center">
      {/* <Hero /> */}
      <div className="w-full h-screen bg-background">
      <Spline
        scene="https://prod.spline.design/arWUhO8PDdkyqwVA/scene.splinecode" 
      />
      </div>
      <div>
        <PlayNow />
      </div>
    </div>
  );
}

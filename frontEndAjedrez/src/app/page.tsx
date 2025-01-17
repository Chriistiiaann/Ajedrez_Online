import Hero from "@/components/hero";
import PlayNow from "@/components/playnow-cta";

export default function Home() {
  return (
    <div className="flex flex-col justify-center">
      <Hero />
      <div>
        <PlayNow />
      </div>
    </div>
  );
}

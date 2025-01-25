import { Button } from "./ui/button";
import Image from "next/image";
import Link from "next/link";

export default function Example() {
    return (
        <div className="bg-background">
            <div className="mx-auto max-w-7xl py-24 sm:px-6 sm:py-32 lg:px-8">
            <div className="relative isolate overflow-hidden bg-background px-6 pt-16 shadow-2xl sm:rounded-3xl sm:px-16 md:pt-24 lg:flex lg:gap-x-20 lg:px-24 lg:pt-0">
                <svg
                viewBox="0 0 1024 1024"
                aria-hidden="true"
                className="absolute left-1/2 top-1/2 -z-10 size-[64rem] -translate-y-1/2 [mask-image:radial-gradient(closest-side,white,transparent)] sm:left-full sm:-ml-80 lg:left-1/2 lg:ml-0 lg:-translate-x-1/2 lg:translate-y-0"
                >
                <circle r={512} cx={512} cy={512} fill="url(#759c1415-0410-454c-8f7c-9a820de03641)" fillOpacity="0.7" />
                <defs>
                    <radialGradient id="759c1415-0410-454c-8f7c-9a820de03641">
                    <stop stopColor="#7775D6" />
                    <stop offset={1} stopColor="#E935C1" />
                    </radialGradient>
                </defs>
                </svg>
                <div className="mx-auto max-w-md text-center lg:mx-0 lg:flex-auto lg:py-32 lg:text-left">
                <h2 className="text-balance text-3xl font-semibold tracking-tight text-white sm:text-4xl">
                    Únete YA a la <span className="text-primary">(próxima)</span> mejor comunidad de ajedrez.
                </h2>
                <p className="mt-6 text-pretty text-lg/8 text-gray-300">
                    ¿A que esperas? <span className="text-secondary">¡Vamos!</span>.
                </p>
                <div className="mt-10 flex items-center justify-center gap-x-6 lg:justify-start">
                    <Button className="text-white bg-accent text-md">Juega ahora</Button>
                    <Link href="/reglas" className="text-sm/6 font-semibold text-white">
                    Aprender a jugar <span aria-hidden="true">→</span>
                    </Link>
                </div>
                </div>
                <div className="relative mt-16 h-80 lg:mt-8">
                <Image
                    src="/chess-board-cta.png"
                    width={800}
                    height={247}
                    alt=""
                    className="absolute left-1/2 top-1/2 max-w-none -translate-x-40 -translate-y-40"
                    priority
                />
                </div>
            </div>
            </div>
        </div>
        )
    }

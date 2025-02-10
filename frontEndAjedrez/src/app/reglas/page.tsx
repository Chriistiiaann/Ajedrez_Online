import Image from 'next/image'

export default function ChessGuidePage() {
    return (
        <div className="min-h-screen bg-gradient-to-br from-background to-foreground text-gray-100">
        <div className="container mx-auto px-4 py-12 max-w-4xl">
            <h1 className="text-5xl font-bold mb-8 text-center bg-clip-text text-transparent bg-gradient-to-r from-secondary to-accent">
            Guía para Aprender a Jugar Ajedrez
            </h1>
            
            <p className="mb-8 text-lg leading-relaxed">
            El ajedrez es un juego estratégico que se juega entre dos jugadores en un tablero con 64 casillas alternadas entre colores claros y oscuros. A continuación, te proporcionamos una guía paso a paso para aprender a jugar.
            </p>

            {chessGuideContent.map((section, index) => (
            <section key={index} className="mb-12 bg-foreground border border-white bg-opacity-50 rounded-lg p-6 shadow-lg transition-all duration-300 hover:shadow-xl hover:bg-opacity-70">
                <h2 className="text-3xl font-semibold mb-4 text-primary">{section.title}</h2>
                {section.content}
            </section>
            ))}
        </div>
        </div>
    )
    }

    const chessGuideContent = [
    {
        title: "1. El Tablero y las Piezas",
        content: (
        <>
            <p className="mb-4">
            El tablero tiene 64 casillas, dispuestas en una cuadrícula de 8x8. Cada jugador comienza con 16 piezas:
            </p>
            <ul className="list-disc pl-6 mb-4 space-y-2">
            <li>1 Rey: La pieza más importante. Si el rey está en jaque mate, el juego termina.</li>
            <li>1 Dama (o Reina): La pieza más poderosa.</li>
            <li>2 Torres: Se mueven en línea recta, horizontal o verticalmente.</li>
            <li>2 Alfiles: Se mueven diagonalmente.</li>
            <li>2 Caballos: Se mueven en forma de 'L'.</li>
            <li>8 Peones: Se mueven hacia adelante, pero capturan en diagonal.</li>
            </ul>
            
            <h3 className="text-2xl font-semibold mb-2 text-secondary">Cómo Colocar las Piezas</h3>
            <ul className="list-disc pl-6 mb-4 space-y-2">
            <li>Coloca el tablero de forma que cada jugador tenga una casilla clara en la esquina derecha.</li>
            <li>La primera fila (más cercana al jugador) tiene las piezas principales en el siguiente orden:
                Torre, Caballo, Alfil, Reina, Rey, Alfil, Caballo, Torre.</li>
            <li>La segunda fila está ocupada por los peones.</li>
            </ul>
            
            <div className="relative w-full h-64 mb-4">
            <Image 
                src="/chess-board-cta.png"
                alt="Disposición inicial del tablero de ajedrez" 
                layout="fill"
                objectFit="contain"
                className="rounded-lg shadow-md"
            />
            </div>
        </>
        )
    },
    {
        title: "2. Objetivo del Juego",
        content: (
        <p>
            El objetivo es dar jaque mate al rey del oponente, es decir, colocarlo bajo amenaza directa de captura sin posibilidad de escape.
        </p>
        )
    },
    {
        title: "3. Movimiento de las Piezas",
        content: (
        <>
            <ul className="list-disc pl-6 mb-4 space-y-2">
            <li>Rey: Una casilla en cualquier dirección.</li>
            <li>Dama: Cualquier número de casillas en línea recta o diagonal.</li>
            <li>Torre: Cualquier número de casillas horizontal o verticalmente.</li>
            <li>Alfil: Cualquier número de casillas diagonalmente.</li>
            <li>Caballo: Dos casillas en una dirección y una en otra formando una 'L'. Puede saltar otras piezas.</li>
            <li>Peón: Una casilla hacia adelante (dos casillas en su primer movimiento). Captura diagonalmente.</li>
            </ul>
            
            <h3 className="text-2xl font-semibold mb-2 text-secondary">Movimientos Especiales</h3>
            <ul className="list-disc pl-6 mb-4 space-y-2">
            <li>Enroque: El rey y una torre se mueven simultáneamente bajo ciertas condiciones.</li>
            <li>Coronación: Un peón que llega al final del tablero puede convertirse en cualquier otra pieza (excepto en rey).</li>
            <li>Captura al Paso: Un peón puede capturar a otro si este último avanza dos casillas desde su posición inicial y termina junto al peón que captura.</li>
            </ul>
        </>
        )
    },
    {
        title: "4. Reglas Básicas",
        content: (
        <ul className="list-disc pl-6 mb-4 space-y-2">
            <li>Cada jugador tiene un turno alternado para mover una pieza.</li>
            <li>No puedes moverte a una casilla ocupada por una de tus propias piezas.</li>
            <li>Si mueves una pieza a una casilla ocupada por una pieza enemiga, la capturas.</li>
            <li>Si tu rey está bajo ataque, debes moverlo o protegerlo. Esto se llama jaque.</li>
            <li>Si no puedes proteger al rey, se llama jaque mate, y el juego termina.</li>
        </ul>
        )
    },
    {
        title: "5. Estrategias Básicas",
        content: (
        <ul className="list-disc pl-6 mb-4 space-y-2">
            <li>Controla el centro: Las casillas centrales (d4, d5, e4, e5) son clave para controlar el tablero.</li>
            <li>Desarrolla tus piezas: Mueve tus piezas principales (caballos y alfiles) antes que los peones.</li>
            <li>Protege a tu rey: Realiza el enroque temprano.</li>
            <li>Evita mover el mismo peón o pieza repetidamente en las primeras jugadas.</li>
        </ul>
        )
    },
    {
        title: "6. Finalización del Juego",
        content: (
        <>
            <p className="mb-4">El juego puede terminar de las siguientes maneras:</p>
            <ul className="list-disc pl-6 mb-4 space-y-2">
            <li>Jaque Mate: Uno de los reyes no puede escapar del ataque.</li>
            <li>Tablas: Puede ocurrir por varias razones:
                <ul className="list-circle pl-6 mt-2 space-y-1">
                <li>Ningún jugador puede dar jaque mate.</li>
                <li>Repetición de movimientos tres veces.</li>
                <li>Ambos jugadores acuerdan tablas.</li>
                <li>Un jugador no tiene movimientos legales y no está en jaque (ahogado).</li>
                </ul>
            </li>
            </ul>
        </>
        )
    }
];


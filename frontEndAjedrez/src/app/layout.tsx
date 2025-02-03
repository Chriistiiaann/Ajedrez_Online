import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";

import { WebsocketProvider } from "@/contexts/webContext-Context";
import Navbar from "@/components/navbar";
import { getAuth } from "@/actions/get-auth";
const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});



export default  async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const authData = await getAuth();
  const idToken = authData.decodedToken?.Id ?? null;

 

  return (
    <html suppressHydrationWarning lang="en" className="dark">
      <body suppressHydrationWarning className={`${geistSans.variable} ${geistMono.variable} antialiased`}>
        <div>

          {!idToken && <Navbar />}

          <main className="bg-gray-100 dark:bg-background text-gray-800 dark:text-gray-200 overflow-y-auto">
            <WebsocketProvider > 
              {children}
             </WebsocketProvider> 
          </main>
        </div>
      </body>
    </html>
  );
}

'use client'

import Link from 'next/link'

export default function NotFound() {
  return (
    <div className="relative flex min-h-screen w-full flex-col group/design-root overflow-x-hidden bg-[#0F172A] font-sans">
      <div className="layout-container flex h-screen grow flex-col items-center justify-center p-4 sm:p-6 lg:p-8">
        <div className="flex flex-col items-center gap-8 text-center max-w-lg">
          <div className="flex flex-col items-center gap-6">
            <div className="flex h-32 w-32 items-center justify-center text-slate-500">
              <span className="material-symbols-outlined !text-9xl">link_off</span>
            </div>
            <div className="flex flex-col items-center gap-4">
              <h1 className="text-7xl sm:text-8xl font-black tracking-tighter text-slate-300">404</h1>
              <div className="flex flex-col items-center gap-2">
                <p className="text-2xl font-bold leading-tight tracking-tight text-white">Page not found</p>
                <p className="text-slate-400 text-base font-normal leading-normal max-w-md">The page you're looking for doesn't exist or has been moved.</p>
              </div>
            </div>
          </div>
          <div className="flex flex-col items-center gap-4 w-full max-w-xs">
            <Link href="/" className="flex w-full cursor-pointer items-center justify-center overflow-hidden rounded-lg h-11 px-6 bg-[#29ABE2] text-white text-base font-bold leading-normal tracking-wide transition-colors hover:bg-[#29ABE2]/90">
              <span className="truncate">Go to Dashboard</span>
            </Link>
            <button onClick={() => window.history.back()} className="text-slate-400 text-sm font-normal leading-normal text-center underline cursor-pointer transition-colors hover:text-white">
              Go back
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

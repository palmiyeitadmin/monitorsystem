'use client'

import { useState } from 'react'
import Link from 'next/link'

export default function ForgotPasswordPage() {
    const [email, setEmail] = useState('')
    const [loading, setLoading] = useState(false)
    const [isSuccess, setIsSuccess] = useState(false)
    const [error, setError] = useState('')

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        setLoading(true)
        setError('')

        try {
            // Simulate API call
            await new Promise(resolve => setTimeout(resolve, 2000))
            setIsSuccess(true)
        } catch (err: any) {
            setError(err?.message || 'Failed to send reset link. Please try again.')
        } finally {
            setLoading(false)
        }
    }

    const handleResend = async () => {
        setIsSuccess(false)
        await handleSubmit(new Event('submit') as any)
    }

    return (
        <div className="min-h-screen bg-slate-900 flex items-center justify-center px-4">
            <div className="w-full max-w-md">
                {!isSuccess ? (
                    /* Reset Password Form State */
                    <div className="flex flex-col gap-6 rounded-xl bg-slate-800 p-8 shadow-2xl">
                        <div className="flex flex-col items-center text-center">
                            <Link
                                href="/login"
                                className="mb-6 flex items-center gap-2 text-sm font-medium text-slate-400 transition-colors hover:text-primary"
                            >
                                <span className="material-symbols-outlined text-base">arrow_back</span>
                                <span>Back to login</span>
                            </Link>
                            <h1 className="text-3xl font-bold text-white tracking-tight">
                                Reset Password
                            </h1>
                            <p className="mt-2 text-base text-slate-400">
                                Enter your email and we'll send you a reset link
                            </p>
                        </div>

                        <form onSubmit={handleSubmit} className="flex flex-col gap-6">
                            <div className="flex flex-col">
                                <label className="mb-2 text-sm font-medium text-slate-300" htmlFor="email">
                                    Email
                                </label>
                                <input
                                    className="form-input h-12 w-full rounded-lg border border-slate-600 bg-slate-700 p-3 text-base text-white placeholder:text-slate-400 focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/50"
                                    id="email"
                                    type="email"
                                    placeholder="e.g., admin@eramonitor.com"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    required
                                    disabled={loading}
                                />
                            </div>

                            {error && (
                                <div className="w-full p-3 rounded-lg bg-red-500/10 border border-red-500/30 text-red-200 text-sm">
                                    {error}
                                </div>
                            )}

                            <button
                                type="submit"
                                disabled={loading}
                                className="flex h-12 w-full cursor-pointer items-center justify-center overflow-hidden rounded-lg bg-primary px-5 text-base font-bold text-slate-900 transition-opacity hover:opacity-90 disabled:opacity-60"
                            >
                                <span>{loading ? 'Sending...' : 'Send Reset Link'}</span>
                            </button>
                        </form>
                    </div>
                ) : (
                    /* Success State */
                    <div className="flex flex-col gap-6 rounded-xl bg-slate-800 p-8 text-center shadow-2xl">
                        <div className="flex flex-col items-center">
                            <div className="mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-green-500/10">
                                <span className="material-symbols-outlined text-4xl text-green-400">
                                    check_circle
                                </span>
                            </div>
                            <h1 className="text-3xl font-bold tracking-tight text-white">
                                Check your email
                            </h1>
                            <p className="mt-2 text-base text-slate-400">
                                We've sent a password reset link to your email address.
                            </p>
                        </div>

                        <div className="mt-2">
                            <button
                                onClick={handleResend}
                                disabled={loading}
                                className="text-sm font-medium text-slate-400 transition-colors hover:text-primary disabled:opacity-60"
                            >
                                Resend email
                            </button>
                        </div>
                    </div>
                )}
            </div>
        </div>
    )
}
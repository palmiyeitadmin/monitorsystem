'use client'

import { useState } from 'react'
import { useAuth } from '@/lib/auth-context'

export default function LoginPage() {
    const { login } = useAuth()
    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const [error, setError] = useState('')
    const [loading, setLoading] = useState(false)
    const [showPassword, setShowPassword] = useState(false)
    const [rememberMe, setRememberMe] = useState(false)

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        setError('')
        setLoading(true)

        try {
            await login(email, password)
        } catch (err: any) {
            setError(err?.message || 'Your password is incorrect. Please try again.')
            setLoading(false)
        }
    }

    return (
        <div className="min-h-screen bg-slate-900 flex items-center justify-center px-4">
            <div className="flex w-full max-w-md flex-col items-center gap-8 rounded-xl bg-slate-800 p-8 shadow-2xl">
                {/* Logo */}
                <div className="flex w-full flex-col items-center justify-center gap-2">
                    <div className="h-10 w-10 rounded-lg bg-primary flex items-center justify-center text-white">
                        <span className="material-symbols-outlined text-2xl">monitor_heart</span>
                    </div>
                    <h1 className="text-xl font-bold text-slate-100">ERA Monitor</h1>
                </div>

                {/* Headline */}
                <h2 className="text-slate-100 tracking-tight text-3xl font-bold leading-tight text-center">
                    Welcome back
                </h2>

                {/* Form */}
                <div className="flex w-full flex-col items-stretch gap-4">
                    {/* Email Field */}
                    <div className="flex w-full flex-col">
                        <label className="flex flex-col flex-1">
                            <p className="text-slate-400 text-sm font-medium leading-normal pb-2">
                                Email
                            </p>
                            <input
                                className="form-input flex w-full min-w-0 flex-1 resize-none overflow-hidden rounded-lg text-slate-100 focus:outline-0 border border-slate-600 bg-slate-700 focus:border-primary focus:ring-2 focus:ring-primary/40 h-12 placeholder:text-slate-400 px-4 text-base font-normal leading-normal"
                                placeholder="Enter your email"
                                type="email"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                required
                                disabled={loading}
                            />
                        </label>
                    </div>

                    {/* Password Field */}
                    <div className="flex w-full flex-col">
                        <label className="flex flex-col flex-1">
                            <p className="text-slate-400 text-sm font-medium leading-normal pb-2">
                                Password
                            </p>
                            <div className="flex w-full flex-1 items-stretch rounded-lg">
                                <input
                                    className={`form-input flex w-full min-w-0 flex-1 resize-none overflow-hidden rounded-lg text-slate-100 focus:outline-0 border ${error ? 'border-red-500 focus:border-red-500 focus:ring-2 focus:ring-red-500/40' : 'border-slate-600 focus:border-primary focus:ring-2 focus:ring-primary/40'
                                        } bg-slate-700 h-12 placeholder:text-slate-400 p-4 rounded-r-none border-r-0 text-base font-normal leading-normal`}
                                    placeholder="Enter your password"
                                    type={showPassword ? 'text' : 'password'}
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    required
                                    disabled={loading}
                                />
                                <div className="text-slate-400 flex border border-slate-600 bg-slate-700 items-center justify-center pr-4 rounded-r-lg border-l-0">
                                    <button
                                        type="button"
                                        className="cursor-pointer hover:text-slate-300 transition-colors"
                                        onClick={() => setShowPassword(!showPassword)}
                                    >
                                        <span className="material-symbols-outlined">
                                            {showPassword ? 'visibility' : 'visibility_off'}
                                        </span>
                                    </button>
                                </div>
                            </div>
                        </label>
                        {error && (
                            <p className="text-red-400 text-sm pt-1.5">{error}</p>
                        )}
                    </div>

                    {/* Remember me & Forgot Password */}
                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                            <input
                                className="form-checkbox h-4 w-4 rounded border-slate-600 bg-slate-700 text-primary checked:bg-primary focus:ring-2 focus:ring-offset-0 focus:ring-offset-slate-800 focus:ring-primary/40"
                                id="remember-me"
                                type="checkbox"
                                checked={rememberMe}
                                onChange={(e) => setRememberMe(e.target.checked)}
                                disabled={loading}
                            />
                            <label className="text-slate-400 text-sm font-normal leading-normal" htmlFor="remember-me">
                                Remember me
                            </label>
                        </div>
                        <a className="text-sm font-medium text-primary hover:text-primary/80 transition-colors" href="#">
                            Forgot password?
                        </a>
                    </div>

                    {/* Sign In Button */}
                    <button
                        type="submit"
                        onClick={handleSubmit}
                        disabled={loading}
                        className="flex h-12 w-full items-center justify-center rounded-lg bg-primary px-6 text-base font-bold text-white shadow-sm transition-colors hover:bg-primary/90 focus:outline-none focus:ring-2 focus:ring-primary/50 focus:ring-offset-2 focus:ring-offset-slate-800 disabled:opacity-60"
                    >
                        {loading ? 'Signing in...' : 'Sign In'}
                    </button>
                </div>

                {/* Divider */}
                <div className="flex w-full items-center gap-4">
                    <hr className="w-full border-t border-slate-600" />
                    <p className="shrink-0 text-sm text-slate-400">or</p>
                    <hr className="w-full border-t border-slate-600" />
                </div>

                {/* SSO Button */}
                <button className="flex h-12 w-full items-center justify-center gap-3 rounded-lg border border-slate-600 bg-transparent px-6 text-base font-medium text-slate-100 shadow-sm transition-colors hover:bg-slate-600/50 focus:outline-none focus:ring-2 focus:ring-primary/50 focus:ring-offset-2 focus:ring-offset-slate-800">
                    <img
                        alt="Microsoft logo"
                        className="h-6 w-6"
                        src="https://lh3.googleusercontent.com/aida-public/AB6AXuC8oE2jA60WThMDg0UcUgV55gI9CcBEZqzN4e4HVJE_Z2NdWKSF6B2zmREvTCbXHU7rbOHt8rz3W70_FSGBBHPwvb_JGIj-0o7NvrsdeX8-VEIakX0ouJb7fd3HrdQlOuU14gIIbXfdgrwUZjq7gk1dHR2lan1hl16hDrflYUYPRosSp0dMA846jQhZcAcO4WoX2ajOzMjAhtUfAcG5_jy3q7jxzvXDww2SHmmT0bbHrHH2tElGABBL4yAtNL71S9V5MRyR2aOdvdY"
                    />
                    Sign in with Microsoft
                </button>

                {/* Footer */}
                <p className="text-slate-400 text-sm text-center">
                    Don't have an account?{' '}
                    <a className="font-medium text-primary hover:text-primary/80 transition-colors" href="#">
                        Contact your administrator
                    </a>
                </p>
            </div>
        </div>
    )
}

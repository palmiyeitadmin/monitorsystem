'use client';

import React from 'react';

export default function SettingsPage() {
    return (
        <div className="max-w-4xl mx-auto space-y-8">
            {/* Page Heading */}
            <div>
                <h1 className="text-4xl font-black leading-tight tracking-tight text-white">Account Settings</h1>
                <p className="text-slate-400 text-base mt-2">Manage your profile, security, and notification preferences.</p>
            </div>

            {/* Profile Card */}
            <div className="bg-[#1e293b] border border-slate-700 rounded-xl">
                <div className="p-6 border-b border-slate-700">
                    <h2 className="text-lg font-bold text-white">Profile</h2>
                    <p className="text-slate-400 text-sm mt-1">Update your photo and personal details here.</p>
                </div>
                <div className="p-6 space-y-6">
                    <div className="flex items-center gap-6">
                        <div className="bg-center bg-no-repeat aspect-square bg-cover rounded-full size-20 bg-slate-700" style={{ backgroundImage: 'url("https://lh3.googleusercontent.com/aida-public/AB6AXuBtH415WyxnBbrJ-R-XN9xNND10h9YWVGh3uf92axTT3By5xUFWBVemsGeOpovL7cPBI733-nAa1jr2JgBTybQHXY5J9x0cvERUF8zEdQP2CZUVWCHLs0UKvfXYcsVXmVSknR-dx6Zv5v8viB6shmo-hOPZZUM-3fSJl1l5zW7FMrONIZKPrX1tRqt141sR5EpQxFiLKNf4LmZCTG7vCJXSAdzdTY9xrh25-e1r8gb6cpJdl5417C-bFCOa3bLCVpdj9UkNdXbmO_k")' }}></div>
                        <button className="px-4 py-2 text-sm font-semibold text-white bg-[#29abe2] rounded-lg hover:bg-[#29abe2]/90 transition-colors">Upload new photo</button>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <label className="flex flex-col">
                            <p className="text-sm font-medium pb-2 text-white">Full Name</p>
                            <input className="w-full rounded-lg border-slate-700 bg-[#283447] focus:border-[#29abe2] focus:ring-[#29abe2] text-sm text-white" type="text" defaultValue="John Doe" />
                        </label>
                        <label className="flex flex-col">
                            <p className="text-sm font-medium pb-2 text-white">Email Address</p>
                            <input className="w-full rounded-lg border-slate-700 bg-[#283447]/50 text-slate-400 text-sm" disabled readOnly type="email" defaultValue="john.doe@example.com" />
                        </label>
                        <label className="flex flex-col">
                            <p className="text-sm font-medium pb-2 text-white">Phone Number</p>
                            <input className="w-full rounded-lg border-slate-700 bg-[#283447] focus:border-[#29abe2] focus:ring-[#29abe2] text-sm text-white" type="tel" defaultValue="+1 (123) 456-7890" />
                        </label>
                        <label className="flex flex-col">
                            <p className="text-sm font-medium pb-2 text-white">Timezone</p>
                            <select className="w-full rounded-lg border-slate-700 bg-[#283447] focus:border-[#29abe2] focus:ring-[#29abe2] text-sm text-white">
                                <option>Pacific Standard Time (PST)</option>
                                <option selected>Eastern Standard Time (EST)</option>
                                <option>Greenwich Mean Time (GMT)</option>
                            </select>
                        </label>
                    </div>
                </div>
                <div className="p-6 bg-black/20 border-t border-slate-700 rounded-b-xl flex justify-end">
                    <button className="px-5 py-2 text-sm font-semibold text-white bg-[#29abe2] rounded-lg hover:bg-[#29abe2]/90 transition-colors disabled:bg-gray-600 disabled:cursor-not-allowed">Save Profile</button>
                </div>
            </div>

            {/* Security Card */}
            <div className="bg-[#1e293b] border border-slate-700 rounded-xl">
                <div className="p-6 border-b border-slate-700">
                    <h2 className="text-lg font-bold text-white">Security</h2>
                    <p className="text-slate-400 text-sm mt-1">Manage your password and two-factor authentication.</p>
                </div>
                <div className="p-6 space-y-8">
                    <div>
                        <h3 className="font-semibold mb-4 text-white">Change Password</h3>
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                            <label className="flex flex-col">
                                <p className="text-sm font-medium pb-2 text-white">Current Password</p>
                                <input className="w-full rounded-lg border-slate-700 bg-[#283447] focus:border-[#29abe2] focus:ring-[#29abe2] text-sm text-white" type="password" />
                            </label>
                            <label className="flex flex-col">
                                <p className="text-sm font-medium pb-2 text-white">New Password</p>
                                <input className="w-full rounded-lg border-slate-700 bg-[#283447] focus:border-[#29abe2] focus:ring-[#29abe2] text-sm text-white" type="password" />
                            </label>
                            <label className="flex flex-col">
                                <p className="text-sm font-medium pb-2 text-white">Confirm New Password</p>
                                <input className="w-full rounded-lg border-slate-700 bg-[#283447] focus:border-[#29abe2] focus:ring-[#29abe2] text-sm text-white" type="password" />
                            </label>
                        </div>
                    </div>
                    <div className="border-t border-slate-700 pt-8">
                        <h3 className="font-semibold mb-4 text-white">Two-Factor Authentication (2FA)</h3>
                        <div className="flex items-start justify-between p-4 rounded-lg bg-black/20">
                            <div>
                                <p className="font-medium text-white">Enable 2FA</p>
                                <p className="text-slate-400 text-sm mt-1">Protect your account with an extra layer of security.</p>
                            </div>
                            <label className="relative inline-flex items-center cursor-pointer">
                                <input defaultChecked className="sr-only peer" type="checkbox" />
                                <div className="w-11 h-6 bg-gray-700 peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-[#29abe2]/50 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-[#29abe2]"></div>
                            </label>
                        </div>
                        <div className="mt-4 flex flex-col md:flex-row items-center gap-6 p-4 border border-slate-700 rounded-lg">
                            <img className="w-32 h-32 rounded-lg" alt="QR Code for 2FA setup" src="https://lh3.googleusercontent.com/aida-public/AB6AXuAlqOhLNJ7RU9Y2kp7qF8MGd7f5Ch2GSgnsT91S-53gPy6BMPZzQr6DOxio96_lTUqR0XBxifGgd8HiUhR5ZtvV-Hy7WNkkt2KyzlK-6bJJ9C1npOS0j_dFWTGBAtFMOq-TxjOBXQ7MYRRZvSHA4QkFsCw7QFuV4XBUGJNqEyOqNRhUKmkf_dEal2b08v37SEe3-I2uGqMTkCfEzbDwtdX2nv13XIEsYRvecgNyQ0LQEIpHNYINBgreVvWEsXxhevtNYOoHr-xesuM" />
                            <div>
                                <p className="font-semibold text-white">Scan QR Code</p>
                                <p className="text-slate-400 text-sm mt-1">Use an authenticator app like Google Authenticator or Authy to scan this code and complete the setup.</p>
                            </div>
                        </div>
                    </div>
                </div>
                <div className="p-6 bg-black/20 border-t border-slate-700 rounded-b-xl flex justify-end">
                    <button className="px-5 py-2 text-sm font-semibold text-white bg-[#29abe2] rounded-lg hover:bg-[#29abe2]/90 transition-colors">Update Security</button>
                </div>
            </div>

            {/* Notification Preferences Card */}
            <div className="bg-[#1e293b] border border-slate-700 rounded-xl">
                <div className="p-6 border-b border-slate-700">
                    <h2 className="text-lg font-bold text-white">Notification Preferences</h2>
                    <p className="text-slate-400 text-sm mt-1">Choose how you want to be notified about alerts.</p>
                </div>
                <div className="p-6 divide-y divide-slate-700">
                    <div className="py-4 flex items-center justify-between">
                        <div><p className="font-medium text-white">Server Down</p></div>
                        <div className="flex items-center gap-4">
                            <label className="flex items-center gap-2 text-sm text-white"><input defaultChecked className="form-checkbox rounded bg-[#283447] border-slate-700 text-[#29abe2] focus:ring-[#29abe2]/50" type="checkbox" /> Email</label>
                            <label className="flex items-center gap-2 text-sm text-white"><input defaultChecked className="form-checkbox rounded bg-[#283447] border-slate-700 text-[#29abe2] focus:ring-[#29abe2]/50" type="checkbox" /> SMS</label>
                            <label className="flex items-center gap-2 text-sm text-white"><input className="form-checkbox rounded bg-[#283447] border-slate-700 text-[#29abe2] focus:ring-[#29abe2]/50" type="checkbox" /> Telegram</label>
                        </div>
                    </div>
                    <div className="py-4 flex items-center justify-between">
                        <div><p className="font-medium text-white">High CPU Usage</p></div>
                        <div className="flex items-center gap-4">
                            <label className="flex items-center gap-2 text-sm text-white"><input defaultChecked className="form-checkbox rounded bg-[#283447] border-slate-700 text-[#29abe2] focus:ring-[#29abe2]/50" type="checkbox" /> Email</label>
                            <label className="flex items-center gap-2 text-sm text-white"><input className="form-checkbox rounded bg-[#283447] border-slate-700 text-[#29abe2] focus:ring-[#29abe2]/50" type="checkbox" /> SMS</label>
                            <label className="flex items-center gap-2 text-sm text-white"><input className="form-checkbox rounded bg-[#283447] border-slate-700 text-[#29abe2] focus:ring-[#29abe2]/50" type="checkbox" /> Telegram</label>
                        </div>
                    </div>
                    <div className="py-4 flex items-center justify-between">
                        <div><p className="font-medium text-white">Low Disk Space</p></div>
                        <div className="flex items-center gap-4">
                            <label className="flex items-center gap-2 text-sm text-white"><input className="form-checkbox rounded bg-[#283447] border-slate-700 text-[#29abe2] focus:ring-[#29abe2]/50" type="checkbox" /> Email</label>
                            <label className="flex items-center gap-2 text-sm text-white"><input defaultChecked className="form-checkbox rounded bg-[#283447] border-slate-700 text-[#29abe2] focus:ring-[#29abe2]/50" type="checkbox" /> SMS</label>
                            <label className="flex items-center gap-2 text-sm text-white"><input defaultChecked className="form-checkbox rounded bg-[#283447] border-slate-700 text-[#29abe2] focus:ring-[#29abe2]/50" type="checkbox" /> Telegram</label>
                        </div>
                    </div>
                </div>
                <div className="p-6 bg-black/20 border-t border-slate-700 rounded-b-xl flex justify-end">
                    <button className="px-5 py-2 text-sm font-semibold text-white bg-[#29abe2] rounded-lg hover:bg-[#29abe2]/90 transition-colors">Save Preferences</button>
                </div>
            </div>

            {/* Sessions Card */}
            <div className="bg-[#1e293b] border border-slate-700 rounded-xl">
                <div className="p-6 border-b border-slate-700">
                    <h2 className="text-lg font-bold text-white">Active Sessions</h2>
                    <p className="text-slate-400 text-sm mt-1">This is a list of devices that have logged into your account.</p>
                </div>
                <div className="overflow-x-auto">
                    <table className="w-full text-sm text-left">
                        <thead className="text-xs uppercase bg-black/20 text-slate-400">
                            <tr>
                                <th className="px-6 py-3" scope="col">Device</th>
                                <th className="px-6 py-3" scope="col">Location</th>
                                <th className="px-6 py-3" scope="col">Last Active</th>
                                <th className="px-6 py-3" scope="col"><span className="sr-only">Action</span></th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-700">
                            <tr className="hover:bg-black/20">
                                <td className="px-6 py-4 font-medium text-white">Chrome on macOS <span className="text-green-500 ml-2">(Current)</span></td>
                                <td className="px-6 py-4 text-slate-400">New York, US</td>
                                <td className="px-6 py-4 text-slate-400">1 min ago</td>
                                <td className="px-6 py-4 text-right"></td>
                            </tr>
                            <tr className="hover:bg-black/20">
                                <td className="px-6 py-4 font-medium text-white">Firefox on Windows</td>
                                <td className="px-6 py-4 text-slate-400">London, UK</td>
                                <td className="px-6 py-4 text-slate-400">2 days ago</td>
                                <td className="px-6 py-4 text-right"><button className="font-semibold text-red-500 hover:text-red-700">Revoke</button></td>
                            </tr>
                            <tr className="hover:bg-black/20">
                                <td className="px-6 py-4 font-medium text-white">Safari on iOS</td>
                                <td className="px-6 py-4 text-slate-400">Tokyo, JP</td>
                                <td className="px-6 py-4 text-slate-400">1 week ago</td>
                                <td className="px-6 py-4 text-right"><button className="font-semibold text-red-500 hover:text-red-700">Revoke</button></td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Danger Zone Card */}
            <div className="bg-[#1e293b] border border-red-500/50 rounded-xl">
                <div className="p-6 border-b border-red-500/50">
                    <h2 className="text-lg font-bold text-red-500">Danger Zone</h2>
                </div>
                <div className="p-6 flex items-center justify-between">
                    <div>
                        <p className="font-semibold text-white">Delete your account</p>
                        <p className="text-slate-400 text-sm mt-1">Once you delete your account, there is no going back. Please be certain.</p>
                    </div>
                    <button className="px-5 py-2 text-sm font-semibold text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors">Delete my account</button>
                </div>
            </div>
        </div>
    );
}

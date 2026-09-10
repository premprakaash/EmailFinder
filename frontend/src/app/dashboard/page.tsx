'use client';

import { useEffect, useState } from 'react';
import DashboardLayout from '@/components/DashboardLayout';
import AuthGuard from '@/components/AuthGuard';
import { api } from '@/lib/api';

interface DashboardData {
  stats: {
    creditBalance: number;
    creditsConsumed: number;
    contactsDiscovered: number;
    emailsVerified: number;
    validEmails: number;
    invalidEmails: number;
    riskyEmails: number;
    exportsUsed: number;
    apiRequests: number;
    currentPlan: string;
  };
  recentSearches: { id: string; domain: string; status: string; createdAt: string }[];
  recentVerifications: { id: string; email: string; status: string | null; createdAt: string }[];
}

export default function DashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null);

  useEffect(() => {
    api<DashboardData>('/api/dashboard').then(setData).catch(console.error);
  }, []);

  const stats = data?.stats;
  const cards = stats ? [
    { label: 'Credits', value: stats.creditBalance.toLocaleString(), sub: `${stats.creditsConsumed.toLocaleString()} used` },
    { label: 'Contacts', value: stats.contactsDiscovered.toLocaleString(), sub: 'discovered' },
    { label: 'Verified', value: stats.emailsVerified.toLocaleString(), sub: `${stats.validEmails} valid` },
    { label: 'Exports', value: stats.exportsUsed.toLocaleString(), sub: 'total exports' },
    { label: 'API Usage', value: stats.apiRequests.toLocaleString(), sub: 'requests' },
    { label: 'Plan', value: stats.currentPlan, sub: `${stats.riskyEmails} risky / ${stats.invalidEmails} invalid` },
  ] : [];

  return (
    <AuthGuard>
      <DashboardLayout>
        <h2 className="text-2xl font-bold text-slate-900 mb-6">Dashboard</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-8">
          {cards.map(c => (
            <div key={c.label} className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
              <p className="text-sm text-slate-500">{c.label}</p>
              <p className="text-2xl font-bold text-slate-900 mt-1">{c.value}</p>
              <p className="text-xs text-slate-400 mt-1">{c.sub}</p>
            </div>
          ))}
        </div>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white rounded-xl border p-5">
            <h3 className="font-semibold mb-3">Recent Searches</h3>
            {data?.recentSearches?.length ? data.recentSearches.map(s => (
              <div key={s.id} className="flex justify-between py-2 border-b text-sm">
                <span>{s.domain}</span><span className="text-slate-500">{s.status}</span>
              </div>
            )) : <p className="text-slate-400 text-sm">No recent searches</p>}
          </div>
          <div className="bg-white rounded-xl border p-5">
            <h3 className="font-semibold mb-3">Recent Verifications</h3>
            {data?.recentVerifications?.length ? data.recentVerifications.map(v => (
              <div key={v.id} className="flex justify-between py-2 border-b text-sm">
                <span>{v.email}</span><span className="text-slate-500">{v.status}</span>
              </div>
            )) : <p className="text-slate-400 text-sm">No recent verifications</p>}
          </div>
        </div>
      </DashboardLayout>
    </AuthGuard>
  );
}

'use client';

import DashboardLayout from '@/components/DashboardLayout';
import AuthGuard from '@/components/AuthGuard';
import { getUser } from '@/lib/api';

export default function UsagePage() {
  const user = getUser();
  return (
    <AuthGuard>
      <DashboardLayout>
        <h2 className="text-2xl font-bold mb-4">Usage</h2>
        <div className="bg-white rounded-xl border p-6">
          <p className="text-slate-600">Current credit balance: <strong>{user?.creditBalance?.toLocaleString()}</strong></p>
          <p className="text-sm text-slate-400 mt-2">Detailed usage analytics available via the dashboard API.</p>
        </div>
      </DashboardLayout>
    </AuthGuard>
  );
}

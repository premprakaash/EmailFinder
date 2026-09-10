'use client';

import DashboardLayout from '@/components/DashboardLayout';
import AuthGuard from '@/components/AuthGuard';

export default function BillingPage() {
  return (
    <AuthGuard>
      <DashboardLayout>
        <h2 className="text-2xl font-bold mb-4">Billing</h2>
        <div className="bg-white rounded-xl border p-6 max-w-lg">
          <h3 className="font-semibold">Starter Plan</h3>
          <p className="text-slate-500 mt-1">1,000 credits/month included</p>
          <p className="text-2xl font-bold mt-4">$0<span className="text-sm font-normal text-slate-400">/mo</span></p>
          <p className="text-sm text-slate-400 mt-4">Upgrade to Professional for 50,000 credits/month at $49/mo.</p>
        </div>
      </DashboardLayout>
    </AuthGuard>
  );
}

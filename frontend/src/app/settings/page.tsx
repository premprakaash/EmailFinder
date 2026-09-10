'use client';

import DashboardLayout from '@/components/DashboardLayout';
import AuthGuard from '@/components/AuthGuard';
import { getUser } from '@/lib/api';

export default function SettingsPage() {
  const user = getUser();
  return (
    <AuthGuard>
      <DashboardLayout>
        <h2 className="text-2xl font-bold mb-4">Settings</h2>
        <div className="bg-white rounded-xl border p-6 max-w-lg space-y-4">
          <div><label className="text-sm text-slate-500">Email</label><p>{user?.email}</p></div>
          <div><label className="text-sm text-slate-500">Name</label><p>{user?.firstName} {user?.lastName}</p></div>
          <div><label className="text-sm text-slate-500">Role</label><p>{user?.role}</p></div>
        </div>
      </DashboardLayout>
    </AuthGuard>
  );
}

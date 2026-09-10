'use client';

import { useState } from 'react';
import DashboardLayout from '@/components/DashboardLayout';
import AuthGuard from '@/components/AuthGuard';
import { api } from '@/lib/api';

export default function ApiKeysPage() {
  const [name, setName] = useState('My API Key');
  const [newKey, setNewKey] = useState('');

  const create = async () => {
    const res = await api<{ key: string }>('/api/api-keys', { method: 'POST', body: JSON.stringify(name) });
    setNewKey(res.key);
  };

  return (
    <AuthGuard>
      <DashboardLayout>
        <h2 className="text-2xl font-bold mb-6">API Keys</h2>
        <div className="bg-white rounded-xl border p-6 max-w-xl space-y-4">
          <input value={name} onChange={e => setName(e.target.value)} className="w-full px-3 py-2 border rounded-lg" placeholder="Key name" />
          <button onClick={create} className="px-6 py-2 bg-indigo-600 text-white rounded-lg">Create API Key</button>
          {newKey && (
            <div className="p-4 bg-green-50 rounded-lg">
              <p className="text-sm text-green-800 font-medium">Your new API key (save it now):</p>
              <code className="block mt-2 text-xs break-all">{newKey}</code>
            </div>
          )}
          <div className="text-sm text-slate-500 mt-4">
            <p className="font-medium">Public API Endpoints:</p>
            <ul className="list-disc ml-4 mt-2 space-y-1">
              <li>POST /api/v1/finder</li>
              <li>POST /api/v1/verifier</li>
              <li>POST /api/v1/domain-search</li>
              <li>POST /api/v1/bulk</li>
              <li>GET /api/v1/credits</li>
            </ul>
          </div>
        </div>
      </DashboardLayout>
    </AuthGuard>
  );
}

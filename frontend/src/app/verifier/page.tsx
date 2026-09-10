'use client';

import { FormEvent, useState } from 'react';
import DashboardLayout from '@/components/DashboardLayout';
import AuthGuard from '@/components/AuthGuard';
import { api } from '@/lib/api';

interface VerifyResult {
  email: string; status: string; score: number; mx: boolean; smtp: boolean;
  catchAll: boolean; disposable: boolean; roleBased: boolean; details?: string;
}

export default function VerifierPage() {
  const [email, setEmail] = useState('john.smith@example.com');
  const [result, setResult] = useState<VerifyResult | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      setResult(await api<VerifyResult>('/api/verifier', { method: 'POST', body: JSON.stringify({ email }) }));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Verification failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthGuard>
      <DashboardLayout>
        <h2 className="text-2xl font-bold mb-6">Email Verifier</h2>
        <form onSubmit={submit} className="bg-white rounded-xl border p-6 max-w-xl space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">Email</label>
            <input type="email" value={email} onChange={e => setEmail(e.target.value)} required
              className="w-full px-3 py-2 border rounded-lg outline-none focus:ring-2 focus:ring-indigo-500" />
          </div>
          {error && <p className="text-red-600 text-sm">{error}</p>}
          <button type="submit" disabled={loading} className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50">
            {loading ? 'Verifying...' : 'Verify'}
          </button>
        </form>

        {result && (
          <div className="mt-8 bg-white rounded-xl border p-6 max-w-xl">
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div><span className="text-slate-500">Status</span><p className="font-semibold text-lg">{result.status}</p></div>
              <div><span className="text-slate-500">Score</span><p className="font-semibold text-lg">{result.score}%</p></div>
              <div><span className="text-slate-500">MX</span><p>{result.mx ? 'Yes' : 'No'}</p></div>
              <div><span className="text-slate-500">SMTP</span><p>{result.smtp ? 'Yes' : 'No'}</p></div>
              <div><span className="text-slate-500">Catch-All</span><p>{result.catchAll ? 'Yes' : 'No'}</p></div>
              <div><span className="text-slate-500">Disposable</span><p>{result.disposable ? 'Yes' : 'No'}</p></div>
              <div><span className="text-slate-500">Role-Based</span><p>{result.roleBased ? 'Yes' : 'No'}</p></div>
            </div>
            {result.details && <p className="mt-4 text-sm text-slate-500">{result.details}</p>}
          </div>
        )}
      </DashboardLayout>
    </AuthGuard>
  );
}

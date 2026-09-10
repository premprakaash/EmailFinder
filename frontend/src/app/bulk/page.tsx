'use client';

import { FormEvent, useState } from 'react';
import DashboardLayout from '@/components/DashboardLayout';
import AuthGuard from '@/components/AuthGuard';
import { api, apiForm } from '@/lib/api';

interface BulkJob {
  id: string; fileName: string; status: string; totalRows: number; processedRows: number;
  foundCount: number; verifiedCount: number; invalidCount: number; riskyCount: number;
  failedCount: number; remaining: number;
}

export default function BulkPage() {
  const [job, setJob] = useState<BulkJob | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const upload = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const file = (e.currentTarget.elements.namedItem('file') as HTMLInputElement).files?.[0];
    if (!file) return;
    setLoading(true);
    setError('');
    try {
      const fd = new FormData();
      fd.append('file', file);
      const res = await apiForm<{ jobId: string }>('/api/bulk', fd);
      pollJob(res.jobId);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Upload failed');
      setLoading(false);
    }
  };

  const pollJob = (id: string) => {
    const interval = setInterval(async () => {
      try {
        const j = await api<BulkJob>(`/api/bulk/${id}`);
        setJob(j);
        if (j.status === 'Completed' || j.status === 'Failed' || j.status === 'Cancelled') {
          clearInterval(interval);
          setLoading(false);
        }
      } catch { clearInterval(interval); setLoading(false); }
    }, 2000);
  };

  return (
    <AuthGuard>
      <DashboardLayout>
        <h2 className="text-2xl font-bold mb-6">Bulk Search</h2>
        <form onSubmit={upload} className="bg-white rounded-xl border p-6 max-w-xl space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">Upload CSV</label>
            <input type="file" name="file" accept=".csv" required className="w-full text-sm" />
            <p className="text-xs text-slate-500 mt-1">Columns: first_name, last_name, domain</p>
          </div>
          {error && <p className="text-red-600 text-sm">{error}</p>}
          <button type="submit" disabled={loading} className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50">
            {loading ? 'Processing...' : 'Start Processing'}
          </button>
        </form>

        {job && (
          <div className="mt-8 grid grid-cols-2 md:grid-cols-4 gap-4">
            {[
              ['Total', job.totalRows], ['Processed', job.processedRows], ['Found', job.foundCount],
              ['Verified', job.verifiedCount], ['Invalid', job.invalidCount], ['Risky', job.riskyCount],
              ['Failed', job.failedCount], ['Remaining', job.remaining],
            ].map(([label, val]) => (
              <div key={label as string} className="bg-white rounded-xl border p-4">
                <p className="text-sm text-slate-500">{label}</p>
                <p className="text-xl font-bold">{val}</p>
              </div>
            ))}
          </div>
        )}
      </DashboardLayout>
    </AuthGuard>
  );
}

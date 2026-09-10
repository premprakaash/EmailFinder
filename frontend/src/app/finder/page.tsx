'use client';

import { FormEvent, useState } from 'react';
import DashboardLayout from '@/components/DashboardLayout';
import AuthGuard from '@/components/AuthGuard';
import { api } from '@/lib/api';

interface Candidate { email: string; confidence: number; status: string; pattern: string }
interface FinderResult { firstName: string; lastName: string; domain: string; candidates: Candidate[]; bestMatch: Candidate | null }

export default function FinderPage() {
  const [form, setForm] = useState({ firstName: 'John', lastName: 'Smith', domain: 'example.com' });
  const [result, setResult] = useState<FinderResult | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      setResult(await api<FinderResult>('/api/finder', { method: 'POST', body: JSON.stringify(form) }));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Finder failed');
    } finally {
      setLoading(false);
    }
  };

  const saveContact = async (c: Candidate) => {
    await api('/api/contacts', {
      method: 'POST',
      body: JSON.stringify({
        firstName: form.firstName, lastName: form.lastName, email: c.email,
        domain: form.domain, status: c.status, confidenceScore: c.confidence, source: 'finder'
      }),
    });
    alert('Contact saved!');
  };

  return (
    <AuthGuard>
      <DashboardLayout>
        <h2 className="text-2xl font-bold mb-6">Email Finder</h2>
        <form onSubmit={submit} className="bg-white rounded-xl border p-6 max-w-xl space-y-4">
          {(['firstName', 'lastName', 'domain'] as const).map(f => (
            <div key={f}>
              <label className="block text-sm font-medium mb-1 capitalize">{f === 'domain' ? 'Company Domain' : f.replace(/([A-Z])/g, ' $1')}</label>
              <input value={form[f]} onChange={e => setForm({ ...form, [f]: e.target.value })} required
                className="w-full px-3 py-2 border rounded-lg outline-none focus:ring-2 focus:ring-indigo-500" />
            </div>
          ))}
          {error && <p className="text-red-600 text-sm">{error}</p>}
          <button type="submit" disabled={loading} className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50">
            {loading ? 'Finding...' : 'Find Email'}
          </button>
        </form>

        {result && (
          <div className="mt-8 bg-white rounded-xl border overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-slate-50">
                <tr>
                  <th className="text-left p-3">Email</th>
                  <th className="text-left p-3">Status</th>
                  <th className="text-left p-3">Confidence</th>
                  <th className="text-left p-3">Pattern</th>
                  <th className="text-left p-3">Actions</th>
                </tr>
              </thead>
              <tbody>
                {result.candidates.map(c => (
                  <tr key={c.email} className="border-t">
                    <td className="p-3 font-mono">{c.email}</td>
                    <td className="p-3"><span className="px-2 py-0.5 rounded-full bg-slate-100 text-xs">{c.status}</span></td>
                    <td className="p-3">{c.confidence}%</td>
                    <td className="p-3 text-slate-500">{c.pattern}</td>
                    <td className="p-3"><button onClick={() => saveContact(c)} className="text-indigo-600 hover:underline text-xs">Save</button></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </DashboardLayout>
    </AuthGuard>
  );
}

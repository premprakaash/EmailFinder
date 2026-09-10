'use client';

import { FormEvent, useState } from 'react';
import DashboardLayout from '@/components/DashboardLayout';
import AuthGuard from '@/components/AuthGuard';
import { api } from '@/lib/api';

interface DomainContact {
  firstName: string; lastName: string; jobTitle?: string; email: string;
  status: string; confidenceScore: number; source: string;
}

export default function DomainSearchPage() {
  const [domain, setDomain] = useState('example.com');
  const [contacts, setContacts] = useState<DomainContact[]>([]);
  const [loading, setLoading] = useState(false);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await api<{ contacts: DomainContact[] }>('/api/domain-search', { method: 'POST', body: JSON.stringify({ domain }) });
      setContacts(res.contacts);
    } catch (err) { alert(err instanceof Error ? err.message : 'Search failed'); }
    finally { setLoading(false); }
  };

  return (
    <AuthGuard>
      <DashboardLayout>
        <h2 className="text-2xl font-bold mb-6">Domain Search</h2>
        <form onSubmit={submit} className="flex gap-2 mb-6">
          <input value={domain} onChange={e => setDomain(e.target.value)} className="px-3 py-2 border rounded-lg flex-1 max-w-md" />
          <button type="submit" disabled={loading} className="px-6 py-2 bg-indigo-600 text-white rounded-lg">{loading ? 'Searching...' : 'Search'}</button>
        </form>
        <div className="bg-white rounded-xl border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-slate-50">
              <tr>
                <th className="text-left p-3">Name</th><th className="text-left p-3">Title</th>
                <th className="text-left p-3">Email</th><th className="text-left p-3">Status</th>
                <th className="text-left p-3">Confidence</th><th className="text-left p-3">Source</th>
              </tr>
            </thead>
            <tbody>
              {contacts.map((c, i) => (
                <tr key={i} className="border-t">
                  <td className="p-3">{c.firstName} {c.lastName}</td>
                  <td className="p-3">{c.jobTitle}</td>
                  <td className="p-3 font-mono">{c.email}</td>
                  <td className="p-3">{c.status}</td>
                  <td className="p-3">{c.confidenceScore}%</td>
                  <td className="p-3">{c.source}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </DashboardLayout>
    </AuthGuard>
  );
}

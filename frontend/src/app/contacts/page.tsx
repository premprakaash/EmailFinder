'use client';

import { useState } from 'react';
import DashboardLayout from '@/components/DashboardLayout';
import AuthGuard from '@/components/AuthGuard';
import { api } from '@/lib/api';

interface Contact {
  id: string; firstName: string; lastName: string; email: string; jobTitle?: string;
  company?: string; domain?: string; status: string; confidenceScore: number;
}

export default function ContactsPage() {
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [search, setSearch] = useState('');

  const load = async () => {
    const res = await api<{ items: Contact[] }>(`/api/contacts?domain=${search}`);
    setContacts(res.items);
  };

  return (
    <AuthGuard>
      <DashboardLayout>
        <h2 className="text-2xl font-bold mb-6">Contacts</h2>
        <div className="flex gap-2 mb-4">
          <input value={search} onChange={e => setSearch(e.target.value)} placeholder="Filter by domain"
            className="px-3 py-2 border rounded-lg flex-1 max-w-xs" />
          <button onClick={load} className="px-4 py-2 bg-indigo-600 text-white rounded-lg">Search</button>
        </div>
        <div className="bg-white rounded-xl border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-slate-50">
              <tr>
                <th className="text-left p-3">Name</th><th className="text-left p-3">Email</th>
                <th className="text-left p-3">Company</th><th className="text-left p-3">Status</th>
                <th className="text-left p-3">Confidence</th>
              </tr>
            </thead>
            <tbody>
              {contacts.map(c => (
                <tr key={c.id} className="border-t">
                  <td className="p-3">{c.firstName} {c.lastName}</td>
                  <td className="p-3 font-mono">{c.email}</td>
                  <td className="p-3">{c.company || c.domain}</td>
                  <td className="p-3">{c.status}</td>
                  <td className="p-3">{c.confidenceScore}%</td>
                </tr>
              ))}
            </tbody>
          </table>
          {!contacts.length && <p className="p-6 text-slate-400 text-sm">No contacts yet. Use Finder to discover and save contacts.</p>}
        </div>
      </DashboardLayout>
    </AuthGuard>
  );
}

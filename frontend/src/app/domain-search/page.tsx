'use client';

import { FormEvent, useState } from 'react';
import DashboardLayout from '@/components/DashboardLayout';
import AuthGuard from '@/components/AuthGuard';
import { api } from '@/lib/api';

interface DomainContact {
  firstName: string; lastName: string; jobTitle?: string; email: string;
  company?: string; status: string; confidenceScore: number; source: string;
}

interface DiscoveryResult {
  queryType: string; company?: string; domain: string; contacts: DomainContact[]; resolvedFrom?: string;
}

interface EnrichmentResult {
  email: string; firstName?: string; lastName?: string; jobTitle?: string;
  company?: string; domain?: string; country?: string; industry?: string; source: string;
}

type Tab = 'company' | 'domain' | 'email';

export default function PeopleDiscoveryPage() {
  const [tab, setTab] = useState<Tab>('company');
  const [company, setCompany] = useState('Example Corp');
  const [domain, setDomain] = useState('example.com');
  const [email, setEmail] = useState('john.smith@example.com');
  const [jobTitle, setJobTitle] = useState('');
  const [discovery, setDiscovery] = useState<DiscoveryResult | null>(null);
  const [enrichment, setEnrichment] = useState<EnrichmentResult | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const discover = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    setDiscovery(null);
    setEnrichment(null);
    try {
      const body = tab === 'company'
        ? { company, jobTitle: jobTitle || undefined }
        : { domain, jobTitle: jobTitle || undefined };
      setDiscovery(await api<DiscoveryResult>('/api/people-discovery', { method: 'POST', body: JSON.stringify(body) }));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Search failed');
    } finally {
      setLoading(false);
    }
  };

  const enrich = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    setDiscovery(null);
    setEnrichment(null);
    try {
      setEnrichment(await api<EnrichmentResult>('/api/enrichment', { method: 'POST', body: JSON.stringify({ email }) }));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Enrichment failed');
    } finally {
      setLoading(false);
    }
  };

  const saveContact = async (c: DomainContact) => {
    await api('/api/contacts', {
      method: 'POST',
      body: JSON.stringify({
        firstName: c.firstName, lastName: c.lastName, email: c.email,
        jobTitle: c.jobTitle, company: c.company, domain: discovery?.domain,
        status: c.status, confidenceScore: c.confidenceScore, source: c.source,
      }),
    });
    alert('Contact saved!');
  };

  const tabs: { id: Tab; label: string; hint: string }[] = [
    { id: 'company', label: 'By Company Name', hint: "Don't know anyone's name? Enter the company name." },
    { id: 'domain', label: 'By Domain', hint: 'Search using the company website domain.' },
    { id: 'email', label: 'By Email', hint: 'Have an email but not a name? Look up the person.' },
  ];

  return (
    <AuthGuard>
      <DashboardLayout>
        <h2 className="text-2xl font-bold mb-2">People Discovery</h2>
        <p className="text-slate-500 mb-6">Find people when you don&apos;t know their name — search by company, domain, or email.</p>

        <div className="flex gap-2 mb-6 flex-wrap">
          {tabs.map(t => (
            <button key={t.id} type="button" onClick={() => { setTab(t.id); setError(''); }}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition ${tab === t.id ? 'bg-indigo-600 text-white' : 'bg-white border text-slate-600 hover:bg-slate-50'}`}>
              {t.label}
            </button>
          ))}
        </div>

        <p className="text-sm text-slate-500 mb-4">{tabs.find(t => t.id === tab)?.hint}</p>

        {tab !== 'email' ? (
          <form onSubmit={discover} className="bg-white rounded-xl border p-6 max-w-xl space-y-4 mb-6">
            {tab === 'company' ? (
              <div>
                <label className="block text-sm font-medium mb-1">Company Name</label>
                <input value={company} onChange={e => setCompany(e.target.value)} placeholder="Example Corp"
                  className="w-full px-3 py-2 border rounded-lg outline-none focus:ring-2 focus:ring-indigo-500" required />
                <p className="text-xs text-slate-400 mt-1">Try: Example Corp, Company Inc, Startup</p>
              </div>
            ) : (
              <div>
                <label className="block text-sm font-medium mb-1">Company Domain</label>
                <input value={domain} onChange={e => setDomain(e.target.value)} placeholder="example.com"
                  className="w-full px-3 py-2 border rounded-lg outline-none focus:ring-2 focus:ring-indigo-500" required />
              </div>
            )}
            <div>
              <label className="block text-sm font-medium mb-1">Job Title Filter (optional)</label>
              <input value={jobTitle} onChange={e => setJobTitle(e.target.value)} placeholder="Engineer, CEO, Marketing"
                className="w-full px-3 py-2 border rounded-lg outline-none focus:ring-2 focus:ring-indigo-500" />
            </div>
            {error && <p className="text-red-600 text-sm">{error}</p>}
            <button type="submit" disabled={loading} className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50">
              {loading ? 'Searching...' : 'Find People'}
            </button>
          </form>
        ) : (
          <form onSubmit={enrich} className="bg-white rounded-xl border p-6 max-w-xl space-y-4 mb-6">
            <div>
              <label className="block text-sm font-medium mb-1">Email Address</label>
              <input type="email" value={email} onChange={e => setEmail(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg outline-none focus:ring-2 focus:ring-indigo-500" required />
              <p className="text-xs text-slate-400 mt-1">Try: john.smith@example.com</p>
            </div>
            {error && <p className="text-red-600 text-sm">{error}</p>}
            <button type="submit" disabled={loading} className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-50">
              {loading ? 'Looking up...' : 'Find Person from Email'}
            </button>
          </form>
        )}

        {discovery && (
          <>
            <div className="mb-4 text-sm text-slate-600">
              Found <strong>{discovery.contacts.length}</strong> people at{' '}
              <strong>{discovery.company || discovery.domain}</strong>
              {discovery.resolvedFrom && <span className="text-slate-400"> (domain resolved from company name)</span>}
            </div>
            <ContactsTable contacts={discovery.contacts} onSave={saveContact} />
          </>
        )}

        {enrichment && (
          <div className="bg-white rounded-xl border p-6 max-w-xl">
            <h3 className="font-semibold text-lg mb-4">Person Found</h3>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div><span className="text-slate-500">Name</span><p className="font-medium">{enrichment.firstName} {enrichment.lastName}</p></div>
              <div><span className="text-slate-500">Job Title</span><p>{enrichment.jobTitle || '—'}</p></div>
              <div><span className="text-slate-500">Company</span><p>{enrichment.company || '—'}</p></div>
              <div><span className="text-slate-500">Domain</span><p>{enrichment.domain || '—'}</p></div>
              <div><span className="text-slate-500">Country</span><p>{enrichment.country || '—'}</p></div>
              <div><span className="text-slate-500">Industry</span><p>{enrichment.industry || '—'}</p></div>
            </div>
          </div>
        )}
      </DashboardLayout>
    </AuthGuard>
  );
}

function ContactsTable({ contacts, onSave }: { contacts: DomainContact[]; onSave: (c: DomainContact) => void }) {
  if (!contacts.length) {
    return <p className="text-slate-400 text-sm">No people found. Try a demo company: Example Corp, Company Inc, or domain example.com</p>;
  }
  return (
    <div className="bg-white rounded-xl border overflow-hidden">
      <table className="w-full text-sm">
        <thead className="bg-slate-50">
          <tr>
            <th className="text-left p-3">Name</th><th className="text-left p-3">Title</th>
            <th className="text-left p-3">Email</th><th className="text-left p-3">Status</th>
            <th className="text-left p-3">Confidence</th><th className="text-left p-3">Actions</th>
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
              <td className="p-3"><button onClick={() => onSave(c)} className="text-indigo-600 hover:underline text-xs">Save</button></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

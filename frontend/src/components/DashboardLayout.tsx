'use client';

import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { clearAuth, getUser } from '@/lib/api';

const nav = [
  { href: '/dashboard', label: 'Dashboard' },
  { href: '/finder', label: 'Finder' },
  { href: '/verifier', label: 'Verifier' },
  { href: '/domain-search', label: 'People Discovery' },
  { href: '/bulk', label: 'Bulk Search' },
  { href: '/contacts', label: 'Contacts' },
  { href: '/exports', label: 'Exports' },
  { href: '/api-keys', label: 'API' },
  { href: '/usage', label: 'Usage' },
  { href: '/billing', label: 'Billing' },
  { href: '/settings', label: 'Settings' },
];

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const user = getUser();

  const logout = () => {
    clearAuth();
    router.push('/login');
  };

  return (
    <div className="min-h-screen flex bg-slate-50">
      <aside className="w-64 bg-slate-900 text-white flex flex-col">
        <div className="p-6 border-b border-slate-700">
          <h1 className="text-xl font-bold tracking-tight">MailForge</h1>
          <p className="text-xs text-slate-400 mt-1">B2B Email Platform</p>
        </div>
        <nav className="flex-1 p-4 space-y-1">
          {nav.map(item => (
            <Link key={item.href} href={item.href}
              className={`block px-3 py-2 rounded-lg text-sm transition ${pathname === item.href ? 'bg-indigo-600 text-white' : 'text-slate-300 hover:bg-slate-800'}`}>
              {item.label}
            </Link>
          ))}
        </nav>
        <div className="p-4 border-t border-slate-700">
          <p className="text-sm text-slate-300 truncate">{user?.email}</p>
          <p className="text-xs text-slate-400">{user?.creditBalance?.toLocaleString()} credits</p>
          <button onClick={logout} className="mt-2 text-xs text-red-400 hover:text-red-300">Logout</button>
        </div>
      </aside>
      <main className="flex-1 overflow-auto">
        <div className="p-8">{children}</div>
      </main>
    </div>
  );
}

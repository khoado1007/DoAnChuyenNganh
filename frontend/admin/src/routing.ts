import type { UserRole } from './types/auth';
export type AdminSection = 'overview' | 'users' | 'assets' | 'agents' | 'tickets';
export type Route = { kind: 'admin'; section: AdminSection } | { kind: 'portal' | 'forbidden' | 'not-found' };
export const homeForRole = (role: UserRole) => '/' + role.toLowerCase();
export function resolveRoute(path: string, role: UserRole): Route {
  const normalized = path.replace(/\/+$/, '') || '/';
  const segments = normalized.split('/').filter(Boolean);
  const group = segments[0];
  if (!['admin', 'employee', 'technician'].includes(group)) return { kind: 'not-found' };
  if ('/' + group !== homeForRole(role)) return { kind: 'forbidden' };
  if (role !== 'Admin') return { kind: segments.length === 1 ? 'portal' : 'not-found' };
  const section = segments[1] || 'overview';
  if (segments.length > 2 || !['overview', 'users', 'assets', 'agents', 'tickets'].includes(section)) return { kind: 'not-found' };
  return { kind: 'admin', section: section as AdminSection };
}

export function destinationAfterLogin(path: string, role: UserRole): string {
  const route = resolveRoute(path, role);
  return route.kind === 'admin' || route.kind === 'portal' ? path : homeForRole(role);
}

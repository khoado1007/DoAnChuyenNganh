import { createApiClient } from './client';
import type { AuthResponse, LoginRequest, UserResponse } from '../types/auth';

const tokenKey = 'itdesk.accessToken';
export const sessionExpiredEvent = 'itdesk:session-expired';
export const getToken = () => sessionStorage.getItem(tokenKey);
export const clearSession = () => sessionStorage.removeItem(tokenKey);
export const api = createApiClient(import.meta.env.VITE_API_BASE_URL || '/api', getToken, () => {
  clearSession();
  window.dispatchEvent(new Event(sessionExpiredEvent));
});

export async function login(values: LoginRequest): Promise<UserResponse> {
  const result = await api<AuthResponse>('/auth/login', { method: 'POST', body: JSON.stringify(values) });
  sessionStorage.setItem(tokenKey, result.accessToken);
  return result.user;
}

export const getCurrentUser = (signal?: AbortSignal) => api<UserResponse>('/auth/me', { signal });

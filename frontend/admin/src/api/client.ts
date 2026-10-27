import type { ApiResponse } from '../types/api.ts';

export class ApiError extends Error {
  status: number;
  errors: Record<string, string[]> | null;

  constructor(message: string, status: number, errors: Record<string, string[]> | null = null) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.errors = errors;
  }
}

export function createApiClient(
  baseUrl: string,
  getToken: () => string | null,
  onUnauthorized: () => void,
  fetcher: typeof fetch = fetch,
) {
  return async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
    const headers = new Headers(init.headers);
    headers.set('Accept', 'application/json');
    if (init.body && !(init.body instanceof FormData)) headers.set('Content-Type', 'application/json');
    const token = getToken();
    if (token) headers.set('Authorization', `Bearer ${token}`);
    const response = await fetcher(`${baseUrl.replace(/\/$/, '')}${path}`, { ...init, headers });
    if (response.status === 401) onUnauthorized();
    let payload: ApiResponse<T>;
    try { payload = await response.json() as ApiResponse<T>; }
    catch { throw new ApiError('Không thể đọc phản hồi từ máy chủ. Vui lòng thử lại.', response.status); }
    if (!response.ok || !payload.success) {
      throw new ApiError(payload.message || 'Yêu cầu không thành công.', response.status, payload.errors);
    }
    return payload.data;
  };
}

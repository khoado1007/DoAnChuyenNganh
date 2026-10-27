import test from 'node:test';
import assert from 'node:assert/strict';

test('API client attaches token and unwraps the response data', async () => {
  const { createApiClient } = await import('../src/api/client.ts');
  const request = createApiClient('/api', () => 'test-token', () => {}, async (url, init) => {
    assert.equal(url, '/api/auth/me');
    assert.equal(new Headers(init.headers).get('Authorization'), 'Bearer test-token');
    return Response.json({ success: true, message: '', data: { username: 'admin' }, errors: null });
  });
  assert.deepEqual(await request('/auth/me'), { username: 'admin' });
});

test('401 clears the session while validation errors preserve field details', async () => {
  const { createApiClient, ApiError } = await import('../src/api/client.ts');
  let invalidated = false;
  const unauthorized = createApiClient('/api', () => 'expired', () => { invalidated = true; }, async () =>
    Response.json({ success: false, message: 'Expired', data: null, errors: null }, { status: 401 }));
  await assert.rejects(unauthorized('/auth/me'), error => error instanceof ApiError && error.status === 401);
  assert.equal(invalidated, true);
  const invalid = createApiClient('/api', () => null, () => {}, async () =>
    Response.json({ success: false, message: 'Invalid', data: null, errors: { username: ['Required'] } }, { status: 400 }));
  await assert.rejects(invalid('/auth/login', { method: 'POST' }), error =>
    error instanceof ApiError && error.errors.username[0] === 'Required');
});

test('non-JSON server failures become readable API errors', async () => {
  const { createApiClient, ApiError } = await import('../src/api/client.ts');
  const request = createApiClient('/api', () => null, () => {}, async () => new Response('Bad gateway', { status: 502 }));
  await assert.rejects(request('/auth/me'), error => error instanceof ApiError && error.status === 502);
});

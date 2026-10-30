import test from 'node:test';
import assert from 'node:assert/strict';

test('route groups enforce exact roles and reject unknown subpaths', async () => {
  const { resolveRoute, homeForRole } = await import('../src/routing.ts');
  assert.equal(homeForRole('Employee'), '/employee');
  assert.deepEqual(resolveRoute('/admin/users/', 'Admin'), { kind: 'admin', section: 'users' });
  assert.deepEqual(resolveRoute('/technician', 'Technician'), { kind: 'portal' });
  assert.deepEqual(resolveRoute('/admin/users', 'Employee'), { kind: 'forbidden' });
  assert.deepEqual(resolveRoute('/employee', 'Admin'), { kind: 'forbidden' });
  assert.deepEqual(resolveRoute('/administrator', 'Admin'), { kind: 'not-found' });
  assert.deepEqual(resolveRoute('/admin/unknown', 'Admin'), { kind: 'not-found' });
});

test('login resumes an allowed deep link and rejects other role or unknown destinations', async () => {
  const { destinationAfterLogin } = await import('../src/routing.ts');
  assert.equal(destinationAfterLogin('/admin/users', 'Admin'), '/admin/users');
  assert.equal(destinationAfterLogin('/admin/assets', 'Employee'), '/employee');
  assert.equal(destinationAfterLogin('/login', 'Admin'), '/admin');
  assert.equal(destinationAfterLogin('//evil.test', 'Admin'), '/admin');
});

const assert = require('node:assert/strict');
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const base = process.env.ITAM_UI_URL || 'http://127.0.0.1:5190';
(async () => {
  const browser = await chromium.launch({ channel: 'msedge', headless: true });
  try {
    const page = await browser.newPage({ viewport: { width: 1440, height: 1000 } });
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    const response = await page.request.post(base + '/api/auth/login', { data: { username: 'admin', password: process.env.ITAM_TEST_PASSWORD } });
    assert.equal(response.status(), 200);
    const jwt = (await response.json()).data.accessToken;
    await page.addInitScript(token => sessionStorage.setItem('itdesk.accessToken', token), jwt);
    await page.goto(base + '/admin/');
    await page.getByRole('button', { name: /Agent/ }).click();
    await page.getByRole('heading', { name: 'Agent', exact: true }).waitFor();
    await page.getByText('Simulated', { exact: true }).first().waitFor();
    await page.getByText('Real', { exact: true }).first().waitFor();
    const code = 'UI-' + Date.now();
    await page.getByLabel('Mã tài sản', { exact: true }).fill(code);
    await page.getByLabel('Tên tài sản', { exact: true }).fill('Browser integration PC');
    await page.getByRole('button', { name: 'Tạo tài sản', exact: true }).click();
    await page.getByLabel('Tài sản chưa đăng ký Agent', { exact: true }).click();
    await page.getByTitle(code + ' · Browser integration PC', { exact: true }).click();
    await page.getByRole('button', { name: 'Cấp token đăng ký', exact: true }).click();
    const field = page.getByRole('textbox', { name: 'Token đăng ký', exact: true });
    await field.waitFor();
    const token = await field.inputValue();
    assert.equal(token.length, 64);
    const enrolled = await page.request.post(base + '/api/agents/enroll', { data: { token, instanceId: require('node:crypto').randomUUID(), mode: 'real', protocolVersion: 1 } });
    assert.equal(enrolled.status(), 200);
    const identity = (await enrolled.json()).data;
    const heartbeat = await page.request.post(base + '/api/agents/heartbeat', {
      headers: { Authorization: 'Agent ' + identity.credential },
      data: { sequence: 1, protocolVersion: 1, hostname: 'UI-TEST', os: 'Test OS', cpu: 'Test CPU', ramBytes: 1024, agentVersion: '1' },
    });
    assert.equal(heartbeat.status(), 200);
    await page.getByRole('button', { name: 'Đã lưu, đóng', exact: true }).click();
    await page.getByRole('button', { name: 'Làm mới', exact: true }).click();
    const row = page.getByRole('row').filter({ hasText: code });
    await row.getByText('Online', { exact: true }).waitFor();
    await row.getByText('Available', { exact: true }).waitFor();
    await row.getByRole('button', { name: 'Vô hiệu hóa', exact: true }).click();
    await page.locator('.ant-popconfirm-buttons .ant-btn-primary').click();
    await row.getByText('Đã vô hiệu hóa', { exact: true }).waitFor();
    assert.equal(await row.getByRole('button', { name: 'Vô hiệu hóa', exact: true }).isDisabled(), true);
    await page.setViewportSize({ width: 390, height: 844 });
    await page.reload();
    await page.getByRole('button', { name: 'Mở điều hướng', exact: true }).click();
    await page.getByRole('button', { name: /Agent/ }).click();
    await page.getByRole('heading', { name: 'Agent', exact: true }).waitFor();
    await page.getByText(code, { exact: true }).waitFor();
    await page.evaluate(() => window.scrollTo(0, 0));
    await page.screenshot({ animations: 'disabled', path: process.env.ITAM_SCREENSHOT || '../docs/agent-mobile.png', fullPage: true });
    assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth), true);
    await page.route('**/api/agents', route => route.fulfill({ status: 503, contentType: 'application/json', body: JSON.stringify({ success: false, message: 'E2E connection unavailable', data: null }) }));
    await page.getByRole('button', { name: 'Làm mới', exact: true }).click();
    await page.getByText('E2E connection unavailable', { exact: true }).waitFor();
    assert.deepEqual(errors, []);
    console.log('PASS: Agent labels, create asset/token, online state, disable, mobile layout and API error display');
  } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });

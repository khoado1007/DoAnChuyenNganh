import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, '.', '');
  return {
    plugins: [react()],
    base: './',
    server: {
      port: 5173,
      strictPort: true,
      proxy: { '/api': { target: env.API_PROXY_TARGET || 'http://localhost:5038', changeOrigin: true } },
    },
  };
});

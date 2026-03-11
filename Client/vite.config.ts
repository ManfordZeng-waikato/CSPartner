import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import mkcert from 'vite-plugin-mkcert'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const isProduction = mode === 'production';
  // Cloudflare Pages: set VITE_DEPLOY_TARGET=cloudflare to output to dist/
  const isCloudflareDeploy = import.meta.env.VITE_DEPLOY_TARGET === 'cloudflare';

  return {
    plugins: [
      react(),
      // Only use mkcert in development for HTTPS
      ...(isProduction ? [] : [mkcert()])
    ],
    server: {
      port: 3000,
      host: true,
      open: false, 
      proxy: {
        '/api': {
          target: 'https://localhost:5001',
          changeOrigin: true,
          secure: false, 
        }
      }
    },
    build: {
      // Cloudflare Pages: dist/ | Same-origin (API serves frontend): ../API/wwwroot
      outDir: isCloudflareDeploy ? 'dist' : (isProduction ? '../API/wwwroot' : 'dist'),
      assetsDir: 'assets',
      sourcemap: false, // Disable sourcemaps in production for smaller builds
      minify: 'esbuild', // Fast minification
      chunkSizeWarningLimit: 1000,
      emptyOutDir: true, // Clear wwwroot before building
      rollupOptions: {
        output: {
          manualChunks: {
            // Split vendor chunks for better caching
            'react-vendor': ['react', 'react-dom', 'react-router'],
            'mui-vendor': ['@mui/material', '@mui/icons-material', '@emotion/react', '@emotion/styled'],
            'query-vendor': ['@tanstack/react-query'],
            'signalr-vendor': ['@microsoft/signalr'],
            'form-vendor': ['react-hook-form', '@hookform/resolvers', 'zod'],
          }
        }
      }
    },
    // Optimize dependencies for production
    optimizeDeps: {
      include: ['react', 'react-dom', 'react-router']
    }
  };
})

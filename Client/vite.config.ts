import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import mkcert from 'vite-plugin-mkcert'

// https://vite.dev/config/
export default defineConfig({
  server: {
    port: 3000,
    host: true, // 允许从网络访问，包括编辑器预览
    open: false, // 不在外部浏览器自动打开
    proxy: {
      '/api': {
        target: 'https://localhost:5001',
        changeOrigin: true,
        secure: false, // 允许自签名证书
      }
    }
  },
  plugins: [react(), mkcert()],
})

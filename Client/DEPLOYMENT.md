# 生产部署指南

本文档说明如何准备和部署 CSPartner 客户端应用到生产环境。

## 前置要求

- Node.js 18+ 和 npm
- 生产环境 API 服务器地址

## 构建步骤

### 1. 安装依赖

```bash
npm install
```

### 2. 配置环境变量

复制 `.env.example` 文件并创建 `.env` 文件：

```bash
cp .env.example .env
```

编辑 `.env` 文件，设置生产环境的 API 基础 URL：

```env
VITE_API_BASE_URL=https://api.yourdomain.com
```

**重要提示：**
- 确保 API URL 包含协议（https://）
- 不要包含尾部斜杠
- API 服务器需要支持 CORS（如果前端和 API 不在同一域名下）

### 3. 运行类型检查

```bash
npm run lint
```

### 4. 构建生产版本

```bash
npm run build
# 或使用明确的生产模式
npm run build:prod
```

构建完成后：
- **与后端一起部署**: 文件将输出到 `../API/wwwroot` 目录
- **独立部署**: 文件将位于 `dist/` 目录中

## 构建产物说明

构建完成后，`dist/` 目录包含：

- `index.html` - 入口 HTML 文件
- `assets/` - 静态资源（JS、CSS、图片等）
  - 文件名称包含哈希值用于缓存控制
  - 已自动进行代码分割和优化

## 部署选项

### 选项 1: 静态文件服务器

将 `dist/` 目录的内容部署到任何静态文件服务器，例如：

- **Nginx**: 配置指向 `dist/` 目录
- **Apache**: 配置 DocumentRoot 为 `dist/` 目录
- **IIS**: 配置网站根目录为 `dist/` 目录
- **CDN**: 上传到云存储（AWS S3、Azure Blob、阿里云 OSS 等）

#### Nginx 配置示例

```nginx
server {
    listen 80;
    server_name yourdomain.com;
    
    root /path/to/dist;
    index index.html;
    
    # 处理客户端路由（SPA）
    location / {
        try_files $uri $uri/ /index.html;
    }
    
    # API 代理（可选，如果需要）
    location /api {
        proxy_pass https://api.yourdomain.com;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
    
    # 静态资源缓存
    location /assets {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
```

### 选项 2: 与后端一起部署（推荐）

如果你的后端是 ASP.NET Core，可以将前端构建产物直接输出到后端的 `wwwroot` 目录，实现前后端一体化部署。

#### 优势

- ✅ 单点部署，简化部署流程
- ✅ 无需配置 CORS（前后端同源）
- ✅ 统一域名和 HTTPS 配置
- ✅ 更简单的反向代理配置

#### 构建步骤

1. **配置环境变量**

   与后端一起部署时，前端使用相对路径调用 API，因此 `VITE_API_BASE_URL` 应该留空：

   ```env
   # .env 或 .env.production
   VITE_API_BASE_URL=
   ```

2. **构建前端**

   ```bash
   cd Client
   npm run build
   ```

   构建完成后，文件会自动输出到 `../API/wwwroot` 目录。

3. **构建和运行后端**

   ```bash
   cd API
   dotnet build
   dotnet run
   ```

   后端启动后，前端应用将自动从 `wwwroot` 目录提供。

#### 构建配置说明

前端 `vite.config.ts` 已配置为在生产构建时自动输出到 `API/wwwroot` 目录：

```typescript
build: {
  outDir: isProduction ? '../API/wwwroot' : 'dist',
  // ...
}
```

#### 后端配置

后端 `Program.cs` 已配置：

1. **静态文件服务**: `app.UseStaticFiles()` 从 `wwwroot` 目录提供静态文件
2. **SPA 回退**: 所有非 API 路由返回 `index.html`，支持客户端路由
3. **CORS**: 生产环境不需要 CORS（前后端同源）

#### 完整部署流程

```bash
# 1. 安装前端依赖
cd Client
npm install

# 2. 构建前端（输出到 API/wwwroot）
npm run build

# 3. 构建后端
cd ../API
dotnet build -c Release

# 4. 发布后端（可选）
dotnet publish -c Release -o ./publish

# 5. 运行
dotnet run
# 或使用发布版本
# cd publish
# dotnet API.dll
```

#### 目录结构

部署后的目录结构：

```
API/
├── wwwroot/              # 前端构建产物（自动生成）
│   ├── index.html
│   ├── assets/
│   │   ├── index-[hash].js
│   │   └── index-[hash].css
│   └── ...
├── Controllers/
├── Program.cs
└── ...
```

#### 注意事项

- ⚠️ 确保 `API/wwwroot` 目录在构建时存在（ASP.NET Core 项目默认会自动创建）
- ⚠️ 每次前端更新后需要重新运行 `npm run build`
- ⚠️ 建议在 CI/CD 流程中先构建前端，再构建后端
- ⚠️ `wwwroot` 目录在 `.gitignore` 中通常已被忽略，构建产物不会提交到代码库

#### 开发模式

在开发模式下，前端仍然使用独立的开发服务器（`npm run dev`），通过代理连接到后端 API。只有在构建生产版本时，前端才会输出到 `wwwroot`。

## 环境变量配置

### 开发环境

开发环境可以不设置 `VITE_API_BASE_URL`，使用 vite 的代理功能：

```env
# .env.development（可选）
VITE_API_BASE_URL=
```

### 生产环境 - 与后端一起部署（推荐）

当与 ASP.NET Core 后端一起部署时，使用相对路径：

```env
# .env.production
VITE_API_BASE_URL=
```

### 生产环境 - 独立部署

如果前端和后端部署在不同的服务器，需要设置完整的 API URL：

```env
# .env.production
VITE_API_BASE_URL=https://api.yourdomain.com
```

**注意：** Vite 的环境变量必须以 `VITE_` 开头才能在客户端代码中访问。

## 构建优化说明

生产构建已配置以下优化：

1. **代码分割**: 自动将大型依赖库拆分为独立的 chunk
   - React 相关库
   - Material-UI 组件库
   - TanStack Query
   - SignalR
   - 表单验证库

2. **资源优化**:
   - ESBuild 压缩
   - Tree-shaking 移除未使用代码
   - 资源文件包含哈希值用于缓存

3. **开发工具移除**:
   - 生产构建自动移除开发相关插件（如 mkcert）

## 验证部署

部署后，检查以下内容：

1. ✅ 应用可以正常加载
2. ✅ API 调用正常工作（检查网络请求）
3. ✅ SignalR 连接正常（评论功能）
4. ✅ 路由跳转正常（刷新页面不会 404）
5. ✅ 静态资源加载正常
6. ✅ HTTPS 配置正确（生产环境推荐）

## 故障排除

### 问题：API 请求失败

- 检查 `VITE_API_BASE_URL` 是否正确设置
- 检查 API 服务器的 CORS 配置
- 检查浏览器控制台的网络错误

### 问题：页面刷新后 404

- 确保服务器配置了 SPA 路由重定向（见 Nginx 配置示例）

### 问题：SignalR 连接失败

- 检查 API 服务器是否支持 WebSocket
- 检查 SignalR Hub 路径是否正确（`/api/hubs/comments`）
- 检查防火墙/代理是否允许 WebSocket 连接

### 问题：构建文件过大

- 检查是否启用了代码分割
- 检查是否移除了 sourcemap（生产构建已自动禁用）
- 考虑使用 CDN 加载大型库（如 Material-UI）

## 持续集成/持续部署 (CI/CD)

### GitHub Actions 示例

```yaml
name: Build and Deploy

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Build
        run: npm run build
        env:
          VITE_API_BASE_URL: ${{ secrets.VITE_API_BASE_URL }}
      
      - name: Deploy
        # 添加你的部署步骤
```

## 安全注意事项

1. **不要在客户端代码中暴露敏感信息**（API keys、密钥等）
2. **使用 HTTPS** 在生产环境强制使用 HTTPS
3. **设置适当的 CSP 头**（内容安全策略）
4. **定期更新依赖** 运行 `npm audit` 检查安全漏洞

## 性能优化建议

1. **启用 Gzip/Brotli 压缩** 在服务器层面
2. **使用 CDN** 加速静态资源加载
3. **配置适当的缓存策略**
4. **监控和优化** 使用 Lighthouse 等工具分析性能

## 支持

如有问题，请查看：
- [Vite 文档](https://vite.dev/)
- [React 文档](https://react.dev/)
- 项目 README.md


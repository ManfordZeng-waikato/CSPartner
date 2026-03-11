# CSPartner 部署准备检查清单

目标部署链路：
- **React 前端** → Cloudflare Pages
- **.NET Web API** → Railway
- **Postgres** → Supabase
- **图片** → Cloudflare R2
- **域名 / DNS / HTTPS** → Cloudflare

---

## 一、安全警告（优先处理）

### 1.1 敏感信息泄露

`API/appsettings.json` 当前包含明文密钥，**即使已在 .gitignore 中，也应立即处理**：

- [ ] **R2**: AccessKeyId, SecretAccessKey
- [ ] **JWT**: SecretKey
- [ ] **Resend**: ApiToken
- [ ] **GitHub OAuth**: ClientSecret
- [ ] **OpenAI**: ApiKey

**建议**：创建 `appsettings.Production.json`（加入 .gitignore），或全部改为环境变量，仅保留非敏感默认值在 appsettings.json。

---

## 二、数据库：SQL Server → Postgres（Supabase）

### 2.1 当前状态

项目使用 **SQL Server**（`UseSqlServer`），需迁移到 **PostgreSQL**（Supabase）。

### 2.2 迁移步骤

1. **添加 Npgsql 包**（`Infrastructure/Infrastructure.csproj`）：
   ```xml
   <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.1" />
   ```
   移除：`Microsoft.EntityFrameworkCore.SqlServer`

2. **修改 `Infrastructure/DependencyInjection.cs`**：
   ```csharp
   options.UseNpgsql(configuration.GetConnectionString("Default"));
   ```

3. **重新生成迁移**（因 SQL Server 与 Postgres 类型不同）：
   - 删除 `Infrastructure/Persistence/Migrations/` 下所有迁移文件
   - 执行：`dotnet ef migrations add InitialCreate --project Infrastructure --startup-project API`
   - 新迁移将使用 Postgres 类型（uuid, timestamp 等）
   - 迁移文件位于 `Infrastructure/Migrations/`

4. **Supabase 连接字符串**（Railway 环境变量）：
   ```
   Host=db.xxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;
   ```
   在 Supabase Dashboard → Project Settings → Database 获取。

---

## 三、React 前端 → Cloudflare Pages

### 3.1 构建输出调整

当前 Vite 将生产构建输出到 `../API/wwwroot`（同源部署）。Cloudflare Pages 需独立构建到 `dist`。

**方案 A**：通过环境变量控制输出目录（推荐）

在 `Client/vite.config.ts` 中：
```ts
// 当 VITE_DEPLOY_TARGET=cloudflare 时输出到 dist
outDir: import.meta.env.VITE_DEPLOY_TARGET === 'cloudflare' ? 'dist' : (isProduction ? '../API/wwwroot' : 'dist'),
```

Cloudflare Pages 构建命令与变量：
- **Build command**: `npm run build:cloudflare`
- **Environment variables**（在 Cloudflare Pages 项目设置中配置）:
  - `VITE_DEPLOY_TARGET` = `cloudflare`（输出到 dist/）
  - `VITE_API_BASE_URL` = `https://your-api.railway.app`

**方案 B**：新增 `build:cloudflare` 脚本

在 `Client/package.json`：
```json
"build:cloudflare": "tsc -b && vite build --mode production"
```
并修改 vite.config 使 production 模式在 Cloudflare 场景下输出到 `dist`（需配合环境变量）。

### 3.2 环境变量

创建 `Client/.env.production.example`：
```env
# Cloudflare Pages 构建时设置
VITE_API_BASE_URL=https://your-api.railway.app
```

Cloudflare Pages 项目设置 → Environment variables 中配置 `VITE_API_BASE_URL`。

### 3.3 Cloudflare Pages 构建配置

- **Build command**: `npm run build:cloudflare`
- **Build output directory**: `dist`
- **Root directory**: `Client`（若 Monorepo 根目录为仓库根，则填 `Client`）
- **Environment variables**（在 Cloudflare Pages → Settings → Environment variables 中配置）:
  - `VITE_DEPLOY_TARGET` = `cloudflare`（必须，否则会输出到 API/wwwroot）
  - `VITE_API_BASE_URL` = `https://your-api.railway.app`

---

## 四、.NET Web API → Railway

### 4.1 环境变量（Railway Variables）

| 变量名 | 说明 | 示例 |
|-------|------|------|
| `ASPNETCORE_ENVIRONMENT` | 环境 | `Production` |
| `ConnectionStrings__Default` | Supabase Postgres 连接串 | 见上文 |
| `ClientApp__ClientUrl` | 前端 URL | `https://your-app.pages.dev` |
| `CloudflareR2__AccessKeyId` | R2 访问密钥 | - |
| `CloudflareR2__SecretAccessKey` | R2 密钥 | - |
| `CloudflareR2__AccountId` | Cloudflare 账户 ID | - |
| `CloudflareR2__BucketName` | 桶名 | `highlights` |
| `CloudflareR2__PublicUrl` | 公开访问 URL | - |
| `CloudflareR2__S3ServiceUrl` | R2 S3 兼容端点 | - |
| `Jwt__Issuer` | JWT 签发者 | `https://your-app.pages.dev` |
| `Jwt__Audience` | JWT 受众 | `https://your-app.pages.dev` |
| `Jwt__SecretKey` | JWT 密钥 | 生产用强随机密钥 |
| `Jwt__ExpirationMinutes` | 过期时间（分钟） | `1440` |
| `Resend__ApiToken` | Resend API Token | - |
| `Resend__FromEmail` | 发件邮箱 | `no-reply@cspartner.org` |
| `Authentication__Github__ClientId` | GitHub OAuth Client ID | - |
| `Authentication__Github__ClientSecret` | GitHub OAuth Client Secret | - |
| `OpenAI__ApiKey` | OpenAI API Key | - |
| `OpenAI__Model` | 模型名 | `gpt-4o` 等 |
| `Seed__DemoData` | 是否种子数据 | `false` |

### 4.2 DataProtection

当前生产环境依赖 Azure Key Vault + Blob Storage。Railway 无 Azure 集成时：

- 不配置 `DataProtection:KeyVaultUri` 和 `DataProtection:BlobStorageConnectionString`
- 将回退到**文件系统**持久化（`DataProtection-Keys/`）
- Railway 文件系统为临时，重启后密钥会丢失，可能导致 OAuth 状态校验失败

**建议**：
- 使用 Railway Volume 挂载持久化目录存放 DataProtection 密钥，或
- 使用其他密钥存储（如 Redis）需自行扩展 `AddDataProtectionConfiguration`

### 4.3 启动与健康检查

- Railway 默认会检测 Web 进程，确保 `dotnet run` 或 `dotnet API.dll` 正确启动
- 可添加 `/health` 端点供 Railway 健康检查使用

---

## 五、CORS 配置（前后端分离）

### 5.1 当前逻辑

- 开发环境：CORS 启用，允许 `localhost:3000`
- 生产环境：**仅开发环境**调用 `UseCors`，生产不启用 CORS

前端在 Cloudflare Pages、API 在 Railway 时，属于**跨域**，必须启用 CORS。

### 5.2 修改建议

1. **`API/Extensions/ServiceCollectionExtensions.cs`**  
   生产环境改为显式允许前端域名，例如：
   ```csharp
   else
   {
       var clientUrl = configuration["ClientApp:ClientUrl"] ?? "";
       policy.WithOrigins(clientUrl)
             .AllowAnyMethod()
             .AllowAnyHeader()
             .AllowCredentials();  // 若使用 Cookie
   }
   ```

2. **`API/Extensions/WebApplicationExtensions.cs`**  
   生产环境也启用 CORS：
   ```csharp
   // 前后端分离时生产环境也需要 CORS
   app.UseCors("AllowReactClient");
   ```

---

## 六、Cloudflare R2

### 6.1 当前状态

已使用 `R2StorageService`，配置正确即可。

### 6.2 R2 Bucket CORS

前端直传 R2 时，需在 R2 Bucket 配置 CORS，允许 Cloudflare Pages 域名：

```json
[
  {
    "AllowedOrigins": ["https://your-app.pages.dev", "https://your-domain.com"],
    "AllowedMethods": ["GET", "PUT", "HEAD"],
    "AllowedHeaders": ["*"],
    "MaxAgeSeconds": 3600
  }
]
```

在 Cloudflare Dashboard → R2 → Bucket → Settings → CORS 中配置。

---

## 七、GitHub OAuth

在 GitHub OAuth App 设置中添加生产回调 URL：

- `https://your-api.railway.app/signin-github`（或实际 API 域名）

---

## 八、部署顺序建议

1. **Supabase**：创建项目，获取连接字符串
2. **数据库迁移**：完成 SQL Server → Postgres 迁移并执行
3. **Railway**：部署 API，配置环境变量，验证健康
4. **Cloudflare R2**：确认 CORS，测试上传
5. **Cloudflare Pages**：部署前端，配置 `VITE_API_BASE_URL`
6. **CORS**：按上文修改，确保跨域请求正常
7. **域名**：在 Cloudflare 配置自定义域名与 DNS

---

## 九、快速检查清单

- [ ] 敏感信息已从 appsettings.json 移除或改为环境变量
- [ ] 已添加 Npgsql，移除 SqlServer，并重新生成 Postgres 迁移
- [ ] Supabase 连接字符串已配置
- [ ] Railway 环境变量已完整配置
- [ ] CORS 已支持生产环境前端域名
- [ ] DataProtection 在 Railway 的持久化方案已确定
- [ ] Vite 构建输出与 Cloudflare Pages 配置一致
- [ ] `VITE_API_BASE_URL` 在 Cloudflare Pages 构建时已设置
- [ ] R2 Bucket CORS 已配置
- [ ] GitHub OAuth 回调 URL 已添加生产域名
- [ ] **种子数据**：
  - 方式 A：`dotnet run --project API -- seed`（需直连 Supabase，pooler 会报错）
  - 方式 B：在 Supabase SQL Editor 执行 `seed.sql`（仅插入 Videos，需先有 demo 用户）

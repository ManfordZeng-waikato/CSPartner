# Production Deployment Guide

This document explains how to prepare and deploy the CSPartner client application to production.

## Prerequisites

- Node.js 18+ and npm
- Production API server address

## Build Steps

### 1. Install Dependencies

```bash
npm install
```

### 2. Configure Environment Variables

Copy the `.env.example` file and create an `.env` file:

```bash
cp .env.example .env
```

Edit the `.env` file and set the production API base URL:

```env
VITE_API_BASE_URL=https://api.yourdomain.com
```

**Important Notes:**
- Ensure the API URL includes the protocol (https://)
- Do not include a trailing slash
- The API server needs to support CORS (if frontend and API are not on the same domain)

### 3. Run Type Check

```bash
npm run lint
```

### 4. Build Production Version

```bash
npm run build
# Or use explicit production mode
npm run build:prod
```

After building:
- **Deploy with backend**: Files will be output to `../API/wwwroot` directory
- **Standalone deployment**: Files will be in the `dist/` directory

## Build Artifacts

After building, the `dist/` directory contains:

- `index.html` - Entry HTML file
- `assets/` - Static resources (JS, CSS, images, etc.)
  - File names include hash values for cache control
  - Automatic code splitting and optimization

## Deployment Options

### Option 1: Static File Server

Deploy the contents of the `dist/` directory to any static file server, such as:

- **Nginx**: Configure to point to `dist/` directory
- **Apache**: Configure DocumentRoot to `dist/` directory
- **IIS**: Configure website root directory to `dist/` directory
- **CDN**: Upload to cloud storage (AWS S3, Azure Blob, Alibaba Cloud OSS, etc.)

#### Nginx Configuration Example

```nginx
server {
    listen 80;
    server_name yourdomain.com;
    
    root /path/to/dist;
    index index.html;
    
    # Handle client-side routing (SPA)
    location / {
        try_files $uri $uri/ /index.html;
    }
    
    # API proxy (optional, if needed)
    location /api {
        proxy_pass https://api.yourdomain.com;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
    
    # Static resource caching
    location /assets {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
```

### Option 2: Deploy with Backend (Recommended)

If your backend is ASP.NET Core, you can output the frontend build artifacts directly to the backend's `wwwroot` directory for integrated frontend-backend deployment.

#### Advantages

- ✅ Single deployment point, simplified deployment process
- ✅ No CORS configuration needed (frontend and backend same origin)
- ✅ Unified domain and HTTPS configuration
- ✅ Simpler reverse proxy configuration

#### Build Steps

1. **Configure Environment Variables**

   When deploying with the backend, the frontend uses relative paths to call the API, so `VITE_API_BASE_URL` should be left empty:

   ```env
   # .env or .env.production
   VITE_API_BASE_URL=
   ```

2. **Build Frontend**

   ```bash
   cd Client
   npm run build
   ```

   After building, files will be automatically output to the `../API/wwwroot` directory.

3. **Build and Run Backend**

   ```bash
   cd API
   dotnet build
   dotnet run
   ```

   After the backend starts, the frontend application will be automatically served from the `wwwroot` directory.

#### Build Configuration

The frontend `vite.config.ts` is configured to automatically output to the `API/wwwroot` directory during production builds:

```typescript
build: {
  outDir: isProduction ? '../API/wwwroot' : 'dist',
  // ...
}
```

#### Backend Configuration

The backend `Program.cs` is configured:

1. **Static File Service**: `app.UseStaticFiles()` serves static files from the `wwwroot` directory
2. **SPA Fallback**: All non-API routes return `index.html`, supporting client-side routing
3. **CORS**: Not needed in production (frontend and backend same origin)

#### Complete Deployment Process

```bash
# 1. Install frontend dependencies
cd Client
npm install

# 2. Build frontend (output to API/wwwroot)
npm run build

# 3. Build backend
cd ../API
dotnet build -c Release

# 4. Publish backend (optional)
dotnet publish -c Release -o ./publish

# 5. Run
dotnet run
# Or use published version
# cd publish
# dotnet API.dll
```

#### Directory Structure

Directory structure after deployment:

```
API/
├── wwwroot/              # Frontend build artifacts (auto-generated)
│   ├── index.html
│   ├── assets/
│   │   ├── index-[hash].js
│   │   └── index-[hash].css
│   └── ...
├── Controllers/
├── Program.cs
└── ...
```

#### Notes

- ⚠️ Ensure the `API/wwwroot` directory exists when building (ASP.NET Core projects create it by default)
- ⚠️ Re-run `npm run build` after each frontend update
- ⚠️ Recommend building frontend first, then backend in CI/CD pipeline
- ⚠️ The `wwwroot` directory is typically ignored in `.gitignore`, build artifacts will not be committed to the repository

#### Development Mode

In development mode, the frontend still uses an independent development server (`npm run dev`), connecting to the backend API through a proxy. Only when building production versions will the frontend output to `wwwroot`.

## Environment Variable Configuration

### Development Environment

In development, you can leave `VITE_API_BASE_URL` unset and use Vite's proxy functionality:

```env
# .env.development (optional)
VITE_API_BASE_URL=
```

### Production Environment - Deploy with Backend (Recommended)

When deploying with ASP.NET Core backend, use relative paths:

```env
# .env.production
VITE_API_BASE_URL=
```

### Production Environment - Standalone Deployment

If frontend and backend are deployed on different servers, set the complete API URL:

```env
# .env.production
VITE_API_BASE_URL=https://api.yourdomain.com
```

**Note:** Vite environment variables must start with `VITE_` to be accessible in client code.

## Build Optimization

Production builds are configured with the following optimizations:

1. **Code Splitting**: Automatically splits large dependency libraries into independent chunks
   - React-related libraries
   - Material-UI component library
   - TanStack Query
   - SignalR
   - Form validation libraries

2. **Resource Optimization**:
   - ESBuild minification
   - Tree-shaking to remove unused code
   - Resource files include hash values for caching

3. **Development Tools Removal**:
   - Production builds automatically remove development-related plugins (such as mkcert)

## Deployment Verification

After deployment, check the following:

1. ✅ Application loads normally
2. ✅ API calls work correctly (check network requests)
3. ✅ SignalR connection works (comment functionality)
4. ✅ Route navigation works (page refresh doesn't result in 404)
5. ✅ Static resources load correctly
6. ✅ HTTPS is configured correctly (recommended for production)

## Troubleshooting

### Issue: API Requests Fail

- Check if `VITE_API_BASE_URL` is set correctly
- Check API server's CORS configuration
- Check browser console for network errors

### Issue: 404 After Page Refresh

- Ensure the server is configured with SPA route redirection (see Nginx configuration example)

### Issue: SignalR Connection Fails

- Check if the API server supports WebSocket
- Check if the SignalR Hub path is correct (`/api/hubs/comments`)
- Check if firewall/proxy allows WebSocket connections

### Issue: Build Files Too Large

- Check if code splitting is enabled
- Check if sourcemaps are removed (production builds automatically disable them)
- Consider using CDN to load large libraries (such as Material-UI)

## Continuous Integration/Continuous Deployment (CI/CD)

### GitHub Actions Example

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
        # Add your deployment steps
```

## Security Considerations

1. **Do not expose sensitive information in client code** (API keys, secrets, etc.)
2. **Use HTTPS** Enforce HTTPS in production
3. **Set appropriate CSP headers** (Content Security Policy)
4. **Regularly update dependencies** Run `npm audit` to check for security vulnerabilities

## Performance Optimization Recommendations

1. **Enable Gzip/Brotli compression** at the server level
2. **Use CDN** to accelerate static resource loading
3. **Configure appropriate caching strategies**
4. **Monitor and optimize** Use tools like Lighthouse to analyze performance

## Support

For issues, please refer to:
- [Vite Documentation](https://vite.dev/)
- [React Documentation](https://react.dev/)
- Project README.md

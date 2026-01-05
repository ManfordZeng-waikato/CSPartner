# CSPartner

A modern CS2 (Counter-Strike 2) highlight video sharing platform built with .NET 10.0 and React 19, featuring AI-powered video descriptions, real-time comments, video uploads, and user profiles. The application follows Clean Architecture principles and uses Cloudflare R2 for object storage.

## 🌐 Live Demo

**🔗 Production Site**: [www.cspartner.org](https://www.cspartner.org) | [Azure App Service](https://cspartner.azurewebsites.net)

> **Note**: This project is deployed on Microsoft Azure and is accessible via the production domain. The application demonstrates a full-stack implementation with modern cloud technologies and CI/CD practices.

## 🚀 Features

- **Video Management**
  - Upload CS2 highlight videos with pre-signed URLs for direct upload to Cloudflare R2
  - Video visibility controls (Public/Private)
  - Video likes and view count tracking
  - Cursor-based pagination for efficient video browsing
  - Video tagging system with highlight types (Ace, Clutch, Flick, SprayTransfer, Wallbang, FunnyMoment, UtilityPlay, OpeningKill)
  - Map and weapon tagging for better video organization

- **AI-Powered Features**
  - **Automatic AI Description Generation**: Automatically generates concise video descriptions using OpenAI based on video metadata (map, weapon, highlight type)
  - Background processing: AI description generation runs automatically after video upload
  - Manual trigger: Users can manually regenerate AI descriptions via API
  - Status tracking: Real-time AI processing status (Pending, Completed, Failed)
  - Error handling: Graceful handling of AI service errors and quota limits

- **Real-time Comments**
  - Threaded comment system with replies
  - Real-time comment updates via SignalR
  - Comment deletion support

- **User Management**
  - JWT-based authentication
  - User registration and login
  - User profile management
  - Role-based access control (User/Admin)

- **Modern Frontend**
  - React 19 with TypeScript
  - Material-UI components
  - React Query for data fetching
  - Form validation with React Hook Form and Zod

- **Future Features** (Planned)
  - **Social Matching & Friend Recommendations**: Intelligent friend recommendation system based on highlight video tags and types
    - Match users with similar gameplay styles and preferences
    - Recommend friends based on shared interests in specific highlight types (Ace, Clutch, Flick, etc.)
    - Connect players who enjoy similar maps and weapons
    - Build a community around CS2 gameplay styles and skills

## 🏗️ Architecture

The project follows **Clean Architecture** principles with clear separation of concerns:

- **Domain**: Core business entities and domain logic (no dependencies)
- **Application**: Use cases, DTOs, and application services (CQRS with MediatR)
- **Infrastructure**: Data persistence, external services (R2 storage, Identity, AI services, Azure services)
- **API**: Controllers, SignalR hubs, and API-specific configurations
- **Client**: React frontend application

### Architecture Highlights

- **Clean Architecture**: Dependency inversion, separation of concerns
- **CQRS Pattern**: Command Query Responsibility Segregation via MediatR
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Dependency Injection**: Full DI container usage
- **Pipeline Behaviors**: Cross-cutting concerns (logging, validation, transactions)
- **Global Exception Handling**: Centralized exception handling middleware for consistent error responses
- **API Documentation**: Swagger/OpenAPI integration with JWT authentication support
- **Real-time Communication**: SignalR for live updates
- **Cloud-Native**: Designed for Azure cloud deployment

## 📋 Prerequisites

- **.NET 10.0 SDK** or later
- **Node.js** 18.x or later
- **SQL Server** (LocalDB for development, or SQL Server for production)
- **Cloudflare R2** account with API token and bucket configured
- **OpenAI API** account with API key (for AI description generation)
- **Resend** account (for email services) - Optional for development
- **GitHub OAuth App** (for GitHub authentication) - Optional
- **Azure Key Vault** (for Data Protection keys in production) - Optional for development

## 🛠️ Technology Stack

### Backend & API
- **.NET 10.0** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core 10.0** - ORM for database operations
- **MediatR 12.4.1** - CQRS pattern implementation
- **SignalR** - Real-time bidirectional communication
- **JWT Bearer Authentication** - Token-based authentication
- **ASP.NET Core Identity** - User management and authentication
- **AWS SDK for S3** - Cloudflare R2 compatibility (S3-compatible API)
- **OpenAI API** - AI-powered video description generation
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Input validation (via MediatR behaviors)

### Frontend
- **React 19** - Latest React framework
- **TypeScript** - Type-safe JavaScript
- **Vite 7.2.4** - Next-generation frontend build tool
- **Material-UI (MUI) 7.3.6** - React component library
- **React Query (TanStack Query) 5.90.12** - Server state management
- **React Router 7.10.1** - Client-side routing
- **React Hook Form 7.68.0** - Performant form library
- **Zod 4.2.1** - TypeScript-first schema validation
- **Axios 1.13.2** - HTTP client
- **SignalR Client (@microsoft/signalr 10.0.0)** - Real-time communication
- **Emotion** - CSS-in-JS styling solution

### Database & Persistence
- **SQL Server / SQL Server LocalDB** - Relational database
- **Entity Framework Core Migrations** - Database versioning

### Cloud Services & Infrastructure

#### Microsoft Azure
- **Azure App Service** - Web application hosting
- **Azure SQL Database** - Managed SQL Server (production)
- **Azure Key Vault** - Secure storage for secrets and keys
- **Azure Blob Storage** - Data Protection key storage
- **Azure Identity (DefaultAzureCredential)** - Managed identity authentication

#### Cloudflare
- **Cloudflare R2** - S3-compatible object storage for video files
- **Cloudflare R2 Public URLs** - CDN for video delivery

#### Third-Party Services
- **OpenAI** - AI service for video description generation
- **Resend** - Transactional email service
- **GitHub OAuth** - Social authentication provider

### DevOps & CI/CD
- **GitHub Actions** - Continuous Integration and Deployment
- **Azure DevOps** - CI/CD pipeline (if applicable)
- **Environment-based Configuration** - Separate dev/staging/prod configs
- **Automated Database Migrations** - EF Core migrations in deployment pipeline

### Development Tools
- **Visual Studio / VS Code** - IDE
- **Postman / Swagger** - API testing
- **Git** - Version control
- **npm / dotnet CLI** - Package management

## 🚀 Quick Start

For a quick setup to get the project running:

1. **Clone the repository**
2. **Create `API/appsettings.json`** using the template below
3. **Configure minimum required settings**:
   - Database connection string
   - Cloudflare R2 credentials (required for video uploads)
   - JWT secret key
4. **Run database migrations**
5. **Start backend and frontend**

See detailed instructions below.

## 📦 Installation

### 1. Clone the Repository

```bash
git clone <repository-url>
cd CSPartner
```

### 2. Backend Setup

1. Navigate to the API directory:
```bash
cd API
```

2. Create `appsettings.json` file (if it doesn't exist) and configure it with the following template:

**⚠️ Important**: The `appsettings.json` file is not included in the repository for security reasons. You need to create it manually.

Create `API/appsettings.json` with the following template:

```json
{
  "ClientApp": {
    "ClientUrl": "https://localhost:3000"
  },
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=CSPartnerDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "CloudflareR2": {
    "AccessKeyId": "your-r2-access-key-id",
    "SecretAccessKey": "your-r2-secret-access-key",
    "AccountId": "your-cloudflare-account-id",
    "BucketName": "your-bucket-name",
    "PublicUrl": "https://your-public-url.r2.dev",
    "S3ServiceUrl": "https://your-account-id.r2.cloudflarestorage.com"
  },
  "Jwt": {
    "Issuer": "https://localhost:3000",
    "Audience": "https://localhost:3000",
    "SecretKey": "YourSuperSecretKeyForJWTTokenGenerationMustBeAtLeast32CharactersLong!",
    "ExpirationMinutes": 1440
  },
  "Resend": {
    "ApiToken": "your-resend-api-token",
    "FromEmail": "no-reply@yourdomain.com"
  },
  "Authentication": {
    "Github": {
      "ClientId": "your-github-oauth-client-id",
      "ClientSecret": "your-github-oauth-client-secret"
    }
  },
  "DataProtection": {
    "KeyVaultUri": "https://your-keyvault-name.vault.azure.net/"
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "Model": "gpt-5"
  },
  "Seed": {
    "DemoData": true,
    "DemoUserPassword": "Demo@12345"
  }
}
```

**Configuration Notes:**

**Required for basic functionality:**
- ✅ **ConnectionStrings**: Update the connection string to match your SQL Server instance
- ✅ **CloudflareR2**: Required for video storage. See [Cloudflare R2 Setup](#cloudflare-r2-setup) section below
- ✅ **Jwt**: Generate a secure secret key (at least 32 characters). For production, use a strong random key
- ✅ **OpenAI**: Required for AI description generation. See [OpenAI Setup](#openai-setup) section below

**Optional (can be omitted for basic testing):**
- ⚪ **Resend**: Optional for development. Required for email features (password reset, etc.). You can leave empty values if not using email features
- ⚪ **Authentication.Github**: Optional. Required only if you want GitHub OAuth login. You can omit this section if not using GitHub login
- ⚪ **DataProtection**: Optional for development. Required for production deployments on Azure. For local development, you can omit this or leave empty
- ⚪ **Seed**: Set `DemoData` to `true` to seed demo data on first run. Set to `false` if you don't want demo data

**Minimum working configuration** (for quick testing):
```json
{
  "ClientApp": {
    "ClientUrl": "https://localhost:3000"
  },
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=CSPartnerDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "CloudflareR2": {
    "AccessKeyId": "your-r2-access-key-id",
    "SecretAccessKey": "your-r2-secret-access-key",
    "AccountId": "your-cloudflare-account-id",
    "BucketName": "your-bucket-name",
    "PublicUrl": "https://your-public-url.r2.dev",
    "S3ServiceUrl": "https://your-account-id.r2.cloudflarestorage.com"
  },
  "Jwt": {
    "Issuer": "https://localhost:3000",
    "Audience": "https://localhost:3000",
    "SecretKey": "YourSuperSecretKeyForJWTTokenGenerationMustBeAtLeast32CharactersLong!",
    "ExpirationMinutes": 1440
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "Model": "gpt-5"
  }
}
```

3. Restore dependencies:
```bash
dotnet restore
```

4. Run database migrations:
```bash
dotnet ef database update --project ../Infrastructure --startup-project .
```

5. Run the API:
```bash
dotnet run
```

The API will be available at `https://localhost:5001` (or the port configured in `launchSettings.json`).

**API Documentation (Swagger)**:
- In development mode, Swagger UI is available at `https://localhost:5001/swagger`
- Swagger supports JWT authentication - click "Authorize" button and enter: `Bearer {your-jwt-token}`
- All API endpoints are documented with request/response schemas

### 3. Frontend Setup

1. Navigate to the Client directory:
```bash
cd Client
```

2. Install dependencies:
```bash
npm install
```

3. **Configure API endpoint** (if needed):
   - The default configuration in `vite.config.ts` proxies `/api` requests to `https://localhost:5001` in development
   - If your API runs on a different port, update the `proxy.target` in `Client/vite.config.ts`

4. Run the development server:
```bash
npm run dev
```

The frontend will be available at `http://localhost:3000`.

### 4. Production Build

To build the frontend for production deployment:

```bash
cd Client
npm run build:prod
```

This will build the React app and output it to `API/wwwroot`, ready to be served by the ASP.NET Core application.

## 🔧 Configuration

### Cloudflare R2 Setup

Cloudflare R2 is used for video file storage. To configure:

1. **Create an R2 bucket** in your Cloudflare dashboard
   - Go to Cloudflare Dashboard → R2 → Create bucket
   - Choose a bucket name (e.g., "highlights")

2. **Create R2 API Token**:
   - Go to Cloudflare Dashboard → R2 → Manage R2 API Tokens
   - Click "Create API token"
   - Select "Object Read & Write" permissions
   - Save the `Access Key ID` and `Secret Access Key`

3. **Get your Account ID**:
   - Found in the Cloudflare dashboard URL or R2 overview page

4. **Configure Public URL** (optional):
   - If you want public access to videos, create a custom domain or use the default R2.dev domain
   - Go to R2 → Your bucket → Settings → Public access
   - Enable public access and note the public URL

5. **Update `appsettings.json`**:
   ```json
   "CloudflareR2": {
     "AccessKeyId": "your-access-key-id",
     "SecretAccessKey": "your-secret-access-key",
     "AccountId": "your-account-id",
     "BucketName": "your-bucket-name",
     "PublicUrl": "https://your-public-url.r2.dev",
     "S3ServiceUrl": "https://your-account-id.r2.cloudflarestorage.com"
   }
   ```

### Database Connection

Update the `ConnectionStrings:Default` in `appsettings.json` to point to your SQL Server instance.

**For LocalDB (Development):**
```json
"ConnectionStrings": {
  "Default": "Server=(localdb)\\MSSQLLocalDB;Database=CSPartnerDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

**For SQL Server:**
```json
"ConnectionStrings": {
  "Default": "Server=localhost;Database=CSPartnerDb;User Id=sa;Password=YourPassword;TrustServerCertificate=true;MultipleActiveResultSets=true"
}
```

### JWT Configuration

1. **Generate a secure secret key** (at least 32 characters):
   ```bash
   # Using PowerShell
   [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
   
   # Or use an online generator
   ```

2. **Update `appsettings.json`**:
   ```json
   "Jwt": {
     "Issuer": "https://localhost:3000",
     "Audience": "https://localhost:3000",
     "SecretKey": "your-generated-secret-key-at-least-32-characters",
     "ExpirationMinutes": 1440
   }
   ```

3. **For production**: Use environment variables or Azure Key Vault instead of storing in `appsettings.json`

### Resend Email Service (Optional)

Resend is used for sending emails (password reset, etc.). To configure:

1. **Sign up** at [resend.com](https://resend.com)
2. **Create an API key** in the Resend dashboard
3. **Verify a domain** (or use the test domain for development)
4. **Update `appsettings.json`**:
   ```json
   "Resend": {
     "ApiToken": "re_your_api_token_here",
     "FromEmail": "no-reply@yourdomain.com"
   }
   ```

**Note**: Email features will not work without this configuration, but the application will still run.

### OpenAI Setup (Required for AI Features)

OpenAI is used for automatic video description generation. To configure:

1. **Sign up** at [platform.openai.com](https://platform.openai.com)
2. **Create an API key** in the OpenAI dashboard
3. **Choose a model** (default is `gpt-5`, but you can use other models like `gpt-4o`, `gpt-4-turbo`, etc.)
4. **Update `appsettings.json`**:
   ```json
   "OpenAI": {
     "ApiKey": "sk-proj-your-api-key-here",
     "Model": "gpt-5"
   }
   ```

**How it works:**
- When a video is uploaded with metadata (map, weapon, highlight type), the system automatically generates a concise description using AI
- The AI description is generated in the background after video creation
- Users can also manually trigger AI description generation via the API endpoint
- AI processing status is tracked (Pending, Completed, Failed) for each video

**Note**: AI description generation requires valid OpenAI API credentials. If not configured, videos will still be created but AI descriptions will not be generated.

### GitHub OAuth (Optional)

To enable GitHub OAuth login:

1. **Create a GitHub OAuth App**:
   - Go to GitHub → Settings → Developer settings → OAuth Apps
   - Click "New OAuth App"
   - Set Authorization callback URL: `https://localhost:5001/api/account/github-callback`
   - Save the `Client ID` and generate a `Client Secret`

2. **Update `appsettings.json`**:
   ```json
   "Authentication": {
     "Github": {
       "ClientId": "your-github-client-id",
       "ClientSecret": "your-github-client-secret"
     }
   }
   ```

**Note**: GitHub OAuth is optional. Users can still register and login with email/password.

### Azure Key Vault (Optional - Production)

For production deployments, Data Protection keys can be stored in Azure Key Vault:

1. **Create an Azure Key Vault**
2. **Update `appsettings.json`**:
   ```json
   "DataProtection": {
     "KeyVaultUri": "https://your-keyvault-name.vault.azure.net/"
   }
   ```

**Note**: For development, Data Protection keys are stored locally. This is optional for local development.

### Database Seeding

To seed demo data on first run:

```json
"Seed": {
  "DemoData": true,
  "DemoUserPassword": "Demo@12345"
}
```

When `DemoData` is `true`, the application will:
- Create default roles (User, Admin)
- Create a demo user: `demo@highlighthub.local` with the password specified in `DemoUserPassword`
- Seed sample videos and comments

**Security Note**: Set `DemoData` to `false` in production environments.

## 📁 Project Structure

```
CSPartner/
├── API/                    # Web API layer (controllers, SignalR hubs)
│   ├── Controllers/       # API endpoints
│   ├── SignalR/          # SignalR hubs for real-time features
│   ├── Middleware/       # Global exception handling middleware
│   ├── Seed/             # Database seeding logic
│   └── wwwroot/          # Static files (frontend build output)
├── Application/           # Application layer (use cases, DTOs)
│   ├── Features/         # Feature-based organization (CQRS)
│   │   └── Videos/       # Video features including AI generation
│   ├── DTOs/             # Data transfer objects
│   │   └── Ai/           # AI-related DTOs
│   ├── Mappings/         # Object mappings
│   └── Behaviors/        # MediatR pipeline behaviors
├── Domain/                # Domain layer (entities, domain logic)
│   ├── Videos/           # Video domain entities (HighlightVideo, HighlightType)
│   ├── Comments/         # Comment domain entities
│   ├── Users/            # User domain entities
│   └── Ai/               # AI status and domain logic
├── Infrastructure/        # Infrastructure layer
│   ├── Persistence/      # EF Core context and configurations
│   ├── Identity/         # ASP.NET Core Identity implementation
│   ├── Storage/          # Cloudflare R2 storage service
│   └── AI/               # AI service implementations (OpenAI)
└── Client/                # React frontend
    ├── src/
    │   ├── app/          # App layout and routing
    │   ├── features/     # Feature-based components
    │   ├── lib/          # Utilities, API client, types
    │   └── main.tsx      # Application entry point
    └── public/           # Static assets
```

## 🔌 API Endpoints

### Authentication
- `POST /api/account/register` - Register a new user
- `POST /api/account/login` - User login
- `GET /api/account/me` - Get current user info

### Videos
- `GET /api/videos` - Get paginated video list (cursor-based)
- `GET /api/videos/{id}` - Get video by ID
- `GET /api/videos/user/{userId}` - Get videos by user
- `POST /api/videos/upload-url` - Get pre-signed URL for video upload
- `POST /api/videos` - Create video record (automatically triggers AI description generation)
- `PUT /api/videos/{id}` - Update video
- `DELETE /api/videos/{id}` - Delete video
- `POST /api/videos/{id}/like` - Toggle video like
- `POST /api/videos/{id}/view` - Increment view count
- `POST /api/videos/{id}/ai-meta` - Manually trigger AI description generation for a video

### Comments
- `GET /api/videos/{videoId}/comments` - Get video comments
- `POST /api/videos/{videoId}/comments` - Create comment
- `PUT /api/comments/{id}` - Update comment
- `DELETE /api/comments/{id}` - Delete comment

### User Profiles
- `GET /api/user-profiles/{userId}` - Get user profile
- `PUT /api/user-profiles/{userId}` - Update user profile

### SignalR Hubs
- `/api/hubs/comments` - Real-time comment updates

## 🧪 Development

### Running Migrations

```bash
cd Infrastructure
dotnet ef migrations add MigrationName --startup-project ../API
dotnet ef database update --startup-project ../API
```

### Database Seeding

Demo data seeding is controlled by the `Seed:DemoData` configuration flag in `appsettings.json`. When enabled, the application will:
- Create default roles (User, Admin)
- Create a demo user (`demo@highlighthub.local`) with the password from `Seed:DemoUserPassword`
- Seed sample videos and comments

To enable seeding, set in `appsettings.json`:
```json
"Seed": {
  "DemoData": true,
  "DemoUserPassword": "YourSecurePassword"
}
```

**Note**: Seeding only runs on first startup. Subsequent runs will skip seeding if data already exists.

### Frontend Development

The frontend uses Vite with hot module replacement. Changes will be reflected immediately in the browser.

### API Testing with Swagger

The API includes Swagger/OpenAPI documentation for easy testing:

1. **Access Swagger UI**: Navigate to `https://localhost:5001/swagger` when the API is running in development mode
2. **JWT Authentication**: 
   - First, login via `/api/account/login` to get a JWT token
   - Click the "Authorize" button in Swagger UI
   - Enter: `Bearer {your-jwt-token}` (replace `{your-jwt-token}` with the actual token)
   - Click "Authorize" to authenticate
3. **Test Endpoints**: All endpoints are documented with request/response schemas and can be tested directly from Swagger UI

### Exception Handling

The application uses a global exception handling middleware (`ExceptionHandlingMiddleware`) that:

- **Centralized Error Handling**: Catches all unhandled exceptions and converts them to appropriate HTTP responses
- **Domain Exception Mapping**: Maps domain exceptions to HTTP status codes:
  - `VideoNotFoundException`, `CommentNotFoundException`, `UserProfileNotFoundException` → 404 Not Found
  - `UnauthorizedOperationException` → 403 Forbidden
  - `AuthenticationRequiredException` → 401 Unauthorized
  - `RateLimitExceededException` → 429 Too Many Requests
  - `DomainException`, `InvalidCommentStateException` → 400 Bad Request
  - Unhandled exceptions → 500 Internal Server Error
- **Consistent Error Format**: All errors are returned in a consistent JSON format with status code and message
- **Logging**: All exceptions are logged with appropriate log levels (Warning for client errors, Error for server errors)

## 🚢 Deployment

### Azure Cloud Deployment

This project is **currently deployed on Microsoft Azure** and demonstrates production-ready cloud architecture:

#### Azure Services Used

1. **Azure App Service**
   - Hosts the ASP.NET Core Web API
   - Automatic scaling and load balancing
   - HTTPS/SSL certificate management
   - Custom domain configuration

2. **Azure SQL Database**
   - Managed SQL Server instance for production
   - Automated backups and high availability
   - Connection string stored in App Service configuration

3. **Azure Key Vault**
   - Secure storage for sensitive configuration:
     - JWT secret keys
     - Database connection strings
     - Third-party API keys (Resend, GitHub OAuth, Cloudflare R2)
   - Integrated with Azure Identity for secure access

4. **Azure Blob Storage**
   - Stores ASP.NET Core Data Protection keys
   - Ensures key persistence across deployments
   - Integrated with Key Vault for encryption

5. **Azure Identity (Managed Identity)**
   - DefaultAzureCredential for service-to-service authentication
   - No secrets in code or configuration files
   - Automatic credential rotation

#### Deployment Architecture

```
┌─────────────────┐
│  Azure App      │
│  Service        │──┐
│  (Web API)      │  │
└─────────────────┘  │
                     │
┌─────────────────┐  │  ┌──────────────────┐
│  Azure SQL      │◄─┼──│  Azure Key Vault │
│  Database       │  │  │  (Secrets)       │
└─────────────────┘  │  └──────────────────┘
                     │
┌─────────────────┐  │  ┌──────────────────┐
│  Cloudflare R2  │◄─┼──│  Azure Blob      │
│  (Video Storage)│  │  │  Storage         │
└─────────────────┘  │  └──────────────────┘
```

### CI/CD Pipeline

The project implements **Continuous Integration and Continuous Deployment** with **active CI/CD pipelines running**:

#### GitHub Actions Workflow (Active)

- ✅ **Automated Build**: On every push to main branch
- ✅ **Automated Testing**: Runs unit tests and integration tests
- ✅ **Automated Deployment**: Deploys to Azure App Service on successful build
- ✅ **Database Migrations**: Automatically runs EF Core migrations
- ✅ **Environment Configuration**: Separate configurations for dev/staging/prod
- ✅ **Continuous Monitoring**: Pipeline status and deployment logs tracked

> **Status**: CI/CD pipeline is **actively running** and automatically deploying updates to production on code changes.

#### Pipeline Stages

1. **Build Stage**
   - Restore .NET dependencies
   - Build solution
   - Build React frontend
   - Run linting and type checking

2. **Test Stage**
   - Run unit tests
   - Run integration tests
   - Code coverage reports

3. **Deploy Stage**
   - Deploy to Azure App Service
   - Run database migrations
   - Update application settings from Key Vault
   - Health check verification

### Manual Deployment Steps

If deploying manually:

1. **Create `appsettings.Production.json`** with production configuration:
   - Use Azure Key Vault references for sensitive data
   - Update connection strings for Azure SQL Database
   - Configure production URLs for ClientApp, JWT Issuer/Audience
   - Set `Seed:DemoData` to `false`

2. **Build the solution**:
```bash
dotnet publish API/API.csproj -c Release -o ./publish
```

3. **Deploy to Azure App Service**:
   - Use Azure Portal, Azure CLI, or Visual Studio
   - Configure App Service settings from Key Vault
   - Enable managed identity for Key Vault access

4. **Configure environment variables** in Azure App Service:
   - Set sensitive values as App Service Application Settings
   - Reference Azure Key Vault secrets
   - The application will read from environment variables automatically

### Frontend Deployment

The frontend is built into `API/wwwroot` and served as static files by the ASP.NET Core application. For production:

```bash
cd Client
npm run build:prod
```

The built files will be in `API/wwwroot` and served automatically by the API. This is handled automatically in the CI/CD pipeline.

### Production Configuration Best Practices

- ✅ **Never commit** `appsettings.json` with real credentials
- ✅ **Use Azure Key Vault** for all secrets in production
- ✅ **Enable Managed Identity** for secure service-to-service communication
- ✅ **Configure CORS** properly for production domain
- ✅ **Enable HTTPS only** in production
- ✅ **Set up monitoring** with Application Insights
- ✅ **Configure auto-scaling** based on traffic
- ✅ **Set up staging slots** for zero-downtime deployments

## ⚠️ Troubleshooting

### Common Issues

1. **Database connection errors**:
   - Ensure SQL Server LocalDB is installed (comes with Visual Studio)
   - Or install SQL Server Express
   - Verify the connection string in `appsettings.json`

2. **Cloudflare R2 errors**:
   - Verify all R2 credentials are correct
   - Ensure the bucket exists and has proper permissions
   - Check that the S3ServiceUrl format is correct: `https://{accountId}.r2.cloudflarestorage.com`

3. **JWT authentication fails**:
   - Ensure `Jwt:SecretKey` is at least 32 characters
   - Verify `Jwt:Issuer` and `Jwt:Audience` match your frontend URL

4. **Frontend can't connect to API**:
   - Ensure the API is running on `https://localhost:5001`
   - Check CORS configuration in `appsettings.json`
   - Verify the proxy configuration in `Client/vite.config.ts`

6. **Email features not working**:
   - Resend configuration is optional for development
   - Email features require valid Resend API token and verified domain

7. **Migrations fail**:
   - Ensure you're running migrations from the correct directory
   - Use: `dotnet ef database update --project ../Infrastructure --startup-project .`
   - From the `API` directory

8. **AI description generation fails**:
   - Verify OpenAI API key is correct in `appsettings.json`
   - Check OpenAI API quota and billing status
   - Ensure the model name is correct (default: `gpt-5`)
   - Check application logs for detailed error messages
   - AI generation runs in background - check video's `aiStatus` field to see processing status
   - If quota is exceeded, videos will still be created but AI descriptions will fail with status `Failed`

## 🔒 Security Considerations

- **JWT tokens** are used for authentication
- **Password requirements**: minimum 8 characters, requires digit and uppercase
- **CORS** is configured for development and production
- **File uploads** use pre-signed URLs for direct upload to R2 (reduces server load)
- **Video file existence** is verified before creating database records
- **AI API keys** are stored securely and should never be committed to version control
- **Sensitive configuration** should use environment variables or Azure Key Vault in production
- **Never commit** `appsettings.json` with real credentials to version control

## 🎯 Project Highlights

This project demonstrates comprehensive full-stack development skills and modern cloud practices:

### Technical Skills Demonstrated

- ✅ **Full-Stack Development**: Modern .NET 10.0 backend with React 19 frontend
- ✅ **AI Integration**: OpenAI API integration for intelligent video description generation
- ✅ **Cloud Architecture**: Production deployment on Microsoft Azure with multiple services
- ✅ **DevOps & CI/CD**: Active continuous integration and deployment pipeline
- ✅ **Security Best Practices**: Azure Key Vault integration, JWT authentication, managed identities
- ✅ **Real-time Features**: SignalR WebSocket implementation for live updates
- ✅ **Software Architecture**: Clean Architecture, CQRS pattern, Repository pattern, Unit of Work
- ✅ **Modern Frontend**: React 19, TypeScript, Material-UI, React Query, Vite
- ✅ **API Design**: RESTful APIs with proper error handling, validation, and documentation
- ✅ **Database Management**: Entity Framework Core, migrations, transaction management
- ✅ **Third-party Integrations**: OpenAI, Cloudflare R2, Resend email service, GitHub OAuth
- ✅ **Background Processing**: Asynchronous AI processing with proper error handling and status tracking

### Azure Cloud Services Experience

- **Azure App Service**: Web application hosting and deployment
- **Azure SQL Database**: Managed database service
- **Azure Key Vault**: Secrets and keys management
- **Azure Blob Storage**: Data Protection key persistence
- **Azure Identity**: Managed identity and service authentication

### Development Practices

- Clean code principles and SOLID design patterns
- Dependency injection and inversion of control
- Automated testing and quality assurance
- Version control with Git
- Agile development methodologies


## 📧 Contact

📧 Email: [manfordnz@hotmail.com](mailto:manfordnz@hotmail.com)



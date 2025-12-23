# CSPartner

A modern video sharing platform built with .NET 10.0 and React 19, featuring real-time comments, video uploads, and user profiles. The application follows Clean Architecture principles and uses Cloudflare R2 for object storage.

## 🚀 Features

- **Video Management**
  - Upload videos with pre-signed URLs for direct upload to Cloudflare R2
  - Video visibility controls (Public/Private)
  - Video likes and view count tracking
  - Cursor-based pagination for efficient video browsing

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

## 🏗️ Architecture

The project follows Clean Architecture principles with clear separation of concerns:

- **Domain**: Core business entities and domain logic
- **Application**: Use cases, DTOs, and application services (CQRS with MediatR)
- **Infrastructure**: Data persistence, external services (R2 storage, Identity)
- **API**: Controllers, SignalR hubs, and API-specific configurations
- **Client**: React frontend application

## 📋 Prerequisites

- **.NET 10.0 SDK** or later
- **Node.js** 18.x or later
- **SQL Server** (LocalDB for development, or SQL Server for production)
- **Cloudflare R2** account with API token and bucket configured

## 🛠️ Technology Stack

### Backend
- .NET 10.0
- ASP.NET Core Web API
- Entity Framework Core 10.0
- MediatR (CQRS pattern)
- SignalR (real-time communication)
- JWT Bearer Authentication
- ASP.NET Core Identity
- AWS SDK for S3 (Cloudflare R2 compatibility)

### Frontend
- React 19
- TypeScript
- Vite
- Material-UI (MUI)
- React Query (TanStack Query)
- React Router
- React Hook Form
- Zod (schema validation)
- Axios
- SignalR Client

### Database
- SQL Server / SQL Server LocalDB

### Storage
- Cloudflare R2 (S3-compatible object storage)

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

2. Configure `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=CSPartnerDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "CloudflareR2": {
    "S3ServiceUrl": "https://your-account-id.r2.cloudflarestorage.com",
    "AccountId": "your-account-id",
    "BucketName": "your-bucket-name",
    "PublicUrl": "https://your-public-url.r2.dev"
  },
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGenerationMustBeAtLeast32CharactersLong!",
    "Issuer": "CSPartner",
    "Audience": "CSPartner",
    "ExpirationMinutes": 1440
  },
  "Seed": {
    "DemoData": true,
    "DemoUserPassword": "P@ssw0rd!"
  }
}
```

3. Restore dependencies and run migrations:
```bash
dotnet restore
dotnet ef database update --project ../Infrastructure
```

4. Run the API:
```bash
dotnet run
```

The API will be available at `https://localhost:5001` (or the port configured in `launchSettings.json`).

### 3. Frontend Setup

1. Navigate to the Client directory:
```bash
cd Client
```

2. Install dependencies:
```bash
npm install
```

3. Configure API endpoint (if needed):
   - The default configuration proxies `/api` requests to `https://localhost:5001` in development
   - Update `vite.config.ts` if your API runs on a different port

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

1. Create an R2 bucket in your Cloudflare dashboard
2. Generate an API token with read/write permissions
3. Configure the following in `appsettings.json`:
   - `S3ServiceUrl`: Your R2 S3-compatible endpoint
   - `AccountId`: Your Cloudflare account ID
   - `BucketName`: Your R2 bucket name
   - `PublicUrl`: Your R2 public URL (if using public access)

### Database Connection

Update the `ConnectionStrings:Default` in `appsettings.json` to point to your SQL Server instance.

### JWT Configuration

Ensure `Jwt:SecretKey` is at least 32 characters long and kept secure in production. Consider using environment variables or Azure Key Vault for production deployments.

## 📁 Project Structure

```
CSPartner/
├── API/                    # Web API layer (controllers, SignalR hubs)
│   ├── Controllers/       # API endpoints
│   ├── SignalR/          # SignalR hubs for real-time features
│   ├── Seed/             # Database seeding logic
│   └── wwwroot/          # Static files (frontend build output)
├── Application/           # Application layer (use cases, DTOs)
│   ├── Features/         # Feature-based organization (CQRS)
│   ├── DTOs/             # Data transfer objects
│   ├── Mappings/         # Object mappings
│   └── Behaviors/        # MediatR pipeline behaviors
├── Domain/                # Domain layer (entities, domain logic)
│   ├── Videos/           # Video domain entities
│   ├── Comments/         # Comment domain entities
│   └── Users/            # User domain entities
├── Infrastructure/        # Infrastructure layer
│   ├── Persistence/      # EF Core context and configurations
│   ├── Identity/         # ASP.NET Core Identity implementation
│   └── Storage/          # Cloudflare R2 storage service
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
- `POST /api/videos` - Create video record
- `PUT /api/videos/{id}` - Update video
- `DELETE /api/videos/{id}` - Delete video
- `POST /api/videos/{id}/like` - Toggle video like
- `POST /api/videos/{id}/view` - Increment view count

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

Demo data seeding is controlled by the `Seed:DemoData` configuration flag. When enabled, the application will:
- Create default roles (User, Admin)
- Create a demo user (`demo@highlighthub.local`)
- Seed sample videos and comments

### Frontend Development

The frontend uses Vite with hot module replacement. Changes will be reflected immediately in the browser.

## 🚢 Deployment

### Backend Deployment

1. Build the solution:
```bash
dotnet publish API/API.csproj -c Release -o ./publish
```

2. Configure production settings in `appsettings.Production.json` or use environment variables

3. Deploy to your hosting platform (Azure App Service, AWS, etc.)

### Frontend Deployment

The frontend is built into `API/wwwroot` and served as static files by the ASP.NET Core application. For production:

```bash
cd Client
npm run build:prod
```

The built files will be in `API/wwwroot` and served automatically by the API.

## 🔒 Security Considerations

- JWT tokens are used for authentication
- Password requirements: minimum 8 characters, requires digit and uppercase
- CORS is configured for development and production
- File uploads use pre-signed URLs for direct upload to R2 (reduces server load)
- Video file existence is verified before creating database records

## 📝 License

[Specify your license here]

## 🤝 Contributing

[Add contribution guidelines if applicable]

## 📧 Contact

[Add contact information if applicable]


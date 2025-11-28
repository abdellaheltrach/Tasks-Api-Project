# Task Management API

A secure, production-ready RESTful API for task management with JWT authentication, refresh tokens, and role-based access control.

## 🏗️ Architecture

This project follows a **Clean Architecture** pattern with clear separation of concerns:

```
LoginApp.Api/          # Presentation Layer (Controllers, Middleware)
LoginApp.Business/     # Business Logic Layer (Services, DTOs)
LoginApp.DataAccess/   # Data Access Layer (Entities, Repositories, EF Core)
```

### Key Design Patterns

- **Repository Pattern**: Abstracts data access logic
- **Dependency Injection**: Promotes loose coupling and testability
- **DTOs (Data Transfer Objects)**: Separates API contracts from database entities
- **Soft Delete**: Preserves data integrity with `IsDeleted` flag

## ✨ Features

### Authentication & Authorization

- ✅ JWT-based authentication with **Access Tokens** (short-lived)
- ✅ **Refresh Tokens** stored in HttpOnly cookies for security
- ✅ Device-specific token management (multi-device support)
- ✅ Role-based authorization (`Guest`, `Admin`)
- ✅ Password hashing with BCrypt

### Task Management

- ✅ Create, Read, Update, Delete (CRUD) operations
- ✅ Task status tracking (Pending, In Progress, Done)
- ✅ Quick status updates via `PATCH` endpoint
- ✅ Due date support
- ✅ Soft delete with recovery option

### Security

- ✅ **IDOR Protection**: Users can only access their own tasks
- ✅ **Input Validation**: Data annotations on all DTOs
- ✅ **CORS Configuration**: Controlled cross-origin access
- ✅ **HttpOnly Cookies**: Refresh tokens inaccessible to JavaScript

## 🛠️ Technologies

- **Backend**: ASP.NET Core (.NET 10.0)
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **Frontend**: Vanilla HTML/CSS/JavaScript
- **ORM**: Entity Framework Core 9.0

## 🚀 Getting Started

### Option 1: Run with Docker (Recommended)

#### Prerequisites

- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose

#### Quick Start

1. **Clone the repository** (if not already done):

   ```bash
   git clone <repository-url>
   cd Tasks-Project
   ```

2. **Build and start containers**:

   ```bash
   docker-compose up --build -d
   ```

3. **Check container status**:

   ```bash
   docker-compose ps
   ```

   Wait until `sqlserver` shows as `healthy` and `api` is `Up`.

4. **Run database migrations** (first time only):

   ```bash
   docker-compose exec api dotnet ef database update
   ```

5. **Access the API**:
   - API: http://localhost:5000
   - Swagger: http://localhost:5000/swagger (if configured)

#### Docker Commands

**View API logs**:

```bash
docker-compose logs -f api
```

**View SQL Server logs**:

```bash
docker-compose logs -f sqlserver
```

**Stop containers**:

```bash
docker-compose down
```

**Reset database** (removes all data):

```bash
docker-compose down -v
docker-compose up --build -d
```

**Connect to SQL Server**:

- Server: `localhost,1433`
- Username: `sa`
- Password: `YourStrong@Password123`
- Database: `LoginDB`

---

### Option 2: Run Locally (Without Docker)

#### Prerequisites

- .NET SDK 10.0 or higher
- SQL Server (or SQL Server Express)
- Visual Studio 2022 / VS Code (optional)

#### Steps

1. Update `appsettings.json` with your local SQL Server connection string
2. Run migrations: `dotnet ef database update`
3. Run the API: `dotnet run --project LoginApp.Api`

## 📚 API Endpoints

### Authentication (`/api/auth`)

| Method | Endpoint    | Description              | Auth Required |
| ------ | ----------- | ------------------------ | ------------- |
| POST   | `/register` | Create new user account  | ❌            |
| POST   | `/login`    | Login and receive tokens | ❌            |
| POST   | `/refresh`  | Refresh access token     | ✅ (Cookie)   |
| POST   | `/logout`   | Invalidate refresh token | ✅            |

### Tasks (`/api/task`)

| Method | Endpoint       | Description                          | Auth Required |
| ------ | -------------- | ------------------------------------ | ------------- |
| GET    | `/`            | Get all tasks for authenticated user | ✅            |
| GET    | `/{id}`        | Get task by ID                       | ✅            |
| POST   | `/`            | Create new task                      | ✅            |
| PUT    | `/`            | Update task                          | ✅            |
| PATCH  | `/{id}/status` | Update task status only              | ✅            |
| DELETE | `/{id}`        | Soft delete task                     | ✅            |

### Request Examples

#### Register

```json
POST /api/auth/register
{
  "username": "john_doe",
  "password": "SecurePass123"
}
```

#### Login

```json
POST /api/auth/login
{
  "username": "john_doe",
  "password": "SecurePass123",
  "deviceId": "optional-device-id",
  "deviceName": "My Laptop"
}
```

#### Create Task

```json
POST /api/task
Authorization: Bearer <access_token>
{
  "title": "Complete project documentation",
  "description": "Write comprehensive README",
  "dueDate": "2025-12-01T00:00:00Z",
  "taskStatusId": 1
}
```

#### Update Status

```json
PATCH /api/task/5/status
Authorization: Bearer <access_token>
{
  "taskId": 5,
  "statusId": 3
}
```

## 🔒 Security Features

### Authentication Flow

1. User logs in with credentials
2. Server validates and returns:
   - **Access Token** (JSON response, short-lived ~15 min)
   - **Refresh Token** (HttpOnly cookie, long-lived ~7 days)
3. Client stores access token in localStorage
4. Client sends access token in `Authorization: Bearer <token>` header
5. When access token expires, use refresh endpoint with cookie

### Authorization

- All `/api/task` endpoints require valid JWT
- Users can only access/modify their own tasks
- Role-based access control ready for future features

## 🗂️ Database Schema

### Users

- `Id` (PK)
- `Username` (Unique)
- `PasswordHash`
- `Role`
- `CreatedAt`

### Tasks

- `Id` (PK)
- `Title`
- `Description`
- `UserId` (FK)
- `TaskStatusId` (FK)
- `DueDate`
- `CreatedAt`
- `IsDeleted`

### TaskStatus

- `Id` (PK): 1 = Pending, 2 = In Progress, 3 = Done
- `Name`

### RefreshTokens

- `Id` (PK)
- `Token`
- `UserId` (FK)
- `DeviceId`
- `DeviceName`
- `ExpiresDate`
- `IsCanceled`

## 📝 Code Quality

- ✅ Input validation with Data Annotations
- ✅ Consistent error handling (404 for not found, 400 for bad input)
- ✅ Constants for magic strings (`UserRoles`)
- ✅ Soft delete pattern for data preservation
- ✅ Background service for token cleanup

## 🔧 Configuration Files

| File                      | Purpose                          | Environment       |
| ------------------------- | -------------------------------- | ----------------- |
| `appsettings.json`        | Base configuration               | Local development |
| `appsettings.Docker.json` | Docker-specific settings         | Docker containers |
| `Dockerfile`              | API container image definition   | Docker            |
| `docker-compose.yml`      | Multi-container orchestration    | Docker            |
| `.dockerignore`           | Excludes files from Docker build | Docker            |

## 🐛 Troubleshooting

### Docker Issues

**Issue**: SQL Server container fails health check

- **Solution**: Increase Docker Desktop memory allocation to at least 4GB
- Go to Docker Desktop → Settings → Resources → Memory

**Issue**: API can't connect to database

- **Solution**: Ensure SQL Server container is healthy first: `docker-compose ps`
- Check logs: `docker-compose logs sqlserver`

**Issue**: Port 1433 or 5000 already in use

- **Solution**: Change port mapping in `docker-compose.yml`:
  ```yaml
  ports:
    - "5001:8080" # For API
    - "1434:1433" # For SQL Server
  ```

**Issue**: Build fails with NuGet restore errors

- **Solution**: Clear Docker build cache and rebuild:
  ```bash
  docker-compose build --no-cache
  ```

**Issue**: Database migrations fail

- **Solution**: Ensure EF Tools are installed in the container. Run manually:
  ```bash
  docker-compose exec api dotnet tool install --global dotnet-ef
  docker-compose exec api dotnet ef database update
  ```

**Issue**: Changes not reflected in container

- **Solution**: Rebuild the image:
  ```bash
  docker-compose up --build
  ```

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

## 👤 Author

Built with ❤️ by ABDELLAH

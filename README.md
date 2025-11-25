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

### Prerequisites

- .NET SDK 10.0 or higher
- SQL Server (or SQL Server Express)
- Visual Studio 2022 / VS Code (optional)

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

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

## 👤 Author

Built with ❤️ by ABDELLAH

# School Equipment Lending Portal

A comprehensive web application for managing school equipment lending operations. This system enables students and staff to request equipment, while administrators manage inventory and approve/process lending requests.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Installation & Setup](#installation--setup)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Database Schema](#database-schema)
- [Authentication & Authorization](#authentication--authorization)
- [Key Features & Workflows](#key-features--workflows)
- [Deployment](#deployment)
- [Contributing](#contributing)

## 🎯 Overview

The School Equipment Lending Portal is a full-stack application designed to streamline the process of lending school equipment to students and staff. It provides role-based access control, real-time inventory management, request tracking, and comprehensive audit trails.

**Key Capabilities:**
- User registration and authentication with JWT tokens
- Equipment inventory management (CRUD operations)
- Equipment borrowing request system
- Request approval workflow for staff/admin
- Real-time availability tracking
- Soft-delete functionality for equipment
- Concurrency control for data consistency

## ✨ Features

### User Features
- **Authentication**: Secure login and registration with password hashing
- **Profile Management**: View and manage user profile information
- **Equipment Browsing**: View available equipment with details
- **Request Equipment**: Submit borrowing requests with quantity and notes
- **Track Requests**: View request status and history

### Admin/Staff Features
- **Equipment Management**: Add, update, and delete equipment items
- **Request Management**: Approve, reject, issue, and track equipment requests
- **Inventory Control**: Monitor equipment availability and reserved quantities
- **User Management**: View all users and their roles

### System Features
- **Role-Based Access Control**: Three roles - Student, Staff, Admin
- **Request Status Workflow**: pending → approved/rejected → issued → returned
- **Concurrency Handling**: Pessimistic locking prevents race conditions
- **Soft Deletes**: Equipment records are soft-deleted to maintain data integrity
- **Comprehensive Logging**: Serilog integration with file and database logging
- **API Documentation**: Swagger/OpenAPI documentation available

## 🛠 Technology Stack

### Backend
- **Framework**: ASP.NET Core 9.0 (Web API)
- **Database**: PostgreSQL 16+
- **ORM**: Entity Framework Core 9.0
- **Authentication**: JWT Bearer Tokens
- **Validation**: FluentValidation
- **Logging**: Serilog with Console, File, and PostgreSQL sinks
- **API Documentation**: Swashbuckle (Swagger/OpenAPI)
- **Password Hashing**: BCrypt.Net-Next
- **Health Checks**: AspNetCore.HealthChecks.Npgsql

### Frontend
- **Framework**: React 18.3 with TypeScript
- **Build Tool**: Vite 5.4
- **Routing**: React Router 7.9
- **HTTP Client**: Axios 1.7
- **UI Styling**: Tailwind CSS 3.4
- **Icons**: Lucide React
- **Notifications**: React Toastify
- **State Management**: React Context API

### DevOps
- **Containerization**: Docker & Docker Compose
- **Database Admin**: pgAdmin 4

## 📁 Project Structure

```
SchoolEquipmentLendingPortal/
│
├── backend/
│   ├── EquipmentLendingApi/
│   │   ├── Controllers/          # API endpoints
│   │   │   ├── AuthController.cs
│   │   │   ├── EquipmentController.cs
│   │   │   ├── ProfileController.cs
│   │   │   └── RequestsController.cs
│   │   ├── Data/                 # Database context
│   │   │   └── AppDbContext.cs
│   │   ├── Dtos/                 # Data transfer objects
│   │   │   ├── AuthDtos.cs
│   │   │   ├── EquipmentDtos.cs
│   │   │   ├── ProfileDto.cs
│   │   │   └── RequestDtos.cs
│   │   ├── Filters/              # Custom filters
│   │   │   └── CustomValidationResultFactory.cs
│   │   ├── Middleware/           # Custom middleware
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── Migrations/           # EF Core migrations
│   │   ├── Model/                # Domain models
│   │   │   ├── ApiResponse.cs
│   │   │   ├── Equipment.cs
│   │   │   ├── Request.cs
│   │   │   └── User.cs
│   │   ├── Validators/           # FluentValidation validators
│   │   │   ├── EquipmentDtoValidator.cs
│   │   │   ├── RequestDtoValidator.cs
│   │   │   ├── UserLoginDtoValidator.cs
│   │   │   └── UserRegisterDtoValidator.cs
│   │   ├── Program.cs            # Application entry point
│   │   ├── appsettings.json      # Configuration
│   │   └── Dockerfile
│   ├── docker-compose.yml        # Docker services configuration
│   └── EquipmentLendingApi.sln   # Solution file
│
└── frontend/
    ├── src/
    │   ├── components/
    │   │   ├── Auth/             # Authentication components
    │   │   │   ├── AuthPage.tsx
    │   │   │   ├── LoginForm.tsx
    │   │   │   └── SignupForm.tsx
    │   │   ├── Borrowing/        # Request management components
    │   │   │   ├── RequestForm.tsx
    │   │   │   └── RequestsManagement.tsx
    │   │   ├── Dashboard/        # Dashboard components
    │   │   │   └── EquipmentDashboard.tsx
    │   │   ├── Equipment/        # Equipment management components
    │   │   │   ├── EquipmentForm.tsx
    │   │   │   └── EquipmentManagement.tsx
    │   │   └── Layout/           # Layout components
    │   │       ├── MainApp.tsx
    │   │       └── Navigation.tsx
    │   ├── contexts/             # React contexts
    │   │   └── AuthContext.tsx
    │   ├── services/             # API service layer
    │   │   ├── apiClient.ts
    │   │   ├── authService.ts
    │   │   ├── equipmentService.ts
    │   │   └── requestsService.ts
    │   ├── types.ts              # TypeScript type definitions
    │   ├── App.tsx               # Root component
    │   └── main.tsx              # Application entry point
    ├── package.json
    ├── vite.config.ts
    └── tailwind.config.js
```

## 📋 Prerequisites

Before you begin, ensure you have the following installed:

- **.NET SDK 9.0** or later
- **Node.js 18+** and npm
- **PostgreSQL 16+** (or use Docker)
- **Docker Desktop** (optional, for containerized deployment)
- **Git** for version control

## 🚀 Installation & Setup

### Clone the Repository

```bash
git clone <repository-url>
cd SchoolEquipmentLendingPortal
```

### Backend Setup

1. Navigate to the backend directory:
```bash
cd backend/EquipmentLendingApi
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Update `appsettings.json` with your database connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=equipment_lending;Username=your_user;Password=your_password"
  },
  "Jwt": {
    "Key": "YourSecretKeyHere_ShouldBeLongAndSecure"
  },
  "AllowedOrigins": "http://localhost:5173"
}
```

4. Create and apply database migrations:
```bash
dotnet ef migrations add InitialCreate --project EquipmentLendingApi
dotnet ef database update --project EquipmentLendingApi
```

### Frontend Setup

1. Navigate to the frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Create a `.env` file (optional, defaults to `/api`):
```env
VITE_API_BASE_URL=http://localhost:5000
```

## ⚙️ Configuration

### Backend Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=equipment_lending;Username=admin;Password=admin123"
  },
  "Jwt": {
    "Key": "SuperSecretKeyForEquipmentLendingPortal2025"
  },
  "AllowedOrigins": "http://localhost:5173",
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning"
      }
    }
  }
}
```

### Frontend Configuration

The frontend uses environment variables for configuration:
- `VITE_API_BASE_URL`: Backend API base URL (default: `/api`)

## 🏃 Running the Application

### Option 1: Manual Setup (Development)

#### Backend

1. Start PostgreSQL database (if not using Docker)

2. Run the API:
```bash
cd backend/EquipmentLendingApi
dotnet run
```

The API will be available at `http://localhost:5000` or `https://localhost:5001`

3. Access Swagger documentation at: `http://localhost:5000/swagger`

#### Frontend

1. Start the development server:
```bash
cd frontend
npm run dev
```

2. Open your browser to `http://localhost:5173`

### Option 2: Docker Compose (Recommended)

1. Navigate to the backend directory:
```bash
cd backend
```

2. Start all services:
```bash
docker-compose up -d
```

This will start:
- **API**: `http://localhost:8080`
- **PostgreSQL**: `localhost:5432`
- **pgAdmin**: `http://localhost:5050`

3. Access pgAdmin:
   - Email: `admin@admin.com`
   - Password: `admin123`

4. For the frontend, still run manually:
```bash
cd frontend
npm run dev
```

### Option 3: Production Build

#### Backend
```bash
cd backend/EquipmentLendingApi
dotnet publish -c Release -o ./publish
```

#### Frontend
```bash
cd frontend
npm run build
```

The production build will be in the `dist` directory.

## 📚 API Documentation

### Base URL
- Development: `http://localhost:5000`
- Docker: `http://localhost:8080`

### Authentication

Most endpoints require JWT authentication. Include the token in the Authorization header:
```
Authorization: Bearer <your-token>
```

### Endpoints

#### Authentication (`/api/auth`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register a new user | No |
| POST | `/api/auth/login` | Login and receive JWT token | No |

**Register Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123",
  "fullName": "John Doe",
  "role": "student"
}
```

**Login Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123"
}
```

#### Profile (`/api/profile`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/profile/me` | Get current user profile | Yes |

#### Equipment (`/api/equipment`)

| Method | Endpoint | Description | Auth Required | Role Required |
|--------|----------|-------------|---------------|---------------|
| GET | `/api/equipment` | Get all equipment | Yes | Any |
| GET | `/api/equipment/{id}` | Get equipment by ID | Yes | Any |
| GET | `/api/equipment/{id}/availability` | Get availability info | Yes | Any |
| POST | `/api/equipment` | Add new equipment | Yes | Admin |
| PUT | `/api/equipment/{id}` | Update equipment | Yes | Admin |
| DELETE | `/api/equipment/{id}` | Delete equipment (soft) | Yes | Admin |

#### Requests (`/api/requests`)

| Method | Endpoint | Description | Auth Required | Role Required |
|--------|----------|-------------|---------------|---------------|
| GET | `/api/requests` | Get requests (filtered by role) | Yes | Any |
| GET | `/api/requests?status=pending` | Get requests by status | Yes | Any |
| GET | `/api/requests/pending` | Get all pending requests | Yes | Staff/Admin |
| POST | `/api/requests` | Create borrow request | Yes | Any |
| PUT | `/api/requests/{id}` | Update request status | Yes | Staff/Admin |

**Create Request:**
```json
{
  "equipmentId": "equipment-id",
  "quantity": 2,
  "notes": "Need for science project"
}
```

**Update Request:**
```json
{
  "status": "approved",
  "dueDate": "2024-12-31T00:00:00Z",
  "adminNotes": "Approved for use"
}
```

### Request Status Flow

```
pending → approved → issued → returned
   ↓         ↓
rejected  cancelled
```

### Response Format

All API responses follow this structure:

```json
{
  "success": true,
  "message": "Operation successful",
  "data": { /* response data */ },
  "errors": null,
  "statusCode": 200
}
```

Error Response:
```json
{
  "success": false,
  "message": "Error message",
  "data": null,
  "errors": ["Error detail 1", "Error detail 2"],
  "statusCode": 400
}
```

## 🗄️ Database Schema

### Users Table
| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | Unique user identifier |
| FullName | string | User's full name |
| Email | string | User email (unique) |
| PasswordHash | string | BCrypt hashed password |
| Role | string | User role (student, staff, admin) |

### Equipment Table
| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | Unique equipment identifier |
| Name | string | Equipment name |
| Category | string | Equipment category |
| Quantity | int | Total quantity |
| AvailableQuantity | int | Available quantity |
| Description | string | Equipment description |
| Condition | string | Equipment condition |
| IsDeleted | bool | Soft delete flag |
| DeletedAt | DateTime? | Deletion timestamp |
| DeletedBy | string? | User who deleted |
| RowVersion | byte[] | Concurrency token |

### Requests Table
| Column | Type | Description |
|--------|------|-------------|
| Id | string (PK) | Unique request identifier |
| UserId | string (FK) | Requesting user ID |
| EquipmentId | string (FK) | Equipment ID |
| Quantity | int | Requested quantity |
| Status | string | Request status |
| RequestedAt | DateTime? | Request timestamp |
| ApprovedAt | DateTime? | Approval timestamp |
| ApprovedBy | string? (FK) | Approving user ID |
| IssuedAt | DateTime? | Issue timestamp |
| DueDate | DateTime? | Expected return date |
| ReturnedAt | DateTime? | Return timestamp |
| Notes | string? | User notes |
| AdminNotes | string? | Admin notes |
| RejectedAt | DateTime? | Rejection timestamp |
| RejectedBy | string? | Rejecting user ID |

### Relationships
- **User → Requests**: One-to-Many (User makes multiple requests)
- **User → ApprovedRequests**: One-to-Many (User approves multiple requests)
- **Equipment → Requests**: One-to-Many (Equipment has multiple requests)
- **Request → Equipment**: Many-to-One
- **Request → User**: Many-to-One (requester)
- **Request → Approver**: Many-to-One (optional)

## 🔐 Authentication & Authorization

### Authentication Flow

1. User registers/logs in via `/api/auth/register` or `/api/auth/login`
2. Server validates credentials and generates JWT token
3. Token is returned to client and stored in localStorage
4. Client includes token in `Authorization: Bearer <token>` header for subsequent requests
5. Server validates token on protected endpoints

### JWT Token Structure

The JWT token contains:
- **Name**: User email
- **Role**: User role (student, staff, admin)
- **NameIdentifier**: User ID
- **Expiration**: 30 minutes from issue

### Authorization Policies

| Policy | Roles | Description |
|--------|-------|-------------|
| `AdminOnly` | admin | Full system access |
| `StaffOrAdmin` | staff, admin | Can manage requests |
| Default | All authenticated users | Can view and create requests |

### Role Permissions

| Action | Student | Staff | Admin |
|--------|---------|-------|-------|
| View Equipment | ✅ | ✅ | ✅ |
| Create Request | ✅ | ✅ | ✅ |
| View Own Requests | ✅ | ✅ | ✅ |
| View All Requests | ❌ | ✅ | ✅ |
| Approve/Reject Requests | ❌ | ✅ | ✅ |
| Manage Equipment | ❌ | ❌ | ✅ |
| Delete Equipment | ❌ | ❌ | ✅ |

## 🔄 Key Features & Workflows

### Equipment Management Workflow

1. **Admin adds equipment**:
   - Specifies name, category, total quantity, available quantity, condition, description
   - System validates that available quantity ≤ total quantity
   - Equipment is created with unique ID

2. **Admin updates equipment**:
   - System uses pessimistic locking (`FOR UPDATE`) to prevent concurrent modifications
   - Validates reserved quantity before allowing quantity reduction
   - Automatically adjusts available quantity when total quantity changes

3. **Admin deletes equipment**:
   - Soft delete (sets `IsDeleted = true`)
   - Validates no active requests exist before deletion
   - Records deletion timestamp and user

### Request Workflow

1. **User creates request**:
   - Selects equipment and quantity
   - System validates:
     - Equipment exists and is available
     - Quantity is valid
     - User doesn't have pending/active request for same equipment
   - Request created with status "pending"

2. **Staff/Admin reviews request**:
   - Views pending requests
   - Can approve or reject

3. **Approval process**:
   - When approved: Available quantity decreases by request quantity
   - Due date can be set
   - Request status changes to "approved"

4. **Equipment issuance**:
   - Status changes from "approved" to "issued"
   - Issued timestamp recorded

5. **Return process**:
   - Status changes from "issued" to "returned"
   - Returned timestamp recorded
   - Available quantity increases by request quantity

### Concurrency Control

The system implements pessimistic locking for critical operations:
- Equipment updates use `SELECT ... FOR UPDATE` to lock rows
- Database transactions ensure atomicity
- Prevents race conditions in quantity calculations

### Quantity Management

- **Total Quantity**: Maximum inventory count
- **Available Quantity**: Currently available for borrowing
- **Reserved Quantity**: Calculated from approved/issued requests
- **Relationship**: Available + Reserved ≤ Total

When updating equipment:
- If total quantity increases: Available quantity increases proportionally
- If total quantity decreases: System validates no reserved quantity would be lost

## 🐳 Deployment

### Docker Deployment

The application includes Docker Compose configuration for easy deployment:

```bash
cd backend
docker-compose up -d
```

Services:
- **equipmentlendingapi**: ASP.NET Core API (port 8080)
- **postgres**: PostgreSQL database (port 5432)
- **pgadmin**: Database administration (port 5050)

### Environment Variables

For production, set these environment variables:

**Backend:**
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection=<production-db-connection>`
- `Jwt__Key=<secure-secret-key>`
- `AllowedOrigins=<frontend-url>`

**Frontend:**
- `VITE_API_BASE_URL=<backend-api-url>`

### Production Considerations

1. **Security**:
   - Use strong JWT secret key
   - Enable HTTPS
   - Configure CORS properly
   - Use secure password policies

2. **Database**:
   - Use managed PostgreSQL service
   - Configure backups
   - Set up connection pooling

3. **Logging**:
   - Configure log aggregation
   - Set up log rotation
   - Monitor error rates

4. **Performance**:
   - Enable response compression
   - Configure caching headers
   - Use CDN for frontend assets

## 🧪 Testing

### Backend Testing

Run unit tests:
```bash
dotnet test
```

### Frontend Testing

Run linting:
```bash
cd frontend
npm run lint
```

Type checking:
```bash
npm run typecheck
```

### Manual Testing

1. **Test Registration & Login**:
   - Register a new user
   - Login with credentials
   - Verify JWT token is received

2. **Test Equipment Management**:
   - Add equipment (admin only)
   - Update equipment quantities
   - Verify concurrency handling

3. **Test Request Workflow**:
   - Create borrow request
   - Approve request (staff/admin)
   - Issue equipment
   - Return equipment

## 📝 Logging

The application uses Serilog for comprehensive logging:

- **Console**: All logs to console
- **File**: Daily rolling logs in `logs/` directory
- **PostgreSQL**: Optional database logging (configured in `appsettings.json`)

Log levels:
- **Information**: Normal operations
- **Warning**: Potential issues
- **Error**: Errors that are handled
- **Fatal**: Unhandled exceptions

## 🔧 Troubleshooting

### Common Issues

1. **Database Connection Error**:
   - Verify PostgreSQL is running
   - Check connection string in `appsettings.json`
   - Ensure database exists

2. **CORS Errors**:
   - Verify `AllowedOrigins` in backend configuration
   - Check frontend is using correct API URL

3. **JWT Token Expired**:
   - Tokens expire after 30 minutes
   - Re-login to get new token

4. **Concurrency Conflicts**:
   - Refresh the page and retry
   - System uses optimistic concurrency control

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 📞 Support

For support, please open an issue in the GitHub repository or contact the development team.


---

**Built with ❤️ for educational institutions**


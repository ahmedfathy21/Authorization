# Authorization System API

A comprehensive **Role-Based Access Control (RBAC) with Permission-Based Authorization** system built with ASP.NET Core 9, Entity Framework Core, and SQL Server. This project demonstrates enterprise-grade authorization patterns with granular permission management.

## 🏗️ Architecture Overview

### **Authorization Flow**

```
User (ApplicationUser)
    ↓
    └──→ Role (ApplicationRole)  [via UserRoles table]
            ↓
            └──→ Permission  [via RolePermissions table]
                    ↓
                    └──→ Access to Features/Actions
```

This multi-layered approach provides:
- **Flexibility**: Permissions can be assigned to multiple roles
- **Scalability**: Easy to add new permissions without code changes
- **Granularity**: Fine-grained control over user access
- **Maintainability**: Centralized permission management

---

## 📊 Database Schema

### **Core Tables**

#### **Users** (AspNetUsers)
Stores user account information with Identity Framework
- `Id` (PK)
- `UserName`, `Email`, `PasswordHash`
- `FirstName`, `LastName`, `IsActive` (custom fields)

#### **Roles** (AspNetRoles)
Defines roles like Admin, Manager, Editor
- `Id` (PK)
- `Name`, `NormalizedName`

#### **UserRoles** (AspNetUserRoles - Junction)
Maps users to roles (many-to-many)
- `UserId` (FK)
- `RoleId` (FK)

#### **Permissions** (Custom)
Stores all available permissions in the system
- `Id` (PK) - int, auto-increment
- `Name` - nvarchar(100), required (e.g., "CreateUser", "DeletePost")
- `Description` - nvarchar(500), optional

#### **RolePermissions** (Custom - Junction)
Maps roles to permissions (many-to-many)
- `Id` (PK) - int, auto-increment
- `RoleId` (FK) - references Roles.Id, cascade delete
- `PermissionId` (FK) - references Permissions.Id, cascade delete
- **Unique Index** on (RoleId, PermissionId) - prevents duplicate assignments

#### **Additional Identity Tables**
- `UserClaims` - Store user-specific claims
- `UserLogins` - Track external login providers
- `RoleClaims` - Store role-specific claims

#### **RefreshTokens** (Custom)
Manages JWT refresh tokens for secure token rotation
- `Id` (PK) - int, auto-increment
- `Token` - nvarchar(200), required, unique
- `Expires` - DateTime, token expiration time
- `Created` - DateTime, token creation time
- `Revoked` - DateTime?, nullable (null = active)
- `ReplacedByToken` - nvarchar(200)?, nullable
- `UserId` (FK) - references Users.Id, cascade delete
- **Computed Property**: `IsActive` → checks if not revoked and not expired

#### **LoginUsers** (Custom)
Audits successful login sessions
- `Id` (PK) - int, auto-increment
- `UserId` (FK) - references Users.Id, cascade delete
- `LoggedInAt` - DateTime (UTC)
- `IpAddress` - nvarchar(64), optional
- `UserAgent` - nvarchar(512), optional

---

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 9.0
- **ORM**: Entity Framework Core 9.0.12
- **Database**: SQL Server 2022 (Docker)
- **Authentication**: ASP.NET Core Identity + JWT Bearer
- **Architecture**: N-Tier with Dependency Injection

### **NuGet Packages**
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.12" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.12" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.6.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.12" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.12" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.12" />
```

---

## 🚀 Setup Instructions

### **Prerequisites**
- .NET 9 SDK
- Docker & Docker Compose
- Git

### **1. Clone the Repository**
```bash
git clone https://github.com/ahmedfathy21/Authorization.git
cd Authorization/AuthSystemAPI
```

### **2. Start SQL Server Container**
```bash
docker compose up -d
```
This will:
- Start SQL Server 2022 container on port 1433
- Create persistent volume for data

### **3. Verify Database Connection**
Check connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=AuthSystemDB;User Id=sa;Password=Your_password123;TrustServerCertificate=True;"
}
```

### **4. Apply Migrations**
```bash
dotnet ef database update
```
This creates all tables:
- Identity tables (Users, Roles, UserRoles, etc.)
- Custom tables (Permissions, RolePermissions)

### **5. Run the Application**
```bash
dotnet run
```
Application starts at `https://localhost:7113`

---

## 📝 Key Configuration Files

### **Program.cs**
```csharp
// DbContext Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Configuration with Password Requirements
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;    
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

### **ApplicationDbContext.cs**
Inherits from `IdentityDbContext<ApplicationUser, ApplicationRole, string>`

**Key DbSets:**
```csharp
public DbSet<Permission> Permissions { get; set; }
public DbSet<RolePermission> RolePermissions { get; set; }
public DbSet<RefreshToken> RefreshTokens { get; set; }
public DbSet<LoginUser> LoginUsers { get; set; }
```

**Relationships Configured in OnModelCreating():**
- Foreign keys from RolePermissions → Roles and Permissions
- Cascade delete enabled (deleting a role removes its permissions)
- Unique constraint on (RoleId, PermissionId) to prevent duplicates

---

## 🔐 How Authorization Works

### **Authorization Check Pattern**
```csharp
// Example: Check if user has a specific permission
var user = await userManager.FindByIdAsync(userId);
var roles = await userManager.GetRolesAsync(user);

var hasPermission = await context.RolePermissions
    .Where(rp => roles.Contains(rp.Role.Name))
    .AnyAsync(rp => rp.Permission.Name == "DeleteUser");

if (hasPermission)
{
    // Allow action
}
```

### **Example Scenario**
1. **Create Permissions**
   - "CreateUser"
   - "DeleteUser"
   - "EditPost"
   - "DeletePost"

2. **Create Roles & Assign Permissions**
   - **Admin Role**: All permissions
   - **Editor Role**: EditPost, CreateUser
   - **Viewer Role**: No permissions

3. **Create User & Assign Role**
   - User A → Admin Role → Access to all actions
   - User B → Editor Role → Can only create and edit posts

---

## � JWT Authentication

The system uses **JWT (JSON Web Tokens)** with **Refresh Token Rotation** for secure authentication.

### **Token Lifecycle**
```
1. User Login → Server validates credentials
   ↓
2. Server generates Access Token (30 min) + Refresh Token (7 days)
   ↓
2.1 Server writes LoginUsers audit record (UserId, LoggedInAt, IpAddress, UserAgent)
  ↓
3. Client stores both tokens
   ↓
4. Access Token expires → Client uses Refresh Token to get new Access Token
   ↓
5. Old Refresh Token is revoked → New Refresh Token issued
   ↓
6. Logout → Refresh Token marked as revoked
```

### **JWT Configuration** (appsettings.json)
```json
"Jwt": {
  "Key": "REPLACE_WITH_A_SECURE_32+_CHAR_SECRET_KEY",
  "Issuer": "AuthSystemAPI",
  "Audience": "AuthSystemAPI",
  "AccessTokenMinutes": 30,
  "RefreshTokenDays": 7
}
```

---

## 📡 API Endpoints

### **Auth Controller** (`/api/auth`)

#### **1. Login**
```http
POST /api/auth/login
Content-Type: application/json

{
  "userName": "john_doe",
  "password": "SecurePass123",
  "rememberMe": false
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "Hd8jf/sL2mK9xP+...",
    "expiresAt": "2026-02-04T13:45:00Z"
  },
  "errors": []
}
```

#### **2. Refresh Token**
```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "Hd8jf/sL2mK9xP+..."
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "newRefreshTokenValue...",
    "expiresAt": "2026-02-04T13:45:00Z"
  }
}
```

#### **3. Logout**
```http
POST /api/auth/logout
Content-Type: application/json

{
  "refreshToken": "Hd8jf/sL2mK9xP+..."
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Logout successful",
  "data": true
}
```

---

### **Users Controller** (`/api/users`)

#### **1. Create User**
```http
POST /api/users
Content-Type: application/json
Authorization: Bearer <access_token>

{
  "userName": "john_doe",
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "password": "SecurePass123",
  "confirmPassword": "SecurePass123",
  "phoneNumber": "1234567890",
  "isActive": true
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "User created successfully",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "userName": "john_doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "1234567890",
    "emailConfirmed": false,
    "phoneNumberConfirmed": false,
    "isActive": true,
    "createdAt": "2026-02-04T12:30:00Z",
    "roles": []
  }
}
```

#### **2. Get User by ID**
```http
GET /api/users/{userId}
Authorization: Bearer <access_token>
```

**Response (200 OK):** User details (same structure as Create User response)

#### **3. Get User by Username**
```http
GET /api/users/by-username/{userName}
Authorization: Bearer <access_token>
```

#### **4. Get All Users (with Pagination & Search)**
```http
GET /api/users?pageNumber=1&pageSize=10&searchTerm=john
Authorization: Bearer <access_token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Retrieved 5 users",
  "data": {
    "items": [
      {
        "id": "uuid...",
        "userName": "john_doe",
        "email": "john@example.com",
        "firstName": "John",
        "lastName": "Doe",
        "phoneNumber": "1234567890",
        "isActive": true,
        "roles": ["Admin"]
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

#### **5. Update User**
```http
PUT /api/users/{userId}
Content-Type: application/json
Authorization: Bearer <access_token>

{
  "firstName": "Jonathan",
  "lastName": "Smith",
  "email": "jonathan@example.com",
  "phoneNumber": "9876543210",
  "isActive": true
}
```

#### **6. Soft Delete User** (marks as inactive)
```http
DELETE /api/users/{userId}
Authorization: Bearer <access_token>
```

#### **7. Permanent Delete User**
```http
DELETE /api/users/{userId}/permanent
Authorization: Bearer <access_token>
```

#### **8. Assign Roles to User**
```http
POST /api/users/{userId}/roles
Content-Type: application/json
Authorization: Bearer <access_token>

["Admin", "Manager"]
```

#### **9. Remove Roles from User**
```http
DELETE /api/users/{userId}/roles
Content-Type: application/json
Authorization: Bearer <access_token>

["Manager"]
```

#### **10. Get User's Roles**
```http
GET /api/users/{userId}/roles
Authorization: Bearer <access_token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Retrieved 2 roles for user 'john_doe'",
  "data": ["Admin", "Manager"]
}
```

---

## 🏗️ Project Structure

```
AuthSystemAPI/
├── Controllers/
│   ├── AuthController.cs                 # Authentication endpoints
│   └── UsersController.cs                # User CRUD endpoints
├── Services/
│   ├── IAuthService.cs                   # Auth service interface
│   ├── AuthService.cs                    # JWT token generation & validation
│   ├── IUserService.cs                   # User service interface
│   └── UserService.cs                    # User business logic
├── Data/
│   └── ApplicationDbContext.cs           # DbContext with Identity & permissions
├── DTOs/
│   ├── LoginDto.cs                       # Login request
│   ├── AuthResponseDto.cs                # Auth response (tokens)
│   ├── RefreshTokenRequestDto.cs         # Refresh token request
│   ├── LogoutDto.cs                      # Logout request
│   ├── CreateUserDto.cs                  # Create user request
│   ├── UpdateUserDto.cs                  # Update user request
│   ├── UserResponseDto.cs                # User response
│   ├── PaginatedResultDto.cs             # Paginated list wrapper
│   └── ApiResponseDto.cs                 # Standard API response wrapper
├── Entities/
│   ├── ApplicationRole.cs                # Custom role entity
│   ├── Permission.cs                     # Permission entity
│   ├── RolePermission.cs                 # Role-Permission junction
│   └── RefreshToken.cs                   # JWT refresh token entity
├── Models/
│   └── ApplicationUser.cs                # Custom user entity with refresh tokens
├── Migrations/                           # EF Core migrations
├── Program.cs                            # DI, authentication, authorization config
├── appsettings.json                      # Connection strings & JWT settings
├── appsettings.Development.json          # Development-specific settings
├── docker-compose.yml                    # SQL Server container definition
├── AuthSystemAPI.csproj                  # Project file
└── README.md                             # This file
```

---

## 🔄 Migration History

### **First Migration: RebuildWithPermissions**
- Created initial Identity tables (Users, Roles, UserRoles, etc.)

### **Second Migration: AddPermissionsAndRolePermissions**
- Created `Permissions` table
- Created `RolePermissions` table (many-to-many junction)
- Configured foreign key constraints and unique indexes

---

## 🔒 Security Features

✅ **Password Policies**
- Minimum 6 characters
- Requires uppercase letter
- Requires lowercase letter
- Requires digit
- Optional special characters

✅ **Database Constraints**
- Foreign key integrity
- Cascade delete to maintain data consistency
- Unique index on role-permission pairs

✅ **Entity Framework**
- Parameterized queries (prevents SQL injection)
- LINQ-based queries (type-safe)

---

## 📈 Next Steps

### **Completed** ✅
- ✅ Users API Controller (CRUD, role management)
- ✅ Auth Controller (login, refresh token, logout)
- ✅ JWT Authentication with token rotation
- ✅ RefreshToken entity and management
- ✅ Base DTOs and response wrapper

### **To Extend This System:**

1. **Roles Controller** (TBD)
   - `/api/roles` - Create, read, update, delete roles
   - Assign/remove permissions from roles

2. **Permissions Controller** (TBD)
   - `/api/permissions` - CRUD operations
   - List all available permissions in system

3. **Authorization Policies** (TBD)
   - Custom permission-based authorization policies
   - Policy-based authorization handlers
   - `[Authorize(Policy = "DeleteUser")]` attributes

4. **Data Seeding** (TBD)
   - Create initial permissions
   - Create default roles (Admin, User, Guest)
   - Seed admin user for first-time setup

5. **Validation & Error Handling** (TBD)
   - FluentValidation for DTOs
   - Global exception handling middleware
   - Standardized error responses

6. **Logging & Audit** (TBD)
   - Log authentication/authorization events
   - Audit trail for permission changes
   - User action logging

7. **Testing** (TBD)
   - Unit tests for services
   - Integration tests with test database
   - Controller endpoint tests

---

## 🐛 Troubleshooting

### **Issue: Connection String Not Working**
```
Ensure:
1. Docker container is running: docker ps
2. SQL Server is healthy: docker logs sqlserver_authsystem
3. Password matches docker-compose.yml
4. Database AuthSystemDB exists
```

### **Issue: Migrations Won't Apply**
```bash
# Rebuild and try again
dotnet clean
dotnet build
dotnet ef database update
```

### **Issue: EntityFrameworkCore.Design Missing**
```
The package is required for EF Core Tools. Verify:
- Package is in .csproj: Microsoft.EntityFrameworkCore.Design
- Run: dotnet restore
```

---

## 📚 Resources

- [ASP.NET Core Identity Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction)

---

## 📄 License

This project is open source and available under the MIT License.

---

## 👤 Author

**Kakashi** - Authorization System Implementation

---

**Last Updated**: January 31, 2026

For issues or questions, please create an issue on GitHub.

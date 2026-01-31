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

---

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 9.0
- **ORM**: Entity Framework Core 9.0.12
- **Database**: SQL Server 2022 (Docker)
- **Authentication**: ASP.NET Core Identity
- **Architecture**: N-Tier with Repository pattern ready

### **NuGet Packages**
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.12" />
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

## 🗂️ Project Structure

```
AuthSystemAPI/
├── Data/
│   └── ApplicationDbContext.cs          # DbContext with Identity & permissions
├── Entities/
│   ├── ApplicationRole.cs               # Custom role entity
│   ├── Permission.cs                    # Permission entity
│   └── RolePermission.cs                # Role-Permission junction entity
├── Models/
│   └── ApplicationUser.cs               # Custom user entity
├── Migrations/                          # EF Core migrations
├── Program.cs                           # Dependency injection & configuration
├── appsettings.json                     # Connection strings & settings
├── appsettings.Development.json         # Development-specific settings
├── docker-compose.yml                   # SQL Server container definition
├── AuthSystemAPI.csproj                 # Project file
└── README.md                            # This file
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

### **To Extend This System:**

1. **Create API Endpoints**
   - `/api/permissions` - CRUD operations
   - `/api/roles` - Manage roles
   - `/api/users` - Manage users and assign roles

2. **Implement Authorization Middleware**
   - Custom permission-based authorization policies
   - Policy-based authorization handlers

3. **Add Seeding**
   - Create initial permissions and roles
   - Seed default admin user

4. **Logging & Audit**
   - Log permission changes
   - Track authorization attempts

5. **Testing**
   - Unit tests for authorization logic
   - Integration tests with test database

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

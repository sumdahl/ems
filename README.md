# Employee Management System

[![Docker Hub](https://img.shields.io/badge/Docker%20Hub-sumdahl%2Femployee--management--system-blue?logo=docker)](https://hub.docker.com/r/sumdahl/employee-management-system)
[![Docker Pulls](https://img.shields.io/docker/pulls/sumdahl/employee-management-system)](https://hub.docker.com/r/sumdahl/employee-management-system)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)](https://www.postgresql.org/)

A comprehensive Employee Management System built with ASP.NET Core 10.0, featuring **dual authentication** (Cookie-based MVC + JWT API), role-based authorization, leave management, attendance tracking, and analytics dashboard.

## ✨ Key Features

- 🔐 **Dual Authentication**: Cookie-based for web UI, JWT for API
- 👥 **Role-Based Access Control**: Admin, Manager, Employee roles
- 📊 **Analytics Dashboard**: Real-time statistics and insights
- 📅 **Leave Management**: Request, approve, and track employee leaves
- ⏰ **Attendance Tracking**: Check-in/out with automatic calculations
- 🏢 **Department Management**: Organize employees by departments
- 🌐 **RESTful API**: Complete JWT-secured API for integrations
- 🐳 **Docker Ready**: Multi-architecture support (AMD64, ARM64)

---

## 🐳 Quick Start with Docker (Recommended)

The fastest way to get started! No need to install .NET or PostgreSQL.

### Create a network for EMS API and PostgreSQL 

```bash
docker network create ems-net
```

### Start PostgreSQL Container
```bash
docker run -d \
  --name ems-postgres \
  --network ems-net \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=ems \
  -p 5432:5432 \
  postgres:15
```

### 🚀 Start EMS API Container (Swagger UI, SignalR , JWT and many more – v1.0.3)
```bash
docker run -d \
  --name ems-api \
  --network ems-net \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Host=ems-postgres;Port=5432;Database=ems;Username=postgres;Password=postgres" \
  sumdahl/employee-management-system:latest
```
### Verify Containers
```bash
docker ps

```
# You should see both running:
```bash
ems-api          0.0.0.0:8080->8080/tcp
ems-postgres     0.0.0.0:5432->5432/tcp
```

### Test EMS API
```bash
http://localhost:8080
```

### Stop/ Remove Containers
```bash
docker stop ems-api ems-postgres
docker rm ems-api ems-postgres
docker network rm ems-net
```

### Run with Docker Compose

```bash
# Clone the repository (or download docker-compose.yml)
git clone https://github.com/sumdahl/ems.git
cd EmployeeManagementSystem

# Start the application and database
docker-compose up -d

# View logs
docker-compose logs -f web

# Stop the application
docker-compose down
```

**Access the application at: http://localhost:8080**

### Run Standalone Container

If you already have PostgreSQL running:

```bash
docker run -d \
  --name ems-app \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=your-db-host;Database=ems;Username=postgres;Password=yourpassword" \
  sumdahl/employee-management-system:latest
```

---

## 💻 Local Development Setup

### Prerequisites
- .NET 10.0 SDK
- PostgreSQL 16+
- Node.js 20+ (for Tailwind CSS)

### Installation

```bash
# Clone the repository
git clone https://github.com/sumdahl/ems.git
cd EmployeeManagementSystem

# Restore dependencies
dotnet restore
npm install

# Update connection string in appsettings.json
# Then run the application
dotnet run
```

**Access the application at: http://localhost:5054**

---

## 🚀 Application Status

✅ **Docker Image Available**: `sumdahl/employee-management-system:latest`  
✅ **Local Development**: http://localhost:5054  
✅ **Docker Deployment**: http://localhost:8080

## 📝 Login Credentials

```
Email: admin@ems.com
Password: Admin@123
Role: Admin 

Email: manager@ems.com
Password: Manager@123
Role: Manager

Email: employee@ems.com
Password: Employee@123
Role: Employee
```

## 🎯 Quick Test Workflow

### 1. Login
- Navigate to http://localhost:5054
- Login with admin credentials above

### 2. Explore Dashboard
- View system statistics
- See total employees, departments, pending leaves, and today's attendance

### 3. Manage Employees
- Click "Employees" in navigation
- View the 2 seeded employees (John Doe, Jane Smith)
- Click "Add Employee" to create a new employee
- Test search and filter functionality

### 4. Create a User Account
- Click "Register User" button (Admin only)
- Create a new user account for testing different roles

### 5. Test Leave Request Workflow
- Logout and login as a regular employee
- Navigate to "Leave Requests" → "Request Leave"
- Submit a leave request
- Logout and login as admin/manager
- Approve or reject the leave request

### 6. Test Attendance
- Login as an employee
- Navigate to "Attendance" → "Check In"
- Check in for the day
- Later, go back to Attendance list and check out

## 📊 Seeded Data

The system includes:
- **5 Departments**: HR, Engineering, Sales, Finance, Operations
- **9 Job Roles**: Various positions across departments
- **1 Admin User**: admin@ems.com
- **2 Sample Employees**: John Doe (Software Engineer), Jane Smith (HR Manager)

## 🔐 Role Capabilities

### Admin
- Full system access
- Create/edit/delete employees
- Manage departments
- Register new users
- Approve/reject leaves
- View all attendance

### Manager
- Create/edit employees
- Manage departments
- Approve/reject leaves
- View team attendance

### Employee
- View own profile
- Submit leave requests
- Check-in/check-out
- View own records

## 🛠️ Technical Details

- **Backend**: ASP.NET Core MVC (.NET 10.0)
- **Database**: PostgreSQL (database: `ems` it's just name)
- **Frontend**: Tailwind CSS (CDN)
- **Authentication**: ASP.NET Identity
- **Port**: http://localhost:5054

## 📁 Project Structure

```
EmployeeManagementSystem/
├── Controllers/          # MVC Controllers
├── Models/              # Entity models
├── Views/               # Razor views
├── Data/                # DbContext and seeding
├── ViewModels/          # View models
└── wwwroot/             # Static files
```

## 🎨 Features Implemented

## 🎨 Features Implemented

- ✅ Role-based authentication and authorization
- ✅ Employee CRUD with search/filter
- ✅ Department management
- ✅ Leave request workflow with approval
- ✅ Attendance tracking with check-in/out
- ✅ Analytics dashboard
- ✅ Modern, responsive UI
- ✅ PostgreSQL database integration

## 🔄 To Stop the Application

Press `Ctrl+C` in the terminal where the application is running.

## 🚀 To Restart

### Unix / macOS / Linux
```bash
cd ~/dotnet_project/EmployeeManagementSystem
dotnet run
```

### Windows
```cmd
cd %USERPROFILE%\dotnet_project\EmployeeManagementSystem
dotnet run

```

## 🚢 Deployment

### Docker Hub
The application is available on Docker Hub:
- **Repository**: [sumdahl/employee-management-system](https://hub.docker.com/r/sumdahl/employee-management-system)
- **Latest**: `docker pull sumdahl/employee-management-system:latest`
- **Version 1.0.0**: `docker pull sumdahl/employee-management-system:v1.0.0`

### Cloud Deployment Options

**AWS ECS/Fargate**
```bash
# Use the Docker image directly
docker pull sumdahl/employee-management-system:latest
```

**Azure Container Instances**
```bash
az container create \
  --resource-group myResourceGroup \
  --name ems-app \
  --image sumdahl/employee-management-system:latest \
  --ports 8080
```

**Google Cloud Run**
```bash
gcloud run deploy ems-app \
  --image sumdahl/employee-management-system:latest \
  --platform managed
```

**DigitalOcean App Platform**
- Use Docker Hub image: `sumdahl/employee-management-system:latest`

### Environment Variables for Production

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__DefaultConnection=Host=db-host;Database=ems;Username=user;Password=pass
```

---

## 🌐 JWT API

The system includes a complete RESTful API with JWT authentication for building frontend applications, mobile apps, or third-party integrations.

### Quick Start

#### 1. Login to Get JWT Token
```bash
curl -X POST http://localhost:5054/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@ems.com",
    "password": "Admin@123"
  }'
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "expiresIn": 3600,
    "user": {
      "id": "...",
      "email": "admin@ems.com",
      "fullName": "System Administrator",
      "roles": ["Admin"]
    }
  }
}
```

#### 2. Use Token to Access Protected Endpoints
```bash
# Get all employees
curl -X GET http://localhost:5054/api/employees \
  -H "Authorization: Bearer {YOUR_TOKEN}"

# Get current user info
curl -X GET http://localhost:5054/api/auth/me \
  -H "Authorization: Bearer {YOUR_TOKEN}"

# Get attendance records
curl -X GET http://localhost:5054/api/attendance \
  -H "Authorization: Bearer {YOUR_TOKEN}"
```

### Available API Endpoints

#### Authentication (`/api/auth`)
- `POST /api/auth/login` - Login with email/password
- `GET /api/auth/me` - Get current user information
- `POST /api/auth/refresh` - Refresh expired token

#### Employees (`/api/employees`)
- `GET /api/employees` - List all employees (with filtering)
- `GET /api/employees/{id}` - Get employee by ID
- `POST /api/employees` - Create employee (Manager/Admin only)
- `PUT /api/employees/{id}` - Update employee (Manager/Admin only)
- `DELETE /api/employees/{id}` - Delete employee (Admin only)

#### Attendance (`/api/attendance`)
- `GET /api/attendance` - List attendance records
- `POST /api/attendance/checkin` - Check in
- `POST /api/attendance/checkout/{id}` - Check out

### API Response Format

All API responses follow this consistent structure:

**Success:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... },
  "errors": []
}
```

**Error:**
```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

### JWT Token Information

- **Access Token Lifetime**: 60 minutes
- **Refresh Token Lifetime**: 7 days
- **Algorithm**: HMAC SHA256
- **Claims**: User ID, Email, Full Name, Role

### Testing with Postman

1. **Create Environment**
   - Variable: `baseUrl` = `http://localhost:5054`
   - Variable: `jwt_token` = (auto-populated)

2. **Login Request**
   ```
   POST {{baseUrl}}/api/auth/login
   Body: { "email": "admin@ems.com", "password": "Admin@123" }
   
   Tests Script:
   var jsonData = pm.response.json();
   if (jsonData.success && jsonData.data.token) {
       pm.environment.set("jwt_token", jsonData.data.token);
   }
   ```

3. **Protected Requests**
   ```
   GET {{baseUrl}}/api/employees
   Authorization: Bearer {{jwt_token}}
   ```

---

## 📚 Additional Documentation

- **[API Documentation](API_DOCUMENTATION.md)** - Complete API reference with all MVC endpoints
- **[JWT Implementation Guide](JWT_IMPLEMENTATION_GUIDE.md)** - Detailed JWT authentication implementation
- **[Test Users Guide](TEST_USERS_GUIDE.md)** - Default user credentials for testing
## 🛠️ Technical Stack

### Backend
- **Framework**: ASP.NET Core 10.0 MVC
- **Language**: C# 13
- **Authentication**: ASP.NET Identity + JWT Bearer
- **Database**: PostgreSQL 16
- **ORM**: Entity Framework Core

### Frontend
- **UI Framework**: Tailwind CSS (CDN)
- **Template Engine**: Razor Pages
- **JavaScript**: Vanilla JS

### DevOps
- **Containerization**: Docker (Multi-arch: AMD64, ARM64)
- **CI/CD**: Docker Hub
- **Deployment**: Docker Compose ready

---

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is open source and available under the MIT License.

## 👨‍💻 Developer

**Developed by Sumiran Dahal**

---

**Enjoy exploring the Employee Management System!** 🎉


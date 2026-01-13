# Employee Management System

[![Docker Hub](https://img.shields.io/badge/Docker%20Hub-sumdahl%2Femployee--management--system-blue?logo=docker)](https://hub.docker.com/r/sumdahl/employee-management-system)
[![Docker Pulls](https://img.shields.io/docker/pulls/sumdahl/employee-management-system)](https://hub.docker.com/r/sumdahl/employee-management-system)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)](https://www.postgresql.org/)

A comprehensive Employee Management System built with ASP.NET Core 10.0, featuring role-based authentication, leave management, attendance tracking, and analytics dashboard.

---

## 🐳 Quick Start with Docker (Recommended)

The fastest way to get started! No need to install .NET or PostgreSQL.

### Pull from Docker Hub

```bash
# Pull the latest image
docker pull sumdahl/employee-management-system:latest

# Or pull a specific version
docker pull sumdahl/employee-management-system:v1.0.0
```

### Run with Docker Compose

```bash
# Clone the repository (or download docker-compose.yml)
git clone <your-repo-url>
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

## 📚 Additional Documentation

- [Docker Deployment Guide](DOCKER_DEPLOYMENT.md) - Comprehensive Docker setup and deployment instructions
- [Feature Roadmap](FEATURE_ROADMAP.md) - Planned features and enhancements
- [Attendance Heatmap Implementation](ATTENDANCE_HEATMAP_IMPLEMENTATION.md) - Attendance visualization details
- [Landing Page Implementation](LANDING_PAGE_IMPLEMENTATION.md) - Landing page design and features

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is open source and available under the MIT License.

## 👨‍💻 Developer

**Developed by Sumiran Dahal**

---

**Enjoy exploring the Employee Management System!** 🎉


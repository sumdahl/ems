# Employee Management System - Quick Start Guide

## 🚀 Application Status

✅ **Application is running successfully at: http://localhost:5054**

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
- **Database**: PostgreSQL (database: `ems`)
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

✅ Role-based authentication and authorization
✅ Employee CRUD with search/filter
✅ Department management
✅ Leave request workflow with approval
✅ Attendance tracking with check-in/out
✅ Analytics dashboard
✅ Modern, responsive UI
✅ PostgreSQL database integration

## 📖 For More Details

See [walkthrough.md](file:///Users/sumdahl/.gemini/antigravity/brain/bb340644-ded6-42b0-898c-10ab95921974/walkthrough.md) for comprehensive documentation.

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

---

**Enjoy exploring our Employee Management System!** 🎉

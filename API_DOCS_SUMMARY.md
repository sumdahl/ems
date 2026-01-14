# API Documentation Summary

## 📚 Documentation Files Created

Three comprehensive documentation files have been created for testing the Employee Management System API in Postman:

### 1. API_DOCUMENTATION.md (24 KB)
**Complete API Reference Documentation**

Contains:
- ✅ All 40+ API endpoints with full details
- ✅ Request/response formats and examples
- ✅ Authentication and authorization requirements
- ✅ Data models and enums
- ✅ Field validations and constraints
- ✅ cURL examples for each endpoint
- ✅ Error response codes
- ✅ Default credentials

**Sections:**
1. Account Management (6 endpoints)
2. Dashboard (1 endpoint)
3. Employees (8 endpoints)
4. Departments (8 endpoints)
5. Attendance (5 endpoints)
6. Leave Requests (6 endpoints)
7. Home (3 endpoints)

### 2. EmployeeManagementSystem.postman_collection.json (29 KB)
**Ready-to-Import Postman Collection**

Contains:
- ✅ Pre-configured requests for all endpoints
- ✅ Automatic anti-forgery token extraction
- ✅ Environment variables (baseUrl, antiforgeryToken)
- ✅ Organized folder structure
- ✅ Test scripts for token handling
- ✅ Sample request bodies with all fields

**Collection Structure:**
```
Employee Management System API
├── Authentication (7 requests)
│   ├── Login Page (GET)
│   ├── Login (POST)
│   ├── Login as Manager
│   ├── Login as Employee
│   ├── Register Page (GET)
│   ├── Register (POST)
│   └── Logout
├── Dashboard (1 request)
├── Employees (7 requests)
├── Departments (7 requests)
├── Attendance (5 requests)
├── Leave Requests (6 requests)
└── Home (2 requests)
```

### 3. POSTMAN_TESTING_GUIDE.md (8 KB)
**Quick Start & Testing Guide**

Contains:
- ✅ Step-by-step setup instructions
- ✅ Authentication workflow
- ✅ Testing scenarios with examples
- ✅ Common issues and solutions
- ✅ Tips for effective testing
- ✅ Data reference (enums, roles, etc.)
- ✅ Advanced testing techniques

---

## 🚀 Quick Start

### Import to Postman (3 steps)
1. Open Postman
2. Click **Import** → Select `EmployeeManagementSystem.postman_collection.json`
3. Done! All endpoints ready to test

### Start Testing (3 steps)
1. Start the application: `dotnet run`
2. In Postman, run: "Login Page (GET)"
3. Then run: "Login (POST)" with default credentials

### Default Credentials
```
Admin:    admin@ems.com    / Admin@123
Manager:  manager@ems.com  / Manager@123
Employee: employee@ems.com / Employee@123
```

---

## 📋 API Endpoints Overview

### Account Management
- `GET/POST /Account/Login` - User authentication
- `GET/POST /Account/Register` - User registration (Admin only)
- `POST /Account/Logout` - Sign out

### Dashboard
- `GET /Dashboard/Index` - Statistics and overview

### Employees
- `GET /Employees/Index` - List all employees (with filters)
- `GET /Employees/Details/{id}` - Employee details
- `GET/POST /Employees/Create` - Create employee (Manager/Admin)
- `GET/POST /Employees/Edit/{id}` - Update employee (Manager/Admin)
- `POST /Employees/Delete/{id}` - Delete employee (Admin only)

### Departments
- `GET /Departments/Index` - List all departments
- `GET /Departments/Details/{id}` - Department details
- `GET/POST /Departments/Create` - Create department (Manager/Admin)
- `GET/POST /Departments/Edit/{id}` - Update department (Manager/Admin)
- `POST /Departments/Delete/{id}` - Delete department (Admin only)

### Attendance
- `GET /Attendance/Index` - List attendance records (with filters)
- `GET/POST /Attendance/CheckIn` - Record check-in
- `POST /Attendance/CheckOut/{id}` - Record check-out
- `GET /Attendance/Reports` - Attendance reports (Manager/Admin)

### Leave Requests
- `GET /LeaveRequests/Index` - List leave requests (with filters)
- `GET /LeaveRequests/Details/{id}` - Leave request details
- `GET/POST /LeaveRequests/Create` - Submit leave request
- `POST /LeaveRequests/Approve/{id}` - Approve request (Manager/Admin)
- `POST /LeaveRequests/Reject/{id}` - Reject request (Manager/Admin)

---

## 🔐 Authorization Matrix

| Endpoint | Admin | Manager | Employee |
|----------|-------|---------|----------|
| Login/Logout | ✅ | ✅ | ✅ |
| Register Users | ✅ | ❌ | ❌ |
| View Dashboard | ✅ | ✅ | ✅ |
| Create/Edit Employees | ✅ | ✅ | ❌ |
| Delete Employees | ✅ | ❌ | ❌ |
| Manage Departments | ✅ | ✅ | ❌ |
| Delete Departments | ✅ | ❌ | ❌ |
| View All Attendance | ✅ | ✅ | Own only |
| Check In/Out | ✅ | ✅ | ✅ |
| Attendance Reports | ✅ | ✅ | ❌ |
| View All Leave Requests | ✅ | ✅ | Own only |
| Submit Leave Request | ✅ | ✅ | ✅ |
| Approve/Reject Leaves | ✅ | ✅ | ❌ |

---

## 🧪 Testing Scenarios

### Scenario 1: Employee Lifecycle
```
1. Login as Admin
2. Create new employee
3. View employee list
4. Edit employee details
5. View updated employee
```

### Scenario 2: Attendance Tracking
```
1. Login as Employee
2. Check in (morning)
3. View attendance list
4. Check out (evening)
5. Verify hours worked calculated
```

### Scenario 3: Leave Request Flow
```
Employee:
1. Login as Employee
2. Submit leave request
3. View request status (Pending)

Manager:
4. Login as Manager
5. View pending requests
6. Approve/Reject request
7. Employee sees updated status
```

### Scenario 4: Department Management
```
1. Login as Manager
2. Create new department
3. View department list
4. Edit department
5. View department details with employees
```

---

## 📊 Data Models

### Employee
```json
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@company.com",
  "phone": "1234567890",
  "hireDate": "2024-01-15",
  "departmentId": 1,
  "roleId": 1,
  "salary": 50000.00,
  "isActive": true,
  "annualLeaveBalance": 20,
  "sickLeaveBalance": 10
}
```

### Attendance
```json
{
  "id": 1,
  "employeeId": 1,
  "date": "2024-01-15",
  "checkInTime": "09:00:00",
  "checkOutTime": "17:30:00",
  "hoursWorked": 8.5,
  "status": 0,
  "notes": "Regular workday"
}
```

### Leave Request
```json
{
  "id": 1,
  "employeeId": 1,
  "leaveType": 0,
  "startDate": "2024-02-01",
  "endDate": "2024-02-05",
  "reason": "Family vacation",
  "status": 0,
  "totalDays": 5
}
```

---

## 🔢 Enums Reference

### Leave Types
- `0` = Annual
- `1` = Sick
- `2` = Personal
- `3` = Unpaid
- `4` = Maternity
- `5` = Paternity

### Attendance Status
- `0` = Present
- `1` = Late (auto-set if check-in after 9:00 AM)
- `2` = Absent
- `3` = OnLeave
- `4` = Holiday

### Leave Status
- `0` = Pending
- `1` = Approved
- `2` = Rejected
- `3` = Cancelled

---

## ⚠️ Important Notes

### Anti-Forgery Tokens
- All POST requests require `__RequestVerificationToken`
- Tokens are automatically extracted by GET requests in the collection
- Always run GET before POST for forms

### Cookie-Based Authentication
- Authentication uses cookies, not JWT tokens
- Cookies are automatically managed by Postman
- Stay logged in across requests in same session

### Date Formats
- Use ISO 8601 format: `YYYY-MM-DD` for dates
- Times are stored in UTC
- Check-in/out times are TimeSpan format: `HH:mm:ss`

### Validation Rules
- Email must be valid format
- Password: min 6 chars, requires digit, uppercase, lowercase, special char
- Employee names: max 50 characters
- Department name: max 100 characters
- Leave reason: max 500 characters

---

## 🛠️ Troubleshooting

### "Invalid anti-forgery token"
→ Run the GET request first to extract fresh token

### "Redirected to login"
→ Run Login flow again (cookies expired)

### "403 Forbidden"
→ Check authorization requirements for endpoint

### "404 Not Found"
→ Verify the ID exists (run List endpoint first)

### "400 Bad Request"
→ Check required fields and data formats

---

## 📁 File Locations

```
/Users/sumdahl/dotnet_project/EmployeeManagementSystem/
├── API_DOCUMENTATION.md                          (Complete API reference)
├── EmployeeManagementSystem.postman_collection.json  (Postman collection)
├── POSTMAN_TESTING_GUIDE.md                      (Quick start guide)
└── README.md                                     (Project documentation)
```

---

## 🎯 Next Steps

1. ✅ **Import Collection**: Import the JSON file into Postman
2. ✅ **Start App**: Run `dotnet run` in project directory
3. ✅ **Test Login**: Run "Login Page (GET)" then "Login (POST)"
4. ✅ **Explore**: Try different endpoints with different roles
5. ✅ **Test Scenarios**: Follow the testing scenarios in the guide

---

## 📖 Additional Resources

- **API_DOCUMENTATION.md** - Detailed endpoint documentation with cURL examples
- **POSTMAN_TESTING_GUIDE.md** - Step-by-step testing instructions
- **README.md** - Project overview and setup instructions
- **FEATURE_ROADMAP.md** - Planned features and improvements

---

## ✨ Features Covered

✅ Complete CRUD operations for all entities  
✅ Role-based access control (Admin, Manager, Employee)  
✅ Cookie-based authentication with ASP.NET Identity  
✅ Anti-forgery token protection  
✅ Data validation and error handling  
✅ Attendance tracking with check-in/out  
✅ Leave request management with approval workflow  
✅ Department and employee management  
✅ Filtering and search capabilities  
✅ Automatic calculations (hours worked, leave days)  

---

**Happy Testing! 🚀**

For questions or issues, refer to the detailed documentation files or check the application logs.

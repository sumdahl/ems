# Employee Management System - API Documentation

## Base URL
```
http://localhost:5054
```

## Authentication
This application uses **ASP.NET Core Identity** with cookie-based authentication. Most endpoints require authentication.

### Authorization Policies
- **AdminPolicy**: Requires `Admin` role
- **ManagerPolicy**: Requires `Admin` or `Manager` role
- **EmployeePolicy**: Requires authenticated user

---

## Table of Contents
1. [Account Management](#account-management)
2. [Dashboard](#dashboard)
3. [Employees](#employees)
4. [Departments](#departments)
5. [Attendance](#attendance)
6. [Leave Requests](#leave-requests)
7. [Home](#home)

---

## Account Management

### 1. Login (GET)
**Endpoint:** `GET /Account/Login`  
**Authorization:** None (Anonymous)  
**Description:** Display login page

**Query Parameters:**
- `returnUrl` (optional): URL to redirect after successful login

**Response:** HTML View

---

### 2. Login (POST)
**Endpoint:** `POST /Account/Login`  
**Authorization:** None (Anonymous)  
**Description:** Authenticate user

**Request Headers:**
```
Content-Type: application/x-www-form-urlencoded
```

**Request Body (Form Data):**
```
Email=admin@ems.com
Password=Admin@123
RememberMe=true
__RequestVerificationToken=<token>
```

**Success Response:**
- Redirects to Dashboard or returnUrl
- Sets authentication cookie

**Error Response:**
- Returns login view with validation errors

**Example cURL:**
```bash
curl -X POST http://localhost:5054/Account/Login \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "Email=admin@ems.com&Password=Admin@123&RememberMe=true"
```

---

### 3. Register (GET)
**Endpoint:** `GET /Account/Register`  
**Authorization:** Admin only  
**Description:** Display registration page

**Response:** HTML View

---

### 4. Register (POST)
**Endpoint:** `POST /Account/Register`  
**Authorization:** Admin only  
**Description:** Create new user account

**Request Headers:**
```
Content-Type: application/x-www-form-urlencoded
Cookie: .AspNetCore.Identity.Application=<auth-cookie>
```

**Request Body (Form Data):**
```
Email=newuser@ems.com
FullName=John Doe
Password=User@123
ConfirmPassword=User@123
Role=Employee
__RequestVerificationToken=<token>
```

**Available Roles:**
- `Admin`
- `Manager`
- `Employee`

**Success Response:**
- Redirects to Dashboard
- Creates user and employee record (if role is Employee or Manager)

**Example cURL:**
```bash
curl -X POST http://localhost:5054/Account/Register \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>" \
  -d "Email=newuser@ems.com&FullName=John Doe&Password=User@123&ConfirmPassword=User@123&Role=Employee"
```

---

### 5. Logout (POST)
**Endpoint:** `POST /Account/Logout`  
**Authorization:** Authenticated  
**Description:** Sign out current user

**Request Headers:**
```
Content-Type: application/x-www-form-urlencoded
Cookie: .AspNetCore.Identity.Application=<auth-cookie>
```

**Request Body:**
```
__RequestVerificationToken=<token>
```

**Success Response:**
- Redirects to Login page
- Clears authentication cookie

---

### 6. Access Denied
**Endpoint:** `GET /Account/AccessDenied`  
**Authorization:** None  
**Description:** Display access denied page

---

## Dashboard

### 1. Dashboard Index
**Endpoint:** `GET /Dashboard/Index`  
**Authorization:** Authenticated  
**Description:** Display dashboard with statistics

**Response Data:**
- Total Employees (active)
- Total Departments
- Pending Leave Requests
- Today's Attendance Count
- Recent Leave Requests (last 5)
- Department Statistics
- Recent Attendance (for employees, last 7 days)

**Example cURL:**
```bash
curl -X GET http://localhost:5054/Dashboard/Index \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>"
```

---

## Employees

### 1. List Employees
**Endpoint:** `GET /Employees/Index`  
**Authorization:** Authenticated  
**Description:** List all employees with filtering

**Query Parameters:**
- `searchString` (optional): Search by name or email
- `departmentId` (optional): Filter by department
- `isActive` (optional): Filter by active status (true/false)

**Example:**
```
GET /Employees/Index?searchString=John&departmentId=1&isActive=true
```

**Example cURL:**
```bash
curl -X GET "http://localhost:5054/Employees/Index?searchString=John" \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>"
```

---

### 2. Employee Details
**Endpoint:** `GET /Employees/Details/{id}`  
**Authorization:** Authenticated  
**Description:** View employee details

**Path Parameters:**
- `id`: Employee ID (integer)

**Example:**
```
GET /Employees/Details/1
```

**Example cURL:**
```bash
curl -X GET http://localhost:5054/Employees/Details/1 \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>"
```

---

### 3. Create Employee (GET)
**Endpoint:** `GET /Employees/Create`  
**Authorization:** Manager or Admin  
**Description:** Display employee creation form

---

### 4. Create Employee (POST)
**Endpoint:** `POST /Employees/Create`  
**Authorization:** Manager or Admin  
**Description:** Create new employee

**Request Headers:**
```
Content-Type: application/x-www-form-urlencoded
Cookie: .AspNetCore.Identity.Application=<auth-cookie>
```

**Request Body (Form Data):**
```
FirstName=John
LastName=Doe
Email=john.doe@company.com
Phone=1234567890
HireDate=2024-01-15
DepartmentId=1
RoleId=1
Salary=50000.00
Address=123 Main St
IsActive=true
AnnualLeaveBalance=20
SickLeaveBalance=10
__RequestVerificationToken=<token>
```

**Field Validations:**
- `FirstName`: Required, max 50 characters
- `LastName`: Required, max 50 characters
- `Email`: Required, valid email format, max 100 characters
- `Phone`: Optional, valid phone format, max 20 characters
- `HireDate`: Required, date format
- `DepartmentId`: Required, must exist
- `RoleId`: Required, must exist
- `Salary`: Optional, decimal(18,2)
- `Address`: Optional, max 200 characters

**Example cURL:**
```bash
curl -X POST http://localhost:5054/Employees/Create \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>" \
  -d "FirstName=John&LastName=Doe&Email=john.doe@company.com&HireDate=2024-01-15&DepartmentId=1&RoleId=1&IsActive=true"
```

---

### 5. Edit Employee (GET)
**Endpoint:** `GET /Employees/Edit/{id}`  
**Authorization:** Manager or Admin  
**Description:** Display employee edit form

**Path Parameters:**
- `id`: Employee ID (integer)

---

### 6. Edit Employee (POST)
**Endpoint:** `POST /Employees/Edit/{id}`  
**Authorization:** Manager or Admin  
**Description:** Update employee information

**Request Body:** Same as Create Employee

**Example cURL:**
```bash
curl -X POST http://localhost:5054/Employees/Edit/1 \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>" \
  -d "Id=1&FirstName=John&LastName=Doe&Email=john.doe@company.com&HireDate=2024-01-15&DepartmentId=1&RoleId=1&IsActive=true"
```

---

### 7. Delete Employee (GET)
**Endpoint:** `GET /Employees/Delete/{id}`  
**Authorization:** Admin only  
**Description:** Display employee deletion confirmation

---

### 8. Delete Employee (POST)
**Endpoint:** `POST /Employees/Delete/{id}`  
**Authorization:** Admin only  
**Description:** Delete employee (soft delete - sets IsActive to false)

**Request Headers:**
```
Content-Type: application/x-www-form-urlencoded
Cookie: .AspNetCore.Identity.Application=<auth-cookie>
```

**Request Body:**
```
__RequestVerificationToken=<token>
```

**Example cURL:**
```bash
curl -X POST http://localhost:5054/Employees/Delete/1 \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>"
```

---

## Departments

### 1. List Departments
**Endpoint:** `GET /Departments/Index`  
**Authorization:** Manager or Admin  
**Description:** List all departments with employee counts

**Example cURL:**
```bash
curl -X GET http://localhost:5054/Departments/Index \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>"
```

---

### 2. Department Details
**Endpoint:** `GET /Departments/Details/{id}`  
**Authorization:** Manager or Admin  
**Description:** View department details with employees and roles

**Path Parameters:**
- `id`: Department ID (integer)

**Example cURL:**
```bash
curl -X GET http://localhost:5054/Departments/Details/1 \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>"
```

---

### 3. Create Department (GET)
**Endpoint:** `GET /Departments/Create`  
**Authorization:** Manager or Admin  
**Description:** Display department creation form

---

### 4. Create Department (POST)
**Endpoint:** `POST /Departments/Create`  
**Authorization:** Manager or Admin  
**Description:** Create new department

**Request Body (Form Data):**
```
Name=Engineering
Description=Software Development Team
__RequestVerificationToken=<token>
```

**Field Validations:**
- `Name`: Required, max 100 characters
- `Description`: Optional, max 500 characters

**Example cURL:**
```bash
curl -X POST http://localhost:5054/Departments/Create \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>" \
  -d "Name=Engineering&Description=Software Development Team"
```

---

### 5. Edit Department (GET)
**Endpoint:** `GET /Departments/Edit/{id}`  
**Authorization:** Manager or Admin  
**Description:** Display department edit form

---

### 6. Edit Department (POST)
**Endpoint:** `POST /Departments/Edit/{id}`  
**Authorization:** Manager or Admin  
**Description:** Update department information

**Request Body (Form Data):**
```
Id=1
Name=Engineering
Description=Updated Description
ManagerId=5
__RequestVerificationToken=<token>
```

**Example cURL:**
```bash
curl -X POST http://localhost:5054/Departments/Edit/1 \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>" \
  -d "Id=1&Name=Engineering&Description=Updated Description"
```

---

### 7. Delete Department (GET)
**Endpoint:** `GET /Departments/Delete/{id}`  
**Authorization:** Admin only  
**Description:** Display department deletion confirmation

---

### 8. Delete Department (POST)
**Endpoint:** `POST /Departments/Delete/{id}`  
**Authorization:** Admin only  
**Description:** Delete department

**Example cURL:**
```bash
curl -X POST http://localhost:5054/Departments/Delete/1 \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>"
```

---

## Attendance

### 1. List Attendance
**Endpoint:** `GET /Attendance/Index`  
**Authorization:** Authenticated  
**Description:** List attendance records with filtering and heatmap data

**Query Parameters:**
- `startDate` (optional): Filter from date (format: YYYY-MM-DD)
- `endDate` (optional): Filter to date (format: YYYY-MM-DD)

**Behavior:**
- Managers/Admins: See all attendance records
- Employees: See only their own records

**Response Includes:**
- Attendance records
- Heatmap data (last 365 days)
- Current employee ID (for check-out button)

**Example:**
```
GET /Attendance/Index?startDate=2024-01-01&endDate=2024-01-31
```

**Example cURL:**
```bash
curl -X GET "http://localhost:5054/Attendance/Index?startDate=2024-01-01&endDate=2024-01-31" \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>"
```

---

### 2. Check In (GET)
**Endpoint:** `GET /Attendance/CheckIn`  
**Authorization:** Authenticated  
**Description:** Display check-in form

**Validation:**
- Checks if user already checked in today
- Redirects to Index if already checked in

---

### 3. Check In (POST)
**Endpoint:** `POST /Attendance/CheckIn`  
**Authorization:** Authenticated  
**Description:** Record employee check-in

**Request Body (Form Data):**
```
Notes=Starting work on project X
__RequestVerificationToken=<token>
```

**Auto-populated Fields:**
- `EmployeeId`: From current user
- `Date`: Current date (UTC)
- `CheckInTime`: Current time (UTC)
- `Status`: Present (if before 9:00 AM) or Late (if after 9:00 AM)
- `CreatedAt`: Current timestamp

**Field Validations:**
- `Notes`: Optional, max 500 characters

**Example cURL:**
```bash
curl -X POST http://localhost:5054/Attendance/CheckIn \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>" \
  -d "Notes=Starting work"
```

---

### 4. Check Out (POST)
**Endpoint:** `POST /Attendance/CheckOut/{id}`  
**Authorization:** Authenticated (own records only)  
**Description:** Record employee check-out

**Path Parameters:**
- `id`: Attendance record ID (integer)

**Request Body:**
```
__RequestVerificationToken=<token>
```

**Auto-calculated Fields:**
- `CheckOutTime`: Current time (UTC)
- `HoursWorked`: Calculated from CheckInTime and CheckOutTime
- `UpdatedAt`: Current timestamp

**Validations:**
- User can only check out their own attendance
- Cannot check out if already checked out

**Example cURL:**
```bash
curl -X POST http://localhost:5054/Attendance/CheckOut/1 \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>"
```

---

### 5. Attendance Reports
**Endpoint:** `GET /Attendance/Reports`  
**Authorization:** Manager or Admin  
**Description:** View attendance reports with filtering

**Query Parameters:**
- `departmentId` (optional): Filter by department
- `month` (optional): Filter by month (format: YYYY-MM-DD)

**Default:** Current month if no month specified

**Example:**
```
GET /Attendance/Reports?departmentId=1&month=2024-01-01
```

**Example cURL:**
```bash
curl -X GET "http://localhost:5054/Attendance/Reports?departmentId=1&month=2024-01-01" \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>"
```

---

## Leave Requests

### 1. List Leave Requests
**Endpoint:** `GET /LeaveRequests/Index`  
**Authorization:** Authenticated  
**Description:** List leave requests with filtering

**Query Parameters:**
- `status` (optional): Filter by status (Pending, Approved, Rejected, Cancelled)
- `startDate` (optional): Filter from date
- `endDate` (optional): Filter to date

**Behavior:**
- Managers/Admins: See all leave requests
- Employees: See only their own requests

**Example:**
```
GET /LeaveRequests/Index?status=Pending
```

**Example cURL:**
```bash
curl -X GET "http://localhost:5054/LeaveRequests/Index?status=Pending" \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>"
```

---

### 2. Leave Request Details
**Endpoint:** `GET /LeaveRequests/Details/{id}`  
**Authorization:** Authenticated  
**Description:** View leave request details

**Path Parameters:**
- `id`: Leave request ID (integer)

**Access Control:**
- Managers/Admins: Can view all
- Employees: Can view only their own

**Example cURL:**
```bash
curl -X GET http://localhost:5054/LeaveRequests/Details/1 \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>"
```

---

### 3. Create Leave Request (GET)
**Endpoint:** `GET /LeaveRequests/Create`  
**Authorization:** Authenticated  
**Description:** Display leave request creation form

---

### 4. Create Leave Request (POST)
**Endpoint:** `POST /LeaveRequests/Create`  
**Authorization:** Authenticated  
**Description:** Submit new leave request

**Request Body (Form Data):**
```
LeaveType=Annual
StartDate=2024-02-01
EndDate=2024-02-05
Reason=Family vacation
__RequestVerificationToken=<token>
```

**Leave Types:**
- `Annual` (0)
- `Sick` (1)
- `Personal` (2)
- `Unpaid` (3)
- `Maternity` (4)
- `Paternity` (5)

**Auto-populated Fields:**
- `EmployeeId`: From current user
- `Status`: Pending
- `CreatedAt`: Current timestamp

**Field Validations:**
- `LeaveType`: Required
- `StartDate`: Required, date format
- `EndDate`: Required, date format, must be >= StartDate
- `Reason`: Required, max 500 characters

**Example cURL:**
```bash
curl -X POST http://localhost:5054/LeaveRequests/Create \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>" \
  -d "LeaveType=0&StartDate=2024-02-01&EndDate=2024-02-05&Reason=Family vacation"
```

---

### 5. Approve Leave Request (POST)
**Endpoint:** `POST /LeaveRequests/Approve/{id}`  
**Authorization:** Manager or Admin  
**Description:** Approve a leave request

**Path Parameters:**
- `id`: Leave request ID (integer)

**Request Body (Form Data):**
```
ApproverComments=Approved for vacation
__RequestVerificationToken=<token>
```

**Auto-populated Fields:**
- `Status`: Approved
- `ApprovedById`: Current user's employee ID
- `ApprovedAt`: Current timestamp

**Validation:**
- Checks if employee has sufficient leave balance
- Deducts leave days from employee balance

**Example cURL:**
```bash
curl -X POST http://localhost:5054/LeaveRequests/Approve/1 \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>" \
  -d "ApproverComments=Approved"
```

---

### 6. Reject Leave Request (POST)
**Endpoint:** `POST /LeaveRequests/Reject/{id}`  
**Authorization:** Manager or Admin  
**Description:** Reject a leave request

**Path Parameters:**
- `id`: Leave request ID (integer)

**Request Body (Form Data):**
```
ApproverComments=Insufficient staffing during this period
__RequestVerificationToken=<token>
```

**Auto-populated Fields:**
- `Status`: Rejected
- `ApprovedById`: Current user's employee ID
- `ApprovedAt`: Current timestamp

**Example cURL:**
```bash
curl -X POST http://localhost:5054/LeaveRequests/Reject/1 \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -H "Cookie: .AspNetCore.Identity.Application=<auth-cookie>" \
  -d "ApproverComments=Insufficient staffing"
```

---

## Home

### 1. Home Index
**Endpoint:** `GET /` or `GET /Home/Index`  
**Authorization:** None  
**Description:** Display landing page

---

### 2. Privacy
**Endpoint:** `GET /Home/Privacy`  
**Authorization:** None  
**Description:** Display privacy policy page

---

### 3. Error
**Endpoint:** `GET /Home/Error`  
**Authorization:** None  
**Description:** Display error page

---

## Data Models

### Employee
```json
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@company.com",
  "phone": "1234567890",
  "hireDate": "2024-01-15T00:00:00Z",
  "terminationDate": null,
  "departmentId": 1,
  "roleId": 1,
  "salary": 50000.00,
  "address": "123 Main St",
  "isActive": true,
  "annualLeaveBalance": 20,
  "sickLeaveBalance": 10,
  "createdAt": "2024-01-15T10:00:00Z",
  "updatedAt": null
}
```

### Department
```json
{
  "id": 1,
  "name": "Engineering",
  "description": "Software Development Team",
  "managerId": 5,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Attendance
```json
{
  "id": 1,
  "employeeId": 1,
  "date": "2024-01-15T00:00:00Z",
  "checkInTime": "09:00:00",
  "checkOutTime": "17:30:00",
  "hoursWorked": 8.5,
  "status": 0,
  "notes": "Regular workday",
  "createdAt": "2024-01-15T09:00:00Z",
  "updatedAt": "2024-01-15T17:30:00Z"
}
```

**Attendance Status Enum:**
- `0` = Present
- `1` = Late
- `2` = Absent
- `3` = OnLeave
- `4` = Holiday

### Leave Request
```json
{
  "id": 1,
  "employeeId": 1,
  "leaveType": 0,
  "startDate": "2024-02-01T00:00:00Z",
  "endDate": "2024-02-05T00:00:00Z",
  "reason": "Family vacation",
  "status": 0,
  "approvedById": null,
  "approverComments": null,
  "approvedAt": null,
  "createdAt": "2024-01-20T10:00:00Z",
  "totalDays": 5
}
```

**Leave Type Enum:**
- `0` = Annual
- `1` = Sick
- `2` = Personal
- `3` = Unpaid
- `4` = Maternity
- `5` = Paternity

**Leave Status Enum:**
- `0` = Pending
- `1` = Approved
- `2` = Rejected
- `3` = Cancelled

---

## Testing in Postman

### Step 1: Setup Environment
1. Create a new environment in Postman
2. Add variable: `baseUrl` = `http://localhost:5054`

### Step 2: Authentication Flow
1. **First Request - Login Page (GET)**
   - Get the login page to obtain anti-forgery token
   - Extract `__RequestVerificationToken` from response

2. **Second Request - Login (POST)**
   - Submit login credentials with token
   - Postman will automatically handle cookies

3. **Subsequent Requests**
   - Cookies are automatically sent with each request
   - Include `__RequestVerificationToken` for POST requests

### Step 3: Extract Anti-Forgery Token
Add this to your Postman Tests tab for GET requests:
```javascript
var html = pm.response.text();
var regex = /name="__RequestVerificationToken".*?value="(.*?)"/;
var match = regex.exec(html);
if (match) {
    pm.environment.set("antiforgeryToken", match[1]);
}
```

### Step 4: Use Token in POST Requests
In your POST request body, include:
```
__RequestVerificationToken={{antiforgeryToken}}
```

### Common Test Scenarios

#### 1. Complete Login Flow
```
GET /Account/Login
POST /Account/Login (with credentials)
GET /Dashboard/Index
```

#### 2. Create Employee Flow
```
POST /Account/Login
GET /Employees/Create
POST /Employees/Create (with employee data)
GET /Employees/Index
```

#### 3. Attendance Flow
```
POST /Account/Login
GET /Attendance/CheckIn
POST /Attendance/CheckIn
GET /Attendance/Index
POST /Attendance/CheckOut/{id}
```

#### 4. Leave Request Flow
```
POST /Account/Login
GET /LeaveRequests/Create
POST /LeaveRequests/Create
GET /LeaveRequests/Index
POST /LeaveRequests/Approve/{id} (as Manager)
```

---

## Default Credentials

Based on the seeded data, you can use these credentials:

**Admin:**
```
Email: admin@ems.com
Password: Admin@123
```

**Manager:**
```
Email: manager@ems.com
Password: Manager@123
```

**Employee:**
```
Email: employee@ems.com
Password: Employee@123
```

---

## Error Responses

### 400 Bad Request
Invalid input data or validation errors

### 401 Unauthorized
Not authenticated (no valid cookie)

### 403 Forbidden
Authenticated but not authorized (insufficient permissions)

### 404 Not Found
Resource not found

### 500 Internal Server Error
Server-side error

---

## Notes

1. **Anti-Forgery Tokens**: All POST requests require `__RequestVerificationToken` for CSRF protection
2. **Cookie-Based Auth**: Authentication uses cookies, not JWT tokens
3. **Date Formats**: Use ISO 8601 format (YYYY-MM-DDTHH:mm:ssZ) for dates
4. **Time Zones**: All dates/times are stored in UTC
5. **Soft Deletes**: Employees are soft-deleted (IsActive = false) rather than hard-deleted
6. **Leave Balance**: Automatically deducted when leave is approved
7. **Attendance Status**: Automatically set to "Late" if check-in is after 9:00 AM

---

## Postman Collection Structure

Recommended folder structure for your Postman collection:

```
Employee Management System
├── Authentication
│   ├── Login (GET)
│   ├── Login (POST)
│   ├── Register (GET)
│   ├── Register (POST)
│   └── Logout (POST)
├── Dashboard
│   └── Get Dashboard
├── Employees
│   ├── List Employees
│   ├── Get Employee Details
│   ├── Create Employee (GET)
│   ├── Create Employee (POST)
│   ├── Edit Employee (GET)
│   ├── Edit Employee (POST)
│   ├── Delete Employee (GET)
│   └── Delete Employee (POST)
├── Departments
│   ├── List Departments
│   ├── Get Department Details
│   ├── Create Department (POST)
│   ├── Edit Department (POST)
│   └── Delete Department (POST)
├── Attendance
│   ├── List Attendance
│   ├── Check In (GET)
│   ├── Check In (POST)
│   ├── Check Out (POST)
│   └── Attendance Reports
└── Leave Requests
    ├── List Leave Requests
    ├── Get Leave Request Details
    ├── Create Leave Request (POST)
    ├── Approve Leave Request (POST)
    └── Reject Leave Request (POST)
```

---

## Additional Resources

- **Application URL**: http://localhost:5054
- **Database**: PostgreSQL
- **Framework**: ASP.NET Core 9.0 with MVC
- **Authentication**: ASP.NET Core Identity

For more information, refer to the project README.md file.

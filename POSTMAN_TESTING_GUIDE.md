# Quick Start Guide - Testing EMS API with Postman

## Setup Instructions

### 1. Import Postman Collection
1. Open Postman
2. Click **Import** button
3. Select the file: `EmployeeManagementSystem.postman_collection.json`
4. Collection will be imported with all endpoints pre-configured

### 2. Environment Setup (Optional but Recommended)
The collection includes default variables, but you can create an environment:

1. Click **Environments** in Postman
2. Create new environment: "EMS Local"
3. Add variables:
   - `baseUrl` = `http://localhost:5054`
   - `antiforgeryToken` = (leave empty, auto-populated)

### 3. Start the Application
```bash
cd /Users/sumdahl/dotnet_project/EmployeeManagementSystem
dotnet run
```

Wait for the message: `Now listening on: http://localhost:5054`

---

## Testing Workflow

### Step 1: Authentication

#### Login Flow (Required for all protected endpoints)
1. **Run: "Login Page (GET)"**
   - This extracts the anti-forgery token automatically
   - Check the Postman Console to see the extracted token

2. **Run: "Login (POST)"**
   - Uses the extracted token
   - Sets authentication cookie
   - Default credentials:
     - Admin: `admin@ems.com` / `Admin@123`
     - Manager: `manager@ems.com` / `Manager@123`
     - Employee: `employee@ems.com` / `Employee@123`

3. **Verify Login**
   - Run: "Get Dashboard"
   - Should return dashboard HTML (not redirect to login)

### Step 2: Test CRUD Operations

#### Create an Employee (Manager/Admin only)
1. Login as Admin or Manager
2. Run: "Create Employee Page (GET)" - extracts token
3. Run: "Create Employee (POST)" - creates employee
4. Run: "List Employees" - verify employee appears

#### View Employee Details
1. Run: "Get Employee Details" (change ID in URL as needed)

#### Update Employee
1. Run: "Edit Employee Page (GET)" - extracts token
2. Run: "Edit Employee (POST)" - updates employee

### Step 3: Test Attendance

#### Check In
1. Login as Employee
2. Run: "Check In Page (GET)" - extracts token
3. Run: "Check In (POST)" - records check-in
4. Run: "List Attendance" - verify check-in appears

#### Check Out
1. Note the attendance ID from the list
2. Update the ID in "Check Out (POST)" URL
3. Run: "Check Out (POST)" - records check-out
4. Run: "List Attendance" - verify hours worked is calculated

### Step 4: Test Leave Requests

#### Submit Leave Request
1. Login as Employee
2. Run: "Create Leave Request Page (GET)" - extracts token
3. Run: "Create Leave Request (POST)" - submits request
4. Run: "List Leave Requests" - verify request appears with "Pending" status

#### Approve Leave Request
1. Logout (run "Logout")
2. Login as Manager (run "Login Page (GET)" then "Login as Manager")
3. Run: "List Leave Requests" - see all pending requests
4. Note the leave request ID
5. Update ID in "Approve Leave Request (POST)" URL
6. Run: "Approve Leave Request (POST)" - approves request
7. Run: "List Leave Requests" - verify status changed to "Approved"

---

## Common Issues & Solutions

### Issue: "Invalid anti-forgery token"
**Solution:**
1. Always run the GET request first (e.g., "Login Page (GET)")
2. Then immediately run the POST request
3. Don't wait too long between GET and POST (tokens expire)

### Issue: "Redirected to login page"
**Solution:**
1. Run "Login Page (GET)" then "Login (POST)" again
2. Ensure cookies are enabled in Postman
3. Check that you're using the correct credentials

### Issue: "403 Forbidden"
**Solution:**
- You're logged in but don't have permission
- Check the endpoint's authorization requirements:
  - Admin only: Register, Delete operations
  - Manager/Admin: Create/Edit employees, departments, approve leaves
  - Employee: Own records only

### Issue: "404 Not Found"
**Solution:**
- Check the ID in the URL exists
- Run "List [Resource]" first to get valid IDs

### Issue: "400 Bad Request / Validation Error"
**Solution:**
- Check all required fields are filled
- Verify data formats (dates, emails, etc.)
- Check field length limits

---

## Testing Scenarios

### Scenario 1: Complete Employee Lifecycle
```
1. Login as Admin
2. Create Employee Page (GET)
3. Create Employee (POST)
4. List Employees
5. Get Employee Details
6. Edit Employee Page (GET)
7. Edit Employee (POST)
8. List Employees (verify changes)
```

### Scenario 2: Daily Attendance Flow
```
1. Login as Employee
2. Check In Page (GET)
3. Check In (POST)
4. List Attendance (verify check-in)
... work during the day ...
5. Check Out (POST)
6. List Attendance (verify hours worked)
```

### Scenario 3: Leave Request Approval Flow
```
Employee:
1. Login as Employee
2. Create Leave Request Page (GET)
3. Create Leave Request (POST)
4. List Leave Requests (see "Pending")
5. Logout

Manager:
6. Login Page (GET)
7. Login as Manager
8. List Leave Requests (see all pending)
9. Approve Leave Request (POST)
10. List Leave Requests (see "Approved")
```

### Scenario 4: Department Management
```
1. Login as Manager
2. List Departments
3. Create Department Page (GET)
4. Create Department (POST)
5. Get Department Details
6. Edit Department Page (GET)
7. Edit Department (POST)
8. List Departments (verify changes)
```

---

## Data Reference

### Leave Types (use numeric value in POST)
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

### User Roles
- `Admin` - Full access
- `Manager` - Can manage employees, departments, approve leaves
- `Employee` - Can view own records, submit leave requests

---

## Tips for Effective Testing

### 1. Use Postman Console
- View > Show Postman Console
- See all requests, responses, and script outputs
- Helpful for debugging token extraction

### 2. Save Responses
- Click "Save Response" to keep examples
- Useful for comparing before/after changes

### 3. Use Variables
- Store frequently used IDs as variables
- Example: `{{employeeId}}`, `{{departmentId}}`

### 4. Test Scripts
All GET requests for forms include automatic token extraction:
```javascript
var html = pm.response.text();
var regex = /name="__RequestVerificationToken".*?value="(.*?)"/;
var match = regex.exec(html);
if (match) {
    pm.environment.set("antiforgeryToken", match[1]);
}
```

### 5. Collection Runner
- Use Collection Runner for automated testing
- Set up test assertions
- Run entire workflows automatically

---

## Advanced Testing

### Test with Different Roles
1. Create 3 separate environments:
   - "EMS Admin"
   - "EMS Manager"
   - "EMS Employee"
2. Each with different credentials
3. Switch environments to test role-based access

### Test Data Validation
Try invalid data to test validation:
- Empty required fields
- Invalid email formats
- End date before start date
- Negative numbers for salary
- Strings longer than max length

### Test Edge Cases
- Check in twice on same day (should fail)
- Check out without check in (should fail)
- Approve leave with insufficient balance
- Delete department with employees
- Create employee with non-existent department

---

## Quick Reference: Default Test Data

After seeding, you should have:

**Users:**
- admin@ems.com (Admin)
- manager@ems.com (Manager)
- employee@ems.com (Employee)

**Departments:**
- IT Department
- HR Department
- Sales Department

**Employees:**
- Multiple seeded employees
- Check `/Employees/Index` for full list

---

## Next Steps

1. ✅ Import collection
2. ✅ Start application
3. ✅ Test login flow
4. ✅ Test one CRUD operation
5. ✅ Test attendance flow
6. ✅ Test leave request flow
7. ✅ Test with different roles
8. ✅ Test validation and error cases

---

## Support

For detailed API documentation, see: `API_DOCUMENTATION.md`

For application features, see: `README.md`

For issues, check:
- Application logs in terminal
- Postman Console
- Browser DevTools (if testing via browser)

# ✅ Test Users Added Successfully!

## 🎯 Available Test Accounts

Your Employee Management System now has **three test users** for comprehensive testing:

### 1. **Admin User** 👑
```
Email:    admin@ems.com
Password: Admin@123
Role:     Admin
Access:   Full system access
```

**Permissions:**
- ✅ All CRUD operations
- ✅ User registration
- ✅ Delete operations
- ✅ View all data
- ✅ Manage all resources

---

### 2. **Manager User** 💼
```
Email:    manager@ems.com
Password: Manager@123
Role:     Manager
Access:   Management operations
```

**Permissions:**
- ✅ Create/Edit employees
- ✅ Create/Edit departments
- ✅ Approve/Reject leave requests
- ✅ View all attendance records
- ✅ View attendance reports
- ❌ Delete operations (Admin only)
- ❌ User registration (Admin only)

---

### 3. **Employee User** 👤
```
Email:    employee@ems.com
Password: Employee@123
Role:     Employee
Access:   Own records only
```

**Permissions:**
- ✅ View own employee details
- ✅ View own attendance records
- ✅ Check in/out
- ✅ Submit leave requests
- ✅ View own leave requests
- ❌ View other employees' data
- ❌ Create/Edit employees
- ❌ Approve leave requests
- ❌ Management operations

---

## 🧪 Testing Scenarios

### Scenario 1: Admin Full Access
```bash
# Login as Admin
curl -X POST http://localhost:5054/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@ems.com","password":"Admin@123"}'

# Can do everything:
- Create employees ✅
- Delete employees ✅
- View all data ✅
- Register new users ✅
```

### Scenario 2: Manager Operations
```bash
# Login as Manager
curl -X POST http://localhost:5054/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"manager@ems.com","password":"Manager@123"}'

# Can manage but not delete:
- Create employees ✅
- Edit employees ✅
- Approve leaves ✅
- View all attendance ✅
- Delete employees ❌ (403 Forbidden)
```

### Scenario 3: Employee Self-Service
```bash
# Login as Employee
curl -X POST http://localhost:5054/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"employee@ems.com","password":"Employee@123"}'

# Can only access own data:
- View own attendance ✅
- Check in/out ✅
- Submit leave request ✅
- View other employees ❌ (limited)
- Create employees ❌ (403 Forbidden)
```

---

## 📊 Test Matrix

| Action | Admin | Manager | Employee |
|--------|-------|---------|----------|
| **Authentication** |
| Login | ✅ | ✅ | ✅ |
| Get current user | ✅ | ✅ | ✅ |
| Refresh token | ✅ | ✅ | ✅ |
| **Employees** |
| List all employees | ✅ | ✅ | ✅ |
| View employee details | ✅ | ✅ | ✅ |
| Create employee | ✅ | ✅ | ❌ |
| Update employee | ✅ | ✅ | ❌ |
| Delete employee | ✅ | ❌ | ❌ |
| **Attendance** |
| View all attendance | ✅ | ✅ | Own only |
| Check in | ✅ | ✅ | ✅ |
| Check out | ✅ | ✅ | ✅ |
| Attendance reports | ✅ | ✅ | ❌ |
| **Leave Requests** |
| View all requests | ✅ | ✅ | Own only |
| Submit request | ✅ | ✅ | ✅ |
| Approve request | ✅ | ✅ | ❌ |
| Reject request | ✅ | ✅ | ❌ |

---

## 🎬 Quick Test Commands

### Test All Three Users
```bash
# Test Admin
curl -X POST http://localhost:5054/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@ems.com","password":"Admin@123"}' \
  | jq '.data.user.roles'

# Test Manager
curl -X POST http://localhost:5054/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"manager@ems.com","password":"Manager@123"}' \
  | jq '.data.user.roles'

# Test Employee
curl -X POST http://localhost:5054/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"employee@ems.com","password":"Employee@123"}' \
  | jq '.data.user.roles'
```

### Test Authorization
```bash
# Get Admin token
ADMIN_TOKEN=$(curl -s -X POST http://localhost:5054/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@ems.com","password":"Admin@123"}' \
  | jq -r '.data.token')

# Get Manager token
MANAGER_TOKEN=$(curl -s -X POST http://localhost:5054/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"manager@ems.com","password":"Manager@123"}' \
  | jq -r '.data.token')

# Get Employee token
EMPLOYEE_TOKEN=$(curl -s -X POST http://localhost:5054/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"employee@ems.com","password":"Employee@123"}' \
  | jq -r '.data.token')

# Test Admin can delete (should work)
curl -X DELETE http://localhost:5054/api/employees/1 \
  -H "Authorization: Bearer $ADMIN_TOKEN"

# Test Manager cannot delete (should get 403)
curl -X DELETE http://localhost:5054/api/employees/1 \
  -H "Authorization: Bearer $MANAGER_TOKEN"

# Test Employee cannot create (should get 403)
curl -X POST http://localhost:5054/api/employees \
  -H "Authorization: Bearer $EMPLOYEE_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{...}'
```

---

## 📝 Postman Testing

### Collection Setup

1. **Create Environment Variables:**
   ```
   admin_token
   manager_token
   employee_token
   ```

2. **Login Requests:**
   - Create 3 login requests (Admin, Manager, Employee)
   - Add test script to save tokens:
   ```javascript
   var jsonData = pm.response.json();
   if (jsonData.success && jsonData.data.token) {
       pm.environment.set("admin_token", jsonData.data.token);
   }
   ```

3. **Test Requests:**
   - Use `{{admin_token}}`, `{{manager_token}}`, `{{employee_token}}`
   - Test different endpoints with different roles
   - Verify 403 Forbidden for unauthorized actions

---

## 🔐 Security Verification

### Test Account Lockout
```bash
# Try 5 wrong passwords (should lock account)
for i in {1..5}; do
  curl -X POST http://localhost:5054/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"email":"employee@ems.com","password":"WrongPassword"}'
done

# 6th attempt should return "Account locked out"
curl -X POST http://localhost:5054/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"employee@ems.com","password":"Employee@123"}'
```

### Test Token Expiration
```bash
# Get token
TOKEN=$(curl -s -X POST http://localhost:5054/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@ems.com","password":"Admin@123"}' \
  | jq -r '.data.token')

# Use token (should work)
curl -X GET http://localhost:5054/api/employees \
  -H "Authorization: Bearer $TOKEN"

# Wait 61 minutes (token expires after 60 minutes)
# Then try again (should get 401 Unauthorized)
```

---

## 🎯 Summary

✅ **3 Test Users Created:**
- Admin (full access)
- Manager (management operations)
- Employee (self-service)

✅ **All Users Working:**
- Login successful ✅
- Tokens generated ✅
- Roles assigned ✅

✅ **Ready for Testing:**
- Postman collection
- cURL commands
- Automated tests
- Role-based access control

---

## 📚 Next Steps

1. **Test in Postman:**
   - Import collection
   - Test all three users
   - Verify role-based access

2. **Test Authorization:**
   - Try unauthorized actions
   - Verify 403 responses
   - Test account lockout

3. **Test Workflows:**
   - Employee submits leave
   - Manager approves leave
   - Admin manages users

4. **Document Results:**
   - Screenshot successful tests
   - Note any issues
   - Update documentation

---

**All test users are ready! Start testing your JWT authentication!** 🚀

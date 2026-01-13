namespace EmployeeManagementSystem.ViewModels;

public class DashboardStats
{
    public int TotalEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int TodayAttendance { get; set; }
}

public class DepartmentStat
{
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
}

public class AttendanceTrend
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class AttendanceHeatmapData
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public string DateString => Date.ToString("yyyy-MM-dd");
}

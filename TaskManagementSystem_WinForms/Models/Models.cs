namespace TaskManagementSystem.Models
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Employee = "Employee";
    }

    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class Employee
    {
        public int Id { get; set; }
        public string EmployeeNumber { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string Password { get; set; } = "";
        public string Department { get; set; } = "";
        public string Role { get; set; } = Roles.Employee;
        public string Email { get; set; } = "";
    }

    public enum TaskPriority
    {
        High = 1,
        Medium = 2,
        Low = 3
    }

    public enum TaskStatus
    {
        NotStarted = 1,
        InProgress = 2,
        Completed = 3,
        Delayed = 4
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Department { get; set; } = "";
        public string EmployeeNumber { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
    }

    public class TaskNote
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = "";
        public string NoteText { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}

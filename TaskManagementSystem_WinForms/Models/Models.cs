namespace TaskManagementSystem.Models
{
    public class Employee
    {
        public string EmployeeId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Password { get; set; } = "";
        public string Department { get; set; } = "";
        public bool IsManager { get; set; }
        public string Email { get; set; } = "";
    }

    public enum TaskPriority
    {
        Low = 1,
        Medium = 2,
        High = 3
    }

    public enum TaskStatus
    {
        NotStarted = 1,
        InProgress = 2,
        Completed = 3,
        Overdue = 4
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Department { get; set; } = "";
        public string EmployeeId { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public string Notes { get; set; } = "";
        public DateTime LastUpdated { get; set; }
    }
}
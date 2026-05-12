using TaskManagementSystem.Models;
using TaskStatusModel = TaskManagementSystem.Models.TaskStatus;

namespace TaskManagementSystem.Services
{
    public class ManagerService
    {
        private readonly Data.MockDatabase _db;

        public ManagerService(Data.MockDatabase database) => _db = database;

        public List<string> GetAllDepartments() => _db.GetAllDepartments();
        public List<Employee> GetAllEmployees() => _db.GetAllEmployees();
        public List<Employee> GetEmployeesByDepartment(string department) => _db.GetEmployeesByDepartment(department);
        public List<TaskItem> GetTasks(string? department = null, TaskPriority? priority = null) => _db.GetTasks(department, priority);

        public bool AddTask(string department, string employeeId, string employeeName, string title, string description, DateTime dueDate, TaskPriority priority)
        {
            var task = new TaskItem
            {
                Department = department,
                EmployeeId = employeeId,
                EmployeeName = employeeName,
                Title = title,
                Description = description,
                DueDate = dueDate,
                Priority = priority,
                Status = TaskStatusModel.NotStarted,
                Notes = ""
            };
            return _db.AddTask(task);
        }

        public bool DeleteTask(int taskId) => _db.DeleteTask(taskId);
        public void DeleteAllTasks(string? department = null, TaskPriority? priority = null) => _db.DeleteAllTasks(department, priority);
    }
}
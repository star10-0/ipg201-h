using TaskManagementSystem.Models;

namespace TaskManagementSystem.Services
{
    public class ManagerService
    {
        private readonly Data.MockDatabase _db;
        public ManagerService(Data.MockDatabase database) => _db = database;

        public List<Department> GetAllDepartments() => _db.GetAllDepartments();
        public List<Employee> GetAllEmployees() => _db.GetAllEmployees();
        public List<Employee> GetEmployeesByDepartment(string department) => _db.GetEmployeesByDepartment(department);
        public List<TaskItem> GetTasks(string? department = null, TaskPriority? priority = null) => _db.GetTasks(department, priority);
        public List<TaskNote> GetTaskNotes(int taskId) => _db.GetTaskNotes(taskId);

        public bool AddTask(TaskItem task)
        {
            var emp = _db.GetAllEmployees().FirstOrDefault(e => e.EmployeeNumber == task.EmployeeNumber);
            if (emp == null || emp.Department != task.Department) return false;
            return _db.AddTask(task);
        }

        public bool DeleteTask(int taskId) => _db.DeleteTask(taskId);
    }
}

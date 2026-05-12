using System.Text.Json;
using TaskManagementSystem.Models;
using TaskStatusModel = TaskManagementSystem.Models.TaskStatus;

namespace TaskManagementSystem.Data
{
    public class MockDatabase
    {
        private const string EmployeesFile = "employees.json";
        private const string TasksFile = "tasks.json";
        private List<Employee> _employees = new();
        private List<TaskItem> _tasks = new();
        private bool _isInitialized = false;

        public void Initialize()
        {
            if (_isInitialized) return;
            _employees = new List<Employee>();
            _tasks = new List<TaskItem>();
            LoadEmployees();
            LoadTasks();
            _isInitialized = true;
        }

        private void LoadEmployees()
        {
            if (File.Exists(EmployeesFile))
            {
                string json = File.ReadAllText(EmployeesFile);
                _employees = JsonSerializer.Deserialize<List<Employee>>(json) ?? new List<Employee>();
            }
            else
            {
                _employees.Add(new Employee { EmployeeId = "00001", Name = "احمد المدير", Password = "123456", Department = "الادارة", IsManager = true, Email = "manager@company.com" });
                _employees.Add(new Employee { EmployeeId = "10001", Name = "محمد احمد", Password = "123456", Department = "تقنية المعلومات", IsManager = false, Email = "mohammed@company.com" });
                _employees.Add(new Employee { EmployeeId = "10002", Name = "فاطمة علي", Password = "123456", Department = "الموارد البشرية", IsManager = false, Email = "fatima@company.com" });
                _employees.Add(new Employee { EmployeeId = "10003", Name = "خالد سعيد", Password = "123456", Department = "المبيعات", IsManager = false, Email = "khaled@company.com" });
                SaveEmployees();
            }
        }

        private void LoadTasks()
        {
            if (File.Exists(TasksFile))
            {
                string json = File.ReadAllText(TasksFile);
                _tasks = JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
            }
            else
            {
                _tasks.Add(new TaskItem { Id = 1, Department = "تقنية المعلومات", EmployeeId = "10001", EmployeeName = "محمد احمد", Title = "تطوير نظام جديد", Description = "تطوير نظام ادارة المهام", CreatedDate = DateTime.Now.AddDays(-5), DueDate = DateTime.Now.AddDays(7), Priority = TaskPriority.High, Status = TaskStatusModel.InProgress, Notes = "", LastUpdated = DateTime.Now });
                _tasks.Add(new TaskItem { Id = 2, Department = "الموارد البشرية", EmployeeId = "10002", EmployeeName = "فاطمة علي", Title = "تحديث السجلات", Description = "تحديث سجلات الموظفين", CreatedDate = DateTime.Now.AddDays(-3), DueDate = DateTime.Now.AddDays(2), Priority = TaskPriority.Medium, Status = TaskStatusModel.NotStarted, Notes = "", LastUpdated = DateTime.Now });
                SaveTasks();
            }
        }

        public void SaveEmployees() { File.WriteAllText(EmployeesFile, JsonSerializer.Serialize(_employees, new JsonSerializerOptions { WriteIndented = true })); }
        public void SaveTasks() { File.WriteAllText(TasksFile, JsonSerializer.Serialize(_tasks, new JsonSerializerOptions { WriteIndented = true })); }

        public Employee? Login(string employeeId, string password) => _employees.FirstOrDefault(e => e.EmployeeId == employeeId && e.Password == password);

        public bool CreateAccount(string name, string password, string department, string employeeId, string email)
        {
            if (_employees.Any(e => e.EmployeeId == employeeId)) return false;
            _employees.Add(new Employee { EmployeeId = employeeId, Name = name, Password = password, Department = department, IsManager = false, Email = email });
            SaveEmployees();
            return true;
        }

        public List<Employee> GetAllEmployees() => new List<Employee>(_employees);
        public List<Employee> GetEmployeesByDepartment(string department) => _employees.Where(e => e.Department == department && !e.IsManager).ToList();
        public List<string> GetAllDepartments() => _employees.Select(e => e.Department).Distinct().ToList();

        public List<TaskItem> GetTasks(string? department = null, TaskPriority? priority = null)
        {
            var query = _tasks.AsEnumerable();
            if (!string.IsNullOrEmpty(department)) query = query.Where(t => t.Department == department);
            if (priority.HasValue) query = query.Where(t => t.Priority == priority.Value);
            return query.ToList();
        }

        public List<TaskItem> GetEmployeeTasks(string employeeId) => _tasks.Where(t => t.EmployeeId == employeeId).ToList();
        public TaskItem? GetTaskById(int taskId) => _tasks.FirstOrDefault(t => t.Id == taskId);

        public bool AddTask(TaskItem task)
        {
            task.Id = _tasks.Count > 0 ? _tasks.Max(t => t.Id) + 1 : 1;
            task.CreatedDate = DateTime.Now;
            task.LastUpdated = DateTime.Now;
            _tasks.Add(task);
            SaveTasks();
            return true;
        }

        public bool UpdateTaskStatus(int taskId, TaskStatusModel newStatus, string? notes = null)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) return false;
            task.Status = newStatus;
            task.LastUpdated = DateTime.Now;
            if (!string.IsNullOrEmpty(notes)) task.Notes = notes;
            SaveTasks();
            return true;
        }

        public bool UpdateTaskNote(int taskId, string note)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) return false;
            task.Notes = note;
            task.LastUpdated = DateTime.Now;
            SaveTasks();
            return true;
        }

        public bool DeleteTask(int taskId)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) return false;
            _tasks.Remove(task);
            SaveTasks();
            return true;
        }

        public void DeleteAllTasks(string? department = null, TaskPriority? priority = null)
        {
            if (string.IsNullOrEmpty(department) && !priority.HasValue) _tasks.Clear();
            else
            {
                var tasksToDelete = _tasks.AsEnumerable();
                if (!string.IsNullOrEmpty(department)) tasksToDelete = tasksToDelete.Where(t => t.Department == department);
                if (priority.HasValue) tasksToDelete = tasksToDelete.Where(t => t.Priority == priority.Value);
                foreach (var task in tasksToDelete.ToList()) _tasks.Remove(task);
            }
            SaveTasks();
        }
    }
}

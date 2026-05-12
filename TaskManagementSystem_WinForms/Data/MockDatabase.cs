using System.Text.Json;
using TaskManagementSystem.Helpers;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Data
{
    public class MockDatabase
    {
        private const string EmployeesFile = "employees.json";
        private const string TasksFile = "tasks.json";
        private const string NotesFile = "tasknotes.json";
        private const string DepartmentsFile = "departments.json";

        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
        private List<Employee> _employees = new();
        private List<TaskItem> _tasks = new();
        private List<TaskNote> _notes = new();
        private List<Department> _departments = new();

        public void Initialize()
        {
            LoadDepartments(); LoadEmployees(); LoadTasks(); LoadNotes();
        }

        private void LoadDepartments()
        {
            _departments = File.Exists(DepartmentsFile)
                ? JsonSerializer.Deserialize<List<Department>>(File.ReadAllText(DepartmentsFile)) ?? new List<Department>()
                : new List<Department>
                {
                    new() { Id = 1, Name = "IT" }, new() { Id = 2, Name = "HR" }, new() { Id = 3, Name = "Sales" }, new() { Id = 4, Name = "Management" }
                };
            SaveDepartments();
        }

        private void LoadEmployees()
        {
            _employees = File.Exists(EmployeesFile)
                ? JsonSerializer.Deserialize<List<Employee>>(File.ReadAllText(EmployeesFile)) ?? new List<Employee>()
                : new List<Employee>();

            if (_employees.Count == 0)
            {
                _employees.Add(new Employee { Id = 1, EmployeeNumber = "00001", EmployeeName = "Admin User", Department = "Management", Role = Roles.Admin, Email = "admin@company.com", Password = SecurityHelper.HashPassword("admin123") });
                _employees.Add(new Employee { Id = 2, EmployeeNumber = "00002", EmployeeName = "Manager User", Department = "Management", Role = Roles.Manager, Email = "manager@company.com", Password = SecurityHelper.HashPassword("manager123") });
                _employees.Add(new Employee { Id = 3, EmployeeNumber = "10001", EmployeeName = "Employee User", Department = "IT", Role = Roles.Employee, Email = "employee@company.com", Password = SecurityHelper.HashPassword("employee123") });
                SaveEmployees();
            }
        }

        private void LoadTasks() => _tasks = File.Exists(TasksFile) ? JsonSerializer.Deserialize<List<TaskItem>>(File.ReadAllText(TasksFile)) ?? new List<TaskItem>() : new List<TaskItem>();
        private void LoadNotes() => _notes = File.Exists(NotesFile) ? JsonSerializer.Deserialize<List<TaskNote>>(File.ReadAllText(NotesFile)) ?? new List<TaskNote>() : new List<TaskNote>();

        public void SaveEmployees() => File.WriteAllText(EmployeesFile, JsonSerializer.Serialize(_employees, _jsonOptions));
        public void SaveTasks() => File.WriteAllText(TasksFile, JsonSerializer.Serialize(_tasks, _jsonOptions));
        public void SaveNotes() => File.WriteAllText(NotesFile, JsonSerializer.Serialize(_notes, _jsonOptions));
        public void SaveDepartments() => File.WriteAllText(DepartmentsFile, JsonSerializer.Serialize(_departments, _jsonOptions));

        public Employee? Login(string usernameOrEmail, string password)
            => _employees.FirstOrDefault(e => (e.EmployeeNumber.Equals(usernameOrEmail.Trim(), StringComparison.OrdinalIgnoreCase) || e.Email.Equals(usernameOrEmail.Trim(), StringComparison.OrdinalIgnoreCase)) && SecurityHelper.VerifyPassword(password, e.Password));

        public Employee? GetEmployeeByUsernameOrEmail(string usernameOrEmail)
            => _employees.FirstOrDefault(e => e.EmployeeNumber.Equals(usernameOrEmail.Trim(), StringComparison.OrdinalIgnoreCase) || e.Email.Equals(usernameOrEmail.Trim(), StringComparison.OrdinalIgnoreCase));

        public bool CreateAccount(string name, string password, string department, string employeeNumber, string email)
        {
            if (_employees.Any(e => e.EmployeeNumber.Equals(employeeNumber.Trim(), StringComparison.OrdinalIgnoreCase))) return false;
            _employees.Add(new Employee { Id = _employees.Count > 0 ? _employees.Max(e => e.Id) + 1 : 1, EmployeeName = name.Trim(), Password = SecurityHelper.HashPassword(password), Department = department.Trim(), EmployeeNumber = employeeNumber.Trim(), Role = Roles.Employee, Email = email.Trim() });
            SaveEmployees();
            return true;
        }

        public List<Department> GetAllDepartments() => _departments.OrderBy(d => d.Name).ToList();
        public List<Employee> GetAllEmployees() => _employees.OrderBy(e => e.EmployeeName).ToList();
        public List<Employee> GetEmployeesByDepartment(string department) => _employees.Where(e => e.Department == department).ToList();

        public List<TaskItem> GetTasks(string? department = null, TaskPriority? priority = null)
        {
            var query = _tasks.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(department)) query = query.Where(t => t.Department == department);
            if (priority.HasValue) query = query.Where(t => t.Priority == priority.Value);
            return query.OrderByDescending(t => t.CreatedDate).ToList();
        }

        public List<TaskItem> GetEmployeeTasks(string employeeNumber, int employeeId)
        {
            var normalizedEmployeeNumber = employeeNumber?.Trim() ?? string.Empty;
            return _tasks
                .Where(t => string.Equals(t.EmployeeNumber?.Trim(), normalizedEmployeeNumber, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.CreatedDate)
                .ToList();
        }
        public TaskItem? GetTaskById(int taskId) => _tasks.FirstOrDefault(t => t.Id == taskId);
        public List<TaskNote> GetTaskNotes(int taskId) => _notes.Where(n => n.TaskId == taskId).OrderBy(n => n.CreatedAt).ToList();

        public bool AddTask(TaskItem task)
        {
            task.Id = _tasks.Count > 0 ? _tasks.Max(t => t.Id) + 1 : 1;
            _tasks.Add(task);
            SaveTasks();
            return true;
        }
        public bool DeleteTask(int taskId) { var task = GetTaskById(taskId); if (task == null) return false; _tasks.Remove(task); _notes.RemoveAll(n => n.TaskId == taskId); SaveTasks(); SaveNotes(); return true; }
        public bool UpdateTaskStatus(int taskId, TaskManagementSystem.Models.TaskStatus status) { var task = GetTaskById(taskId); if (task == null) return false; task.Status = status; SaveTasks(); return true; }
        public bool AddTaskNote(int taskId, int employeeId, string employeeNumber, string note)
        {
            _notes.Add(new TaskNote
            {
                Id = _notes.Count > 0 ? _notes.Max(n => n.Id) + 1 : 1,
                TaskId = taskId,
                EmployeeId = employeeId,
                EmployeeNumber = employeeNumber,
                NoteText = note.Trim(),
                CreatedAt = DateTime.Now
            });
            SaveNotes();
            return true;
        }
    }
}

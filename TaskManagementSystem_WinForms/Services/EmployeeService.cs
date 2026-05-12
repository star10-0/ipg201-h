using TaskManagementSystem.Models;
using TaskStatusModel = TaskManagementSystem.Models.TaskStatus;

namespace TaskManagementSystem.Services
{
    public class EmployeeService
    {
        private readonly Data.MockDatabase _db;

        public EmployeeService(Data.MockDatabase database) => _db = database;

        public List<TaskItem> GetMyTasks(string employeeId) => _db.GetEmployeeTasks(employeeId);

        public bool UpdateTaskStatus(int taskId, TaskStatusModel newStatus, string? notes = null) => _db.UpdateTaskStatus(taskId, newStatus, notes);

        public bool AddNote(int taskId, string note) => _db.UpdateTaskNote(taskId, note);
    }
}
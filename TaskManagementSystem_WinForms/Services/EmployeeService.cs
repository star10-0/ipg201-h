using TaskManagementSystem.Models;

namespace TaskManagementSystem.Services
{
    public class EmployeeService
    {
        private readonly Data.MockDatabase _db;
        public EmployeeService(Data.MockDatabase database) => _db = database;

        public List<TaskItem> GetMyTasks(string employeeNumber) => _db.GetEmployeeTasks(employeeNumber);
        public List<TaskNote> GetTaskNotes(int taskId) => _db.GetTaskNotes(taskId);

        public bool UpdateTaskStatus(int taskId, TaskManagementSystem.Models.TaskStatus newStatus)
        {
            var task = _db.GetTaskById(taskId);
            if (task == null) return false;
            var valid = (task.Status == TaskManagementSystem.Models.TaskStatus.NotStarted && newStatus == TaskManagementSystem.Models.TaskStatus.InProgress)
                        || (task.Status == TaskManagementSystem.Models.TaskStatus.InProgress && newStatus == TaskManagementSystem.Models.TaskStatus.Completed)
                        || task.Status == newStatus;
            return valid && _db.UpdateTaskStatus(taskId, newStatus);
        }

        public bool AddNote(int taskId, string employeeNumber, string note)
            => !string.IsNullOrWhiteSpace(note) && _db.AddTaskNote(taskId, employeeNumber, note);
    }
}

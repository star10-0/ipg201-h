using TaskManagementSystem.Models;

namespace TaskManagementSystem.Services
{
    public class EmployeeService
    {
        private readonly Data.MockDatabase _db;
        public EmployeeService(Data.MockDatabase database) => _db = database;

        public List<TaskItem> GetMyTasks(string employeeNumber) => _db.GetEmployeeTasks(employeeNumber);
        public List<TaskNote> GetTaskNotes(int taskId) => _db.GetTaskNotes(taskId);

        public bool UpdateTaskStatus(int taskId, TaskStatus newStatus)
        {
            var task = _db.GetTaskById(taskId);
            if (task == null) return false;
            var valid = (task.Status == TaskStatus.NotStarted && newStatus == TaskStatus.InProgress)
                        || (task.Status == TaskStatus.InProgress && newStatus == TaskStatus.Completed)
                        || task.Status == newStatus;
            return valid && _db.UpdateTaskStatus(taskId, newStatus);
        }

        public bool AddNote(int taskId, string employeeNumber, string note)
            => !string.IsNullOrWhiteSpace(note) && _db.AddTaskNote(taskId, employeeNumber, note);
    }
}

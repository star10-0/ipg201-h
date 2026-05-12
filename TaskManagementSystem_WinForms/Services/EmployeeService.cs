using TaskManagementSystem.Models;
using TaskStatusModel = TaskManagementSystem.Models.TaskStatus;

namespace TaskManagementSystem.Services
{
    public class EmployeeService
    {
        private readonly Data.MockDatabase _db;
        public EmployeeService(Data.MockDatabase database) => _db = database;

        public List<TaskItem> GetMyTasks(string employeeNumber, int employeeId)
            => _db.GetEmployeeTasks(employeeNumber, employeeId);

        public List<TaskNote> GetTaskNotes(int taskId) => _db.GetTaskNotes(taskId);

        public bool UpdateTaskStatus(int taskId, string employeeNumber, TaskStatusModel newStatus, out string errorMessage)
        {
            errorMessage = string.Empty;
            var task = _db.GetTaskById(taskId);
            if (task == null)
            {
                errorMessage = "المهمة غير موجودة.";
                return false;
            }

            if (!string.Equals(task.EmployeeNumber, employeeNumber, StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "لا يمكنك تعديل مهمة لا تخصك.";
                return false;
            }

            var valid = (task.Status == TaskStatusModel.NotStarted && newStatus == TaskStatusModel.InProgress)
                        || (task.Status == TaskStatusModel.InProgress && newStatus == TaskStatusModel.Completed)
                        || task.Status == newStatus;

            if (!valid)
            {
                errorMessage = "انتقال الحالة غير مسموح.";
                return false;
            }

            return _db.UpdateTaskStatus(taskId, newStatus);
        }

        public bool AddNote(int taskId, int employeeId, string employeeNumber, string note, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(note))
            {
                errorMessage = "لا يمكن إضافة ملاحظة فارغة.";
                return false;
            }

            var task = _db.GetTaskById(taskId);
            if (task == null)
            {
                errorMessage = "المهمة غير موجودة.";
                return false;
            }

            if (!string.Equals(task.EmployeeNumber, employeeNumber, StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "لا يمكنك إضافة ملاحظة على مهمة لا تخصك.";
                return false;
            }

            return _db.AddTaskNote(taskId, employeeId, employeeNumber, note);
        }
    }
}

using TaskManagementSystem.Models;

namespace TaskManagementSystem.Services
{
    public class AuthService
    {
        private readonly Data.MockDatabase _db;

        public AuthService(Data.MockDatabase database) => _db = database;

        public Employee? Login(string employeeId, string password) => _db.Login(employeeId, password);

        public bool CreateAccount(string name, string password, string department, string employeeId, string email) => _db.CreateAccount(name, password, department, employeeId, email);

        public bool RequestPasswordReset(string employeeId, out string managerEmail)
        {
            managerEmail = string.Empty;
            var employee = _db.GetAllEmployees().FirstOrDefault(e => e.EmployeeId == employeeId);
            if (employee == null) return false;

            var manager = _db.GetAllEmployees().FirstOrDefault(e => e.IsManager);
            if (manager == null) return false;

            managerEmail = manager.Email;
            var message = $"طلب استعادة كلمة سر من الموظف: {employee.Name} ({employee.EmployeeId}) بتاريخ {DateTime.Now:yyyy-MM-dd HH:mm}";
            File.AppendAllText("password_reset_requests.log", message + Environment.NewLine);
            return true;
        }
    }
}

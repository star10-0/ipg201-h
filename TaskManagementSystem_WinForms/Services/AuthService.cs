using System.Net;
using System.Net.Mail;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Services
{
    public class AuthService
    {
        private readonly Data.MockDatabase _db;
        public AuthService(Data.MockDatabase database) => _db = database;

        public Employee? Login(string username, string password) => _db.Login(username, password);

        public bool CreateAccount(string name, string password, string department, string employeeNumber, string email)
            => _db.CreateAccount(name, password, department, employeeNumber, email);

        public bool RequestPasswordReset(string usernameOrEmail, out string error)
        {
            error = string.Empty;
            var employee = _db.GetEmployeeByUsernameOrEmail(usernameOrEmail);
            if (employee == null) { error = "User not found."; return false; }
            var manager = _db.GetAllEmployees().FirstOrDefault(e => e.Role == Roles.Manager || e.Role == Roles.Admin);
            if (manager == null || string.IsNullOrWhiteSpace(manager.Email)) { error = "Manager/Admin email not configured."; return false; }

            try
            {
                using var client = new SmtpClient("smtp.example.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential("no-reply@company.com", "smtp-password")
                };
                var body = $"Password reset assistance required.\nEmployee Name: {employee.EmployeeName}\nEmployee ID: {employee.EmployeeNumber}\nDepartment: {employee.Department}\nRequest Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                client.Send(new MailMessage("no-reply@company.com", manager.Email, "Password Reset Assistance", body));
                return true;
            }
            catch (Exception ex)
            {
                error = $"Email sending failed: {ex.Message}";
                return false;
            }
        }
    }
}

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
            if (employee == null) { error = "المستخدم غير موجود."; return false; }
            var manager = _db.GetAllEmployees().FirstOrDefault(e => e.Role == Roles.Manager || e.Role == Roles.Admin);
            if (manager == null || string.IsNullOrWhiteSpace(manager.Email)) { error = "بريد المسؤول غير مهيأ."; return false; }

            try
            {
                using var client = new SmtpClient("smtp.example.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential("no-reply@company.com", "smtp-password")
                };
                var body = $"طلب مساعدة لإعادة تعيين كلمة المرور.\nاسم الموظف: {employee.EmployeeName}\nرقم الموظف: {employee.EmployeeNumber}\nالقسم: {employee.Department}\nوقت الطلب: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                client.Send(new MailMessage("no-reply@company.com", manager.Email, "طلب إعادة تعيين كلمة المرور", body));
                return true;
            }
            catch (Exception ex)
            {
                error = $"فشل إرسال البريد الإلكتروني: {ex.Message}";
                return false;
            }
        }
    }
}

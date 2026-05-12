using TaskManagementSystem.Models;

namespace TaskManagementSystem.Services
{
    public class AuthService
    {
        private readonly Data.MockDatabase _db;

        public AuthService(Data.MockDatabase database) => _db = database;

        public Employee? Login(string employeeId, string password) => _db.Login(employeeId, password);

        public bool CreateAccount(string name, string password, string department, string employeeId) => _db.CreateAccount(name, password, department, employeeId);
    }
}
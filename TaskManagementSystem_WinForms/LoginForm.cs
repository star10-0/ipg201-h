using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services;

namespace TaskManagementSystem
{
    public partial class LoginForm : Form
    {
        private readonly MockDatabase _database = new();
        private readonly AuthService _authService;

        public LoginForm()
        {
            _database.Initialize();
            _authService = new AuthService(_database);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Login - Task Management"; ClientSize = new Size(420, 300);
            var txtUser = new TextBox { Top = 50, Left = 120, Width = 220 };
            var txtPass = new TextBox { Top = 90, Left = 120, Width = 220, UseSystemPasswordChar = true };
            var btnLogin = new Button { Text = "Login", Top = 130, Left = 120, Width = 100 };
            var btnCreate = new Button { Text = "Create New Account", Top = 170, Left = 120, Width = 220 };
            var btnForgot = new Button { Text = "Forgot Password", Top = 210, Left = 120, Width = 220 };
            Controls.AddRange(new Control[] { new Label { Text = "Username / Email", Top = 50, Left = 20 }, new Label { Text = "Password", Top = 90, Left = 20 }, txtUser, txtPass, btnLogin, btnCreate, btnForgot });

            btnLogin.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Text)) { MessageBox.Show("Username and password are required."); return; }
                var user = _authService.Login(txtUser.Text.Trim(), txtPass.Text);
                if (user == null) { MessageBox.Show("Invalid credentials."); return; }
                Hide();
                Form frm = (user.Role == Roles.Admin || user.Role == Roles.Manager) ? new ManagerForm(user, _database) : new EmployeeForm(user, _database);
                frm.FormClosed += (_, _) => Close(); frm.Show();
            };
            btnCreate.Click += (s, e) => new CreateAccountForm(_database).ShowDialog();
            btnForgot.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtUser.Text)) { MessageBox.Show("Enter username or email."); return; }
                if (_authService.RequestPasswordReset(txtUser.Text.Trim(), out var error)) MessageBox.Show("Reset request sent to manager/admin.");
                else MessageBox.Show(error);
            };
        }
    }
}

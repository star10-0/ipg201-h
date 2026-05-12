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
            Text = "تسجيل الدخول - نظام إدارة المهام"; ClientSize = new Size(420, 300);
            var txtUser = new TextBox { Top = 50, Left = 120, Width = 220 };
            var txtPass = new TextBox { Top = 90, Left = 120, Width = 220, UseSystemPasswordChar = true };
            var btnLogin = new Button { Text = "تسجيل الدخول", Top = 130, Left = 120, Width = 100 };
            var btnCreate = new Button { Text = "إنشاء حساب جديد", Top = 170, Left = 120, Width = 220 };
            var btnForgot = new Button { Text = "نسيت كلمة المرور", Top = 210, Left = 120, Width = 220 };
            Controls.AddRange(new Control[] { new Label { Text = "اسم المستخدم / البريد الإلكتروني", Top = 50, Left = 20 }, new Label { Text = "كلمة المرور", Top = 90, Left = 20 }, txtUser, txtPass, btnLogin, btnCreate, btnForgot });

            btnLogin.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Text)) { MessageBox.Show("اسم المستخدم وكلمة المرور مطلوبان."); return; }
                var user = _authService.Login(txtUser.Text.Trim(), txtPass.Text);
                if (user == null) { MessageBox.Show("بيانات الدخول غير صحيحة."); return; }
                Hide();
                Form frm = (user.Role == Roles.Admin || user.Role == Roles.Manager) ? new ManagerForm(user, _database) : new EmployeeForm(user, _database);
                frm.FormClosed += (_, _) => Close(); frm.Show();
            };
            btnCreate.Click += (s, e) => new CreateAccountForm(_database).ShowDialog();
            btnForgot.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtUser.Text)) { MessageBox.Show("الرجاء إدخال اسم المستخدم أو البريد الإلكتروني."); return; }
                if (_authService.RequestPasswordReset(txtUser.Text.Trim(), out var error)) MessageBox.Show("تم إرسال طلب إعادة التعيين إلى المسؤول.");
                else MessageBox.Show(error);
            };
        }
    }
}

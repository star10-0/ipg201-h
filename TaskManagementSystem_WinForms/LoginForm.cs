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
            Text = "تسجيل الدخول - نظام إدارة المهام"; ClientSize = new Size(500, 340);
            Font = new Font("Segoe UI", 10);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;

            var txtUser = new TextBox { Top = 55, Left = 160, Width = 300 };
            var txtPass = new TextBox { Top = 105, Left = 160, Width = 300, UseSystemPasswordChar = true };
            var btnLogin = new Button { Text = "تسجيل الدخول", Top = 160, Left = 160, Width = 300, Height = 38 };
            var btnCreate = new Button { Text = "إنشاء حساب جديد", Top = 208, Left = 160, Width = 300, Height = 38 };
            var btnForgot = new Button { Text = "نسيت كلمة المرور", Top = 256, Left = 160, Width = 300, Height = 38 };
            Controls.AddRange(new Control[]
            {
                new Label { Text = "اسم المستخدم / البريد الإلكتروني", Top = 58, Left = 20, Width = 130 },
                new Label { Text = "كلمة المرور", Top = 108, Left = 20, Width = 130 },
                txtUser, txtPass, btnLogin, btnCreate, btnForgot
            });

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

using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services;

namespace TaskManagementSystem
{
    public partial class LoginForm : Form
    {
        private readonly MockDatabase _database;
        private readonly AuthService _authService;

        public LoginForm()
        {
            _database = new MockDatabase();
            _database.Initialize();
            _authService = new AuthService(_database);

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "تسجيل الدخول - نظام ادارة المهام";
            this.Size = new Size(400, 300);
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitle = new Label { Text = "تسجيل الدخول", Top = 20, Left = 130, AutoSize = true, Font = new Font("Arial", 14, FontStyle.Bold) };
            Label lblId = new Label { Text = "الرقم الوظيفي:", Top = 70, Left = 50 };
            Label lblPassword = new Label { Text = "كلمة السر:", Top = 120, Left = 50 };

            TextBox txtId = new TextBox { Name = "txtId", Top = 65, Left = 150, Width = 180 };
            TextBox txtPassword = new TextBox { Name = "txtPassword", Top = 115, Left = 150, Width = 180, UseSystemPasswordChar = true };

            Button btnLogin = new Button { Text = "تسجيل الدخول", Top = 170, Left = 140, Width = 100 };
            Button btnCreate = new Button { Text = "انشاء حساب", Top = 210, Left = 140, Width = 100 };
            Button btnForgotPassword = new Button { Text = "نسيت كلمة السر", Top = 245, Left = 125, Width = 130 };

            btnLogin.Click += (s, e) =>
            {
                var user = _authService.Login(txtId.Text, txtPassword.Text);
                if (user != null)
                {
                    if (user.IsManager)
                    {
                        this.Hide();
                        var form = new ManagerForm(user, _database);
                        form.FormClosed += (s1, e1) => this.Close();
                        form.Show();
                    }
                    else
                    {
                        this.Hide();
                        var form = new EmployeeForm(user, _database);
                        form.FormClosed += (s1, e1) => this.Close();
                        form.Show();
                    }
                }
                else
                {
                    MessageBox.Show("الرقم الوظيفي او كلمة السر غير صحيحة!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCreate.Click += (s, e) =>
            {
                var form = new CreateAccountForm(_database);
                form.ShowDialog();
            };

            btnForgotPassword.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtId.Text))
                {
                    MessageBox.Show("ادخل الرقم الوظيفي لإرسال طلب استعادة كلمة السر.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_authService.RequestPasswordReset(txtId.Text, out var managerEmail))
                {
                    MessageBox.Show($"تم إرسال طلب استعادة كلمة السر إلى بريد المدير: {managerEmail}", "تم الإرسال", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("الرقم الوظيفي غير موجود.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            this.Controls.AddRange(new Control[] { lblTitle, lblId, lblPassword, txtId, txtPassword, btnLogin, btnCreate, btnForgotPassword });
        }
    }
}

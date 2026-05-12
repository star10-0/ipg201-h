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
            this.ClientSize = new Size(460, 360);
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitle = new Label { Text = "تسجيل الدخول", Top = 20, Left = 0, Width = 460, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 16, FontStyle.Bold) };
            Label lblId = new Label { Text = "الرقم الوظيفي:", Top = 90, Left = 320, Width = 110, TextAlign = ContentAlignment.MiddleLeft };
            Label lblPassword = new Label { Text = "كلمة السر:", Top = 145, Left = 320, Width = 110, TextAlign = ContentAlignment.MiddleLeft };

            TextBox txtId = new TextBox { Name = "txtId", Top = 85, Left = 70, Width = 230 };
            TextBox txtPassword = new TextBox { Name = "txtPassword", Top = 140, Left = 70, Width = 230, UseSystemPasswordChar = true };

            Button btnLogin = new Button { Text = "تسجيل الدخول", Top = 200, Left = 165, Width = 130, Height = 36 };
            Button btnCreate = new Button { Text = "انشاء حساب", Top = 245, Left = 165, Width = 130, Height = 36 };
            Button btnForgotPassword = new Button { Text = "نسيت كلمة السر", Top = 290, Left = 150, Width = 160, Height = 36 };

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

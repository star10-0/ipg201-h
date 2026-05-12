using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services;

namespace TaskManagementSystem
{
    public partial class CreateAccountForm : Form
    {
        private readonly MockDatabase _database;
        private readonly AuthService _authService;

        public CreateAccountForm(MockDatabase database)
        {
            _database = database;
            _authService = new AuthService(database);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "انشاء حساب جديد";
            this.Size = new Size(400, 430);
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblTitle = new Label { Text = "انشاء حساب جديد", Top = 10, Left = 130, AutoSize = true, Font = new Font("Arial", 12, FontStyle.Bold) };
            Label lblName = new Label { Text = "الاسم الكامل:", Top = 50, Left = 50 };
            Label lblPassword = new Label { Text = "كلمة السر:", Top = 90, Left = 50 };
            Label lblDept = new Label { Text = "القسم:", Top = 130, Left = 50 };
            Label lblId = new Label { Text = "الرقم الوظيفي:", Top = 170, Left = 50 };
            Label lblEmail = new Label { Text = "البريد الإلكتروني:", Top = 210, Left = 50 };

            TextBox txtName = new TextBox { Name = "txtName", Top = 45, Left = 150, Width = 180 };
            TextBox txtPassword = new TextBox { Name = "txtPassword", Top = 85, Left = 150, Width = 180 };
            ComboBox cmbDept = new ComboBox { Name = "cmbDept", Top = 125, Left = 150, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            TextBox txtId = new TextBox { Name = "txtId", Top = 165, Left = 150, Width = 180 };
            TextBox txtEmail = new TextBox { Name = "txtEmail", Top = 205, Left = 150, Width = 180 };

            var depts = _database.GetAllDepartments();
            foreach (var d in depts) cmbDept.Items.Add(d);
            if (cmbDept.Items.Count > 0) cmbDept.SelectedIndex = 0;

            Button btnCreate = new Button { Text = "انشاء", Top = 260, Left = 150, Width = 80 };
            Button btnCancel = new Button { Text = "الغاء", Top = 260, Left = 240, Width = 80 };

            btnCreate.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPassword.Text) || string.IsNullOrWhiteSpace(txtId.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("الرجاء ملء جميع الحقول!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (cmbDept.SelectedItem == null)
                {
                    MessageBox.Show("الرجاء اختيار القسم!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (_authService.CreateAccount(txtName.Text.Trim(), txtPassword.Text, cmbDept.SelectedItem.ToString()!, txtId.Text.Trim(), txtEmail.Text.Trim()))
                {
                    MessageBox.Show("تم انشاء الحساب بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("الرقم الوظيفي موجود مسبقاً!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitle, lblName, lblPassword, lblDept, lblId, lblEmail, txtName, txtPassword, cmbDept, txtId, txtEmail, btnCreate, btnCancel });
        }
    }
}

using TaskManagementSystem.Data;
using TaskManagementSystem.Services;

namespace TaskManagementSystem
{
    public partial class CreateAccountForm : Form
    {
        private readonly MockDatabase _database;
        private readonly AuthService _authService;
        public CreateAccountForm(MockDatabase database) { _database = database; _authService = new AuthService(database); InitializeComponent(); }

        private void InitializeComponent()
        {
            Text = "Register"; Size = new Size(420, 340);
            var txtName = new TextBox { Top = 30, Left = 160, Width = 220 };
            var txtPass = new TextBox { Top = 70, Left = 160, Width = 220, UseSystemPasswordChar = true };
            var cmbDept = new ComboBox { Top = 110, Left = 160, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            var txtNo = new TextBox { Top = 150, Left = 160, Width = 220 };
            var txtEmail = new TextBox { Top = 190, Left = 160, Width = 220 };
            foreach (var d in _database.GetAllDepartments()) cmbDept.Items.Add(d.Name); if (cmbDept.Items.Count > 0) cmbDept.SelectedIndex = 0;
            var btn = new Button { Text = "Create", Top = 240, Left = 160, Width = 100 };
            Controls.AddRange(new Control[] { new Label { Text = "Employee Name", Top = 30, Left = 20 }, txtName, new Label { Text = "Password", Top = 70, Left = 20 }, txtPass, new Label { Text = "Department", Top = 110, Left = 20 }, cmbDept, new Label { Text = "Employee Number", Top = 150, Left = 20 }, txtNo, new Label { Text = "Email", Top = 190, Left = 20 }, txtEmail, btn });
            btn.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPass.Text) || cmbDept.SelectedItem == null || string.IsNullOrWhiteSpace(txtNo.Text)) { MessageBox.Show("All required fields must be filled."); return; }
                var ok = _authService.CreateAccount(txtName.Text, txtPass.Text, cmbDept.SelectedItem.ToString()!, txtNo.Text, txtEmail.Text);
                MessageBox.Show(ok ? "Account created successfully." : "Duplicate employee number or save failure.");
                if (ok) Close();
            };
        }
    }
}

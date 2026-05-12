using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services;

namespace TaskManagementSystem
{
    public partial class ManagerForm : Form
    {
        private readonly ManagerService _service;
        private DataGridView _grid = new();
        private ComboBox _deptFilter = new();
        private ComboBox _priorityFilter = new();

        public ManagerForm(Employee user, MockDatabase db) { _service = new ManagerService(db); InitializeComponent(); LoadData(); }

        private void InitializeComponent()
        {
            Text = "Admin/Manager Dashboard"; Size = new Size(1200, 650);
            _deptFilter = new ComboBox { Top = 10, Left = 10, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            _priorityFilter = new ComboBox { Top = 10, Left = 200, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            var btnAdd = new Button { Text = "Add Task", Top = 10, Left = 330 }; var btnDelete = new Button { Text = "Delete Task", Top = 10, Left = 420 };
            _grid = new DataGridView { Top = 50, Left = 10, Width = 1160, Height = 550, ReadOnly = true, AutoGenerateColumns = true };
            Controls.AddRange(new Control[] { _deptFilter, _priorityFilter, btnAdd, btnDelete, _grid });
            _deptFilter.SelectedIndexChanged += (_, _) => LoadData(); _priorityFilter.SelectedIndexChanged += (_, _) => LoadData();
            btnAdd.Click += (_, _) => AddTask(); btnDelete.Click += (_, _) => DeleteTask();
        }

        private void LoadData()
        {
            var depts = _service.GetAllDepartments().Select(d => d.Name).ToList();
            if (_deptFilter.Items.Count == 0) { _deptFilter.Items.Add("All"); depts.ForEach(d => _deptFilter.Items.Add(d)); _deptFilter.SelectedIndex = 0; _priorityFilter.Items.AddRange(new[] { "All", "High", "Medium", "Low" }); _priorityFilter.SelectedIndex = 0; }
            string? dept = _deptFilter.SelectedIndex <= 0 ? null : _deptFilter.SelectedItem?.ToString();
            TaskPriority? pr = _priorityFilter.SelectedItem?.ToString() switch { "High" => TaskPriority.High, "Medium" => TaskPriority.Medium, "Low" => TaskPriority.Low, _ => null };
            var rows = _service.GetTasks(dept, pr).Select(t => new { t.Id, t.Department, t.EmployeeName, t.EmployeeNumber, TaskTitle = t.Title, DetailedDescription = t.Description, CreationDate = t.CreatedDate, DeliveryDate = t.DueDate, TaskPriority = t.Priority, TaskStatus = t.Status, Notes = string.Join(" | ", _service.GetTaskNotes(t.Id).Select(n => n.NoteText)) }).ToList();
            _grid.DataSource = rows;
        }

        private void AddTask()
        {
            var dlg = new Form { Text = "Add New Task", Size = new Size(450, 420), StartPosition = FormStartPosition.CenterParent };
            var cmbDept = new ComboBox { Top = 20, Left = 170, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            var cmbEmp = new ComboBox { Top = 60, Left = 170, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            var txtNo = new TextBox { Top = 100, Left = 170, Width = 240, ReadOnly = true };
            var txtTitle = new TextBox { Top = 140, Left = 170, Width = 240 };
            var txtDesc = new TextBox { Top = 180, Left = 170, Width = 240, Height = 60, Multiline = true };
            var dtCreate = new DateTimePicker { Top = 250, Left = 170, Width = 240, Value = DateTime.Now };
            var dtDue = new DateTimePicker { Top = 280, Left = 170, Width = 240, Value = DateTime.Now.AddDays(7) };
            var cmbPriority = new ComboBox { Top = 310, Left = 170, Width = 115, DropDownStyle = ComboBoxStyle.DropDownList };
            var cmbStatus = new ComboBox { Top = 310, Left = 295, Width = 115, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPriority.Items.AddRange(new[] { "High", "Medium", "Low" }); cmbPriority.SelectedIndex = 1;
            cmbStatus.Items.AddRange(new[] { "NotStarted", "InProgress", "Completed", "Delayed" }); cmbStatus.SelectedIndex = 0;
            foreach (var d in _service.GetAllDepartments()) cmbDept.Items.Add(d.Name); if (cmbDept.Items.Count > 0) cmbDept.SelectedIndex = 0;
            cmbDept.SelectedIndexChanged += (_, _) => { cmbEmp.Items.Clear(); var emps = _service.GetEmployeesByDepartment(cmbDept.SelectedItem?.ToString() ?? "").Where(e => e.Role == Roles.Employee).ToList(); emps.ForEach(e => cmbEmp.Items.Add($"{e.EmployeeName} ({e.EmployeeNumber})")); if (cmbEmp.Items.Count > 0) cmbEmp.SelectedIndex = 0; };
            cmbEmp.SelectedIndexChanged += (_, _) => { var emp = _service.GetEmployeesByDepartment(cmbDept.SelectedItem?.ToString() ?? "").Where(e => e.Role == Roles.Employee).ElementAtOrDefault(cmbEmp.SelectedIndex); txtNo.Text = emp?.EmployeeNumber ?? ""; };
            cmbDept.SelectedIndex = 0;
            var btn = new Button { Text = "Save", Top = 340, Left = 170 }; btn.Click += (_, _) => {
                if (cmbDept.SelectedItem == null || cmbEmp.SelectedIndex < 0 || string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtDesc.Text)) { MessageBox.Show("All fields are required."); return; }
                var emp = _service.GetEmployeesByDepartment(cmbDept.SelectedItem.ToString()!).Where(e => e.Role == Roles.Employee).ElementAt(cmbEmp.SelectedIndex);
                var task = new TaskItem { Department = cmbDept.SelectedItem.ToString()!, EmployeeName = emp.EmployeeName, EmployeeNumber = emp.EmployeeNumber, Title = txtTitle.Text.Trim(), Description = txtDesc.Text.Trim(), CreatedDate = dtCreate.Value, DueDate = dtDue.Value, Priority = Enum.Parse<TaskPriority>(cmbPriority.SelectedItem!.ToString()!), Status = Enum.Parse<TaskStatus>(cmbStatus.SelectedItem!.ToString()!) };
                if (!_service.AddTask(task)) { MessageBox.Show("Employee must belong to selected department."); return; }
                LoadData(); dlg.Close();
            };
            dlg.Controls.AddRange(new Control[] { new Label { Text = "Department", Top = 20, Left = 20 }, cmbDept, new Label { Text = "Employee Name", Top = 60, Left = 20 }, cmbEmp, new Label { Text = "Employee Number", Top = 100, Left = 20 }, txtNo, new Label { Text = "Task Title", Top = 140, Left = 20 }, txtTitle, new Label { Text = "Detailed Description", Top = 180, Left = 20 }, txtDesc, new Label { Text = "Creation Date", Top = 250, Left = 20 }, dtCreate, new Label { Text = "Delivery Date", Top = 280, Left = 20 }, dtDue, new Label { Text = "Priority / Status", Top = 310, Left = 20 }, cmbPriority, cmbStatus, btn });
            dlg.ShowDialog();
        }
        private void DeleteTask() { if (_grid.CurrentRow == null) return; var id = (int)_grid.CurrentRow.Cells["Id"].Value; _service.DeleteTask(id); LoadData(); }
    }
}

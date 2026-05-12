using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services;
using TaskStatusModel = TaskManagementSystem.Models.TaskStatus;

namespace TaskManagementSystem
{
    public partial class EmployeeForm : Form
    {
        private readonly Employee _user;
        private readonly EmployeeService _service;
        private DataGridView _grid = new();
        public EmployeeForm(Employee user, MockDatabase db) { _user = user; _service = new EmployeeService(db); if (_user.Role != Roles.Employee) { MessageBox.Show("غير مصرح لك بالدخول إلى لوحة الموظف."); Close(); return; } InitializeComponent(); LoadTasks(); }

        private void InitializeComponent()
        {
            Text = "لوحة الموظف"; Size = new Size(1100, 600);
            Controls.Add(new Label { Text = $"الاسم: {_user.EmployeeName} | الرقم: {_user.EmployeeNumber} | القسم: {_user.Department}", Top = 10, Left = 10, Width = 800 });
            var btnStatus = new Button { Text = "تحديث الحالة", Top = 35, Left = 10 }; var btnNote = new Button { Text = "إضافة ملاحظة", Top = 35, Left = 130 };
            _grid = new DataGridView { Top = 70, Left = 10, Width = 1060, Height = 480, ReadOnly = true, AutoGenerateColumns = true };
            Controls.AddRange(new Control[] { btnStatus, btnNote, _grid });
            btnStatus.Click += (_, _) => ChangeStatus(); btnNote.Click += (_, _) => AddNote();
        }

        private void LoadTasks()
        {
            _grid.DataSource = _service.GetMyTasks(_user.EmployeeNumber).Select(t => new { المعرف = t.Id, العنوان = t.Title, الوصف = t.Description, الأولوية = t.Priority, الحالة = t.Status, تاريخ_الإنشاء = t.CreatedDate, تاريخ_التسليم = t.DueDate, الملاحظات = string.Join(" | ", _service.GetTaskNotes(t.Id).Select(n => n.NoteText)) }).ToList();
        }

        private void ChangeStatus()
        {
            if (_grid.CurrentRow == null) return;
            var id = (int)_grid.CurrentRow.Cells["المعرف"].Value;
            var current = Enum.Parse<TaskStatusModel>(_grid.CurrentRow.Cells["الحالة"].Value.ToString()!);
            TaskStatusModel target = current == TaskStatusModel.NotStarted ? TaskStatusModel.InProgress : current == TaskStatusModel.InProgress ? TaskStatusModel.Completed : current;
            if (!_service.UpdateTaskStatus(id, target)) { MessageBox.Show("انتقال الحالة غير مسموح."); return; }
            LoadTasks();
        }

        private void AddNote()
        {
            if (_grid.CurrentRow == null) return;
            var id = (int)_grid.CurrentRow.Cells["المعرف"].Value;
            var note = Microsoft.VisualBasic.Interaction.InputBox("أدخل الملاحظة", "ملاحظات المهمة", "");
            if (string.IsNullOrWhiteSpace(note)) return;
            _service.AddNote(id, _user.EmployeeNumber, note);
            LoadTasks();
        }
    }
}

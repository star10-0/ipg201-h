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
        public EmployeeForm(Employee user, MockDatabase db) { _user = user; _service = new EmployeeService(db); InitializeComponent(); LoadTasks(); }

        private void InitializeComponent()
        {
            Text = "Employee Dashboard"; Size = new Size(1100, 600);
            Controls.Add(new Label { Text = $"Name: {_user.EmployeeName} | Number: {_user.EmployeeNumber} | Department: {_user.Department}", Top = 10, Left = 10, Width = 800 });
            var btnStatus = new Button { Text = "Change Status", Top = 35, Left = 10 }; var btnNote = new Button { Text = "Add Note", Top = 35, Left = 130 };
            _grid = new DataGridView { Top = 70, Left = 10, Width = 1060, Height = 480, ReadOnly = true, AutoGenerateColumns = true };
            Controls.AddRange(new Control[] { btnStatus, btnNote, _grid });
            btnStatus.Click += (_, _) => ChangeStatus(); btnNote.Click += (_, _) => AddNote();
        }

        private void LoadTasks()
        {
            _grid.DataSource = _service.GetMyTasks(_user.EmployeeNumber).Select(t => new { t.Id, Title = t.Title, Description = t.Description, Priority = t.Priority, Status = t.Status, CreationDate = t.CreatedDate, DeliveryDate = t.DueDate, Notes = string.Join(" | ", _service.GetTaskNotes(t.Id).Select(n => n.NoteText)) }).ToList();
        }

        private void ChangeStatus()
        {
            if (_grid.CurrentRow == null) return;
            var id = (int)_grid.CurrentRow.Cells["Id"].Value;
            var current = Enum.Parse<TaskStatusModel>(_grid.CurrentRow.Cells["Status"].Value.ToString()!);
            TaskStatusModel target = current == TaskStatusModel.NotStarted ? TaskStatusModel.InProgress : current == TaskStatusModel.InProgress ? TaskStatusModel.Completed : current;
            if (!_service.UpdateTaskStatus(id, target)) { MessageBox.Show("Invalid status transition."); return; }
            LoadTasks();
        }

        private void AddNote()
        {
            if (_grid.CurrentRow == null) return;
            var id = (int)_grid.CurrentRow.Cells["Id"].Value;
            var note = Microsoft.VisualBasic.Interaction.InputBox("Enter note", "Task Note", "");
            if (string.IsNullOrWhiteSpace(note)) return;
            _service.AddNote(id, _user.EmployeeNumber, note);
            LoadTasks();
        }
    }
}

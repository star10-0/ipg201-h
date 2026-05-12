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

        private Label _employeeInfoLabel = new();
        private DataGridView _grid = new();
        private ComboBox _statusCombo = new();
        private TextBox _noteTextBox = new();

        public EmployeeForm(Employee user, MockDatabase db)
        {
            _user = user;
            _service = new EmployeeService(db);

            if (_user.Role != Roles.Employee)
            {
                MessageBox.Show("غير مصرح لك بالدخول إلى لوحة الموظف.");
                Close();
                return;
            }

            InitializeComponent();
            BindEmployeeData();
            Load += EmployeeForm_Load;
        }

        private void InitializeComponent()
        {
            Text = "لوحة الموظف";
            Size = new Size(1200, 700);

            _employeeInfoLabel = new Label { Top = 10, Left = 10, Width = 1150, Height = 30 };

            var btnStatus = new Button { Name = "btnUpdateStatus", Text = "تحديث الحالة", Top = 45, Left = 10, Width = 120 };
            var btnNote = new Button { Name = "btnAddNote", Text = "إضافة ملاحظة", Top = 45, Left = 140, Width = 120 };
            var btnRefresh = new Button { Name = "btnRefreshTasks", Text = "تحديث المهام", Top = 45, Left = 270, Width = 120 };

            _statusCombo = new ComboBox
            {
                Name = "cmbStatus",
                Top = 45,
                Left = 420,
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _statusCombo.Items.AddRange(new[] { "لم تبدأ", "قيد التنفيذ", "مكتملة" });
            _statusCombo.SelectedIndex = 0;

            _noteTextBox = new TextBox
            {
                Name = "txtNote",
                Top = 45,
                Left = 600,
                Width = 480,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            _grid = new DataGridView
            {
                Name = "gridTasks",
                Top = 80,
                Left = 10,
                Width = 1150,
                Height = 570,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Controls.AddRange(new Control[] { _employeeInfoLabel, btnStatus, btnNote, btnRefresh, _statusCombo, _noteTextBox, _grid });

            btnStatus.Click += BtnStatus_Click;
            btnNote.Click += BtnNote_Click;
            btnRefresh.Click += BtnRefresh_Click;
        }

        private void EmployeeForm_Load(object? sender, EventArgs e)
        {
            LoadTasks();
        }

        private void BindEmployeeData()
        {
            _employeeInfoLabel.Text = $"الاسم: {_user.EmployeeName} | رقم الموظف: {_user.EmployeeNumber} | القسم: {_user.Department}";
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => LoadTasks();

        private void BtnStatus_Click(object? sender, EventArgs e) => ChangeStatus();

        private void BtnNote_Click(object? sender, EventArgs e) => AddNote();

        private void LoadTasks()
        {
            try
            {
                var tasks = _service.GetMyTasks(_user.EmployeeNumber, _user.Id);
                if (tasks.Count == 0)
                {
                    _grid.DataSource = null;
                    MessageBox.Show("لا توجد مهام حالياً");
                    return;
                }

                var rows = tasks.Select(t => new
                {
                    TaskID = t.Id,
                    عنوان_المهمة = t.Title,
                    الوصف = t.Description,
                    الأولوية = ToArabicPriority(t.Priority),
                    الحالة = ToArabicStatus(t.Status),
                    تاريخ_الإنشاء = t.CreatedDate.ToString("yyyy-MM-dd"),
                    تاريخ_التسليم = t.DueDate.ToString("yyyy-MM-dd"),
                    الملاحظات = string.Join(" | ", _service.GetTaskNotes(t.Id).Select(n => n.NoteText))
                }).ToList();

                _grid.DataSource = rows;
                _grid.Columns["TaskID"].Visible = false;
                
                _grid.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المهام: {ex.Message}");
            }
        }

        private void ChangeStatus()
        {
            try
            {
                if (_grid.CurrentRow == null)
                {
                    MessageBox.Show("يرجى اختيار مهمة أولاً.");
                    return;
                }

                var id = (int)_grid.CurrentRow.Cells["TaskID"].Value;
                var selectedStatus = _statusCombo.SelectedItem?.ToString() ?? "";
                var targetStatus = ParseArabicStatus(selectedStatus);

                if (targetStatus == null)
                {
                    MessageBox.Show("يرجى اختيار حالة صحيحة.");
                    return;
                }

                if (!_service.UpdateTaskStatus(id, _user.EmployeeNumber, targetStatus.Value, out var errorMessage))
                {
                    MessageBox.Show(errorMessage);
                    return;
                }

                MessageBox.Show("تم تحديث حالة المهمة بنجاح");
                LoadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحديث الحالة: {ex.Message}");
            }
        }

        private void AddNote()
        {
            try
            {
                if (_grid.CurrentRow == null)
                {
                    MessageBox.Show("يرجى اختيار مهمة أولاً.");
                    return;
                }

                var note = _noteTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(note))
                {
                    MessageBox.Show("لا يمكن إضافة ملاحظة فارغة.");
                    return;
                }

                var id = (int)_grid.CurrentRow.Cells["TaskID"].Value;
                if (!_service.AddNote(id, _user.Id, _user.EmployeeNumber, note, out var errorMessage))
                {
                    MessageBox.Show(errorMessage);
                    return;
                }

                _noteTextBox.Clear();
                MessageBox.Show("تمت إضافة الملاحظة بنجاح");
                LoadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء إضافة الملاحظة: {ex.Message}");
            }
        }

        private static string ToArabicStatus(TaskStatusModel status)
            => status switch
            {
                TaskStatusModel.NotStarted => "لم تبدأ",
                TaskStatusModel.InProgress => "قيد التنفيذ",
                TaskStatusModel.Completed => "مكتملة",
                TaskStatusModel.Delayed => "متأخرة",
                _ => status.ToString()
            };

        private static string ToArabicPriority(TaskPriority priority)
            => priority switch
            {
                TaskPriority.High => "عالية",
                TaskPriority.Medium => "متوسطة",
                TaskPriority.Low => "منخفضة",
                _ => priority.ToString()
            };

        private static TaskStatusModel? ParseArabicStatus(string status)
            => status switch
            {
                "لم تبدأ" => TaskStatusModel.NotStarted,
                "قيد التنفيذ" => TaskStatusModel.InProgress,
                "مكتملة" => TaskStatusModel.Completed,
                _ => null
            };
    }
}

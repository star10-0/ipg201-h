using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services;
using TaskStatusModel = TaskManagementSystem.Models.TaskStatus;

namespace TaskManagementSystem
{
    public partial class ManagerForm : Form
    {
        private readonly ManagerService _service;
        private DataGridView _tasksGrid = new();
        private DataGridView _employeesGrid = new();

        private ComboBox _deptFilter = new();
        private ComboBox _priorityFilter = new();

        private ComboBox _taskDeptCombo = new();
        private ComboBox _taskEmployeeCombo = new();
        private TextBox _taskEmployeeNumber = new();
        private TextBox _taskTitleText = new();
        private TextBox _taskDescriptionText = new();
        private DateTimePicker _taskCreatedDate = new();
        private DateTimePicker _taskDueDate = new();
        private ComboBox _taskPriorityCombo = new();
        private ComboBox _taskStatusCombo = new();

        public ManagerForm(Employee user, MockDatabase db)
        {
            if (user.Role != Roles.Admin && user.Role != Roles.Manager)
            {
                MessageBox.Show("غير مصرح لك بالدخول إلى لوحة المسؤول.");
                Close();
                return;
            }

            _service = new ManagerService(db);
            InitializeComponent();
            LoadLookupData();
            LoadTasks();
            LoadEmployeesSection();
        }

        private void InitializeComponent()
        {
            Text = "لوحة تحكم المسؤول/المدير";
            Size = new Size(1400, 840);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            BackColor = Color.FromArgb(245, 248, 252);
            Font = new Font("Segoe UI", 10);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 2 };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            Controls.Add(root);

            var leftPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3 };
            leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 67));
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            root.Controls.Add(leftPanel, 0, 0);

            var filterGroup = new GroupBox { Text = "تصفية المهام", Dock = DockStyle.Fill, Padding = new Padding(12) };
            var filterPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, AutoSize = true };
            _deptFilter = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            _priorityFilter = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            var deleteBtn = new Button { Text = "حذف المهمة", Width = 130, Height = 34, BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            deleteBtn.FlatAppearance.BorderSize = 0;
            filterPanel.Controls.AddRange(new Control[]
            {
                deleteBtn,
                _priorityFilter,
                new Label{Text="الأولوية", AutoSize=true, TextAlign = ContentAlignment.MiddleCenter, Padding=new Padding(0,8,8,0)},
                _deptFilter,
                new Label{Text="القسم", AutoSize=true, TextAlign = ContentAlignment.MiddleCenter, Padding=new Padding(0,8,8,0)}
            });
            filterGroup.Controls.Add(filterPanel);
            leftPanel.Controls.Add(filterGroup, 0, 0);

            var tasksGroup = new GroupBox { Text = "جميع المهام", Dock = DockStyle.Fill, Padding = new Padding(10) };
            _tasksGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            _tasksGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            _tasksGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllHeaders;
            _tasksGrid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            _tasksGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            tasksGroup.Controls.Add(_tasksGrid);
            leftPanel.Controls.Add(tasksGroup, 0, 1);

            var employeesGroup = new GroupBox { Text = "الأقسام والموظفون", Dock = DockStyle.Fill, Padding = new Padding(10) };
            _employeesGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            employeesGroup.Controls.Add(_employeesGrid);
            leftPanel.Controls.Add(employeesGroup, 0, 2);

            var addTaskGroup = new GroupBox { Text = "إضافة مهمة", Dock = DockStyle.Fill, Padding = new Padding(14) };
            root.Controls.Add(addTaskGroup, 1, 0);

            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
            addTaskGroup.Controls.Add(form);

            _taskDeptCombo = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            _taskEmployeeCombo = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            _taskEmployeeNumber = new TextBox { Dock = DockStyle.Top };
            _taskTitleText = new TextBox { Dock = DockStyle.Top };
            _taskDescriptionText = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 110, ScrollBars = ScrollBars.Vertical };
            _taskCreatedDate = new DateTimePicker { Dock = DockStyle.Top, Format = DateTimePickerFormat.Short };
            _taskDueDate = new DateTimePicker { Dock = DockStyle.Top, Format = DateTimePickerFormat.Short };
            _taskPriorityCombo = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            _taskStatusCombo = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            var addBtn = new Button { Text = "إضافة مهمة", Height = 38, Dock = DockStyle.Top, BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            addBtn.FlatAppearance.BorderSize = 0;

            AddFormRow(form, "القسم", _taskDeptCombo);
            AddFormRow(form, "اسم الموظف", _taskEmployeeCombo);
            AddFormRow(form, "الرقم الوظيفي", _taskEmployeeNumber);
            AddFormRow(form, "عنوان المهمة", _taskTitleText);
            AddFormRow(form, "الوصف التفصيلي", _taskDescriptionText);
            AddFormRow(form, "تاريخ الإنشاء", _taskCreatedDate);
            AddFormRow(form, "تاريخ التسليم", _taskDueDate);
            AddFormRow(form, "الأولوية", _taskPriorityCombo);
            AddFormRow(form, "الحالة", _taskStatusCombo);
            form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            form.Controls.Add(new Label(), 0, form.RowCount);
            form.Controls.Add(addBtn, 1, form.RowCount);
            form.RowCount++;

            _deptFilter.SelectedIndexChanged += (_, _) => LoadTasks();
            _priorityFilter.SelectedIndexChanged += (_, _) => LoadTasks();
            _taskDeptCombo.SelectedIndexChanged += (_, _) => PopulateDepartmentEmployees();
            _taskEmployeeCombo.SelectedIndexChanged += (_, _) => FillEmployeeNumberFromSelection();
            addBtn.Click += (_, _) => AddTaskFromForm();
            deleteBtn.Click += (_, _) => DeleteTask();
        }

        private static void AddFormRow(TableLayoutPanel form, string label, Control control)
        {
            form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            form.Controls.Add(new Label { Text = label, AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(0, 8, 0, 0) }, 0, form.RowCount);
            form.Controls.Add(control, 1, form.RowCount);
            form.RowCount++;
        }

        private void LoadLookupData()
        {
            var departments = _service.GetAllDepartments().Select(d => d.Name).ToList();

            _deptFilter.Items.Clear();
            _deptFilter.Items.Add("الكل");
            departments.ForEach(d => _deptFilter.Items.Add(d));
            _deptFilter.SelectedIndex = 0;

            _priorityFilter.Items.Clear();
            _priorityFilter.Items.AddRange(new[] { "الكل", "عالية", "متوسطة", "منخفضة" });
            _priorityFilter.SelectedIndex = 0;

            _taskDeptCombo.Items.Clear();
            departments.ForEach(d => _taskDeptCombo.Items.Add(d));
            if (_taskDeptCombo.Items.Count > 0) _taskDeptCombo.SelectedIndex = 0;

            _taskPriorityCombo.Items.Clear();
            _taskPriorityCombo.Items.AddRange(new[] { "عالية", "متوسطة", "منخفضة" });
            _taskPriorityCombo.SelectedIndex = 1;

            _taskStatusCombo.Items.Clear();
            _taskStatusCombo.Items.AddRange(new[] { "لم تبدأ", "قيد التنفيذ", "مكتملة", "متأخرة" });
            _taskStatusCombo.SelectedIndex = 0;

            _taskCreatedDate.Value = DateTime.Today;
            _taskDueDate.Value = DateTime.Today.AddDays(7);
        }

        private void PopulateDepartmentEmployees()
        {
            _taskEmployeeCombo.Items.Clear();
            var emps = _service.GetEmployeesByDepartment(_taskDeptCombo.SelectedItem?.ToString() ?? "")
                .Where(e => e.Role == Roles.Employee)
                .ToList();
            emps.ForEach(e => _taskEmployeeCombo.Items.Add($"{e.EmployeeName} ({e.EmployeeNumber})"));
            if (_taskEmployeeCombo.Items.Count > 0) _taskEmployeeCombo.SelectedIndex = 0;
            else _taskEmployeeNumber.Text = string.Empty;
        }

        private void FillEmployeeNumberFromSelection()
        {
            var employee = GetSelectedDepartmentEmployee();
            _taskEmployeeNumber.Text = employee?.EmployeeNumber ?? string.Empty;
        }

        private Employee? GetSelectedDepartmentEmployee()
        {
            if (_taskDeptCombo.SelectedItem == null || _taskEmployeeCombo.SelectedIndex < 0) return null;
            return _service.GetEmployeesByDepartment(_taskDeptCombo.SelectedItem.ToString()!)
                .Where(e => e.Role == Roles.Employee)
                .ElementAtOrDefault(_taskEmployeeCombo.SelectedIndex);
        }

        private void LoadTasks()
        {
            string? dept = _deptFilter.SelectedIndex <= 0 ? null : _deptFilter.SelectedItem?.ToString();
            TaskPriority? pr = _priorityFilter.SelectedItem?.ToString() switch
            {
                "عالية" => TaskPriority.High,
                "متوسطة" => TaskPriority.Medium,
                "منخفضة" => TaskPriority.Low,
                _ => null
            };

            var rows = _service.GetTasks(dept, pr).Select(t => new
            {
                المعرف = t.Id,
                القسم = t.Department,
                اسم_الموظف = t.EmployeeName,
                الرقم_الوظيفي = t.EmployeeNumber,
                عنوان_المهمة = t.Title,
                الوصف = t.Description,
                تاريخ_الإنشاء = t.CreatedDate.ToString("yyyy-MM-dd"),
                تاريخ_التسليم = t.DueDate.ToString("yyyy-MM-dd"),
                الأولوية = ToArabicPriority(t.Priority),
                الحالة = ToArabicStatus(t.Status),
                آخر_ملاحظة = _service.GetTaskNotes(t.Id).LastOrDefault()?.NoteText ?? ""
            }).ToList();

            _tasksGrid.DataSource = rows;
            if (_tasksGrid.Columns.Contains("المعرف")) _tasksGrid.Columns["المعرف"].Visible = false;
            _tasksGrid.Refresh();
        }

        private void LoadEmployeesSection()
        {
            var rows = _service.GetAllEmployees()
                .Where(e => e.Role == Roles.Employee)
                .OrderBy(e => e.Department)
                .ThenBy(e => e.EmployeeName)
                .Select(e => new { القسم = e.Department, اسم_الموظف = e.EmployeeName, الرقم_الوظيفي = e.EmployeeNumber })
                .ToList();
            _employeesGrid.DataSource = rows;
        }

        private void AddTaskFromForm()
        {
            if (_taskDeptCombo.SelectedItem == null || _taskEmployeeCombo.SelectedIndex < 0 ||
                string.IsNullOrWhiteSpace(_taskEmployeeNumber.Text) || string.IsNullOrWhiteSpace(_taskTitleText.Text) ||
                string.IsNullOrWhiteSpace(_taskDescriptionText.Text) || _taskPriorityCombo.SelectedItem == null ||
                _taskStatusCombo.SelectedItem == null)
            {
                MessageBox.Show("يرجى تعبئة جميع الحقول المطلوبة.");
                return;
            }

            var employee = GetSelectedDepartmentEmployee();
            if (employee == null)
            {
                MessageBox.Show("الموظف المحدد غير موجود.");
                return;
            }

            if (!string.Equals(employee.EmployeeNumber, _taskEmployeeNumber.Text.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("الرقم الوظيفي لا يطابق الموظف المحدد.");
                return;
            }

            if (!string.Equals(employee.Department, _taskDeptCombo.SelectedItem.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("الموظف لا ينتمي إلى القسم المحدد.");
                return;
            }

            if (_taskDueDate.Value.Date < _taskCreatedDate.Value.Date)
            {
                MessageBox.Show("تاريخ التسليم يجب أن يكون بعد تاريخ الإنشاء.");
                return;
            }

            var task = new TaskItem
            {
                Department = employee.Department,
                EmployeeName = employee.EmployeeName,
                EmployeeNumber = employee.EmployeeNumber,
                Title = _taskTitleText.Text.Trim(),
                Description = _taskDescriptionText.Text.Trim(),
                CreatedDate = _taskCreatedDate.Value.Date,
                DueDate = _taskDueDate.Value.Date,
                Priority = ParseArabicPriority(_taskPriorityCombo.SelectedItem!.ToString()!),
                Status = ParseArabicStatus(_taskStatusCombo.SelectedItem!.ToString()!) ?? TaskStatusModel.NotStarted
            };

            if (!_service.AddTask(task))
            {
                MessageBox.Show("تعذر إضافة المهمة. تحقق من بيانات الموظف والقسم.");
                return;
            }

            MessageBox.Show("تمت إضافة المهمة بنجاح");
            LoadTasks();
            _taskTitleText.Clear();
            _taskDescriptionText.Clear();
        }

        private void DeleteTask()
        {
            if (_tasksGrid.CurrentRow == null)
            {
                MessageBox.Show("يرجى اختيار مهمة للحذف.");
                return;
            }

            var confirm = MessageBox.Show("هل أنت متأكد من حذف المهمة؟", "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            var id = (int)_tasksGrid.CurrentRow.Cells["المعرف"].Value;
            if (!_service.DeleteTask(id))
            {
                MessageBox.Show("تعذر حذف المهمة.");
                return;
            }

            MessageBox.Show("تم حذف المهمة بنجاح");
            LoadTasks();
        }

        private static string ToArabicPriority(TaskPriority priority)
            => priority switch { TaskPriority.High => "عالية", TaskPriority.Medium => "متوسطة", TaskPriority.Low => "منخفضة", _ => priority.ToString() };

        private static string ToArabicStatus(TaskStatusModel status)
            => status switch { TaskStatusModel.NotStarted => "لم تبدأ", TaskStatusModel.InProgress => "قيد التنفيذ", TaskStatusModel.Completed => "مكتملة", TaskStatusModel.Delayed => "متأخرة", _ => status.ToString() };

        private static TaskPriority ParseArabicPriority(string priority)
            => priority switch { "عالية" => TaskPriority.High, "متوسطة" => TaskPriority.Medium, _ => TaskPriority.Low };

        private static TaskStatusModel? ParseArabicStatus(string status)
            => status switch { "لم تبدأ" => TaskStatusModel.NotStarted, "قيد التنفيذ" => TaskStatusModel.InProgress, "مكتملة" => TaskStatusModel.Completed, "متأخرة" => TaskStatusModel.Delayed, _ => null };
    }
}

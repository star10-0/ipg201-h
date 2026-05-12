using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskStatusModel = TaskManagementSystem.Models.TaskStatus;
using TaskManagementSystem.Services;

namespace TaskManagementSystem
{
    public partial class ManagerForm : Form
    {
        private readonly Employee _currentUser;
        private readonly MockDatabase _database;
        private readonly ManagerService _managerService;
        private ListView lvTasks = new();
        private ListView lvEmployees = new();
        private ComboBox cmbFilterDept = new();
        private ComboBox cmbFilterPriority = new();

        public ManagerForm(Employee user, MockDatabase database)
        {
            _currentUser = user;
            _database = database;
            _managerService = new ManagerService(database);
            InitializeComponent();
            LoadDepartmentsInFilters();
            LoadEmployees();
            LoadTasks();
        }

        private void InitializeComponent()
        {
            this.Text = $"لوحة تحكم المدير - {_currentUser.Name}";
            this.Size = new Size(1200, 650);
            this.RightToLeft = RightToLeft.Yes;
            this.StartPosition = FormStartPosition.CenterScreen;

            Button btnViewTasks = new Button { Text = "عرض المهام", Top = 20, Left = 20, Width = 110, Height = 35 };
            Button btnAddTask = new Button { Text = "اضافة مهمة", Top = 20, Left = 140, Width = 110, Height = 35 };
            Button btnDeleteTask = new Button { Text = "حذف مهمة", Top = 20, Left = 260, Width = 110, Height = 35 };
            Button btnDeleteFiltered = new Button { Text = "حذف حسب الفلترة", Top = 20, Left = 380, Width = 130, Height = 35 };
            Button btnLogout = new Button { Text = "تسجيل الخروج", Top = 20, Left = 1030, Width = 120, Height = 35 };

            Label lblFilterDept = new Label { Text = "فلترة القسم:", Top = 70, Left = 20, Width = 90 };
            cmbFilterDept = new ComboBox { Top = 66, Left = 120, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            Label lblFilterPriority = new Label { Text = "فلترة الأولوية:", Top = 70, Left = 320, Width = 110 };
            cmbFilterPriority = new ComboBox { Top = 66, Left = 440, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };

            lvTasks = new ListView
            {
                Top = 105,
                Left = 20,
                Width = 760,
                Height = 500,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvTasks.Columns.Add("الرقم", 50);
            lvTasks.Columns.Add("العنوان", 140);
            lvTasks.Columns.Add("الموظف", 100);
            lvTasks.Columns.Add("القسم", 100);
            lvTasks.Columns.Add("الحالة", 80);
            lvTasks.Columns.Add("الاولوية", 80);
            lvTasks.Columns.Add("تاريخ الإنشاء", 100);
            lvTasks.Columns.Add("تاريخ التسليم", 100);

            lvEmployees = new ListView
            {
                Top = 105,
                Left = 800,
                Width = 350,
                Height = 500,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvEmployees.Columns.Add("القسم", 110);
            lvEmployees.Columns.Add("اسم الموظف", 120);
            lvEmployees.Columns.Add("الرقم الوظيفي", 100);

            btnViewTasks.Click += (s, e) => LoadTasks();
            btnAddTask.Click += (s, e) => ShowAddTaskForm();
            btnDeleteTask.Click += (s, e) => DeleteSelectedTask();
            btnDeleteFiltered.Click += (s, e) => DeleteFilteredTasks();
            btnLogout.Click += (s, e) => { this.Close(); };
            cmbFilterDept.SelectedIndexChanged += (s, e) => LoadTasks();
            cmbFilterPriority.SelectedIndexChanged += (s, e) => LoadTasks();

            this.Controls.AddRange(new Control[] { btnViewTasks, btnAddTask, btnDeleteTask, btnDeleteFiltered, btnLogout, lblFilterDept, cmbFilterDept, lblFilterPriority, cmbFilterPriority, lvTasks, lvEmployees });
        }

        private void LoadDepartmentsInFilters()
        {
            cmbFilterDept.Items.Clear();
            cmbFilterDept.Items.Add("كل الأقسام");
            foreach (var dept in _managerService.GetAllDepartments().Where(d => d != "الادارة")) cmbFilterDept.Items.Add(dept);
            cmbFilterDept.SelectedIndex = 0;

            cmbFilterPriority.Items.Clear();
            cmbFilterPriority.Items.Add("كل الأولويات");
            cmbFilterPriority.Items.Add("عالية");
            cmbFilterPriority.Items.Add("متوسطة");
            cmbFilterPriority.Items.Add("منخفضة");
            cmbFilterPriority.SelectedIndex = 0;
        }

        private void LoadEmployees()
        {
            lvEmployees.Items.Clear();
            foreach (var dept in _managerService.GetAllDepartments().Where(d => d != "الادارة"))
            {
                foreach (var emp in _managerService.GetEmployeesByDepartment(dept))
                {
                    var item = new ListViewItem(dept);
                    item.SubItems.Add(emp.Name);
                    item.SubItems.Add(emp.EmployeeId);
                    lvEmployees.Items.Add(item);
                }
            }
        }

        private void LoadTasks()
        {
            lvTasks.Items.Clear();
            string? selectedDept = cmbFilterDept.SelectedIndex <= 0 ? null : cmbFilterDept.SelectedItem?.ToString();
            TaskPriority? selectedPriority = cmbFilterPriority.SelectedIndex switch
            {
                1 => TaskPriority.High,
                2 => TaskPriority.Medium,
                3 => TaskPriority.Low,
                _ => null
            };

            var tasks = _managerService.GetTasks(selectedDept, selectedPriority);
            foreach (var t in tasks)
            {
                var item = new ListViewItem(t.Id.ToString());
                item.SubItems.Add(t.Title);
                item.SubItems.Add(t.EmployeeName);
                item.SubItems.Add(t.Department);
                item.SubItems.Add(GetStatusText(t.Status));
                item.SubItems.Add(GetPriorityText(t.Priority));
                item.SubItems.Add(t.CreatedDate.ToString("yyyy-MM-dd"));
                item.SubItems.Add(t.DueDate.ToString("yyyy-MM-dd"));
                item.Tag = t;
                lvTasks.Items.Add(item);
            }
        }

        private void ShowAddTaskForm()
        {
            var depts = _database.GetAllDepartments().Where(d => d != "الادارة").ToList();
            if (depts.Count == 0)
            {
                MessageBox.Show("لا توجد اقسام متاحة!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var form = new Form { Text = "اضافة مهمة جديدة", Size = new Size(450, 450), RightToLeft = RightToLeft.Yes, StartPosition = FormStartPosition.CenterScreen };

            Label lblDept = new Label { Text = "القسم:", Top = 20, Left = 20 };
            ComboBox cmbDept = new ComboBox { Top = 20, Left = 120, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var d in depts) cmbDept.Items.Add(d);
            if (cmbDept.Items.Count > 0) cmbDept.SelectedIndex = 0;

            Label lblEmp = new Label { Text = "الموظف:", Top = 60, Left = 20 };
            ComboBox cmbEmp = new ComboBox { Top = 60, Left = 120, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblTitle = new Label { Text = "العنوان:", Top = 100, Left = 20 };
            TextBox txtTitle = new TextBox { Top = 100, Left = 120, Width = 250 };

            Label lblDesc = new Label { Text = "الوصف:", Top = 140, Left = 20 };
            TextBox txtDesc = new TextBox { Top = 140, Left = 120, Width = 250, Height = 50, Multiline = true };

            Label lblDue = new Label { Text = "تاريخ التسليم:", Top = 220, Left = 20 };
            DateTimePicker dtpDue = new DateTimePicker { Top = 220, Left = 120, Width = 250 };

            Label lblPriority = new Label { Text = "الاولوية:", Top = 260, Left = 20 };
            ComboBox cmbPriority = new ComboBox { Top = 260, Left = 120, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPriority.Items.AddRange(new object[] { "عالية", "متوسطة", "منخفضة" });
            cmbPriority.SelectedIndex = 1;

            Button btnSave = new Button { Text = "حفظ", Top = 320, Left = 120, Width = 100 };
            Button btnCancel = new Button { Text = "الغاء", Top = 320, Left = 240, Width = 100 };

            cmbDept.SelectedIndexChanged += (s, e) =>
            {
                cmbEmp.Items.Clear();
                if (cmbDept.SelectedItem != null)
                {
                    var emps = _managerService.GetEmployeesByDepartment(cmbDept.SelectedItem.ToString()!);
                    foreach (var emp in emps) cmbEmp.Items.Add(emp.Name);
                    if (cmbEmp.Items.Count > 0) cmbEmp.SelectedIndex = 0;
                }
            };
            cmbDept.SelectedIndex = 0;

            btnSave.Click += (s, e) =>
            {
                if (cmbEmp.SelectedItem == null || string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    MessageBox.Show("الرجاء ملء جميع الحقول!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var emps = _managerService.GetEmployeesByDepartment(cmbDept.SelectedItem.ToString()!);
                var emp = emps[cmbEmp.SelectedIndex];
                var priority = cmbPriority.SelectedIndex switch
                {
                    0 => TaskPriority.High,
                    1 => TaskPriority.Medium,
                    _ => TaskPriority.Low
                };

                _managerService.AddTask(cmbDept.SelectedItem.ToString()!, emp.EmployeeId, emp.Name, txtTitle.Text, txtDesc.Text, dtpDue.Value, priority);
                MessageBox.Show("تم اضافة المهمة بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTasks();
                form.Close();
            };

            btnCancel.Click += (s, e) => form.Close();
            form.Controls.AddRange(new Control[] { lblDept, cmbDept, lblEmp, cmbEmp, lblTitle, txtTitle, lblDesc, txtDesc, lblDue, dtpDue, lblPriority, cmbPriority, btnSave, btnCancel });
            form.ShowDialog();
        }

        private void DeleteSelectedTask()
        {
            if (lvTasks.SelectedItems.Count == 0)
            {
                MessageBox.Show("الرجاء اختيار مهمة!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var task = (TaskItem)lvTasks.SelectedItems[0].Tag;
            var result = MessageBox.Show($"هل تريد حذف المهمة '{task.Title}'؟", "تاكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _managerService.DeleteTask(task.Id);
                MessageBox.Show("تم حذف المهمة بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTasks();
            }
        }

        private void DeleteFilteredTasks()
        {
            string? selectedDept = cmbFilterDept.SelectedIndex <= 0 ? null : cmbFilterDept.SelectedItem?.ToString();
            TaskPriority? selectedPriority = cmbFilterPriority.SelectedIndex switch
            {
                1 => TaskPriority.High,
                2 => TaskPriority.Medium,
                3 => TaskPriority.Low,
                _ => null
            };

            var result = MessageBox.Show("هل تريد حذف جميع المهام حسب الفلترة الحالية؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            _managerService.DeleteAllTasks(selectedDept, selectedPriority);
            LoadTasks();
            MessageBox.Show("تم حذف المهام بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string GetStatusText(TaskStatusModel status) => status switch { TaskStatusModel.NotStarted => "لم تبدأ", TaskStatusModel.InProgress => "قيد التنفيذ", TaskStatusModel.Completed => "مكتملة", TaskStatusModel.Overdue => "متاخرة", _ => "غير معروفة" };
        private string GetPriorityText(TaskPriority priority) => priority switch { TaskPriority.High => "عالية", TaskPriority.Medium => "متوسطة", TaskPriority.Low => "منخفضة", _ => "غير معروفة" };
    }
}

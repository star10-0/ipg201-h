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

        public ManagerForm(Employee user, MockDatabase database)
        {
            _currentUser = user;
            _database = database;
            _managerService = new ManagerService(database);
            InitializeComponent();
            LoadTasks();
        }

        private void InitializeComponent()
        {
            this.Text = $"لوحة تحكم المدير - {_currentUser.Name}";
            this.Size = new Size(800, 500);
            this.RightToLeft = RightToLeft.Yes;
            this.StartPosition = FormStartPosition.CenterScreen;

            Button btnViewTasks = new Button { Text = "عرض المهام", Top = 20, Left = 20, Width = 120, Height = 35 };
            Button btnAddTask = new Button { Text = "اضافة مهمة", Top = 20, Left = 150, Width = 120, Height = 35 };
            Button btnDeleteTask = new Button { Text = "حذف مهمة", Top = 20, Left = 280, Width = 120, Height = 35 };
            Button btnLogout = new Button { Text = "تسجيل الخروج", Top = 20, Left = 600, Width = 120, Height = 35 };

            lvTasks = new ListView
            {
                Top = 70,
                Left = 20,
                Width = 700,
                Height = 380,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvTasks.Columns.Add("الرقم", 50);
            lvTasks.Columns.Add("العنوان", 150);
            lvTasks.Columns.Add("الموظف", 100);
            lvTasks.Columns.Add("القسم", 100);
            lvTasks.Columns.Add("الحالة", 80);
            lvTasks.Columns.Add("الاولوية", 80);
            lvTasks.Columns.Add("تاريخ التسليم", 100);

            btnViewTasks.Click += (s, e) => LoadTasks();
            btnAddTask.Click += (s, e) => ShowAddTaskForm();
            btnDeleteTask.Click += (s, e) => DeleteSelectedTask();
            btnLogout.Click += (s, e) => { this.Close(); };

            this.Controls.AddRange(new Control[] { btnViewTasks, btnAddTask, btnDeleteTask, btnLogout, lvTasks });
        }

        private void LoadTasks()
        {
            lvTasks.Items.Clear();
            var tasks = _managerService.GetTasks();
            foreach (var t in tasks)
            {
                var item = new ListViewItem(t.Id.ToString());
                item.SubItems.Add(t.Title);
                item.SubItems.Add(t.EmployeeName);
                item.SubItems.Add(t.Department);
                item.SubItems.Add(GetStatusText(t.Status));
                item.SubItems.Add(GetPriorityText(t.Priority));
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

            btnSave.Click += (s, e) =>
            {
                if (cmbEmp.SelectedItem == null || string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    MessageBox.Show("الرجاء ملء جميع الحقول!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var emps = _managerService.GetEmployeesByDepartment(cmbDept.SelectedItem.ToString()!);
                var emp = emps[cmbEmp.SelectedIndex];
                var priority = (TaskPriority)(cmbPriority.SelectedIndex + 1);

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
            var result = MessageBox.Show($"هل تريد حذف المهمة '{task.Title}'?", "تاكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _managerService.DeleteTask(task.Id);
                MessageBox.Show("تم حذف المهمة بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTasks();
            }
        }

        private string GetStatusText(TaskStatusModel status) => status switch { TaskStatusModel.NotStarted => "لم تبدأ", TaskStatusModel.InProgress => "قيد التنفيذ", TaskStatusModel.Completed => "مكتملة", TaskStatusModel.Overdue => "متاخرة", _ => "غير معروفة" };
        private string GetPriorityText(TaskPriority priority) => priority switch { TaskPriority.High => "عالية", TaskPriority.Medium => "متوسطة", TaskPriority.Low => "منخفضة", _ => "غير معروفة" };
    }
}
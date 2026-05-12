using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using TaskStatusModel = TaskManagementSystem.Models.TaskStatus;
using TaskManagementSystem.Services;

namespace TaskManagementSystem
{
    public partial class EmployeeForm : Form
    {
        private readonly Employee _currentUser;
        private readonly MockDatabase _database;
        private readonly EmployeeService _employeeService;
        private ListView lvTasks = new();

        public EmployeeForm(Employee user, MockDatabase database)
        {
            _currentUser = user;
            _database = database;
            _employeeService = new EmployeeService(database);
            InitializeComponent();
            LoadTasks();
        }

        private void InitializeComponent()
        {
            this.Text = $"لوحة الموظف - {_currentUser.Name}";
            this.Size = new Size(800, 500);
            this.RightToLeft = RightToLeft.Yes;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblInfo = new Label
            {
                Text = $"القسم: {_currentUser.Department}  |  الرقم الوظيفي: {_currentUser.EmployeeId}",
                Top = 15,
                Left = 20,
                AutoSize = true,
                Font = new Font("Arial", 10)
            };

            Button btnViewTasks = new Button { Text = "عرض مهامي", Top = 50, Left = 20, Width = 120, Height = 35 };
            Button btnUpdateStatus = new Button { Text = "تحديث الحالة", Top = 50, Left = 150, Width = 120, Height = 35 };
            Button btnAddNote = new Button { Text = "اضافة ملاحظة", Top = 50, Left = 280, Width = 120, Height = 35 };
            Button btnLogout = new Button { Text = "تسجيل الخروج", Top = 50, Left = 600, Width = 120, Height = 35 };

            lvTasks = new ListView
            {
                Top = 100,
                Left = 20,
                Width = 700,
                Height = 350,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvTasks.Columns.Add("الرقم", 50);
            lvTasks.Columns.Add("العنوان", 180);
            lvTasks.Columns.Add("الحالة", 100);
            lvTasks.Columns.Add("الاولوية", 80);
            lvTasks.Columns.Add("تاريخ التسليم", 100);
            lvTasks.Columns.Add("ملاحظة", 150);

            btnViewTasks.Click += (s, e) => LoadTasks();
            btnUpdateStatus.Click += (s, e) => UpdateTaskStatus();
            btnAddNote.Click += (s, e) => AddNoteToTask();
            btnLogout.Click += (s, e) => { this.Close(); };

            this.Controls.AddRange(new Control[] { lblInfo, btnViewTasks, btnUpdateStatus, btnAddNote, btnLogout, lvTasks });
        }

        private void LoadTasks()
        {
            lvTasks.Items.Clear();
            var tasks = _employeeService.GetMyTasks(_currentUser.EmployeeId);
            foreach (var t in tasks)
            {
                var item = new ListViewItem(t.Id.ToString());
                item.SubItems.Add(t.Title);
                item.SubItems.Add(GetStatusText(t.Status));
                item.SubItems.Add(GetPriorityText(t.Priority));
                item.SubItems.Add(t.DueDate.ToString("yyyy-MM-dd"));
                item.SubItems.Add(t.Notes);
                item.Tag = t;
                lvTasks.Items.Add(item);
            }
        }

        private void UpdateTaskStatus()
        {
            if (lvTasks.SelectedItems.Count == 0)
            {
                MessageBox.Show("الرجاء اختيار مهمة!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var task = (TaskItem)lvTasks.SelectedItems[0].Tag;
            var form = new Form { Text = "تحديث حالة المهمة", Size = new Size(300, 200), RightToLeft = RightToLeft.Yes, StartPosition = FormStartPosition.CenterScreen };

            Label lblStatus = new Label { Text = "الحالة الجديدة:", Top = 30, Left = 20 };
            ComboBox cmbStatus = new ComboBox { Top = 30, Left = 120, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            if (task.Status == TaskStatusModel.NotStarted)
                cmbStatus.Items.Add("قيد التنفيذ");
            else if (task.Status == TaskStatusModel.InProgress)
                cmbStatus.Items.Add("مكتملة");
            else
                cmbStatus.Items.Add(GetStatusText(task.Status));
            cmbStatus.SelectedIndex = 0;

            Button btnSave = new Button { Text = "حفظ", Top = 100, Left = 80, Width = 80 };
            Button btnCancel = new Button { Text = "الغاء", Top = 100, Left = 170, Width = 80 };

            btnSave.Click += (s, e) =>
            {
                TaskStatusModel newStatus;
                if (cmbStatus.SelectedItem!.ToString() == "قيد التنفيذ") newStatus = TaskStatusModel.InProgress;
                else if (cmbStatus.SelectedItem!.ToString() == "مكتملة") newStatus = TaskStatusModel.Completed;
                else newStatus = task.Status;

                if (_employeeService.UpdateTaskStatus(task.Id, newStatus))
                {
                    MessageBox.Show("تم تحديث الحالة بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTasks();
                    form.Close();
                }
                else
                {
                    MessageBox.Show("لا يمكن الانتقال لهذه الحالة حسب تسلسل التنفيذ.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            btnCancel.Click += (s, e) => form.Close();
            form.Controls.AddRange(new Control[] { lblStatus, cmbStatus, btnSave, btnCancel });
            form.ShowDialog();
        }

        private void AddNoteToTask()
        {
            if (lvTasks.SelectedItems.Count == 0)
            {
                MessageBox.Show("الرجاء اختيار مهمة!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var task = (TaskItem)lvTasks.SelectedItems[0].Tag;
            var form = new Form { Text = "اضافة ملاحظة", Size = new Size(400, 200), RightToLeft = RightToLeft.Yes, StartPosition = FormStartPosition.CenterScreen };

            Label lblNote = new Label { Text = "الملاحظة:", Top = 20, Left = 20 };
            TextBox txtNote = new TextBox { Top = 20, Left = 100, Width = 250, Height = 80, Multiline = true, Text = task.Notes };

            Button btnSave = new Button { Text = "حفظ", Top = 120, Left = 100, Width = 80 };
            Button btnCancel = new Button { Text = "الغاء", Top = 120, Left = 190, Width = 80 };

            btnSave.Click += (s, e) =>
            {
                _employeeService.AddNote(task.Id, txtNote.Text);
                MessageBox.Show("تم اضافة الملاحظة بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTasks();
                form.Close();
            };

            btnCancel.Click += (s, e) => form.Close();
            form.Controls.AddRange(new Control[] { lblNote, txtNote, btnSave, btnCancel });
            form.ShowDialog();
        }

        private string GetStatusText(TaskStatusModel status) => status switch { TaskStatusModel.NotStarted => "لم تبدأ", TaskStatusModel.InProgress => "قيد التنفيذ", TaskStatusModel.Completed => "مكتملة", TaskStatusModel.Overdue => "متاخرة", _ => "غير معروفة" };
        private string GetPriorityText(TaskPriority priority) => priority switch { TaskPriority.High => "عالية", TaskPriority.Medium => "متوسطة", TaskPriority.Low => "منخفضة", _ => "غير معروفة" };
    }
}

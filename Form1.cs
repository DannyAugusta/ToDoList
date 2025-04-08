using System;
using System.Windows.Forms;

namespace To_Do_List
{
    public partial class Form1 : Form
    {
        private Database database = new Database(); // Instance databáze
        private TextBox txtTitle;                   // TextBox pro název úkolu
        private TextBox txtDescription;             // TextBox pro popis úkolu
        private DateTimePicker dtpDueDate;          // DateTimePicker pro datum splnění

        public Form1()
        {
            StartPosition = FormStartPosition.CenterScreen; // Zobrazí formulář na střed obrazovky

            InitializeComponent();
            LoadTasks();                 // Načtení úkolů při startu
        }

        // Kliknutí na tlačítko „Přidat úkol“
        private void btnAddTask_Click(object sender, EventArgs e)
        {
            // Otevření formuláře pro nový úkol s výchozími hodnotami
            using (EditTaskForm editForm = new EditTaskForm(-1, "", "", DateTime.Now))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    // Získání dat z formuláře
                    string title = editForm.txtTitle.Text;
                    string description = editForm.txtDescription.Text;
                    DateTime dueDate = editForm.dtpDueDate.Value;

                    // Přidání do databáze
                    database.AddTask(title, description, dueDate);
                    LoadTasks(); // Obnovení seznamu
                }
            }
        }

        // Kliknutí na tlačítko „Upravit úkol“
        private void btnEditTask_Click(object sender, EventArgs e)
        {
            if (dataList.SelectedRows.Count > 0)
            {
                // Získání vybraného úkolu z datagridview
                int taskId = (int)dataList.SelectedRows[0].Cells[0].Value;
                string title = dataList.SelectedRows[0].Cells[1].Value.ToString();
                string description = dataList.SelectedRows[0].Cells[2].Value.ToString();
                DateTime dueDate = Convert.ToDateTime(dataList.SelectedRows[0].Cells["DueDate"].Value);

                // Otevře se formulář s předvyplněnými daty
                using (EditTaskForm editForm = new EditTaskForm(taskId, title, description, dueDate))
                {
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadTasks(); // Aktualizace seznamu
                    }
                }
            }
        }

        // Kliknutí na tlačítko „Smazat úkol“
        private void btnDeleteTask_Click(object sender, EventArgs e)
        {
            if (dataList.SelectedRows.Count > 0)
            {
                int taskId = (int)dataList.SelectedRows[0].Cells[0].Value;
                database.DeleteTask(taskId); // Smazání z databáze
                LoadTasks();                // Obnovení seznamu
            }
        }

        // Řeší checkbox (IsCompleted) – když se zaškrtne, musí se změna uložit
        private void DataList_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataList.IsCurrentCellDirty)
            {
                dataList.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        // Po změně hodnoty v checkboxu (stav dokončení)
        private void DataList_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataList.Columns["IsCompleted"].Index)
            {
                int taskId = Convert.ToInt32(dataList.Rows[e.RowIndex].Cells["Id"].Value);
                bool isCompleted = Convert.ToBoolean(dataList.Rows[e.RowIndex].Cells["IsCompleted"].Value);

                database.UpdateIsCompleted(taskId, isCompleted); // Aktualizace v databázi
            }
        }

        // Načte všechny úkoly z databáze a zobrazí je v datagridview
        private void LoadTasks()
        {
            // Připojení událostí
            dataList.CellValueChanged += DataList_CellValueChanged;
            dataList.CurrentCellDirtyStateChanged += DataList_CurrentCellDirtyStateChanged;

            var tasks = database.GetTasks();
            dataList.DataSource = tasks; // Přiřadí seznam do tabulky
        }
    }
}
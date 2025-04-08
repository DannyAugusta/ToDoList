using System;
using System.Windows.Forms;

namespace To_Do_List
{
    public partial class EditTaskForm : Form
    {
        // Uchovává ID úkolu (pro rozlišení, jestli jde o úpravu nebo nový úkol)
        public int TaskId { get; private set; }

        // Instance databáze pro ukládání dat
        private Database database = new Database();

        // Konstruktor formuláře – předávané hodnoty se zobrazí v polích pro úpravu
        public EditTaskForm(int taskId, string title, string description, DateTime dueDate)
        {
            StartPosition = FormStartPosition.CenterScreen; // Zobrazí formulář na střed obrazovky
            InitializeComponent();

            TaskId = taskId;

            txtTitle.Text = title;              // Vyplní název úkolu do textového pole
            txtDescription.Text = description;  // Vyplní popis úkolu do textového pole
            dtpDueDate.Value = dueDate;         // Nastaví hodnotu datumu
        }

        // Tlačítko Uložit – uloží změny do databáze (nebo vytvoří nový úkol)
        private void btnSave_Click_1(object sender, EventArgs e)
        {
            string newTitle = txtTitle.Text;
            string newDescription = txtDescription.Text;
            DateTime newDueDate = dtpDueDate.Value;

            // Pokud má úkol platné ID, aktualizujeme ho
            if (TaskId != 0)
            {
                database.UpdateTask(TaskId, newTitle, newDescription, newDueDate);
            }
            else
            {
                // Pokud ID není nastavené, přidáme nový úkol
                database.AddTask(newTitle, newDescription, newDueDate);
            }

            DialogResult = DialogResult.OK; // Vrátí výsledek do hlavního formuláře
            Close(); // Zavře editační formulář
        }

        // Tlačítko Zrušit – zavře formulář bez uložení
        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
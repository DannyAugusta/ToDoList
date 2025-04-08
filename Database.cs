using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace To_Do_List
{
    public class Database
    {
        // Připojovací řetězec k SQLite databázi
        private string connectionString = "Data Source=tasks.db;Version=3;";

        // Konstruktor – při vytvoření databáze vytvoří tabulku, pokud ještě neexistuje
        public Database()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        ID INTEGER PRIMARY KEY,
                        Title TEXT,
                        Description TEXT,
                        IsCompleted INTEGER,
                        DueDate TEXT
                    )";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery(); // Vytvoření tabulky, pokud neexistuje
                }
            }
        }

        // Vrátí nejbližší volné ID (např. 1, 2, 4 → vrátí 3)
        private int GetNextAvailableId(SQLiteConnection connection)
        {
            var usedIds = new List<int>();

            string query = "SELECT ID FROM Tasks ORDER BY ID ASC";
            using (var command = new SQLiteCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    usedIds.Add(reader.GetInt32(0)); // Načti všechna používaná ID
                }
            }

            int nextId = 1;
            foreach (int id in usedIds)
            {
                if (id != nextId)
                    break; // Pokud se ID neshoduje s očekávaným pořadím, vrátíme to jako nové volné ID
                nextId++;
            }

            return nextId;
        }

        // Přidá nový úkol do databáze
        public void AddTask(string title, string description, DateTime dueDate)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                int nextId = GetNextAvailableId(connection); // Získání volného ID

                string query = "INSERT INTO Tasks (ID, Title, Description, IsCompleted, DueDate) " +
                               "VALUES (@ID, @Title, @Description, 0, @DueDate)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", nextId);
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@DueDate", dueDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery(); // Vloží záznam do tabulky
                }
            }
        }

        // Načte všechny úkoly z databáze jako seznam Task objektů
        public List<Task> GetTasks()
        {
            List<Task> tasks = new List<Task>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Tasks";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new Task
                        {
                            ID = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Description = reader.GetString(2),
                            IsCompleted = reader.GetInt32(3) == 1, // 1 = true, 0 = false
                            DueDate = DateTime.Parse(reader.GetString(4))
                        });
                    }
                }
            }
            return tasks;
        }

        // Aktualizuje název, popis a termín splnění úkolu podle ID
        public void UpdateTask(int taskId, string title, string description, DateTime dueDate)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Tasks SET Title = @Title, Description = @Description, DueDate = @DueDate WHERE ID = @ID";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@DueDate", dueDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@ID", taskId);
                    command.ExecuteNonQuery(); // Provede update záznamu
                }
            }
        }

        // Aktualizuje stav dokončení úkolu (zaškrtnuto/nezaškrtnuto)
        public void UpdateIsCompleted(int taskId, bool isCompleted)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand("UPDATE Tasks SET IsCompleted = @IsCompleted WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@IsCompleted", isCompleted ? 1 : 0); // bool → int
                command.Parameters.AddWithValue("@Id", taskId);
                command.ExecuteNonQuery(); // Uloží změnu stavu
            }
        }

        // Smaže úkol podle ID
        public void DeleteTask(int taskId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Tasks WHERE ID = @ID";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", taskId);
                    command.ExecuteNonQuery(); // Provede smazání
                }
            }
        }
    }

    // Reprezentuje jeden úkol – datový model
    public class Task
    {
        public int ID { get; set; }                  // Primární klíč
        public string Title { get; set; }            // Název úkolu
        public string Description { get; set; }      // Popis
        public bool IsCompleted { get; set; }        // Dokončeno
        public DateTime DueDate { get; set; }        // Termín splnění
    }
}
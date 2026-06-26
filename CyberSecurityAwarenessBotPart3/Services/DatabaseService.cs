using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace CyberSecurityAwarenessBotPart3.Services
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class DatabaseService
    {
        // IMPORTANT: replace YOUR_PASSWORD_HERE with your actual MySQL root password
        private readonly string connectionString =
            "Server=localhost;Database=CyberSecurityBotDB;Uid=root;Pwd=Molobela5$;";

        public bool AddTask(string title, string description, DateTime? reminderDate)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Tasks (Title, Description, ReminderDate, IsCompleted) " +
                                   "VALUES (@title, @desc, @reminder, false)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@desc", description);
                        cmd.Parameters.AddWithValue("@reminder", reminderDate.HasValue ? (object)reminderDate.Value : DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Error (AddTask): " + ex.Message);
                return false;
            }
        }

        public List<TaskItem> GetAllTasks()
        {
            var tasks = new List<TaskItem>();

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Id, Title, Description, ReminderDate, IsCompleted FROM Tasks ORDER BY Id DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new TaskItem
                            {
                                Id = reader.GetInt32("Id"),
                                Title = reader.GetString("Title"),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description"),
                                ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate")) ? null : reader.GetDateTime("ReminderDate"),
                                IsCompleted = reader.GetBoolean("IsCompleted")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Error (GetAllTasks): " + ex.Message);
            }

            return tasks;
        }

        public bool MarkComplete(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Tasks SET IsCompleted = true WHERE Id = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Error (MarkComplete): " + ex.Message);
                return false;
            }
        }


        public bool SetReminder(int id, DateTime reminderDate)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Tasks SET ReminderDate = @reminder WHERE Id = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@reminder", reminderDate);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Error (SetReminder): " + ex.Message);
                return false;
            }
        }

        public bool DeleteTask(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM Tasks WHERE Id = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Error (DeleteTask): " + ex.Message);
                return false;
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void SaveUserPreference(string key, string value)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Use INSERT ... ON DUPLICATE KEY UPDATE to either insert or update
                    string query = @"INSERT INTO UserPreferences (PrefKey, PrefValue) 
                                     VALUES (@key, @value) 
                                     ON DUPLICATE KEY UPDATE PrefValue = @value";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@key", key);
                        cmd.Parameters.AddWithValue("@value", value);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Error (SaveUserPreference): " + ex.Message);
            }
        }

        public string GetUserPreference(string key)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT PrefValue FROM UserPreferences WHERE PrefKey = @key";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@key", key);
                        var result = cmd.ExecuteScalar();
                        return result?.ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Error (GetUserPreference): " + ex.Message);
                return "";
            }
        }
    }
}
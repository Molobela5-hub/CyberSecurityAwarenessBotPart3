using System.Collections.Generic;

namespace CyberSecurityAwarenessBotPart3.Services
{
    public class MemoryService
    {
        private Dictionary<string, string> memory = new Dictionary<string, string>();
        private DatabaseService dbService;

        public MemoryService()
        {
            dbService = new DatabaseService();
            LoadFromDatabase();
        }

        private void LoadFromDatabase()
        {
            // Load UserName
            string userName = dbService.GetUserPreference("UserName");
            if (!string.IsNullOrEmpty(userName))
            {
                memory["UserName"] = userName;
            }

            // Load FavouriteTopic
            string favTopic = dbService.GetUserPreference("FavouriteTopic");
            if (!string.IsNullOrEmpty(favTopic))
            {
                memory["FavouriteTopic"] = favTopic;
            }
        }

        public void Remember(string key, string value)
        {
            memory[key] = value;

            // Persist to database
            dbService.SaveUserPreference(key, value);
        }

        public string Recall(string key)
        {
            if (memory.ContainsKey(key))
            {
                return memory[key];
            }

            return "";
        }
    }
}

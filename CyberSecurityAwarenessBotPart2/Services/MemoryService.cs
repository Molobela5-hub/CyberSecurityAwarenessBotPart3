using System.Collections.Generic;

namespace CyberSecurityAwarenessBotPart2.Services
{
    public class MemoryService
    {
        private Dictionary<string, string> memory = new Dictionary<string, string>();

        public void Remember(string key, string value)
        {
            memory[key] = value;
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
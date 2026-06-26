using System.Collections.Generic;

namespace CyberSecurityAwarenessBotPart3.Services
{
    public class SentimentService
    {
        private Dictionary<string, List<string>> emotions =
            new Dictionary<string, List<string>>()
        {
            {
                "worried",
                new List<string> { "worried", "scared", "anxious", "nervous" }
            },

            {
                "frustrated",
                new List<string> { "frustrated", "angry", "annoyed", "upset" }
            },

            {
                "confused",
                new List<string> { "confused", "lost", "unsure", "don't understand" }
            },

            {
                "happy",
                new List<string> { "happy", "excited", "great", "good" }
            },

            {
                "curious",
                new List<string> { "curious", "interested", "wondering" }
            }
        };

        public string DetectEmotion(string input)
        {
            input = input.ToLower();

            foreach (var emotion in emotions)
            {
                foreach (var word in emotion.Value)
                {
                    if (input.Contains(word))
                    {
                        return emotion.Key;
                    }
                }
            }

            return "neutral";
        }
    }
}

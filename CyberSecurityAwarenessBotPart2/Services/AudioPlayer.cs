using System.Media;
using System.IO;

namespace CyberSecurityAwarenessBotPart2
{
    public static class AudioPlayer
    {
        public static void PlayGreeting(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    SoundPlayer player = new SoundPlayer(filePath);
                    player.Load(); // Load the sound first
                    player.Play(); // Play asynchronously (non-blocking)
                }
                // Silently continue if file not found (no console output in WPF)
            }
            catch
            {
                // Silently handle any audio errors (no console output in WPF)
            }
        }
    }
}

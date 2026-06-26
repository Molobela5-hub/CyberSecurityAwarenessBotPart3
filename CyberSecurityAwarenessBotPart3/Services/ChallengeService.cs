using System;
using System.Collections.Generic;
using System.Text;

namespace CyberSecurityAwarenessBotPart3.Services
{
    internal class ChallengeService
    {
        

        public string GetRandomChallenge()
        {
            Random rand = new Random();
            int randomChallenge = rand.Next(1, 4);

            if (randomChallenge == 1) 
                return GetPhishingChallenge();
            else if (randomChallenge == 2) 
                return GetPasswordChallenge();
            else 
                return GetPublicWiFiChallenge();
        }

        public string GetPhishingChallenge()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== CHALLENGE 1: Phishing Email Detection ===");
            sb.AppendLine();
            sb.AppendLine("You receive this message:");
            sb.AppendLine("\"Your bank account is locked. Click here to fix it.\"");
            sb.AppendLine();
            sb.AppendLine("1. Click the link");
            sb.AppendLine("2. Ignore it");
            sb.AppendLine("3. Report it");

            return sb.ToString();
        }

        public string EvaluatePhishingAnswer(string choice)
        {
            switch (choice)
            {
                case "1":
                    return "❌ That is risky! This is likely a phishing scam. Banks never ask you to click links in emails. Always visit the official website directly.";

                case "2":
                    return "⚠️ Better, but reporting it is safer. Reporting helps protect others from falling victim to the same scam.";

                case "3":
                    return "✅ Excellent! You handled it correctly. Reporting phishing attempts helps banks and authorities track scammers.";

                default:
                    return "Invalid choice.";
            }
        }

        public string GetPasswordChallenge()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== CHALLENGE 2: Password Security ===");
            sb.AppendLine();
            sb.AppendLine("You need to create a password for your new online banking account.");
            sb.AppendLine("Which password should you choose?");
            sb.AppendLine();
            sb.AppendLine("1. MyName123");
            sb.AppendLine("2. password");
            sb.AppendLine("3. T@9mK#pL2$qR");
            sb.AppendLine("4. 12345678");

            return sb.ToString();
        }

        public string EvaluatePasswordAnswer(string choice)
        {
            switch (choice)
            {
                case "1":
                    return "❌ Weak password! Contains your name and is too predictable. Avoid using personal information like names, birthdays, or common words.";

                case "2":
                    return "❌ Extremely weak! This is one of the most common passwords hackers try first. Never use dictionary words or common passwords like 'password' or 'admin'.";

                case "3":
                    return "✅ Perfect! This is a strong password. It combines uppercase, lowercase, numbers, and special characters. Length: 12+ characters! Pro tip: Use a password manager to store complex passwords securely.";

                case "4":
                    return "❌ Terrible choice! Sequential numbers are extremely easy to crack. Hackers use automated tools that try common patterns like this instantly.";

                default:
                    return "Invalid choice.";
            }
        }

        public string GetPublicWiFiChallenge()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== CHALLENGE 3: Public Wi-Fi Safety ===");
            sb.AppendLine();
            sb.AppendLine("You're at a coffee shop and need to check your bank balance urgently.");
            sb.AppendLine("There's a free public Wi-Fi network called 'Free_Coffee_WiFi'.");
            sb.AppendLine();
            sb.AppendLine("What should you do?");
            sb.AppendLine("1. Connect and check your bank account immediately");
            sb.AppendLine("2. Use your mobile data instead");
            sb.AppendLine("3. Connect but only browse non-sensitive websites");

            return sb.ToString();
        }

        public string EvaluatePublicWiFiAnswer(string choice)
        {
            switch (choice)
            {
                case "1":
                    return "❌ Very risky! Public Wi-Fi is not secure for banking. Hackers can intercept data on public networks using 'man-in-the-middle' attacks. Your login credentials and financial data could be stolen!";

                case "2":
                    return "✅ Excellent decision! Mobile data is much safer. Mobile networks are encrypted, making them more secure for sensitive activities. If you must use public Wi-Fi, always use a VPN for encryption.";

                case "3":
                    return "⚠️ Partial credit, but still not ideal. Even 'non-sensitive' browsing can expose information. Hackers can track websites you visit. Best practice: Save sensitive tasks for secure networks only.";

                default:
                    return "Invalid choice.";
            }
        }

        public string GetRandomTip()
        {
            string[] tips =
            {
                "Use strong passwords.",
                "Do not click unknown links.",
                "Enable two-factor authentication.",
                "Avoid public Wi-Fi for sensitive data.",
                "Phishing is very common."
            };

            Random rand = new Random();
            return "Tip: " + tips[rand.Next(tips.Length)];
        }
    }
}

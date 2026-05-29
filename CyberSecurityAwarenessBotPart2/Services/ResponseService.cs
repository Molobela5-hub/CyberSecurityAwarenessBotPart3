using System;
using System.Collections.Generic

namespace CyberSecurityAwarenessBotPart2.Services
{
    public class ResponseService
    {
        private string lastTopic = "";

        private Dictionary<string, List<string>> responses;
        private Dictionary<string, List<string>> additionalTips;
        private Dictionary<string, int> tipIndex; // Track which tip to show next

        public ResponseService()
        {
            tipIndex = new Dictionary<string, int>();

            responses = new Dictionary<string, List<string>>()
            {
                {
                    "password",
                    new List<string>()
                    {
                        "Use strong passwords with symbols and numbers.",
                        "Avoid using your name in passwords.",
                        "Use different passwords for different accounts."
                    }
                },

                {
                    "phishing",
                    new List<string>()
                    {
                        "Never click suspicious email links.",
                        "Phishing scams often create urgency.",
                        "Always verify the sender email address."
                    }
                },

                {
                    "cybersecurity",
                    new List<string>()
                    {
                        "Cybersecurity is the practice of protecting systems, networks, and data from attacks.",
                        "Cybersecurity helps keep your personal information safe online.",
                        "Learning cybersecurity is a great way to stay safe in the digital world."
                    }
                },

                {
                    "scam",
                    new List<string>()
                    {
                        "Online scams often ask for personal information.",
                        "Be careful of messages asking for money urgently.",
                        "Never share OTPs or passwords online."
                    }
                },

                {
                    "browsing",
                    new List<string>()
                    {
                        "Always check for HTTPS in the URL before entering sensitive information.",
                        "Use incognito mode when using shared computers.",
                        "Clear your browsing history and cookies regularly."
                    }
                },

                {
                    "2fa",
                    new List<string>()
                    {
                        "Two-factor authentication adds an extra layer of security to your accounts.",
                        "Enable 2FA on all accounts that support it.",
                        "Use an authenticator app instead of SMS when possible."
                    }
                },

                {
                    "malware",
                    new List<string>()
                    {
                        "Malware is malicious software designed to harm your computer or steal data.",
                        "Never download software from untrusted sources.",
                        "Keep your antivirus software updated to protect against malware."
                    }
                }
            };

            // Additional tips for "tell me more" functionality
            additionalTips = new Dictionary<string, List<string>>()
            {
                {
                    "password",
                    new List<string>()
                    {
                        "Enable two-factor authentication for extra security on all accounts.",
                        "Use a password manager to generate and store complex passwords securely.",
                        "Change your passwords every 3-6 months, especially for important accounts.",
                        "Never reuse passwords across different sites - one breach could compromise all accounts.",
                        "Make passwords at least 12 characters long with uppercase, lowercase, numbers, and symbols.",
                        "Avoid dictionary words - use random combinations or passphrases instead."
                    }
                },

                {
                    "phishing",
                    new List<string>()
                    {
                        "Be careful of emails creating urgency or fear - that's a common phishing tactic.",
                        "Hover over links before clicking to see the real URL destination.",
                        "Check for spelling errors in emails - legitimate companies proofread their messages.",
                        "Never enter sensitive information on websites reached through email links.",
                        "Be suspicious of unexpected attachments, even from known contacts.",
                        "Verify requests for money or information by calling the company directly using official numbers."
                    }
                },

                {
                    "scam",
                    new List<string>()
                    {
                        "Never send money to unknown people online, no matter how convincing the story.",
                        "If an offer seems too good to be true, it probably is - be skeptical.",
                        "Don't trust caller ID - scammers can fake it to appear legitimate.",
                        "Real government agencies won't threaten you over the phone or ask for gift cards.",
                        "Be wary of romance scams - scammers build trust over time before asking for money.",
                        "Research companies online before making purchases - check reviews and ratings."
                    }
                },

                {
                    "browsing",
                    new List<string>()
                    {
                        "Use a VPN when connecting to public Wi-Fi networks for added security.",
                        "Keep your browser and plugins up to date to protect against vulnerabilities.",
                        "Disable auto-fill for passwords and credit cards on shared devices.",
                        "Use privacy-focused search engines like DuckDuckGo if you're concerned about tracking.",
                        "Be careful what you download - only use trusted sources for software.",
                        "Log out of accounts when finished, especially on shared computers."
                    }
                },

                {
                    "2fa",
                    new List<string>()
                    {
                        "Authenticator apps like Google Authenticator are more secure than SMS codes.",
                        "Keep backup codes in a safe place in case you lose access to your 2FA device.",
                        "Some platforms offer hardware security keys - these are the most secure option.",
                        "Never share your 2FA codes with anyone - legitimate services won't ask for them.",
                        "Set up 2FA on email accounts first - they're often used to reset other passwords.",
                        "Use biometric authentication (fingerprint/face) when available for added convenience."
                    }
                },

                {
                    "cybersecurity",
                    new List<string>()
                    {
                        "Keep all your software and operating systems updated with the latest security patches.",
                        "Use antivirus software and keep it updated to protect against malware.",
                        "Back up your important data regularly - the 3-2-1 rule: 3 copies, 2 different media, 1 offsite.",
                        "Be cautious about what you share on social media - it can be used for social engineering.",
                        "Encrypt sensitive files and use secure cloud storage services.",
                        "Educate yourself continuously - cyber threats evolve constantly."
                    }
                },

                {
                    "malware",
                    new List<string>()
                    {
                        "Install reputable antivirus software and run regular scans on your system.",
                        "Be extremely cautious with email attachments - scan them before opening.",
                        "Ransomware can encrypt your files - regular backups are your best defense.",
                        "Avoid clicking pop-ups that claim your computer is infected - they're often fake.",
                        "Keep your operating system and all software updated to patch security vulnerabilities.",
                        "Use ad-blockers to prevent malicious ads from infecting your system."
                    }
                }
            };
        }

        public string GetLastTopic()
        {
            return lastTopic;
        }

        public string GetAdditionalTip(string topic)
        {
            if (string.IsNullOrEmpty(topic) || !additionalTips.ContainsKey(topic))
            {
                return "I don't have more information on that topic. Try asking about passwords, phishing, scams, browsing, 2FA, or cybersecurity.";
            }

            // Initialize tip index for this topic if not exists
            if (!tipIndex.ContainsKey(topic))
            {
                tipIndex[topic] = 0;
            }

            List<string> tips = additionalTips[topic];
            string tip = tips[tipIndex[topic]];

            // Move to next tip (cycle through)
            tipIndex[topic] = (tipIndex[topic] + 1) % tips.Count;

            return tip;
        }

        public string GetResponse(string input)
        {
            input = input.ToLower();

            // Check for virus keyword and map it to malware
            if (input.Contains("virus") && !input.Contains("antivirus"))
            {
                input = input.Replace("virus", "malware");
            }

            // Check for ransomware keyword and map it to malware
            if (input.Contains("ransomware"))
            {
                input = input.Replace("ransomware", "malware");
            }

            // Check for spam keyword and map it to phishing
            if (input.Contains("spam"))
            {
                input = input.Replace("spam", "phishing");
            }

            // Check for email keyword and map it to phishing
            if (input.Contains("email") && !input.Contains("phishing"))
            {
                input = input.Replace("email", "phishing");
            }

            // Check for hacker keyword and map it to cybersecurity
            if (input.Contains("hacker") || input.Contains("hacking"))
            {
                input = "cybersecurity " + input;
            }

            // Check for wifi keyword and map it to browsing
            if (input.Contains("wifi") || input.Contains("wi-fi"))
            {
                input = input.Replace("wifi", "browsing").Replace("wi-fi", "browsing");
            }

            foreach (var keyword in responses.Keys)
            {
                if (input.Contains(keyword))
                {
                    lastTopic = keyword;

                    Random random = new Random();

                    List<string> keywordResponses = responses[keyword];

                    return keywordResponses[random.Next(keywordResponses.Count)];
                }
            }

            return "I'm not sure I understand. Try asking about passwords, phishing, scams, browsing, 2FA, or cybersecurity.";
        }
    }
}

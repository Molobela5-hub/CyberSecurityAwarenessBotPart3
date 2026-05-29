using System.Windows;
using CyberSecurityAwarenessBotPart2.Services;

namespace CyberSecurityAwarenessBotPart2
{
     public partial class MainWindow : Window
    {
        private ResponseService responseService = new();
        private SentimentService sentimentService = new();
        private MemoryService memoryService = new();
        private ChallengeService challengeService = new();

        private bool challengeActive = false;
        private string currentChallenge = "";
        private string lastDiscussedTopic = ""; // Track last topic for "tell me more" commands
        private int messageCount = 0; // Track total messages sent

        public MessageDelegate DisplayMessage;

        public MainWindow()
        {
            InitializeComponent();

            DisplayMessage = message =>
            {
                // Add timestamp to user and bot messages
                string timestamp = DateTime.Now.ToString("HH:mm");
                string messageWithTime = $"[{timestamp}] {message}";
                ChatListBox.Items.Add(messageWithTime);
                // Auto-scroll to the latest message
                if (ChatListBox.Items.Count > 0)
                {
                    ChatListBox.ScrollIntoView(ChatListBox.Items[ChatListBox.Items.Count - 1]);
                }
            };

            // Play greeting sound on startup
            string greetingPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Greetings.wav");
            AudioPlayer.PlayGreeting(greetingPath);

            // Display welcome message
            ShowWelcomeMessage();
        }

        private void ShowWelcomeMessage()
        {
            DisplayMessage("Bot: Welcome to the Cybersecurity Awareness Bot! 🔐");
            DisplayMessage("Bot: I'm here to help you learn about cybersecurity.");
            DisplayMessage("Bot: You can:");
            DisplayMessage("Bot:   • Ask me about passwords, phishing, scams, or browsing safety");
            DisplayMessage("Bot:   • Type 'start challenge' to test your knowledge");
            DisplayMessage("Bot:   • Tell me your name: 'My name is [your name]'");
            DisplayMessage("Bot:   • Set your favorite topic: 'My favourite topic is [topic]'");
            DisplayMessage("Bot:   • Type 'help' for more commands");
            DisplayMessage("Bot:   • Type 'clear' to clear the chat");
            DisplayMessage("Bot: Ask me anything! 😊");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {

        string userMessage = UserInputTextBox.Text;


            if (string.IsNullOrWhiteSpace(userMessage))
                return;

            // Increment message counter
            messageCount++;

            // Help command
            if (userMessage.ToLower() == "help")
            {
                DisplayMessage("You: " + userMessage);
                DisplayMessage("Bot: Here are the available commands:");
                DisplayMessage("Bot:   • 'hi/hello/hey' - Greet the bot");
                DisplayMessage("Bot:   • 'start challenge' - Take a cybersecurity quiz");
                DisplayMessage("Bot:   • 'my name is [name]' - Tell me your name");
                DisplayMessage("Bot:   • 'what's my name' - I'll tell you your name");
                DisplayMessage("Bot:   • 'my favourite topic is [topic]' - Set your favorite topic");
                DisplayMessage("Bot:   • 'tell me a tip' - Get a personalized tip");
                DisplayMessage("Bot:   • 'thank you/thanks' - Express gratitude");
                DisplayMessage("Bot:   • 'bye/goodbye' - Say farewell");
                DisplayMessage("Bot:   • 'stats' - View your message statistics");
                DisplayMessage("Bot:   • 'clear' - Clear the chat history");
                DisplayMessage("Bot:   • Ask about: passwords, phishing, scams, browsing, 2FA, malware");
                UserInputTextBox.Clear();
                return;
            }

            // Clear command
            if (userMessage.ToLower() == "clear")
            {
                ChatListBox.Items.Clear();
                DisplayMessage("Bot: Chat cleared! How can I help you? 😊");
                UserInputTextBox.Clear();
                return;
            }

            // Stats command
            if (userMessage.ToLower() == "stats")
            {
                DisplayMessage("You: " + userMessage);
                DisplayMessage($"Bot: 📊 You've sent {messageCount} messages in this session!");
                UserInputTextBox.Clear();
                return;
            }

            // Greeting responses
            string lowerMessage = userMessage.ToLower().Trim();
            if (lowerMessage == "hi" || lowerMessage == "hello" || lowerMessage == "hey" || 
                lowerMessage == "hi there" || lowerMessage == "hello there" || lowerMessage == "greetings" ||
                lowerMessage == "good morning" || lowerMessage == "good afternoon" || lowerMessage == "good evening")
            {
                DisplayMessage("You: " + userMessage);

                string userName = memoryService.Recall("UserName");

                if (!string.IsNullOrEmpty(userName))
                {
                    string[] personalizedGreetings = 
                    {
                        $"Bot: Hello {userName}! How can I help you with cybersecurity today? 😊",
                        $"Bot: Hi {userName}! What would you like to learn about today? 🔐",
                        $"Bot: Hey {userName}! Ready to boost your security knowledge? 💡",
                        $"Bot: Greetings {userName}! What cybersecurity topic interests you? 👋"
                    };

                    Random rand = new Random();
                    DisplayMessage(personalizedGreetings[rand.Next(personalizedGreetings.Length)]);
                }
                else
                {
                    string[] genericGreetings = 
                    {
                        "Bot: Hello! I'm your Cybersecurity Awareness Bot. How can I help you? 😊",
                        "Bot: Hi there! What would you like to learn about cybersecurity today? 🔐",
                        "Bot: Hey! Ready to learn about online safety? Ask me anything! 💡",
                        "Bot: Greetings! I'm here to help you stay safe online. What's on your mind? 👋"
                    };

                    Random rand = new Random();
                    DisplayMessage(genericGreetings[rand.Next(genericGreetings.Length)]);
                }

                UserInputTextBox.Clear();
                return;
            }

            if (userMessage.ToLower() == "start challenge")
            {
                DisplayMessage("You: " + userMessage);

                currentChallenge = challengeService.GetRandomChallenge();
                challengeActive = true;

                DisplayMessage("Bot: " + currentChallenge);
                DisplayMessage("Bot: Enter your answer (1, 2, 3, or 4):");

                UserInputTextBox.Clear();
                return;
            }


            if (userMessage.ToLower().Contains("my favourite topic is"))
            {
                string topic = userMessage
                    .Replace("my favourite topic is", "", StringComparison.OrdinalIgnoreCase)
                    .Trim();

                DisplayMessage("You: " + userMessage);

                memoryService.Remember("FavouriteTopic", topic);

                DisplayMessage("Bot: I'll remember that you're interested in " + topic + ".");

                UserInputTextBox.Clear();
                return;
            }

            if (userMessage.ToLower().Contains("tell me a tip"))
            {
                DisplayMessage("You: " + userMessage);
                string favouriteTopic = memoryService.Recall("FavouriteTopic");

                if (!string.IsNullOrEmpty(favouriteTopic))
                {
                    DisplayMessage(
                        "Bot: Since you're interested in " +
                        favouriteTopic +
                        ", here's a cybersecurity tip: Always stay informed about the latest threats."
                    );
                }
                else
                {
                    DisplayMessage("Bot: I don't know your favourite topic yet.");
                }

                UserInputTextBox.Clear();
                return;
            }

            // "Tell me more" conversation flow commands
            if (lowerMessage == "tell me more" || lowerMessage == "explain more" || 
                lowerMessage == "another tip" || lowerMessage == "give me another example" ||
                lowerMessage == "more" || lowerMessage == "continue" || lowerMessage == "tell me another")
            {
                DisplayMessage("You: " + userMessage);

                // Try to get the last topic from ResponseService first
                string topic = responseService.GetLastTopic();

                // If no topic from ResponseService, try favorite topic from memory
                if (string.IsNullOrEmpty(topic))
                {
                    topic = memoryService.Recall("FavouriteTopic");
                }

                // If still no topic, use lastDiscussedTopic
                if (string.IsNullOrEmpty(topic))
                {
                    topic = lastDiscussedTopic;
                }

                if (!string.IsNullOrEmpty(topic))
                {
                    string additionalTip = responseService.GetAdditionalTip(topic);
                    DisplayMessage($"Bot: Here's more about {topic}: {additionalTip}");
                    lastDiscussedTopic = topic; // Update last discussed topic
                }
                else
                {
                    DisplayMessage("Bot: I'm not sure what topic you want me to explain more about.");
                    DisplayMessage("Bot: Try asking about passwords, phishing, scams, browsing, 2FA, or cybersecurity first!");
                }

                UserInputTextBox.Clear();
                return;
            }

            // Goodbye responses
            if (lowerMessage == "bye" || lowerMessage == "goodbye" || lowerMessage == "see you" || 
                lowerMessage == "see ya" || lowerMessage == "farewell" || lowerMessage == "exit" ||
                lowerMessage == "quit" || lowerMessage == "thanks bye" || lowerMessage == "thank you bye")
            {
                DisplayMessage("You: " + userMessage);

                string userName = memoryService.Recall("UserName");

                if (!string.IsNullOrEmpty(userName))
                {
                    string[] personalizedGoodbyes = 
                    {
                        $"Bot: Goodbye {userName}! Stay safe online! 🔐",
                        $"Bot: See you later {userName}! Remember to use strong passwords! 👋",
                        $"Bot: Take care {userName}! Keep learning about cybersecurity! 💙",
                        $"Bot: Farewell {userName}! Stay vigilant against threats! 🛡️"
                    };

                    Random rand = new Random();
                    DisplayMessage(personalizedGoodbyes[rand.Next(personalizedGoodbyes.Length)]);
                }
                else
                {
                    string[] genericGoodbyes = 
                    {
                        "Bot: Goodbye! Stay safe online! 🔐",
                        "Bot: See you later! Remember what you've learned! 👋",
                        "Bot: Take care! Keep your data secure! 💙",
                        "Bot: Farewell! Stay vigilant against cyber threats! 🛡️"
                    };

                    Random rand = new Random();
                    DisplayMessage(genericGoodbyes[rand.Next(genericGoodbyes.Length)]);
                }

                UserInputTextBox.Clear();
                return;
            }

            if (userMessage.ToLower().StartsWith("my name is"))
            {
                string name = userMessage
                    .Replace("My name is", "", StringComparison.OrdinalIgnoreCase)
                    .Trim();

                memoryService.Remember("UserName", name);

                DisplayMessage("You: " + userMessage);
                DisplayMessage("Bot: Nice to meet you, " + name + ". I'll remember your name.");

                UserInputTextBox.Clear();
                return;
            }

            if (userMessage.ToLower().Contains("what's my name") ||
    userMessage.ToLower().Contains("what is my name"))
            {
                string name = memoryService.Recall("UserName");

                DisplayMessage("You: " + userMessage);

                if (!string.IsNullOrEmpty(name))
                {
                    DisplayMessage("Bot: Your name is " + name + ".");
                }
                else
                {
                    DisplayMessage("Bot: I don't know your name yet.");
                }

                UserInputTextBox.Clear();
                return;
            }

            // Thank you responses
            if (lowerMessage == "thanks" || lowerMessage == "thank you" || lowerMessage == "thx" || 
                lowerMessage == "thanks!" || lowerMessage == "thank you!" || lowerMessage == "ty" ||
                lowerMessage == "appreciate it" || lowerMessage == "thanks a lot")
            {
                DisplayMessage("You: " + userMessage);

                string[] thankYouResponses = 
                {
                    "Bot: You're welcome! Happy to help you stay safe online! 😊",
                    "Bot: My pleasure! Keep learning about cybersecurity! 🔐",
                    "Bot: Anytime! Feel free to ask more questions! 💡",
                    "Bot: Glad I could help! Stay secure! 🛡️",
                    "Bot: You're very welcome! Knowledge is your best defense! 💙"
                };

                Random rand = new Random();
                DisplayMessage(thankYouResponses[rand.Next(thankYouResponses.Length)]);

                UserInputTextBox.Clear();
                return;
            }

            if (challengeActive)
            {
                DisplayMessage("You: " + userMessage);

                string result = "";

                if (currentChallenge.Contains("Phishing"))
                {
                    result = challengeService.EvaluatePhishingAnswer(userMessage);
                }
                else if (currentChallenge.Contains("Password"))
                {
                    result = challengeService.EvaluatePasswordAnswer(userMessage);
                }
                else if (currentChallenge.Contains("Wi-Fi"))
                {
                    result = challengeService.EvaluatePublicWiFiAnswer(userMessage);
                }

                DisplayMessage("Bot: " + result);
                DisplayMessage("Bot: " + challengeService.GetRandomTip());

                challengeActive = false;

                UserInputTextBox.Clear();
                return;
            }

            // Display user message
            DisplayMessage("You: " + userMessage);

            // Temporary bot response
            string botResponse = responseService.GetResponse(userMessage);



            //Emotion detection
            string emotion = sentimentService.DetectEmotion(userMessage);

            // Display bot response
            if (emotion == "worried")
            {
                DisplayMessage("Bot: I can see you're concerned. " + botResponse);
            }
            else if (emotion == "confused")
            {
                DisplayMessage("Bot: Let me explain that more clearly. " + botResponse);
            }
            else if (emotion == "frustrated")
            {
                DisplayMessage("Bot: I understand this can be frustrating. " + botResponse);
            }
            else if (emotion == "happy")
            {
                DisplayMessage("Bot: Great to hear! " + botResponse);
            }
            else if (emotion == "curious")
            {
                DisplayMessage("Bot: That's a great question! " + botResponse);
            }
            else
            {
                DisplayMessage("Bot: " + botResponse);
            }

            // Clear textbox
            UserInputTextBox.Clear();
        }

        private void UserInputTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void UserInputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SendButton_Click(sender, e);
            }
        }
    }
}
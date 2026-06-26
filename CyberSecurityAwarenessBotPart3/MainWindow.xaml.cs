using System.Windows;
using CyberSecurityAwarenessBotPart3.Services;

namespace CyberSecurityAwarenessBotPart3
{
     public partial class MainWindow : Window
    {
        private ResponseService responseService = new();
        private SentimentService sentimentService = new();
        private MemoryService memoryService = new();
        private ChallengeService challengeService = new();
        private DatabaseService dbService = new();

        private bool challengeActive = false;
        private string currentChallenge = "";
        private string lastDiscussedTopic = ""; // Track last topic for "tell me more" commands
        private int messageCount = 0; // Track total messages sent
        private bool awaitingReminder = false;
        private int pendingTaskId = 0;
        private QuizService quizService = new();
        private bool quizActive = false;
        private List<string> activityLog = new List<string>();
        private int tasksCompleted = 0;
        private int quizzesCompleted = 0;
        private bool hasIntroduced = false;
        private Random random = new Random(); // Shared Random instance for better randomness

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
            string? userName = memoryService.Recall("UserName");

            if (!string.IsNullOrEmpty(userName))
            {
                // Returning user - personalized welcome
                DisplayMessage($"Bot: Welcome back, {userName}! 👋 Great to see you again!");
                DisplayMessage("Bot: Ready to continue learning about cybersecurity?");
                DisplayMessage("Bot: 💡 Quick suggestions:");
                DisplayMessage("Bot:   • Type 'start quiz' to test your knowledge");
                DisplayMessage("Bot:   • Type 'show tasks' to see your to-do list");
                DisplayMessage("Bot:   • Ask me anything about cybersecurity");
                hasIntroduced = true;
            }
            else
            {
                // New user - full introduction
                DisplayMessage("Bot: Welcome to the Cybersecurity Awareness Bot! 🔐");
                DisplayMessage("Bot: I'm here to help you learn about cybersecurity in an interactive way.");
                DisplayMessage("Bot:");
                DisplayMessage("Bot: 🎯 Let's get started! You can:");
                DisplayMessage("Bot:   • Tell me your name: 'My name is [your name]'");
                DisplayMessage("Bot:   • Take a quiz: 'start quiz'");
                DisplayMessage("Bot:   • Ask questions: 'What is phishing?'");
                DisplayMessage("Bot:   • Manage tasks: 'Add task - [description]'");
                DisplayMessage("Bot:");
                DisplayMessage("Bot: 💬 Try saying 'My name is [your name]' to begin!");
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {

        string userMessage = UserInputTextBox.Text;


            if (string.IsNullOrWhiteSpace(userMessage))
                return;

            // Increment message counter
            messageCount++;

            // NLP-style intent detection for varied command phrasings
            string intent = DetectIntent(userMessage);

            // Handle "add_task" intent
            if (intent == "add_task" && !userMessage.ToLower().StartsWith("add task -") && !userMessage.ToLower().StartsWith("add task-"))
            {
                DisplayMessage("You: " + userMessage);
                DisplayMessage("Bot: It sounds like you want to add a task! Please use the format: 'Add task - [your task description]'");
                UserInputTextBox.Clear();
                return; 
            }

            // Handle "show_tasks" intent
            if (intent == "show_tasks" && userMessage.ToLower() != "show tasks" && userMessage.ToLower() != "view tasks") 
            {
                DisplayMessage("You: " + userMessage); 

                var tasks = dbService.GetAllTasks();

                if (tasks.Count == 0)
                {
                    DisplayMessage("Bot: You don't have any tasks yet. Try 'Add task - [description]' to create one.");
                }
                else
                {
                    DisplayMessage($"Bot: Here are your tasks ({tasks.Count} total):");

                    foreach (var task in tasks)
                    {
                        string status = task.IsCompleted ? "✅ Done" : "⏳ Pending";
                        string reminderText = task.ReminderDate.HasValue
                            ? $" | Reminder: {task.ReminderDate.Value:dd MMM yyyy}"
                            : "";

                        DisplayMessage($"Bot:   [{task.Id}] {task.Title} - {status}{reminderText}");
                    }
                }

                UserInputTextBox.Clear();
                return;
            }

            // Handle "start_quiz" intent
            if (intent == "start_quiz" && userMessage.ToLower() != "start quiz")
            {
                DisplayMessage("You: " + userMessage);

                quizService.ResetQuiz();
                quizActive = true;

                LogActivity("Quiz started");
                DisplayMessage("Bot: 🎓 Welcome to the Cybersecurity Quiz!");
                DisplayMessage("Bot: Test your knowledge with multiple-choice and true/false questions.");
                DisplayMessage("Bot: Let's begin!");

                ShowNextQuizQuestion();

                UserInputTextBox.Clear();
                return;
            }

            // Handle "show_log" intent
            if (intent == "show_log")
            {
                DisplayMessage("You: " + userMessage);
                ShowActivityLog();
                UserInputTextBox.Clear();
                return;
            }

            if (userMessage.ToLower().StartsWith("add task -") || userMessage.ToLower().StartsWith("add task-"))
            {
                DisplayMessage("You: " + userMessage);

                string taskDescription = userMessage.Substring(userMessage.IndexOf('-') + 1).Trim();

                bool success = dbService.AddTask(taskDescription, taskDescription, null);

                if (success)
                {
                    var tasks = dbService.GetAllTasks();
                    pendingTaskId = tasks.Count > 0 ? tasks[0].Id : 0; // most recent task (GetAllTasks orders by Id DESC)
                    awaitingReminder = true;

                    LogActivity($"Task added: '{taskDescription}'");
                    DisplayMessage($"Bot: Task added: '{taskDescription}'. Would you like a reminder? (e.g. 'Yes, remind me in 3 days' or 'No')");
                }
                else
                {
                    DisplayMessage("Bot: Sorry, I couldn't save that task. Please try again.");
                }

                UserInputTextBox.Clear();
                return;
            }

           

            if (awaitingReminder)
            {
                DisplayMessage("You: " + userMessage);

                string lower = userMessage.ToLower();

                if (lower.Contains("no"))
                {
                    DisplayMessage("Bot: No problem, no reminder set.");
                    awaitingReminder = false;
                }
                else
                {
                    // Look for a number in the message (e.g. "remind me in 3 days")
                    var match = System.Text.RegularExpressions.Regex.Match(userMessage, @"\d+");

                    if (match.Success)
                    {
                        int days = int.Parse(match.Value);
                        DateTime reminderDate = DateTime.Now.AddDays(days);

                        bool updated = dbService.SetReminder(pendingTaskId, reminderDate);

                        if (updated)
                        {
                            LogActivity($"Reminder set for task [{pendingTaskId}] in {days} day(s)");
                            DisplayMessage($"Bot: Got it! I'll remind you in {days} day(s) (on {reminderDate:dd MMM yyyy}).");
                        }
                        else
                            DisplayMessage("Bot: Sorry, I couldn't set that reminder.");
                    }
                    else
                    {
                        DisplayMessage("Bot: I didn't catch a number of days. Try 'remind me in 3 days' or say 'No'.");
                        UserInputTextBox.Clear();
                        return; // stay in awaitingReminder mode
                    }

                    awaitingReminder = false;
                }

                UserInputTextBox.Clear();
                return;
            }

            if (userMessage.ToLower() == "show tasks" || userMessage.ToLower() == "view tasks")
            {
                DisplayMessage("You: " + userMessage);

                var tasks = dbService.GetAllTasks();

                if (tasks.Count == 0)
                {
                    DisplayMessage("Bot: You don't have any tasks yet. Try 'Add task - [description]' to create one.");
                }
                else
                {
                    DisplayMessage($"Bot: Here are your tasks ({tasks.Count} total):");

                    foreach (var task in tasks)
                    {
                        string status = task.IsCompleted ? "✅ Done" : "⏳ Pending";
                        string reminderText = task.ReminderDate.HasValue
                            ? $" | Reminder: {task.ReminderDate.Value:dd MMM yyyy}"
                            : "";

                        DisplayMessage($"Bot:   [{task.Id}] {task.Title} - {status}{reminderText}");
                    }
                }

                UserInputTextBox.Clear();
                return;
            }

            if (userMessage.ToLower().StartsWith("complete task"))
            {
                DisplayMessage("You: " + userMessage);

                string idText = userMessage.Replace("complete task", "", StringComparison.OrdinalIgnoreCase).Trim();

                if (int.TryParse(idText, out int taskId))
                {
                    bool success = dbService.MarkComplete(taskId);

                    if (success)
                    {
                        tasksCompleted++;
                        LogActivity($"Task [{taskId}] marked as completed");
                        DisplayMessage($"Bot: Task [{taskId}] marked as completed! ✅");

                        // Interactive suggestions after completing a task
                        if (tasksCompleted == 1)
                        {
                            DisplayMessage("Bot: Great job! 🎉 Keep track of your progress with 'show tasks'");
                        }
                        else if (tasksCompleted % 3 == 0)
                        {
                            DisplayMessage("Bot: You're on a roll! 💪 Want to test your knowledge? Try 'start quiz'");
                        }
                        else
                        {
                            DisplayMessage("Bot: 💡 Tip: Type 'show activity log' to see all your recent actions!");
                        }
                    }
                    else
                        DisplayMessage("Bot: Sorry, I couldn't update that task.");
                }
                else
                {
                    DisplayMessage("Bot: Please tell me the task number, e.g. 'complete task 1'.");
                }

                UserInputTextBox.Clear();
                return;
            }

            if (userMessage.ToLower().StartsWith("delete task"))
            {
                DisplayMessage("You: " + userMessage);

                string idText = userMessage.Replace("delete task", "", StringComparison.OrdinalIgnoreCase).Trim();

                if (int.TryParse(idText, out int taskId))
                {
                    bool success = dbService.DeleteTask(taskId);

                    if (success)
                    {
                        LogActivity($"Task [{taskId}] deleted");
                        DisplayMessage($"Bot: Task [{taskId}] deleted.");
                    }
                    else
                        DisplayMessage("Bot: Sorry, I couldn't delete that task.");
                }
                else
                {
                    DisplayMessage("Bot: Please tell me the task number, e.g. 'delete task 1'.");
                }

                UserInputTextBox.Clear();
                return;
            }

            // Help command
            if (userMessage.ToLower() == "help")
            {
                string? userName = memoryService.Recall("UserName");
                string greeting = !string.IsNullOrEmpty(userName) ? userName : "there";

                DisplayMessage("You: " + userMessage);
                DisplayMessage($"Bot: Hi {greeting}! 👋 Here's your interactive guide:");
                DisplayMessage("Bot:");
                DisplayMessage("Bot: 🎯 GETTING STARTED");
                DisplayMessage("Bot:   • 'my name is [name]' - Introduce yourself");
                DisplayMessage("Bot:   • 'start quiz' - Test your knowledge");
                DisplayMessage("Bot:   • 'hi/hello' - Get a personalized greeting");
                DisplayMessage("Bot:");
                DisplayMessage("Bot: 📝 TASK MANAGEMENT");
                DisplayMessage("Bot:   • 'Add task - [description]' - Create a to-do");
                DisplayMessage("Bot:   • 'show tasks' - View your task list");
                DisplayMessage("Bot:   • 'complete task [number]' - Mark as done");
                DisplayMessage("Bot:   • 'delete task [number]' - Remove a task");
                DisplayMessage("Bot:");
                DisplayMessage("Bot: 💬 LEARNING");
                DisplayMessage("Bot:   • Ask: 'What is phishing?'");
                DisplayMessage("Bot:   • Ask: 'How to create strong passwords?'");
                DisplayMessage("Bot:   • 'tell me more' - Get additional info");
                DisplayMessage("Bot:   • 'my favourite topic is [topic]' - Set preference");
                DisplayMessage("Bot:");
                DisplayMessage("Bot: 📊 TRACKING");
                DisplayMessage("Bot:   • 'show activity log' - See recent actions");
                DisplayMessage("Bot:   • 'stats' - View your statistics");
                DisplayMessage("Bot:   • 'clear' - Clear chat history");
                DisplayMessage("Bot:");
                DisplayMessage("Bot: 💡 Try: 'start quiz' or ask me any cybersecurity question!");
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
                string? userName = memoryService.Recall("UserName");
                string greeting = !string.IsNullOrEmpty(userName) ? userName : "friend";

                DisplayMessage("You: " + userMessage);
                DisplayMessage($"Bot: 📊 Your Activity Stats, {greeting}:");
                DisplayMessage($"Bot:   💬 Messages sent: {messageCount}");
                DisplayMessage($"Bot:   ✅ Tasks completed: {tasksCompleted}");
                DisplayMessage($"Bot:   🎓 Quizzes completed: {quizzesCompleted}");
                DisplayMessage($"Bot:   📝 Total actions logged: {activityLog.Count}");
                DisplayMessage("Bot:");

                if (quizzesCompleted == 0)
                {
                    DisplayMessage("Bot: 💡 Haven't tried a quiz yet? Type 'start quiz' to test your knowledge!");
                }
                else if (tasksCompleted == 0)
                {
                    DisplayMessage("Bot: 💡 Stay organized! Try: 'Add task - Review security settings'");
                }
                else
                {
                    DisplayMessage("Bot: 🌟 You're doing great! Keep up the good work!");
                }

                UserInputTextBox.Clear();
                return;
            }

            // Activity log command
            if (userMessage.ToLower().Contains("show activity log") || 
                userMessage.ToLower().Contains("show log") || 
                userMessage.ToLower().Contains("what have you done"))
            {
                DisplayMessage("You: " + userMessage);
                ShowActivityLog();
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

                string? userName = memoryService.Recall("UserName");

                if (!string.IsNullOrEmpty(userName))
                {
                    string[] personalizedGreetings = 
                    {
                        $"Bot: Hello {userName}! How can I help you with cybersecurity today? 😊",
                        $"Bot: Hi {userName}! What would you like to learn about today? 🔐",
                        $"Bot: Hey {userName}! Ready to boost your security knowledge? 💡",
                        $"Bot: Greetings {userName}! What cybersecurity topic interests you? 👋"
                    };

                    DisplayMessage(personalizedGreetings[random.Next(personalizedGreetings.Length)]);
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

                    DisplayMessage(genericGreetings[random.Next(genericGreetings.Length)]);
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

            // Quiz feature
            if (userMessage.ToLower() == "start quiz")
            {
                DisplayMessage("You: " + userMessage);

                quizService.ResetQuiz();
                quizActive = true;

                LogActivity("Quiz started");
                DisplayMessage("Bot: 🎓 Welcome to the Cybersecurity Quiz!");
                DisplayMessage("Bot: Test your knowledge with multiple-choice and true/false questions.");
                DisplayMessage("Bot: Let's begin!");

                ShowNextQuizQuestion();

                UserInputTextBox.Clear();
                return;
            }


            if (userMessage.ToLower().Contains("my favourite topic is"))
            {
                string? topic = userMessage
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
                string? favouriteTopic = memoryService.Recall("FavouriteTopic");

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
                string? topic = responseService.GetLastTopic();

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

                string? userName = memoryService.Recall("UserName");

                if (!string.IsNullOrEmpty(userName))
                {
                    string[] personalizedGoodbyes = 
                    {
                        $"Bot: Goodbye {userName}! Stay safe online! 🔐",
                        $"Bot: See you later {userName}! Remember to use strong passwords! 👋",
                        $"Bot: Take care {userName}! Keep learning about cybersecurity! 💙",
                        $"Bot: Farewell {userName}! Stay vigilant against threats! 🛡️"
                    };

                    DisplayMessage(personalizedGoodbyes[random.Next(personalizedGoodbyes.Length)]);
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

                    DisplayMessage(genericGoodbyes[random.Next(genericGoodbyes.Length)]);
                }

                UserInputTextBox.Clear();
                return;
            }

            // Name introduction - with variations
            string lowerInput = userMessage.ToLower();
            string? name = null;

            if (lowerInput.StartsWith("my name is "))
            {
                name = userMessage.Substring("my name is ".Length).Trim();
            }
            else if (lowerInput.StartsWith("i'm ") || lowerInput.StartsWith("im "))
            {
                string prefix = lowerInput.StartsWith("i'm ") ? "i'm " : "im ";
                string remainder = userMessage.Substring(prefix.Length).Trim();

                // Extract name from patterns like "I'm John" or "I'm called John"
                if (remainder.ToLower().StartsWith("called "))
                {
                    name = remainder.Substring("called ".Length).Trim();
                }
                else
                {
                    name = remainder;
                }
            }
            else if (lowerInput.StartsWith("call me "))
            {
                name = userMessage.Substring("call me ".Length).Trim();
            }

            if (!string.IsNullOrEmpty(name))
            {
                memoryService.Remember("UserName", name);
                hasIntroduced = true;

                DisplayMessage("You: " + userMessage);
                DisplayMessage("Bot: Nice to meet you, " + name + "! 👋 I'll remember your name.");
                DisplayMessage("Bot:");
                DisplayMessage("Bot: Now let's make your cybersecurity journey interactive! Here are some ideas:");
                DisplayMessage("Bot:   🎯 'start quiz' - Test your knowledge");
                DisplayMessage("Bot:   📝 'Add task - [description]' - Create security to-dos");
                DisplayMessage("Bot:   💬 Ask me: 'What is phishing?' or 'How to stay safe online?'");
                DisplayMessage("Bot:");
                DisplayMessage("Bot: What would you like to explore first?");

                UserInputTextBox.Clear();
                return;
            }

            // What's my name - with variations
            string lowerMsg = userMessage.ToLower();
            if (lowerMsg.Contains("what's my name") ||
                lowerMsg.Contains("what is my name") ||
                lowerMsg.Contains("whats my name") ||
                lowerMsg.Contains("do you know my name") ||
                lowerMsg.Contains("do you remember my name") ||
                lowerMsg.Contains("tell me my name"))
            {
                string? recalledName = memoryService.Recall("UserName");

                DisplayMessage("You: " + userMessage);

                if (!string.IsNullOrEmpty(recalledName))
                {
                    DisplayMessage("Bot: Your name is " + recalledName + ".");
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

                DisplayMessage(thankYouResponses[random.Next(thankYouResponses.Length)]);

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

            // Handle quiz answers
            if (quizActive)
            {
                DisplayMessage("You: " + userMessage);

                string feedback = quizService.CheckAnswer(userMessage);
                DisplayMessage("Bot: " + feedback);

                if (quizService.HasNextQuestion())
                {
                    ShowNextQuizQuestion();
                }
                else
                {
                    quizzesCompleted++;
                    int score = quizService.GetScore();
                    int total = quizService.GetTotalQuestions();

                    DisplayMessage("Bot: " + quizService.GetFinalFeedback());
                    LogActivity($"Quiz completed - scored {score}/{total}");

                    // Interactive follow-up based on score
                    double percentage = (double)score / total * 100;

                    if (percentage >= 80)
                    {
                        DisplayMessage("Bot: 🌟 Excellent work! You're a cybersecurity star!");
                        DisplayMessage("Bot: 💡 Want to apply your knowledge? Try 'Add task - Review my account passwords'");
                    }
                    else if (percentage >= 60)
                    {
                        DisplayMessage("Bot: 👍 Good job! Want to learn more?");
                        DisplayMessage("Bot: 💡 Ask me: 'Tell me about phishing' or 'How to create strong passwords'");
                    }
                    else
                    {
                        DisplayMessage("Bot: 📚 Keep learning! Practice makes perfect.");
                        DisplayMessage("Bot: 💡 Try asking: 'What is two-factor authentication?' or take another quiz later!");
                    }

                    quizActive = false;
                }

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

            // Show engagement tips periodically
            ShowEngagementTip();

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

        private void ShowNextQuizQuestion()
        {
            var q = quizService.GetCurrentQuestion();
            int questionNumber = quizService.GetCurrentQuestionNumber();
            int totalQuestions = quizService.GetTotalQuestions();

            DisplayMessage($"Bot: 📝 Question {questionNumber}/{totalQuestions}:");
            DisplayMessage($"Bot: {q.Question}");

            foreach (var option in q.Options)
            {
                DisplayMessage($"Bot:   {option}");
            }

            if (q.IsTrueFalse)
            {
                DisplayMessage("Bot: Please answer with True or False.");
            }
            else
            {
                DisplayMessage("Bot: Please answer with the letter (A, B, C, or D).");
            }
        }

        private string DetectIntent(string input)
        {
            string lower = input.ToLower();

            // Detect task-adding intent
            if ((lower.Contains("remind") || lower.Contains("task")) &&
                (lower.Contains("add") || lower.Contains("create") || lower.Contains("set")) &&
                !lower.StartsWith("add task -") && !lower.StartsWith("add task-"))
            {
                return "add_task";
            }

            // Detect "show tasks" intent
            if (lower.Contains("task") &&
                (lower.Contains("show") || lower.Contains("view") || lower.Contains("list") ||
                 lower.Contains("what have i") || lower.Contains("my tasks")))
            {
                return "show_tasks";
            }

            // Detect quiz intent
            if (lower.Contains("quiz") || lower.Contains("test my knowledge") || lower.Contains("test me"))
            {
                return "start_quiz";
            }

            // Detect activity log intent (more specific to avoid matching names containing "log")
            if ((lower.Contains("activity log") || lower.Contains("show log") || 
                 lower.Contains("what have you done") || lower.Contains("recent actions")) &&
                !lower.StartsWith("my name is"))
            {
                return "show_log";
            }

            return "unknown";
        }

        private void LogActivity(string description)
        {
            string entry = $"{DateTime.Now:dd MMM HH:mm} - {description}";
            activityLog.Add(entry);
        }

        private void ShowActivityLog()
        {
            if (activityLog.Count == 0)
            {
                DisplayMessage("Bot: No activity recorded yet.");
            }
            else
            {
                DisplayMessage("Bot: Here's a summary of recent actions:");

                // Get last 5 entries in reverse order (most recent first)
                int startIndex = Math.Max(0, activityLog.Count - 5);
                var recentEntries = activityLog.Skip(startIndex).Reverse().ToList();

                for (int i = 0; i < recentEntries.Count; i++)
                {
                    DisplayMessage($"Bot:   {i + 1}. {recentEntries[i]}");
                }
            }
        }

        private void ShowEngagementTip()
        {
            string? userName = memoryService.Recall("UserName");
            string greeting = !string.IsNullOrEmpty(userName) ? userName : "there";

            // Provide contextual tips based on user activity
            if (messageCount % 10 == 0 && messageCount > 0)
            {
                string[] tips = new string[]
                {
                    $"💡 Tip: You've sent {messageCount} messages! Type 'stats' to see your activity.",
                    "💡 Did you know? You can type 'show activity log' to see what we've accomplished together!",
                    "💡 Quick tip: Use 'start quiz' to test your cybersecurity knowledge anytime!",
                    "💡 Pro tip: You can set reminders for tasks. Try adding a task first!",
                    "💡 Reminder: Type 'help' to see all available commands!"
                };

                DisplayMessage("Bot: " + tips[random.Next(tips.Length)]);
            }
            else if (!hasIntroduced && messageCount == 3)
            {
                DisplayMessage($"Bot: Hey {greeting}! 👋 I noticed you haven't introduced yourself yet.");
                DisplayMessage("Bot: Type 'My name is [your name]' so I can personalize your experience!");
            }
            else if (hasIntroduced && quizzesCompleted == 0 && messageCount == 5)
            {
                DisplayMessage($"Bot: {greeting}, ready for a challenge? 🎯");
                DisplayMessage("Bot: Type 'start quiz' to test your cybersecurity knowledge!");
            }
        }

        private void ShowSmartSuggestion(string context)
        {
            string? userName = memoryService.Recall("UserName");
            string name = !string.IsNullOrEmpty(userName) ? userName : "friend";

            switch (context.ToLower())
            {
                case "learning":
                    DisplayMessage($"Bot: Want to learn more, {name}? Try asking:");
                    DisplayMessage("Bot:   • 'What is two-factor authentication?'");
                    DisplayMessage("Bot:   • 'How do I create a strong password?'");
                    DisplayMessage("Bot:   • 'Tell me about phishing scams'");
                    break;

                case "tasks":
                    DisplayMessage("Bot: 📝 Task management tips:");
                    DisplayMessage("Bot:   • Add a task: 'Add task - [description]'");
                    DisplayMessage("Bot:   • View tasks: 'show tasks'");
                    DisplayMessage("Bot:   • Complete: 'complete task [number]'");
                    break;

                case "quiz":
                    DisplayMessage("Bot: 🎓 Quiz tips:");
                    DisplayMessage("Bot:   • Start: 'start quiz'");
                    DisplayMessage("Bot:   • Answer with A, B, C, D or True/False");
                    DisplayMessage("Bot:   • See your progress in the activity log!");
                    break;
            }
        }
    }
}
using System;
using System.Collections.Generic;

namespace CyberSecurityAwarenessBotPart3.Services
{
    public class QuizQuestion
    {
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public bool IsTrueFalse { get; set; } = false;
    }

    public class QuizService
    {
        private List<QuizQuestion> questions;
        private int currentIndex = 0;
        private int score = 0;

        public QuizService()
        {
            questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "What should you do if you receive an email asking for your password?",
                    Options = new List<string> { "A) Reply with your password", "B) Delete the email", "C) Report the email as phishing", "D) Ignore it" },
                    CorrectAnswer = "C",
                    Explanation = "Reporting phishing emails helps prevent scams and protects others."
                },
                new QuizQuestion
                {
                    Question = "Using the same password for multiple accounts is safe.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = "False",
                    Explanation = "Reusing passwords means one breach can compromise all your accounts.",
                    IsTrueFalse = true
                },
                new QuizQuestion
                {
                    Question = "What is two-factor authentication (2FA) primarily used for?",
                    Options = new List<string> { "A) Faster logins", "B) Extra layer of account security", "C) Changing your password", "D) Deleting your account" },
                    CorrectAnswer = "B",
                    Explanation = "2FA adds an extra layer of security beyond just your password."
                },
                new QuizQuestion
                {
                    Question = "Public Wi-Fi is always safe for online banking.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = "False",
                    Explanation = "Public Wi-Fi can expose your data to hackers using man-in-the-middle attacks.",
                    IsTrueFalse = true
                },
                new QuizQuestion
                {
                    Question = "Which of these is a sign of a phishing email?",
                    Options = new List<string> { "A) Urgent language demanding action", "B) Personalised greeting from a known contact", "C) No links or attachments", "D) Sent from a verified company domain" },
                    CorrectAnswer = "A",
                    Explanation = "Phishing emails often create urgency to pressure you into acting without thinking."
                },
                new QuizQuestion
                {
                    Question = "A strong password should include uppercase, lowercase, numbers, and symbols.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = "True",
                    Explanation = "Mixing character types makes passwords much harder to crack.",
                    IsTrueFalse = true
                },
                new QuizQuestion
                {
                    Question = "What is malware?",
                    Options = new List<string> { "A) A type of antivirus", "B) Software designed to harm or exploit systems", "C) A strong password", "D) A firewall setting" },
                    CorrectAnswer = "B",
                    Explanation = "Malware is malicious software designed to damage or gain unauthorized access to systems."
                },
                new QuizQuestion
                {
                    Question = "It's safe to click links in unexpected emails if they look official.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = "False",
                    Explanation = "Scammers often disguise phishing links to look official. Always verify the source first.",
                    IsTrueFalse = true
                },
                new QuizQuestion
                {
                    Question = "What should you do before downloading software online?",
                    Options = new List<string> { "A) Download from any website", "B) Only download from trusted, official sources", "C) Disable your antivirus first", "D) Share the link with friends" },
                    CorrectAnswer = "B",
                    Explanation = "Downloading only from trusted sources reduces the risk of installing malware."
                },
                new QuizQuestion
                {
                    Question = "Social engineering relies on manipulating people rather than hacking systems directly.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = "True",
                    Explanation = "Social engineering exploits human trust and psychology rather than technical vulnerabilities.",
                    IsTrueFalse = true
                },
                new QuizQuestion
                {
                    Question = "What does VPN stand for?",
                    Options = new List<string> { "A) Virtual Private Network", "B) Verified Public Node", "C) Virus Protection Network", "D) Variable Password Node" },
                    CorrectAnswer = "A",
                    Explanation = "A VPN (Virtual Private Network) encrypts your internet connection for added privacy and security."
                }
            };
        }

        public void ResetQuiz()
        {
            currentIndex = 0;
            score = 0;
        }

        public bool HasNextQuestion()
        {
            return currentIndex < questions.Count;
        }

        public QuizQuestion GetCurrentQuestion()
        {
            return questions[currentIndex];
        }

        public int GetCurrentQuestionNumber()
        {
            return currentIndex + 1;
        }

        public int GetTotalQuestions()
        {
            return questions.Count;
        }

        public string CheckAnswer(string userAnswer)
        {
            var current = questions[currentIndex];
            string cleanedAnswer = userAnswer.Trim().ToUpper();

            bool isCorrect;

            if (current.IsTrueFalse)
            {
                isCorrect = cleanedAnswer.StartsWith("T") && current.CorrectAnswer == "True" ||
                            cleanedAnswer.StartsWith("F") && current.CorrectAnswer == "False";
            }
            else
            {
                isCorrect = cleanedAnswer == current.CorrectAnswer;
            }

            string result;

            if (isCorrect)
            {
                score++;
                result = "✅ Correct! " + current.Explanation;
            }
            else
            {
                result = $"❌ Incorrect. The correct answer was {current.CorrectAnswer}. {current.Explanation}";
            }

            currentIndex++;
            return result;
        }

        public int GetScore()
        {
            return score;
        }

        public string GetFinalFeedback()
        {
            double percentage = (double)score / questions.Count * 100;

            if (percentage >= 80)
                return $"🏆 Great job! You scored {score}/{questions.Count}. You're a cybersecurity pro!";
            else if (percentage >= 50)
                return $"👍 Good effort! You scored {score}/{questions.Count}. Keep learning to stay safe online!";
            else
                return $"📚 You scored {score}/{questions.Count}. Keep learning to stay safe online!";
        }
    }
}
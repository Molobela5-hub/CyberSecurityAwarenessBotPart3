<img width="1907" height="868" alt="image" src="https://github.com/user-attachments/assets/74497251-ce01-4f0c-8431-2f8a8c3773de" />

# Cybersecurity Awareness Chatbot — Part 3 / POE

A WPF (C#) desktop chatbot that educates users on cybersecurity topics, manages cybersecurity-related tasks with reminders, runs a knowledge quiz, simulates basic NLP-style intent detection, and keeps an activity log of its own actions.

This is the final part (Part 3/POE) of a three-part project, building on Part 1 (console chatbot) and Part 2 (WPF GUI with keyword recognition, memory, and sentiment detection).

## Features

- **Voice greeting, ASCII art, and styled console-to-GUI interface** (from Part 1 & 2)
- **Keyword recognition, memory, and sentiment detection** (from Part 2)
- **Task Assistant with MySQL database integration** — add, view, complete, and delete cybersecurity tasks, with optional reminders
- **Cybersecurity Quiz** — 10+ multiple-choice and true/false questions, with instant feedback and a final score
- **NLP Simulation** — recognizes varied phrasing for common commands (e.g. "show my tasks", "test my knowledge")
- **Activity Log** — tracks recent actions (tasks added, reminders set, quizzes completed) with timestamps

## Requirements

- Windows OS
- Visual Studio 2022 (or later) with the **.NET Desktop Development** workload installed
- **MySQL Server** (8.0 or later) and **MySQL Workbench** (or any MySQL client)
- The **MySql.Data** NuGet package (already referenced in the project; Visual Studio will restore it automatically)

## Setup Instructions

### 1. Install MySQL

If you don't already have MySQL installed, download and install **MySQL Installer** from:
https://dev.mysql.com/downloads/installer/

During installation, choose the **Full** setup type to include both MySQL Server and MySQL Workbench. Set your own root password when prompted — you'll use this in Step 3.

### 2. Create the database

Open **MySQL Workbench**, connect to your local MySQL server, and run the following SQL script to create the database and table:

```sql
CREATE DATABASE CyberSecurityBotDB;

USE CyberSecurityBotDB;

CREATE TABLE Tasks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Description TEXT,
    ReminderDate DATETIME NULL,
    IsCompleted BOOLEAN DEFAULT FALSE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

### 3. Update the connection string

Open `Services/DatabaseService.cs` and find this line near the top of the class:

```csharp
private readonly string connectionString =
    "Server=localhost;Database=CyberSecurityBotDB;Uid=root;Pwd=YOUR_PASSWORD_HERE;";
```

Replace `YOUR_PASSWORD_HERE` with **your own MySQL root password** (the one you set during MySQL installation).

### 4. Run the project

1. Open `CyberSecurityAwarenessBotPart3.sln` in Visual Studio
2. Let NuGet restore the `MySql.Data` package automatically (or right-click the project → Manage NuGet Packages → install `MySql.Data` manually if needed)
3. Press **F5** (or click the green Run button) to build and launch the application

## How to Use the Chatbot

| Command example | What it does |
|---|---|
| `My name is [name]` | Bot remembers your name |
| `Tell me about password safety` | Cybersecurity keyword response |
| `Add task - Enable two-factor authentication` | Adds a new task to the database |
| `Yes, remind me in 3 days` | Sets a reminder on the most recently added task (after being prompted) |
| `Show tasks` | Lists all saved tasks with their status and any reminders |
| `Complete task 1` | Marks task #1 as completed |
| `Delete task 1` | Deletes task #1 |
| `Start quiz` | Begins the 10+ question cybersecurity knowledge quiz |
| `Show activity log` | Displays the last 5 recorded bot actions |
| `Help` | Lists all available commands |

The chatbot also understands varied phrasing for several commands (e.g. "Can you show me my tasks?", "Test my knowledge", "What have you done?") thanks to its NLP simulation feature.

## Project Structure

```
CyberSecurityAwarenessBotPart3/
├── Assets/              # ASCII art and audio greeting file
├── Models/              # UserProfile model
├── Services/
│   ├── AudioPlayer.cs
│   ├── ChallengeService.cs
│   ├── DatabaseService.cs   # MySQL connection & task CRUD
│   ├── MemoryService.cs
│   ├── MessageDelegate.cs
│   ├── QuizService.cs       # Quiz questions & scoring
│   ├── ResponseService.cs
│   └── SentimentService.cs
├── MainWindow.xaml / .cs     # Main chat interface and command logic
└── App.xaml
```

## CI/CD

This repository uses **GitHub Actions** for Continuous Integration. The workflow automatically builds the project on every push to verify it compiles successfully. See the `.github/workflows` folder and the green checkmark badge on commits for build status.

## Notes

- All Part 1 and Part 2 functionality (voice greeting, ASCII art, keyword recognition, memory, sentiment detection) remains fully functional alongside the new Part 3 features.
- The Activity Log is stored in memory for the current session only; tasks are the only data persisted permanently, via the MySQL database.


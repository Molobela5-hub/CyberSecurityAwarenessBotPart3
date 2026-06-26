# Fix for Name Memory Issue

## Problem
The chatbot wasn't remembering your name because it was only storing data in memory (RAM), which is lost when the application closes.

## Solution
Added database persistence for user preferences (name, favorite topic, etc.)

## Setup Required

### Step 1: Create the UserPreferences Table
You need to run the SQL script to create a new table in your database.

**Option A: Using MySQL Workbench**
1. Open MySQL Workbench
2. Connect to your CyberSecurityBotDB database
3. Open the file: `CreateUserPreferencesTable.sql`
4. Click Execute (⚡ icon)

**Option B: Using Command Line**
1. Open Command Prompt or PowerShell
2. Navigate to your MySQL bin folder (e.g., `C:\Program Files\MySQL\MySQL Server 8.0\bin`)
3. Run:
   ```
   mysql -u root -p
   ```
4. Enter your password: `Molobela5$`
5. Run:
   ```sql
   USE CyberSecurityBotDB;
   CREATE TABLE IF NOT EXISTS UserPreferences (
	   PrefKey VARCHAR(50) PRIMARY KEY,
	   PrefValue TEXT
   );
   ```

**Option C: Quick Copy-Paste**
Just copy and paste this SQL into MySQL Workbench query window:
```sql
USE CyberSecurityBotDB;

CREATE TABLE IF NOT EXISTS UserPreferences (
	PrefKey VARCHAR(50) PRIMARY KEY,
	PrefValue TEXT
);
```

### Step 2: Run Your Application
After creating the table, run your chatbot application.

## How It Works Now

✅ **"My name is Lehlogonolo"** 
   - Stores in database immediately
   - Persists between application restarts

✅ **"What's my name"**
   - Retrieves from database
   - Works even after closing and reopening the app

✅ **"My favourite topic is passwords"**
   - Also persists to database

## Database Table Structure

**UserPreferences Table:**
- `PrefKey` (VARCHAR 50, PRIMARY KEY) - e.g., "UserName", "FavouriteTopic"
- `PrefValue` (TEXT) - e.g., "Lehlogonolo", "passwords"

## Code Changes Made

1. **MemoryService.cs** - Now loads and saves to database
2. **DatabaseService.cs** - Added SaveUserPreference() and GetUserPreference() methods
3. **CreateUserPreferencesTable.sql** - SQL script to create the table

## Verify It's Working

After setup:
1. Type: `My name is Lehlogonolo`
2. Bot responds: `Nice to meet you, Lehlogonolo. I'll remember your name.`
3. Close the application completely
4. Reopen the application
5. Type: `Hi` or `Hello`
6. Bot should greet you with: `Hello Lehlogonolo!` (using your name)
7. Type: `What's my name`
8. Bot responds: `Your name is Lehlogonolo.`

If step 6 doesn't show your name, the database table might not be created correctly.

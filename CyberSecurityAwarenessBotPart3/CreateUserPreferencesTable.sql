-- Create UserPreferences table to store user settings like name and favorite topic
-- Run this in MySQL Workbench or your MySQL client

USE CyberSecurityBotDB;

CREATE TABLE IF NOT EXISTS UserPreferences (
	PrefKey VARCHAR(50) PRIMARY KEY,
	PrefValue TEXT
);

-- Optionally verify the table was created
SHOW TABLES;

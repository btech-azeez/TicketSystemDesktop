# Database Queries Reference
## Customer Support Ticket System

---

## 1. DATABASE SETUP QUERIES

### Create Database and Tables

```sql
-- Create Database
DROP DATABASE IF EXISTS TicketSystemDB;
CREATE DATABASE TicketSystemDB;
USE TicketSystemDB;

-- Users Table
CREATE TABLE Users (
    UserId INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    Role ENUM('User', 'Admin') NOT NULL DEFAULT 'User',
    Email VARCHAR(100),
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE,
    INDEX idx_username (Username),
    INDEX idx_role (Role)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Tickets Table
CREATE TABLE Tickets (
    TicketId INT AUTO_INCREMENT PRIMARY KEY,
    TicketNumber VARCHAR(20) NOT NULL UNIQUE,
    Subject VARCHAR(200) NOT NULL,
    Description TEXT NOT NULL,
    Priority ENUM('Low', 'Medium', 'High') NOT NULL,
    Status ENUM('Open', 'In Progress', 'Closed') NOT NULL DEFAULT 'Open',
    CreatedBy INT NOT NULL,
    AssignedTo INT NULL,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    LastModifiedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    FOREIGN KEY (AssignedTo) REFERENCES Users(UserId),
    INDEX idx_ticket_number (TicketNumber),
    INDEX idx_status (Status),
    INDEX idx_created_by (CreatedBy),
    INDEX idx_assigned_to (AssignedTo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Ticket Status History Table
CREATE TABLE TicketStatusHistory (
    HistoryId INT AUTO_INCREMENT PRIMARY KEY,
    TicketId INT NOT NULL,
    OldStatus ENUM('Open', 'In Progress', 'Closed') NULL,
    NewStatus ENUM('Open', 'In Progress', 'Closed') NOT NULL,
    ChangedBy INT NOT NULL,
    ChangedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    Comments TEXT,
    FOREIGN KEY (TicketId) REFERENCES Tickets(TicketId) ON DELETE CASCADE,
    FOREIGN KEY (ChangedBy) REFERENCES Users(UserId),
    INDEX idx_ticket_id (TicketId),
    INDEX idx_changed_date (ChangedDate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Ticket Comments Table
CREATE TABLE TicketComments (
    CommentId INT AUTO_INCREMENT PRIMARY KEY,
    TicketId INT NOT NULL,
    CommentText TEXT NOT NULL,
    CommentedBy INT NOT NULL,
    IsInternal BOOLEAN DEFAULT FALSE,
    CommentDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (TicketId) REFERENCES Tickets(TicketId) ON DELETE CASCADE,
    FOREIGN KEY (CommentedBy) REFERENCES Users(UserId),
    INDEX idx_ticket_id (TicketId),
    INDEX idx_comment_date (CommentDate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### Insert Sample Data

```sql
-- Insert Default Users
INSERT INTO Users (Username, Password, FullName, Role, Email) VALUES
('admin', 'admin123', 'System Administrator', 'Admin', 'admin@ticketsystem.com'),
('john.doe', 'user123', 'John Doe', 'User', 'john.doe@example.com'),
('jane.smith', 'user123', 'Jane Smith', 'User', 'jane.smith@example.com'),
('bob.admin', 'admin123', 'Bob Admin', 'Admin', 'bob.admin@ticketsystem.com');

-- Insert Sample Tickets
INSERT INTO Tickets (TicketNumber, Subject, Description, Priority, Status, CreatedBy, AssignedTo) VALUES
('TKT-00001', 'Login Issue', 'Unable to login to the system', 'High', 'Open', 2, NULL),
('TKT-00002', 'Feature Request', 'Need export functionality', 'Medium', 'In Progress', 3, 1),
('TKT-00003', 'Bug Report', 'Application crashes on startup', 'High', 'Closed', 2, 4);

-- Insert Sample Status History
INSERT INTO TicketStatusHistory (TicketId, OldStatus, NewStatus, ChangedBy, Comments) VALUES
(1, NULL, 'Open', 2, 'Ticket created'),
(2, NULL, 'Open', 3, 'Ticket created'),
(2, 'Open', 'In Progress', 1, 'Working on this feature'),
(3, NULL, 'Open', 2, 'Ticket created'),
(3, 'Open', 'In Progress', 4, 'Investigating the issue'),
(3, 'In Progress', 'Closed', 4, 'Bug fixed in latest release');

-- Insert Sample Comments
INSERT INTO TicketComments (TicketId, CommentText, CommentedBy, IsInternal) VALUES
(1, 'I have tried resetting my password but still cannot login', 2, FALSE),
(2, 'This would be very helpful for reporting', 3, FALSE),
(2, 'Checking feasibility with development team', 1, TRUE),
(3, 'Issue occurs on Windows 10 machines', 2, FALSE),
(3, 'Root cause identified - missing DLL file', 4, TRUE);

COMMIT;
```

---

## 2. COMMON QUERIES

### User Management

```sql
-- Get User by Username and Password (Login)
SELECT UserId, Username, Password, FullName, Role, Email, CreatedDate, IsActive 
FROM Users 
WHERE Username = 'admin' AND Password = 'admin123' AND IsActive = 1;

-- Get All Admin Users
SELECT UserId, Username, FullName, Role, Email 
FROM Users 
WHERE Role = 'Admin' AND IsActive = 1;

-- Get All Users
SELECT UserId, Username, FullName, Role, Email, CreatedDate, IsActive 
FROM Users 
ORDER BY FullName;

-- Get Active Users Count
SELECT COUNT(*) as ActiveUsers FROM Users WHERE IsActive = 1;
```

### Ticket Management

```sql
-- Generate Next Ticket Number
SELECT MAX(CAST(SUBSTRING(TicketNumber, 5) AS UNSIGNED)) as LastNumber 
FROM Tickets;

-- Create New Ticket
INSERT INTO Tickets (TicketNumber, Subject, Description, Priority, Status, CreatedBy) 
VALUES ('TKT-00004', 'New Issue', 'Description here', 'Medium', 'Open', 2);

-- Get All Tickets with User Names
SELECT t.*, 
       u1.FullName as CreatedByName, 
       u2.FullName as AssignedToName
FROM Tickets t
LEFT JOIN Users u1 ON t.CreatedBy = u1.UserId
LEFT JOIN Users u2 ON t.AssignedTo = u2.UserId
ORDER BY t.CreatedDate DESC;

-- Get Tickets by User
SELECT t.*, 
       u1.FullName as CreatedByName, 
       u2.FullName as AssignedToName
FROM Tickets t
LEFT JOIN Users u1 ON t.CreatedBy = u1.UserId
LEFT JOIN Users u2 ON t.AssignedTo = u2.UserId
WHERE t.CreatedBy = 2
ORDER BY t.CreatedDate DESC;

-- Get Ticket by ID with Details
SELECT t.*, 
       u1.FullName as CreatedByName, 
       u2.FullName as AssignedToName
FROM Tickets t
LEFT JOIN Users u1 ON t.CreatedBy = u1.UserId
LEFT JOIN Users u2 ON t.AssignedTo = u2.UserId
WHERE t.TicketId = 1;

-- Update Ticket (Assign and Status)
UPDATE Tickets 
SET AssignedTo = 1, 
    Status = 'In Progress',
    LastModifiedDate = NOW()
WHERE TicketId = 1;

-- Get Tickets by Status
SELECT * FROM Tickets WHERE Status = 'Open' ORDER BY CreatedDate DESC;

-- Get Tickets by Priority
SELECT * FROM Tickets WHERE Priority = 'High' ORDER BY CreatedDate DESC;

-- Get Unassigned Tickets
SELECT * FROM Tickets WHERE AssignedTo IS NULL ORDER BY CreatedDate DESC;
```

### Status History Management

```sql
-- Add Status History
INSERT INTO TicketStatusHistory (TicketId, OldStatus, NewStatus, ChangedBy, Comments) 
VALUES (1, 'Open', 'In Progress', 1, 'Started working on this');

-- Get Ticket History with User Names
SELECT h.*, u.FullName as ChangedByName
FROM TicketStatusHistory h
LEFT JOIN Users u ON h.ChangedBy = u.UserId
WHERE h.TicketId = 1
ORDER BY h.ChangedDate DESC;

-- Get All Status Changes by User
SELECT h.*, t.TicketNumber, t.Subject
FROM TicketStatusHistory h
JOIN Tickets t ON h.TicketId = t.TicketId
WHERE h.ChangedBy = 1
ORDER BY h.ChangedDate DESC;

-- Get Recent Status Changes (Last 24 hours)
SELECT h.*, t.TicketNumber, u.FullName as ChangedByName
FROM TicketStatusHistory h
JOIN Tickets t ON h.TicketId = t.TicketId
LEFT JOIN Users u ON h.ChangedBy = u.UserId
WHERE h.ChangedDate >= DATE_SUB(NOW(), INTERVAL 24 HOUR)
ORDER BY h.ChangedDate DESC;
```

### Comments Management

```sql
-- Add Comment to Ticket
INSERT INTO TicketComments (TicketId, CommentText, CommentedBy, IsInternal) 
VALUES (1, 'This is a comment', 2, FALSE);

-- Get Ticket Comments with User Names
SELECT c.*, u.FullName as CommentedByName
FROM TicketComments c
LEFT JOIN Users u ON c.CommentedBy = u.UserId
WHERE c.TicketId = 1
ORDER BY c.CommentDate DESC;

-- Get Public Comments Only (for users)
SELECT c.*, u.FullName as CommentedByName
FROM TicketComments c
LEFT JOIN Users u ON c.CommentedBy = u.UserId
WHERE c.TicketId = 1 AND c.IsInternal = FALSE
ORDER BY c.CommentDate DESC;

-- Get All Comments by User
SELECT c.*, t.TicketNumber, t.Subject
FROM TicketComments c
JOIN Tickets t ON c.TicketId = t.TicketId
WHERE c.CommentedBy = 2
ORDER BY c.CommentDate DESC;

-- Get Recent Comments (Last 7 days)
SELECT c.*, t.TicketNumber, u.FullName as CommentedByName
FROM TicketComments c
JOIN Tickets t ON c.TicketId = t.TicketId
LEFT JOIN Users u ON c.CommentedBy = u.UserId
WHERE c.CommentDate >= DATE_SUB(NOW(), INTERVAL 7 DAY)
ORDER BY c.CommentDate DESC;
```

---

## 3. REPORTING QUERIES

### Dashboard Statistics

```sql
-- Total Tickets Count
SELECT COUNT(*) as TotalTickets FROM Tickets;

-- Tickets by Status
SELECT Status, COUNT(*) as Count 
FROM Tickets 
GROUP BY Status;

-- Tickets by Priority
SELECT Priority, COUNT(*) as Count 
FROM Tickets 
GROUP BY Priority;

-- Open Tickets Count
SELECT COUNT(*) as OpenTickets 
FROM Tickets 
WHERE Status = 'Open';

-- Tickets Assigned to Admin
SELECT u.FullName as Admin, COUNT(t.TicketId) as TicketCount
FROM Users u
LEFT JOIN Tickets t ON u.UserId = t.AssignedTo
WHERE u.Role = 'Admin'
GROUP BY u.UserId, u.FullName;

-- Average Tickets per User
SELECT AVG(ticket_count) as AvgTicketsPerUser
FROM (
    SELECT CreatedBy, COUNT(*) as ticket_count
    FROM Tickets
    GROUP BY CreatedBy
) as user_tickets;
```

### Performance Reports

```sql
-- Tickets Created Today
SELECT COUNT(*) as CreatedToday 
FROM Tickets 
WHERE DATE(CreatedDate) = CURDATE();

-- Tickets Closed This Week
SELECT COUNT(*) as ClosedThisWeek 
FROM Tickets 
WHERE Status = 'Closed' 
AND YEARWEEK(LastModifiedDate) = YEARWEEK(NOW());

-- Most Active Users (by ticket creation)
SELECT u.FullName, COUNT(t.TicketId) as TicketCount
FROM Users u
LEFT JOIN Tickets t ON u.UserId = t.CreatedBy
GROUP BY u.UserId, u.FullName
ORDER BY TicketCount DESC
LIMIT 10;

-- Most Active Admins (by actions)
SELECT u.FullName, COUNT(h.HistoryId) as Actions
FROM Users u
LEFT JOIN TicketStatusHistory h ON u.UserId = h.ChangedBy
WHERE u.Role = 'Admin'
GROUP BY u.UserId, u.FullName
ORDER BY Actions DESC
LIMIT 10;

-- Average Resolution Time (Open to Closed)
SELECT AVG(TIMESTAMPDIFF(HOUR, t.CreatedDate, h.ChangedDate)) as AvgHoursToClose
FROM Tickets t
JOIN TicketStatusHistory h ON t.TicketId = h.TicketId
WHERE h.NewStatus = 'Closed';
```

### Ticket Analysis

```sql
-- High Priority Open Tickets
SELECT t.TicketNumber, t.Subject, t.CreatedDate, u.FullName as CreatedBy
FROM Tickets t
JOIN Users u ON t.CreatedBy = u.UserId
WHERE t.Priority = 'High' AND t.Status = 'Open'
ORDER BY t.CreatedDate;

-- Unassigned Tickets
SELECT TicketNumber, Subject, Priority, CreatedDate
FROM Tickets
WHERE AssignedTo IS NULL AND Status != 'Closed'
ORDER BY Priority DESC, CreatedDate;

-- Tickets with Most Comments
SELECT t.TicketNumber, t.Subject, COUNT(c.CommentId) as CommentCount
FROM Tickets t
LEFT JOIN TicketComments c ON t.TicketId = c.TicketId
GROUP BY t.TicketId, t.TicketNumber, t.Subject
HAVING CommentCount > 0
ORDER BY CommentCount DESC;

-- Tickets with No Activity (No comments or status changes)
SELECT t.TicketNumber, t.Subject, t.Status, t.CreatedDate
FROM Tickets t
WHERE NOT EXISTS (
    SELECT 1 FROM TicketComments WHERE TicketId = t.TicketId
)
AND NOT EXISTS (
    SELECT 1 FROM TicketStatusHistory 
    WHERE TicketId = t.TicketId AND OldStatus IS NOT NULL
)
ORDER BY t.CreatedDate;
```

---

## 4. MAINTENANCE QUERIES

### Database Cleanup

```sql
-- Delete Old Closed Tickets (older than 1 year)
DELETE FROM Tickets 
WHERE Status = 'Closed' 
AND LastModifiedDate < DATE_SUB(NOW(), INTERVAL 1 YEAR);

-- Archive Old History Records (create archive table first)
-- CREATE TABLE TicketStatusHistory_Archive LIKE TicketStatusHistory;

INSERT INTO TicketStatusHistory_Archive
SELECT * FROM TicketStatusHistory
WHERE ChangedDate < DATE_SUB(NOW(), INTERVAL 1 YEAR);

DELETE FROM TicketStatusHistory
WHERE ChangedDate < DATE_SUB(NOW(), INTERVAL 1 YEAR);
```

### Data Validation

```sql
-- Find Orphaned Tickets (invalid user references)
SELECT t.* FROM Tickets t
LEFT JOIN Users u1 ON t.CreatedBy = u1.UserId
WHERE u1.UserId IS NULL;

-- Find Tickets with Invalid Status Transitions
SELECT * FROM TicketStatusHistory
WHERE OldStatus = 'Closed' AND NewStatus != 'Closed';

-- Check Database Integrity
SELECT 
    (SELECT COUNT(*) FROM Users) as Users,
    (SELECT COUNT(*) FROM Tickets) as Tickets,
    (SELECT COUNT(*) FROM TicketStatusHistory) as History,
    (SELECT COUNT(*) FROM TicketComments) as Comments;
```

### Reset Database

```sql
-- Reset to Initial State
SET FOREIGN_KEY_CHECKS = 0;
TRUNCATE TABLE TicketComments;
TRUNCATE TABLE TicketStatusHistory;
TRUNCATE TABLE Tickets;
TRUNCATE TABLE Users;
SET FOREIGN_KEY_CHECKS = 1;

-- Reset Auto Increment
ALTER TABLE Users AUTO_INCREMENT = 1;
ALTER TABLE Tickets AUTO_INCREMENT = 1;
ALTER TABLE TicketStatusHistory AUTO_INCREMENT = 1;
ALTER TABLE TicketComments AUTO_INCREMENT = 1;

-- Re-insert sample data (use queries from section 1)
```

---

## 5. ADVANCED QUERIES

### Complex Reports

```sql
-- Ticket Aging Report
SELECT 
    t.TicketNumber,
    t.Subject,
    t.Status,
    t.Priority,
    DATEDIFF(NOW(), t.CreatedDate) as DaysOpen,
    u.FullName as AssignedTo
FROM Tickets t
LEFT JOIN Users u ON t.AssignedTo = u.UserId
WHERE t.Status != 'Closed'
ORDER BY DaysOpen DESC;

-- User Performance Report
SELECT 
    u.FullName as User,
    COUNT(DISTINCT t.TicketId) as TotalTickets,
    COUNT(DISTINCT CASE WHEN t.Status = 'Closed' THEN t.TicketId END) as ClosedTickets,
    COUNT(DISTINCT c.CommentId) as TotalComments,
    MAX(t.CreatedDate) as LastTicketDate
FROM Users u
LEFT JOIN Tickets t ON u.UserId = t.CreatedBy
LEFT JOIN TicketComments c ON u.UserId = c.CommentedBy
WHERE u.Role = 'User'
GROUP BY u.UserId, u.FullName;

-- Ticket Timeline (all activities)
SELECT 
    'Status Change' as ActivityType,
    h.ChangedDate as ActivityDate,
    CONCAT(u.FullName, ' changed status from ', COALESCE(h.OldStatus, 'None'), ' to ', h.NewStatus) as Activity
FROM TicketStatusHistory h
JOIN Users u ON h.ChangedBy = u.UserId
WHERE h.TicketId = 1
UNION ALL
SELECT 
    'Comment' as ActivityType,
    c.CommentDate as ActivityDate,
    CONCAT(u.FullName, ' added comment: ', LEFT(c.CommentText, 50)) as Activity
FROM TicketComments c
JOIN Users u ON c.CommentedBy = u.UserId
WHERE c.TicketId = 1
ORDER BY ActivityDate DESC;
```

---

## Connection String

```
Server=localhost;Database=TicketSystemDB;User=root;Password=Shaik@12345;Port=3306;
```

## Default Credentials

**Admin Users:**
- Username: `admin` | Password: `admin123`
- Username: `bob.admin` | Password: `admin123`

**Regular Users:**
- Username: `john.doe` | Password: `user123`
- Username: `jane.smith` | Password: `user123`

---

**Note:** These queries are designed for MySQL 8.0+. Some syntax may need adjustment for other database systems.

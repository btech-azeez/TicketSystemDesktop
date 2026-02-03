-- Database Creation Script for Ticket System
-- MySQL Database

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

-- Insert Default Admin User (Password: admin123)
-- Note: In production, use proper password hashing
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

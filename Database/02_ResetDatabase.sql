-- Reset Database Script
-- Use this to reset the database to initial state for testing

USE TicketSystemDB;

-- Clear existing data
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

-- Insert Users
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

-- Insert Status History
INSERT INTO TicketStatusHistory (TicketId, OldStatus, NewStatus, ChangedBy, Comments) VALUES
(1, NULL, 'Open', 2, 'Ticket created'),
(2, NULL, 'Open', 3, 'Ticket created'),
(2, 'Open', 'In Progress', 1, 'Working on this feature'),
(3, NULL, 'Open', 2, 'Ticket created'),
(3, 'Open', 'In Progress', 4, 'Investigating the issue'),
(3, 'In Progress', 'Closed', 4, 'Bug fixed in latest release');

-- Insert Comments
INSERT INTO TicketComments (TicketId, CommentText, CommentedBy, IsInternal) VALUES
(1, 'I have tried resetting my password but still cannot login', 2, FALSE),
(2, 'This would be very helpful for reporting', 3, FALSE),
(2, 'Checking feasibility with development team', 1, TRUE),
(3, 'Issue occurs on Windows 10 machines', 2, FALSE),
(3, 'Root cause identified - missing DLL file', 4, TRUE);

COMMIT;

SELECT 'Database reset successfully!' as Message;
SELECT COUNT(*) as UserCount FROM Users;
SELECT COUNT(*) as TicketCount FROM Tickets;
SELECT COUNT(*) as HistoryCount FROM TicketStatusHistory;
SELECT COUNT(*) as CommentCount FROM TicketComments;

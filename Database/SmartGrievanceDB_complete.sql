USE master;
GO
IF DB_ID('SmartGrievanceDB') IS NULL
    CREATE DATABASE SmartGrievanceDB;
GO
USE SmartGrievanceDB;
GO

CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL,
    Description VARCHAR(255)
);
CREATE TABLE Departments (
    DepartmentID INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName VARCHAR(100) NOT NULL,
    Description VARCHAR(255)
);
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    RoleID INT NOT NULL,
    DepartmentID INT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID),
    FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID)
);
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(100) NOT NULL,
    DepartmentID INT NOT NULL,
    Description VARCHAR(255),
    FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID)
);
CREATE TABLE Grievances (
    GrievanceID INT IDENTITY(1,1) PRIMARY KEY,
    SubmitterUserID INT NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Description VARCHAR(MAX) NOT NULL,
    Status VARCHAR(50) NOT NULL,
    CategoryID INT NULL,
    Priority VARCHAR(20) NULL,
    AssignedOfficerID INT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    ResolvedAt DATETIME NULL,
    FOREIGN KEY (SubmitterUserID) REFERENCES Users(UserID),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    FOREIGN KEY (AssignedOfficerID) REFERENCES Users(UserID)
);
CREATE TABLE GrievanceAIRecommendations (
    RecommendationID INT IDENTITY(1,1) PRIMARY KEY,
    GrievanceID INT NOT NULL,
    PredictedCategoryID INT NULL,
    PredictedPriority VARCHAR(20),
    ConfidenceScore DECIMAL(5,4),
    RecommendationDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (GrievanceID) REFERENCES Grievances(GrievanceID),
    FOREIGN KEY (PredictedCategoryID) REFERENCES Categories(CategoryID)
);
CREATE TABLE GrievanceHistory (
    HistoryID INT IDENTITY(1,1) PRIMARY KEY,
    GrievanceID INT NOT NULL,
    ActionTaken VARCHAR(100) NOT NULL,
    OldValue VARCHAR(255),
    NewValue VARCHAR(255),
    ChangedByUserID INT NOT NULL,
    ChangeDate DATETIME DEFAULT GETDATE(),
    Comments VARCHAR(MAX),
    FOREIGN KEY (GrievanceID) REFERENCES Grievances(GrievanceID),
    FOREIGN KEY (ChangedByUserID) REFERENCES Users(UserID)
);
CREATE TABLE SimilarGrievances (
    SimilarityID INT IDENTITY(1,1) PRIMARY KEY,
    PrimaryGrievanceID INT NOT NULL,
    SimilarGrievanceID INT NOT NULL,
    SimilarityScore DECIMAL(5,4),
    IdentifiedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PrimaryGrievanceID) REFERENCES Grievances(GrievanceID),
    FOREIGN KEY (SimilarGrievanceID) REFERENCES Grievances(GrievanceID),
    CHECK (PrimaryGrievanceID <> SimilarGrievanceID)
);
CREATE TABLE GrievanceAttachments (
    AttachmentID INT IDENTITY(1,1) PRIMARY KEY,
    GrievanceID INT NOT NULL,
    FilePath VARCHAR(500),
    FileName VARCHAR(255),
    UploadedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (GrievanceID) REFERENCES Grievances(GrievanceID)
);

INSERT INTO Roles (RoleName, Description) VALUES
('User','Employee who submits grievances'),
('Grievance Officer','Officer responsible for handling grievances'),
('Administrator','System administrator');

INSERT INTO Departments (DepartmentName, Description) VALUES
('IT','Information Technology Department'),
('HR','Human Resources Department'),
('Finance','Finance and Accounts Department'),
('Facilities','Facilities and Maintenance Department'),
('Administration','General Administration Department');

INSERT INTO Users (FullName,Email,PasswordHash,RoleID,DepartmentID) VALUES
('Tithi Sharma','tithi.sharma@org.com','HASH001',1,NULL),
('Rahul Mehta','rahul.mehta@org.com','HASH002',1,NULL),
('Ananya Singh','ananya.singh@org.com','HASH003',1,NULL),
('Arjun Verma','arjun.verma@org.com','HASH004',1,NULL),
('Neha Kapoor','neha.kapoor@org.com','HASH005',1,NULL),
('Rohan Malhotra','rohan.malhotra@org.com','HASH006',2,1),
('Priya Nair','priya.nair@org.com','HASH007',2,2),
('Amit Joshi','amit.joshi@org.com','HASH008',2,3),
('Sneha Rao','sneha.rao@org.com','HASH009',2,4),
('Vikram Shah','vikram.shah@org.com','HASH010',2,5),
('System Admin','admin@org.com','HASH011',3,5);

INSERT INTO Categories (CategoryName,DepartmentID,Description) VALUES
('Network Issue',1,'Internet, VPN and network related issues'),
('Hardware Issue',1,'Computer and hardware related issues'),
('Software Issue',1,'Application and software related issues'),
('Payroll',2,'Salary and payroll related issues'),
('Leave Management',2,'Leave and attendance related issues'),
('Employee Benefits',2,'Employee benefit related issues'),
('Salary Discrepancy',3,'Issues related to salary payments'),
('Reimbursement',3,'Expense and reimbursement related issues'),
('Building Maintenance',4,'Building maintenance issues'),
('Electrical Issue',4,'Electrical and power related issues'),
('Cleaning and Sanitation',4,'Cleaning and sanitation issues'),
('General Administration',5,'General administrative issues'),
('Access Card',5,'Office access card related issues'),
('Security',5,'Security related concerns');

INSERT INTO Grievances
(SubmitterUserID,Title,Description,Status,CategoryID,Priority,AssignedOfficerID,CreatedAt,UpdatedAt,ResolvedAt) VALUES
(1,'VPN access unavailable','Unable to access the company VPN since morning and unable to access required work resources.','In Progress',1,'High',6,'2026-08-01 09:15','2026-08-01 10:20',NULL),
(2,'Laptop not starting','Assigned laptop does not start despite being connected to power.','Resolved',2,'Medium',6,'2026-08-01 10:10','2026-08-01 15:30','2026-08-01 15:30'),
(3,'HR portal error','Unable to submit an application through the employee HR portal.','Under Review',3,'Medium',6,'2026-08-02 11:00','2026-08-02 11:30',NULL),
(4,'Salary not credited','Monthly salary has not been credited to the employee account.','Assigned',4,'High',7,'2026-08-02 09:00','2026-08-02 10:00',NULL),
(5,'Incorrect leave balance','The available leave balance displayed in the employee portal is incorrect.','Resolved',5,'Medium',7,'2026-08-03 12:20','2026-08-04 16:00','2026-08-04 16:00'),
(1,'Missing travel reimbursement','Approved travel reimbursement has not been credited.','In Progress',8,'High',8,'2026-08-04 10:15','2026-08-04 11:10',NULL),
(2,'Air conditioning not working','Air conditioning is not functioning in the second floor work area.','Assigned',9,'Medium',9,'2026-08-05 14:00','2026-08-05 14:30',NULL),
(3,'Power fluctuation','Frequent power fluctuations are affecting workstations in the office.','Under Review',10,'High',9,'2026-08-05 15:45','2026-08-05 16:10',NULL),
(4,'Cleaning required','Common area requires immediate cleaning and sanitation.','Resolved',11,'Low',9,'2026-08-06 08:30','2026-08-06 12:00','2026-08-06 12:00'),
(5,'Access card not working','Employee access card is not allowing entry through the main gate.','In Progress',13,'High',10,'2026-08-06 09:20','2026-08-06 09:50',NULL),
(1,'Repeated VPN failure','VPN connection is repeatedly dropping during working hours.','Under Review',1,'High',6,'2026-08-07 10:00','2026-08-07 10:30',NULL),
(2,'Duplicate salary issue','Salary has not been credited for the current month.','Submitted',4,'High',NULL,'2026-08-07 11:15','2026-08-07 11:15',NULL),
(3,'Office security concern','Unauthorized entry was observed near the restricted office area.','Assigned',14,'Critical',10,'2026-08-07 13:00','2026-08-07 13:30',NULL),
(4,'Keyboard malfunction','Keyboard assigned to the workstation is not functioning properly.','Submitted',2,'Low',NULL,'2026-08-08 09:30','2026-08-08 09:30',NULL),
(5,'Incorrect salary amount','Salary credited is lower than the amount stated in the salary statement.','Under Review',7,'High',8,'2026-08-08 10:00','2026-08-08 10:40',NULL);

INSERT INTO GrievanceAIRecommendations
(GrievanceID,PredictedCategoryID,PredictedPriority,ConfidenceScore,RecommendationDate) VALUES
(1,1,'High',0.9700,'2026-08-01 09:16'),
(2,2,'Medium',0.9500,'2026-08-01 10:11'),
(3,3,'Medium',0.9100,'2026-08-02 11:01'),
(4,4,'High',0.9600,'2026-08-02 09:01'),
(5,5,'Medium',0.9400,'2026-08-03 12:21'),
(6,8,'High',0.9300,'2026-08-04 10:16'),
(7,9,'Medium',0.9200,'2026-08-05 14:01'),
(8,10,'High',0.8800,'2026-08-05 15:46'),
(9,11,'Low',0.9800,'2026-08-06 08:31'),
(10,13,'High',0.9500,'2026-08-06 09:21'),
(11,1,'High',0.8900,'2026-08-07 10:01'),
(12,4,'High',0.9600,'2026-08-07 11:16'),
(13,14,'Critical',0.9100,'2026-08-07 13:01'),
(14,2,'Low',0.9700,'2026-08-08 09:31'),
(15,7,'High',0.9400,'2026-08-08 10:01');

INSERT INTO GrievanceHistory
(GrievanceID,ActionTaken,OldValue,NewValue,ChangedByUserID,ChangeDate,Comments) VALUES
(1,'Status Changed','Submitted','Under Review',6,'2026-08-01 09:45','Grievance reviewed by IT officer.'),
(1,'Assigned',NULL,'Rohan Malhotra',6,'2026-08-01 10:00','Assigned to IT grievance officer.'),
(1,'Status Changed','Under Review','In Progress',6,'2026-08-01 10:20','Technical investigation initiated.'),
(2,'Status Changed','Submitted','Assigned',6,'2026-08-01 10:30','Hardware issue assigned to IT officer.'),
(2,'Status Changed','Assigned','Resolved',6,'2026-08-01 15:30','Laptop power issue resolved.'),
(4,'Status Changed','Submitted','Assigned',7,'2026-08-02 10:00','Payroll grievance assigned for verification.'),
(5,'Status Changed','Submitted','Resolved',7,'2026-08-04 16:00','Leave balance corrected.'),
(7,'Status Changed','Submitted','Assigned',9,'2026-08-05 14:30','Facilities officer assigned for inspection.'),
(9,'Status Changed','Submitted','Resolved',9,'2026-08-06 12:00','Cleaning completed.'),
(10,'Status Changed','Submitted','In Progress',10,'2026-08-06 09:50','Access card being reconfigured.'),
(13,'Status Changed','Submitted','Assigned',10,'2026-08-07 13:30','Security officer assigned for investigation.');

INSERT INTO SimilarGrievances
(PrimaryGrievanceID,SimilarGrievanceID,SimilarityScore,IdentifiedDate) VALUES
(1,11,0.9200,'2026-08-07 10:02'),
(4,12,0.9500,'2026-08-07 11:17'),
(2,14,0.8700,'2026-08-08 09:32'),
(4,15,0.8100,'2026-08-08 10:02');

INSERT INTO GrievanceAttachments
(GrievanceID,FilePath,FileName,UploadedAt) VALUES
(1,'/uploads/grievances/1/','vpn_error_screenshot.png','2026-08-01 09:17'),
(2,'/uploads/grievances/2/','laptop_issue.jpg','2026-08-01 10:12'),
(4,'/uploads/grievances/4/','salary_statement.pdf','2026-08-02 09:05'),
(6,'/uploads/grievances/6/','reimbursement_receipt.pdf','2026-08-04 10:18'),
(8,'/uploads/grievances/8/','power_fluctuation_video.mp4','2026-08-05 15:50'),
(13,'/uploads/grievances/13/','security_incident.jpg','2026-08-07 13:05');

SELECT * FROM Roles;
SELECT * FROM Departments;
SELECT * FROM Users;
SELECT * FROM Categories;
SELECT * FROM Grievances;
SELECT * FROM GrievanceAIRecommendations;
SELECT * FROM GrievanceHistory;
SELECT * FROM SimilarGrievances;
SELECT * FROM GrievanceAttachments;

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

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    PasswordSalt VARCHAR(255) NULL,
    RoleID INT NOT NULL,
    DepartmentID INT NULL, -- FK added below to avoid circular dependency
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    LastLoginAt DATETIME NULL,
    MustChangePassword BIT DEFAULT 0,
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

CREATE TABLE Departments (
    DepartmentID INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName VARCHAR(100) NOT NULL,
    Description VARCHAR(255),
    EscalationOfficerID INT NULL,
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (EscalationOfficerID) REFERENCES Users(UserID)
);

-- Add the missing FK for Users now that Departments exists
ALTER TABLE Users
ADD CONSTRAINT FK_Users_Departments FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID);

CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(100) NOT NULL,
    DepartmentID INT NOT NULL,
    Description VARCHAR(255),
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID)
);

CREATE TABLE Grievances (
    GrievanceID INT IDENTITY(1,1) PRIMARY KEY,
    GrievanceCode VARCHAR(50) UNIQUE NOT NULL, -- GRV-YYYY-NNNNNN
    SubmitterUserID INT NOT NULL,
    SubmitterDepartmentID INT NULL,
    Title VARCHAR(200) NOT NULL,
    Description VARCHAR(MAX) NOT NULL,
    Status VARCHAR(50) NOT NULL,
    CategoryID INT NULL,
    Priority VARCHAR(20) NULL,
    AssignedOfficerID INT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    ResolvedAt DATETIME NULL,
    ClosedAt DATETIME NULL,
    SlaDueAt DATETIME NULL,
    ResolutionNotes VARCHAR(MAX) NULL,
    IsDuplicateOfGrievanceID INT NULL,
    ReopenCount INT DEFAULT 0,
    FOREIGN KEY (SubmitterUserID) REFERENCES Users(UserID),
    FOREIGN KEY (SubmitterDepartmentID) REFERENCES Departments(DepartmentID),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    FOREIGN KEY (AssignedOfficerID) REFERENCES Users(UserID),
    FOREIGN KEY (IsDuplicateOfGrievanceID) REFERENCES Grievances(GrievanceID)
);

CREATE TABLE GrievanceAIRecommendations (
    RecommendationID INT IDENTITY(1,1) PRIMARY KEY,
    GrievanceID INT NOT NULL,
    PredictedCategoryID INT NULL,
    PredictedPriority VARCHAR(20),
    ConfidenceScore DECIMAL(5,4),
    PriorityConfidenceScore DECIMAL(5,4) NULL,
    TopCandidatesJson VARCHAR(MAX) NULL,
    ModelVersion VARCHAR(50) NULL,
    WasCategoryAccepted BIT NULL,
    WasPriorityAccepted BIT NULL,
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
    IsInternal BIT DEFAULT 0,
    FOREIGN KEY (GrievanceID) REFERENCES Grievances(GrievanceID),
    FOREIGN KEY (ChangedByUserID) REFERENCES Users(UserID)
);

CREATE TABLE SimilarGrievances (
    SimilarityID INT IDENTITY(1,1) PRIMARY KEY,
    PrimaryGrievanceID INT NOT NULL,
    SimilarGrievanceID INT NOT NULL,
    SimilarityScore DECIMAL(5,4),
    OfficerAction VARCHAR(50) NULL, -- Confirmed / Linked / Dismissed / Pending
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
    ContentType VARCHAR(100) NULL,
    FileSizeBytes BIGINT NULL,
    UploadedByUserID INT NOT NULL,
    UploadedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (GrievanceID) REFERENCES Grievances(GrievanceID),
    FOREIGN KEY (UploadedByUserID) REFERENCES Users(UserID)
);

CREATE TABLE Notifications (
    NotificationID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    GrievanceID INT NULL,
    Type VARCHAR(50) NOT NULL,
    Message VARCHAR(MAX) NOT NULL,
    IsRead BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (GrievanceID) REFERENCES Grievances(GrievanceID)
);

-- ==========================================
-- PRD Section 10.4 Required Indexes
-- ==========================================
CREATE INDEX IX_Grievances_Status_Priority ON Grievances(Status, Priority);
CREATE INDEX IX_Grievances_AssignedOfficer_Status ON Grievances(AssignedOfficerID, Status);
CREATE INDEX IX_Grievances_Submitter_Created ON Grievances(SubmitterUserID, CreatedAt DESC);
CREATE INDEX IX_Grievances_Category ON Grievances(CategoryID);
CREATE INDEX IX_Grievances_SlaDueAt ON Grievances(SlaDueAt) WHERE Status NOT IN ('Resolved', 'Closed');

CREATE INDEX IX_GrievanceHistory_Grievance_Date ON GrievanceHistory(GrievanceID, ChangeDate);
CREATE INDEX IX_SimilarGrievances_Primary_Score ON SimilarGrievances(PrimaryGrievanceID, SimilarityScore DESC);

-- Base Insert queries for testing are identical to earlier so omitting them for brevity.

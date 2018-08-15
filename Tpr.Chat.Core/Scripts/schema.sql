CREATE TABLE tprchat.dbo.ChatSessions (
  AppealId UNIQUEIDENTIFIER NOT NULL
 ,AppealNumber INT NOT NULL
 ,StartTime DATETIME NOT NULL
 ,FinishTime DATETIME NOT NULL
 ,CurrentExpertKey INT NULL
 ,ApplicationDate DATETIME NOT NULL
 ,RegistrationDate DATETIME NOT NULL
 ,Subject NVARCHAR(100) NOT NULL
 ,ExamDate DATETIME NOT NULL
) ON [PRIMARY]
GO

CREATE UNIQUE INDEX KEY_ChatSessions_AppealId
ON tprchat.dbo.ChatSessions (AppealId)
ON [PRIMARY]
GO

CREATE UNIQUE INDEX KEY_ChatSessions_AppealNumber
ON tprchat.dbo.ChatSessions (AppealNumber)
ON [PRIMARY]
GO

CREATE TABLE tprchat.dbo.ChatMessages (
  Id INT IDENTITY
 ,AppealId UNIQUEIDENTIFIER NOT NULL
 ,CreateDate DATETIME NOT NULL
 ,MessageString NVARCHAR(MAX) NULL
 ,ChatMessageTypeId INT NULL
 ,NickName VARCHAR(100) NULL
 ,CONSTRAINT PK_ChatMessages_Id PRIMARY KEY CLUSTERED (Id)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE tprchat.dbo.ChatMessages
ADD CONSTRAINT FK_ChatMessages_AppealId FOREIGN KEY (AppealId) REFERENCES dbo.ChatSessions (AppealId)
GO

CREATE TABLE tprchat.dbo.QuickReplies (
  Id INT IDENTITY
 ,MessageText VARCHAR(255) NOT NULL
 ,CONSTRAINT PK_QuickReplies_Id PRIMARY KEY CLUSTERED (Id)
) ON [PRIMARY]
GO
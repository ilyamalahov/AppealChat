CREATE TABLE dbo.ChatSessions (
  AppealId uniqueidentifier NOT NULL,
  AppealNumber int NOT NULL,
  StartTime datetime NOT NULL,
  FinishTime datetime NOT NULL,
  CurrentExpertKey int NULL,
  ApplicationDate datetime NOT NULL,
  RegistrationDate datetime NOT NULL,
  Subject nvarchar(100) NOT NULL,
  ExamDate datetime NOT NULL
)
ON [PRIMARY]
GO

CREATE UNIQUE INDEX KEY_ChatSessions_AppealId
  ON dbo.ChatSessions (AppealId)
  ON [PRIMARY]
GO

CREATE UNIQUE INDEX KEY_ChatSessions_AppealNumber
  ON dbo.ChatSessions (AppealNumber)
  ON [PRIMARY]
GO

CREATE TABLE dbo.ChatMessages (
  Id int IDENTITY,
  AppealId uniqueidentifier NOT NULL,
  CreateDate datetime NOT NULL,
  MessageString nvarchar(max) NULL,
  ChatMessageTypeId int NULL,
  NickName nvarchar(100) NULL,
  CONSTRAINT PK_ChatMessages_Id PRIMARY KEY CLUSTERED (Id)
)
ON [PRIMARY]
TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE dbo.ChatMessages
  ADD CONSTRAINT FK_ChatMessages_AppealId FOREIGN KEY (AppealId) REFERENCES dbo.ChatSessions (AppealId)
GO

CREATE TABLE dbo.SessionExperts (
  Id int IDENTITY,
  ExpertKey int NOT NULL,
  AppealId uniqueidentifier NOT NULL,
  CONSTRAINT PK_SessionExperts_Id PRIMARY KEY CLUSTERED (Id),
  CONSTRAINT KEY_SessionExperts_Key UNIQUE (ExpertKey)
)
ON [PRIMARY]
GO
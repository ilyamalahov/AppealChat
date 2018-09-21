--
-- Скрипт сгенерирован Devart dbForge Studio for SQL Server, Версия 5.5.369.0
-- Домашняя страница продукта: http://www.devart.com/ru/dbforge/sql/studio
-- Дата скрипта: 9/20/2018 10:46:13 AM
-- Версия сервера: 12.00.2000
--



--
-- Создать таблицу [dbo].[QuickReplies]
--
PRINT (N'Создать таблицу [dbo].[QuickReplies]')
GO
CREATE TABLE dbo.QuickReplies (
  Id int IDENTITY,
  MessageText nvarchar(50) NOT NULL,
  CONSTRAINT PK_QuickReplies_Id PRIMARY KEY CLUSTERED (Id)
)
ON [PRIMARY]
GO

--
-- Создать таблицу [dbo].[ChatSessions]
--
PRINT (N'Создать таблицу [dbo].[ChatSessions]')
GO
CREATE TABLE dbo.ChatSessions (
  AppealId uniqueidentifier NOT NULL,
  AppealNumber int NOT NULL,
  StartTime datetime NOT NULL,
  FinishTime datetime NOT NULL,
  CurrentExpertKey int NULL,
  ApplicationDate datetime NOT NULL,
  RegistrationDate datetime NOT NULL,
  SubjectName nvarchar(100) NOT NULL,
  ExamDate datetime NOT NULL,
  CommissionStartTime datetime NOT NULL,
  CommissionFinishTime datetime NOT NULL,
  CommissionLink varchar(max) NOT NULL,
  IsEarlyCompleted BIT NOT NULL DEFAULT (0),
  EarlyCompleteTime DATETIME NULL
)
ON [PRIMARY]
TEXTIMAGE_ON [PRIMARY]
GO

--
-- Создать индекс [KEY_ChatSessions_AppealId] для объекта типа таблица [dbo].[ChatSessions]
--
PRINT (N'Создать индекс [KEY_ChatSessions_AppealId] для объекта типа таблица [dbo].[ChatSessions]')
GO
CREATE UNIQUE INDEX KEY_ChatSessions_AppealId
  ON dbo.ChatSessions (AppealId)
  ON [PRIMARY]
GO

--
-- Создать индекс [KEY_ChatSessions_AppealNumber] для объекта типа таблица [dbo].[ChatSessions]
--
PRINT (N'Создать индекс [KEY_ChatSessions_AppealNumber] для объекта типа таблица [dbo].[ChatSessions]')
GO
CREATE UNIQUE INDEX KEY_ChatSessions_AppealNumber
  ON dbo.ChatSessions (AppealNumber)
  ON [PRIMARY]
GO

--
-- Создать таблицу [dbo].[ChatMessages]
--
PRINT (N'Создать таблицу [dbo].[ChatMessages]')
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

--
-- Создать внешний ключ [FK_ChatMessages_AppealId] для объекта типа таблица [dbo].[ChatMessages]
--
PRINT (N'Создать внешний ключ [FK_ChatMessages_AppealId] для объекта типа таблица [dbo].[ChatMessages]')
GO
ALTER TABLE dbo.ChatMessages
  ADD CONSTRAINT FK_ChatMessages_AppealId FOREIGN KEY (AppealId) REFERENCES dbo.ChatSessions (AppealId)
GO

--
-- Создать таблицу [dbo].[MemberReplacements]
--
PRINT (N'Создать таблицу [dbo].[MemberReplacements]')
GO
CREATE TABLE dbo.MemberReplacements (
  Id uniqueidentifier NOT NULL,
  AppealId uniqueidentifier NOT NULL,
  RequestTime datetime NOT NULL,
  OldMember int NOT NULL,
  ReplaceTime datetime NULL,
  NewMember int NULL,
  CONSTRAINT PK_MemberReplacements_Id PRIMARY KEY CLUSTERED (Id)
)
ON [PRIMARY]
GO

--
-- Создать индекс [KEY_MemberReplacements_AppealId] для объекта типа таблица [dbo].[MemberReplacements]
--
PRINT (N'Создать индекс [KEY_MemberReplacements_AppealId] для объекта типа таблица [dbo].[MemberReplacements]')
GO
CREATE UNIQUE INDEX KEY_MemberReplacements_AppealId
  ON dbo.MemberReplacements (AppealId)
  ON [PRIMARY]
GO
SET NOEXEC OFF
GO
--
-- Скрипт сгенерирован Devart dbForge Studio for SQL Server, Версия 5.5.369.0
-- Домашняя страница продукта: http://www.devart.com/ru/dbforge/sql/studio
-- Дата скрипта: 8/17/2018 12:33:39 PM
-- Версия сервера: 10.50.4042
--



--
-- Создать таблицу [dbo].[QuickReplies]
--
PRINT (N'Создать таблицу [dbo].[QuickReplies]')
GO
CREATE TABLE dbo.QuickReplies (
  Id int IDENTITY,
  MessageText varchar(255) NOT NULL,
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
  CommissionLink varchar(max) NOT NULL
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
  NickName varchar(100) NULL,
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
SET NOEXEC OFF
GO
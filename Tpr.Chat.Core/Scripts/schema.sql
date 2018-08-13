--
-- Скрипт сгенерирован Devart dbForge Studio for SQL Server, Версия 5.5.369.0
-- Домашняя страница продукта: http://www.devart.com/ru/dbforge/sql/studio
-- Дата скрипта: 8/13/2018 5:18:53 PM
-- Версия сервера: 10.50.4042
--



--
-- Создать таблицу [dbo].[Experts]
--
PRINT (N'Создать таблицу [dbo].[Experts]')
GO
CREATE TABLE dbo.Experts (
  Id int NOT NULL,
  Name varchar(655) NOT NULL,
  CONSTRAINT PK_Experts_Id PRIMARY KEY CLUSTERED (Id)
)
ON [PRIMARY]
GO

--
-- Создать таблицу [dbo].[AppealInfo]
--
PRINT (N'Создать таблицу [dbo].[AppealInfo]')
GO
CREATE TABLE dbo.AppealInfo (
  Number int NOT NULL,
  ApplicationDate datetime NOT NULL,
  RegistrationDate datetime NOT NULL,
  Subject varchar(100) NOT NULL,
  ExamDate datetime NOT NULL,
  CONSTRAINT PK_AppealInfo_Number PRIMARY KEY CLUSTERED (Number)
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
  CurrentExpert int NULL,
  CONSTRAINT KEY_ChatSessions_AppealNumber2 UNIQUE (AppealNumber)
)
ON [PRIMARY]
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
-- Создать внешний ключ [FK_ChatSessions_AppealNumber] для объекта типа таблица [dbo].[ChatSessions]
--
PRINT (N'Создать внешний ключ [FK_ChatSessions_AppealNumber] для объекта типа таблица [dbo].[ChatSessions]')
GO
ALTER TABLE dbo.ChatSessions WITH NOCHECK
  ADD CONSTRAINT FK_ChatSessions_AppealNumber FOREIGN KEY (AppealNumber) REFERENCES dbo.AppealInfo (Number)
GO

--
-- Создать внешний ключ [FK_ChatSessions_CurrentExpert] для объекта типа таблица [dbo].[ChatSessions]
--
PRINT (N'Создать внешний ключ [FK_ChatSessions_CurrentExpert] для объекта типа таблица [dbo].[ChatSessions]')
GO
ALTER TABLE dbo.ChatSessions
  ADD CONSTRAINT FK_ChatSessions_CurrentExpert FOREIGN KEY (CurrentExpert) REFERENCES dbo.Experts (Id)
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
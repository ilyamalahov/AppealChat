INSERT [dbo].[ChatMessageTypes] ([Id], [Name]) VALUES (1, N'Подключился')
GO
INSERT [dbo].[ChatMessageTypes] ([Id], [Name]) VALUES (2, N'Сообщение')
GO
INSERT [dbo].[ChatMessageTypes] ([Id], [Name]) VALUES (3, N'Отключился')
GO
INSERT [dbo].[ChatSessions] ([AppealId], [AppealNumber], [StartTime], [FinishTime]) VALUES (N'b8819639-82a0-4c34-92a6-747006a164f7', 1001, CAST(N'2018-07-18T15:00:00.000' AS DateTime), CAST(N'2018-07-19T15:00:00.000' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[ChatMessages] ON 
GO
INSERT [dbo].[ChatMessages] ([Id], [AppealId], [CreateDate], [MessageString], [NickName], [ChatMessageTypeId]) VALUES (1, N'b8819639-82a0-4c34-92a6-747006a164f7', CAST(N'2018-07-18T10:44:15.113' AS DateTime), N'', N'Эксперт', 1)
GO
INSERT [dbo].[ChatMessages] ([Id], [AppealId], [CreateDate], [MessageString], [NickName], [ChatMessageTypeId]) VALUES (2, N'b8819639-82a0-4c34-92a6-747006a164f7', CAST(N'2018-07-18T10:44:49.233' AS DateTime), N'Я не согласен с оценкой', N'Апеллянт', 2)
GO
INSERT [dbo].[ChatMessages] ([Id], [AppealId], [CreateDate], [MessageString], [NickName], [ChatMessageTypeId]) VALUES (4, N'b8819639-82a0-4c34-92a6-747006a164f7', CAST(N'2018-07-18T14:34:19.707' AS DateTime), NULL, N'Эксперт 123', 1)
GO
INSERT [dbo].[ChatMessages] ([Id], [AppealId], [CreateDate], [MessageString], [NickName], [ChatMessageTypeId]) VALUES (5, N'b8819639-82a0-4c34-92a6-747006a164f7', CAST(N'2018-07-18T14:34:20.107' AS DateTime), N'Все оценки согласно критериям', N'Эксперт 123', 2)
GO
INSERT [dbo].[ChatMessages] ([Id], [AppealId], [CreateDate], [MessageString], [NickName], [ChatMessageTypeId]) VALUES (6, N'b8819639-82a0-4c34-92a6-747006a164f7', CAST(N'2018-07-18T14:34:20.503' AS DateTime), NULL, N'Эксперт 123', 3)
GO
INSERT [dbo].[ChatMessages] ([Id], [AppealId], [CreateDate], [MessageString], [NickName], [ChatMessageTypeId]) VALUES (7, N'b8819639-82a0-4c34-92a6-747006a164f7', CAST(N'2018-07-18T14:35:58.570' AS DateTime), NULL, N'Эксперт 123', 1)
GO
INSERT [dbo].[ChatMessages] ([Id], [AppealId], [CreateDate], [MessageString], [NickName], [ChatMessageTypeId]) VALUES (8, N'b8819639-82a0-4c34-92a6-747006a164f7', CAST(N'2018-07-18T14:35:58.867' AS DateTime), N'Все оценки согласно критериям', N'Эксперт 123', 2)
GO
INSERT [dbo].[ChatMessages] ([Id], [AppealId], [CreateDate], [MessageString], [NickName], [ChatMessageTypeId]) VALUES (9, N'b8819639-82a0-4c34-92a6-747006a164f7', CAST(N'2018-07-18T14:35:59.123' AS DateTime), NULL, N'Эксперт 123', 3)
GO
SET IDENTITY_INSERT [dbo].[ChatMessages] OFF
GO

CREATE TABLE [dbo].[TheTable]
(
	[Ids] INT NOT NULL PRIMARY KEY,
	[Name] VARCHAR(MAX) NULL,
	[AnotherName] AS [Ids] + 1 + Name
)

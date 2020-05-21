CREATE FUNCTION [dbo].[func_GetActualFiles]
(
	@workspaceId int,
	@mapped int
)
RETURNS 
@Result TABLE
(
	[PK] int NOT NULL PRIMARY KEY, 
	[ID] int NOT NULL,
	[Guid] uniqueidentifier NOT NULL,
	[Path] nvarchar(260) NOT NULL,
	[FileID] int NOT NULL,
	[IsFile] bit NOT NULL,
	[IsCheckedOut] bit NOT NULL,
	[Length] bigint NOT NULL
)
AS
BEGIN
	DECLARE @fileType uniqueidentifier;
	SET @fileType = '16c93dd1-46cf-41b0-9760-37fb72d1c55a';
	DECLARE @allMapped int;
	IF (@mapped = 0)
		SET @allMapped = 0;
	ELSE
		SELECT @allMapped = 1 FROM [dbo].[WorkspaceFolders] wf WHERE wf.WorkspaceID = @workspaceId AND wf.[FolderGuid] = 0x0;

	WITH Tree([PK], [ID], [Guid], [IsMapped], [Path], [FileID], [IsFile], [IsCheckedOut], [Length]) AS 
	(
		SELECT f.[$PK], f.[$ID], f.[$Guid], 
			   ISNULL(@allMapped, (SELECT 1 FROM [dbo].[WorkspaceFolders] wf WHERE wf.WorkspaceID = @workspaceId AND wf.[FolderGuid] = f.[$Guid])), 
			   CAST('$/' + f.[Name] + '/' AS nvarchar(255)), f.[$FileID], 
			   CASE f.[$Type] WHEN @fileType THEN 1 ELSE 0 END, 
			   CASE WHEN f.[$WorkspaceID] = @workspaceId THEN 1 ELSE 0 END, 
			   f.[Length]
		FROM [dbo].[Files] f
		WHERE f.[$ParentGuid] = 0x0 AND f.[$Deleted] = 0 AND 
			  ((f.[$WorkspaceID] <> @workspaceId AND f.[$State] IN (1, 2)) OR (f.[$WorkspaceID] = @workspaceId AND f.[$State] IN (1, 3)))
		UNION ALL
		SELECT child.[$PK], child.[$ID], child.[$Guid], 
			   ISNULL(parent.[IsMapped], (SELECT 1 FROM [dbo].[WorkspaceFolders] wf WHERE wf.WorkspaceID = @workspaceId AND wf.[FolderGuid] = child.[$Guid])), 
			   CAST(parent.[Path] + child.[Name] + N'/' AS nvarchar(255)), child.[$FileID], 
			   CASE child.[$Type] WHEN @fileType THEN 1 ELSE 0 END, 
			   CASE WHEN child.[$WorkspaceID] = @workspaceId THEN 1 ELSE 0 END, 
			   child.[Length]
		FROM [dbo].[Files] child
			INNER JOIN [Tree] parent ON parent.[Guid] = child.[$ParentGuid] AND parent.[IsFile] = 0
		WHERE [$Deleted] = 0 AND 
		      ((child.[$WorkspaceID] <> @workspaceId AND child.[$State] IN (1, 2)) OR (child.[$WorkspaceID] = @workspaceId AND child.[$State] IN (1, 3)))
	)
	INSERT INTO @Result ([PK], [ID], [Guid], [Path], [FileID], [IsFile], [IsCheckedOut], [Length])
	SELECT [PK], [ID], [Guid], [Path], [FileID], [IsFile], [IsCheckedOut], [Length] FROM [Tree]
		WHERE [IsMapped] = @mapped;
	
	RETURN 
END
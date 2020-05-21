
CREATE FUNCTION [dbo].[func_IsFileMapped](@fileGuid uniqueidentifier, @workspaceId int)
RETURNS bit
AS
BEGIN
	IF (@fileGuid IS NULL OR @fileGuid = 0x0)
	BEGIN
		IF EXISTS (SELECT [WorkspaceID] FROM [dbo].[WorkspaceFolders] WHERE [FolderGuid] = @fileGuid AND [WorkspaceID] = @workspaceId)
			RETURN 1;
		
		RETURN NULL;
	END
		
	DECLARE @result bit;
	DECLARE @parentGuid uniqueidentifier;

	SELECT @parentGuid = f.[$ParentGuid], @result = (CASE WHEN wf.[FolderGuid] IS NULL THEN 0 ELSE 1 END) 
	FROM [dbo].[Files] f
		LEFT OUTER JOIN [dbo].[WorkspaceFolders] wf ON wf.[FolderGuid] = f.[$Guid] AND wf.[WorkspaceID] = @workspaceId
	WHERE f.[$Guid] = @fileGuid AND 
	(
		(f.[$WorkspaceID] <> @workspaceId AND f.[$State] IN (1, 2)) OR 
		(f.[$WorkspaceID] = @workspaceId AND f.[$State] IN (1, 3))
	);

	IF (@result = 1)
		RETURN @result;
	 
	RETURN [dbo].[func_IsFileMapped](@parentGuid, @workspaceId);
END

GO



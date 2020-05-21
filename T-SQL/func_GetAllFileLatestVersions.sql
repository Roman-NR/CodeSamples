CREATE FUNCTION [dbo].[func_GetAllFileLatestVersions]
(
	@workspaceId int,
	@mapped int
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT ISNULL(af.[Guid], wf.[FileEntityGuid]) AS [Guid], 
		   ISNULL(af.[PK], wf.[FileEntityPK]) AS [PK],
		   wf.[Path] AS [FromPath],
		   af.[Path] AS [ToPath],
		   wf.[FileID] AS [OldFileID],
		   af.[FileID] AS [FileID],
		   ISNULL(af.[IsFile], wf.[IsFile]) AS [IsFile],
		   af.[Length],
		   wf.[NeedUpdate],
		   fs.[Checksum] AS [OldFileChecksum]
	FROM [dbo].[func_GetActualFiles](@workspaceId, @mapped) af 
		FULL OUTER JOIN (SELECT * FROM [dbo].[WorkspaceFiles] WHERE [WorkspaceID] = @workspaceId) wf ON wf.[FileEntityID] = af.[ID]
		LEFT OUTER JOIN [dbo].[FileStorage] fs ON fs.[ID] = wf.[FileID]
	WHERE @mapped = 0 OR (ISNULL(af.[IsCheckedOut], 0) = 0 AND (ISNULL(wf.[FileEntityPK], 0) <> ISNULL(af.[PK], 0) OR wf.[NeedUpdate] = 1))
)
GO

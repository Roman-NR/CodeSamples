using System;

namespace CodeSamples.Workflow.Configuration
{
    public interface IStageChangeHistoryConfiguration
    {
        Guid EntitySetGuid { get; }
        Guid EntityTypeGuid { get; }
        Guid StagePropertyGuid { get; }
        Guid CommentPropertyGuid { get; }
        Guid DateTimePropertyGuid { get; }
        Guid ResponsibleLinkGuid { get; }
        Guid OwnerLinkGuid { get; }
    }
}
using System;

namespace CodeSamples.Workflow.Configuration
{
    public partial class EntityWorkflowConfiguration
    {
        private class StageChangeHistoryConfiguration : IStageChangeHistoryConfiguration
        {
            public Guid EntitySetGuid { get; set; }
            public Guid EntityTypeGuid { get; set; }
            public Guid StagePropertyGuid { get; set; }
            public Guid CommentPropertyGuid { get; set; }
            public Guid DateTimePropertyGuid { get; set; }
            public Guid ResponsibleLinkGuid { get; set; }
            public Guid OwnerLinkGuid { get; set; }
        }
    }
}

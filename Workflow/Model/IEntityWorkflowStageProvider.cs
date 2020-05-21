using System;
using System.Windows.Media;
/* -- */

namespace CodeSamples.Workflow
{
    internal interface IEntityWorkflowStageProvider : IComparable<IEntityWorkflowStageProvider>
    {
        object Id { get; }
        string Name { get; }
        string CodeName { get; }
        Guid IconImageGuid { get; }
        Color? IconImageColor { get; }
        bool IsReadOnly(Entity entity);
    }
}

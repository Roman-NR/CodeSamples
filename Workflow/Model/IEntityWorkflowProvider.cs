using System;
using System.Collections.Generic;
using System.Threading.Tasks;
/* -- */

namespace CodeSamples.Workflow
{
    internal interface IEntityWorkflowProvider
    {
        Guid Guid { get; }
        EntitySetInfo EntitySet { get; }
        bool GetEntityStageId(Entity entity, DataTransaction transaction, out object stageId);
        bool IsCommandVisibleInLinks { get; }
        string Name { get; }
        string CodeName { get; }
        string PropertyName { get; }
        string PropertyCodeName { get; }

        bool IsCommandVisible(Guid linkGuid);
        DataTransaction ForceChangeStage(IEnumerable<Entity> entities, EntityWorkflowStage stage, DataTransaction dataTransaction);
        ConditionItemData GetConditionItem(List<Guid> links, SearchCondition condition);
        IEnumerable<EntityWorkflowStageChange> GetStageChanges(EntityWorkflow workflow, Entity entity, IEnumerable<WorkflowStageChangeData> dataList);
    }
}

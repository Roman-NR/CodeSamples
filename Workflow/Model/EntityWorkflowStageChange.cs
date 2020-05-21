using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSamples.Workflow
{
    public class EntityWorkflowStageChange
    {
        public EntityWorkflowStageChange(
            EntityWorkflow workflow, EntityWorkflowStage stage, EntityWorkflowTransition transition, 
            Guid userGuid, DateTime changeTime, string actionName, string comment)
        {
            Workflow = workflow;
            Stage = stage;
            Transition = transition;
            UserGuid = userGuid;
            ChangeTime = changeTime;
            ActionName = actionName;
            Comment = comment;
        }

        public EntityWorkflow Workflow { get; }
        public EntityWorkflowStage Stage { get; }
        public EntityWorkflowTransition Transition { get; }
        public Guid UserGuid { get; }
        public DateTime ChangeTime { get; }
        public string ActionName { get; }
        public string Comment { get; }

        public override string ToString()
        {
            return ActionName;
        }
    }
}

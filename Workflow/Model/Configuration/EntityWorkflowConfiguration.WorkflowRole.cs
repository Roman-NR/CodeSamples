using System;
/* -- */

namespace CodeSamples.Workflow.Configuration
{
    public partial class EntityWorkflowConfiguration
    {
        private class WorkflowRole : IEntityWorkflowRole
        {
            private Predicate<Entity> _roleMatch;

            public WorkflowRole(Predicate<Entity> roleMatch)
            {
                _roleMatch = roleMatch;
            }

            public bool Check(Entity entity)
            {
                return _roleMatch(entity);
            }
        }
    }
}

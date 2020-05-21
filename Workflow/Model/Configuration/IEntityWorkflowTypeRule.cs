using System;
using System.Collections.Generic;
/* -- */

namespace CodeSamples.Workflow.Configuration
{
    public interface IEntityWorkflowTypeRule
    {
        IEntityWorkflowTypeRule ExcludeDerived();
        IEntityWorkflowTypeRule When(Predicate<EntityType> typeCheck);
        bool Check(EntityType type);
        IEnumerable<EntityType> GetTypes(EntityTypes types);
    }
}
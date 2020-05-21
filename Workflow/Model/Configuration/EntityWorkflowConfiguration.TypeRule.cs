using System;
using System.Collections.Generic;
/* -- */

namespace CodeSamples.Workflow.Configuration
{
    public partial class EntityWorkflowConfiguration
    {
        private class TypeRule : IEntityWorkflowTypeRule
        {
            private Guid _typeGuid;
            private bool _excludeDerived;
            private Predicate<EntityType> _typeCheck;

            public TypeRule(Guid typeGuid)
            {
                _typeGuid = typeGuid;
            }

            public IEntityWorkflowTypeRule ExcludeDerived()
            {
                _excludeDerived = true;
                return this;
            }

            public IEntityWorkflowTypeRule When(Predicate<EntityType> typeCheck)
            {
                _typeCheck = typeCheck;
                return this;
            }

            public bool Check(EntityType type)
            {
                return (_excludeDerived ? type.Guid == _typeGuid : type.IsDerived(_typeGuid)) &&
                       (_typeCheck == null || _typeCheck(type));
            }

            public IEnumerable<EntityType> GetTypes(EntityTypes types)
            {
                if (_excludeDerived)
                {
                    EntityType type = types.Find(_typeGuid);
                    if (type != null && (_typeCheck == null || _typeCheck(type)))
                        yield return type;
                }
                else
                {
                    foreach (EntityType type in types)
                    {
                        if (type.IsDerived(_typeGuid) && (_typeCheck == null || _typeCheck(type)))
                            yield return type;
                    }
                }
            }
        }
    }
}

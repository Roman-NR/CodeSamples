using System;

namespace CodeSamples.Workflow.Configuration
{
    public partial class EntityWorkflowConfiguration
    {
        private class StageInfo : IEntityWorkflowStage
        {
            private object _propertyValue;

            public StageInfo(object propertyValue)
            {
                _propertyValue = propertyValue;
            }

            public object StageId
            {
                get { return _propertyValue; }
            }
        }
    }
}

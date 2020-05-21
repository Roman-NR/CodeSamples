using System;
/* -- */

namespace CodeSamples.Workflow.Configuration
{
    public partial class EntityWorkflowConfiguration
    {
        private class SystemRole : IEntityWorkflowRole
        {
            private Guid _systemRoleGuid;

            public SystemRole(Guid systemRoleGuid)
            {
                _systemRoleGuid = systemRoleGuid;
            }

            public bool Check(Entity entity)
            {
                return entity.Connection.Workspace.IsInGroupOrRole(_systemRoleGuid);
            }
        }
    }
}
